using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace DofusZaapHelper
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private const int SW_RESTORE = 9;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const byte VK_RETURN = 0x0D;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_A = 0x41;
        private const byte VK_V = 0x56;
        private const byte VK_SPACE = 0x20;

        [STAThread]
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length >= 2 && args[0] == "--generate-assets")
            {
                GenerateStaticAssets(args[1]);
                return;
            }

            if (args.Length >= 3 && args[0] == "--generate-zaap-icons")
            {
                GenerateAllZaapIcons(args[1], args[2]);
                return;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;

            string line;
            while ((line = Console.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var req = serializer.Deserialize<Dictionary<string, object>>(line);
                    string cmd = GetStr(req, "cmd", "");
                    string id = GetStr(req, "id", "0");

                    if (cmd == "renderZaap")
                    {
                        string name = GetStr(req, "name", "Zaap");
                        string area = GetStr(req, "area", "Dofus");
                        string coords = GetStr(req, "coords", "[0, 0]");
                        string status = GetStr(req, "status", "");

                        string b64 = RenderZaapButton(name, area, coords, status);
                        WriteJson(serializer, new Dictionary<string, object> {
                            { "id", id },
                            { "ok", true },
                            { "image", "data:image/png;base64," + b64 }
                        });
                    }
                    else if (cmd == "execute")
                    {
                        string text = GetStr(req, "text", "/zaap 5,-18");
                        string mode = GetStr(req, "mode", "autotype_space");
                        string multiMode = GetStr(req, "multiMode", "active");
                        bool focusDofus = GetBool(req, "focusDofus", true);
                        int delayMs = GetInt(req, "delayMs", 60);

                        bool ok = ExecuteCommand(text, mode, multiMode, focusDofus, delayMs);
                        WriteJson(serializer, new Dictionary<string, object> {
                            { "id", id },
                            { "ok", ok }
                        });
                    }
                }
                catch (Exception ex)
                {
                    WriteJson(serializer, new Dictionary<string, object> {
                        { "ok", false },
                        { "error", ex.Message }
                    });
                }
            }
        }

        private static void WriteJson(JavaScriptSerializer serializer, Dictionary<string, object> obj)
        {
            Console.WriteLine(serializer.Serialize(obj));
            Console.Out.Flush();
        }

        private static string GetStr(Dictionary<string, object> d, string k, string def)
        {
            if (d != null && d.ContainsKey(k) && d[k] != null) return d[k].ToString();
            return def;
        }

        private static int GetInt(Dictionary<string, object> d, string k, int def)
        {
            if (d != null && d.ContainsKey(k) && d[k] != null)
            {
                int v;
                if (int.TryParse(d[k].ToString(), out v)) return v;
            }
            return def;
        }

        private static bool GetBool(Dictionary<string, object> d, string k, bool def)
        {
            if (d != null && d.ContainsKey(k) && d[k] != null)
            {
                bool v;
                if (bool.TryParse(d[k].ToString(), out v)) return v;
            }
            return def;
        }

        private static bool ExecuteCommand(string text, string mode, string multiMode, bool focusDofus, int delayMs)
        {
            try
            {
                Clipboard.SetText(text);
                if (mode == "autotype" || mode == "autotype_space" || mode == "autotype_enter")
                {
                    List<IntPtr> dofusWindows = GetAllDofusWindows();
                    IntPtr fg = GetForegroundWindow();

                    if (multiMode == "all" && dofusWindows.Count > 1)
                    {
                        IntPtr initialWindow = dofusWindows.Contains(fg) ? fg : dofusWindows[0];
                        foreach (IntPtr hwnd in dofusWindows)
                        {
                            FocusWindowHandle(hwnd);
                            Thread.Sleep(130);
                            SendChatCommandKeystrokes(mode, delayMs);
                            Thread.Sleep(110);
                        }
                        FocusWindowHandle(initialWindow);
                    }
                    else if (multiMode == "cycle" && dofusWindows.Count > 1)
                    {
                        int currentIdx = dofusWindows.IndexOf(fg);
                        if (currentIdx < 0) currentIdx = 0;

                        IntPtr currentHwnd = dofusWindows[currentIdx];
                        if (fg != currentHwnd)
                        {
                            FocusWindowHandle(currentHwnd);
                            Thread.Sleep(120);
                        }
                        else
                        {
                            Thread.Sleep(30);
                        }

                        SendChatCommandKeystrokes(mode, delayMs);
                        Thread.Sleep(90);

                        int nextIdx = (currentIdx + 1) % dofusWindows.Count;
                        FocusWindowHandle(dofusWindows[nextIdx]);
                    }
                    else
                    {
                        if (focusDofus && dofusWindows.Count > 0)
                        {
                            if (!dofusWindows.Contains(fg))
                            {
                                FocusWindowHandle(dofusWindows[0]);
                                Thread.Sleep(120);
                            }
                            else
                            {
                                Thread.Sleep(30);
                            }
                        }
                        SendChatCommandKeystrokes(mode, delayMs);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SendChatCommandKeystrokes(string mode, int delayMs)
        {
            byte openChatKey = (mode == "autotype_enter") ? VK_RETURN : VK_SPACE;
            TapKey(openChatKey, 45);

            Thread.Sleep(Math.Max(90, delayMs));

            KeyDown(VK_CONTROL);
            Thread.Sleep(15);
            TapKey(VK_A, 25);
            Thread.Sleep(15);
            TapKey(VK_V, 30);
            Thread.Sleep(15);
            KeyUp(VK_CONTROL);

            Thread.Sleep(60);

            TapKey(VK_RETURN, 45);
        }

        private static void KeyDown(byte vk)
        {
            byte scan = (byte)MapVirtualKey(vk, 0);
            keybd_event(vk, scan, 0, UIntPtr.Zero);
        }

        private static void KeyUp(byte vk)
        {
            byte scan = (byte)MapVirtualKey(vk, 0);
            keybd_event(vk, scan, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void TapKey(byte vk, int holdMs = 40)
        {
            KeyDown(vk);
            Thread.Sleep(holdMs);
            KeyUp(vk);
        }

        private static List<IntPtr> GetAllDofusWindows()
        {
            List<Process> matched = new List<Process>();
            Process[] procs = Process.GetProcesses();

            foreach (Process p in procs)
            {
                try
                {
                    if (p.MainWindowHandle == IntPtr.Zero) continue;
                    string pName = p.ProcessName ?? "";
                    if (pName.Equals("Dofus", StringComparison.OrdinalIgnoreCase))
                    {
                        matched.Add(p);
                    }
                }
                catch { }
            }

            if (matched.Count == 0)
            {
                foreach (Process p in procs)
                {
                    try
                    {
                        if (p.MainWindowHandle == IntPtr.Zero) continue;
                        string pName = p.ProcessName ?? "";
                        string wTitle = p.MainWindowTitle ?? "";
                        if (pName.IndexOf("DofusHelper", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                        if (pName.IndexOf("Ankama Launcher", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                        if (pName.IndexOf("Dofus", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            wTitle.IndexOf(" - Dofus", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matched.Add(p);
                        }
                    }
                    catch { }
                }
            }

            matched.Sort((a, b) => a.Id.CompareTo(b.Id));

            List<IntPtr> handles = new List<IntPtr>();
            foreach (Process p in matched)
            {
                try
                {
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        handles.Add(p.MainWindowHandle);
                    }
                }
                catch { }
            }
            return handles;
        }

        private static void FocusWindowHandle(IntPtr target)
        {
            if (target == IntPtr.Zero) return;
            IntPtr fg = GetForegroundWindow();
            if (fg == target) return;

            if (IsIconic(target)) ShowWindow(target, SW_RESTORE);
            uint dummy;
            uint fgThread = GetWindowThreadProcessId(fg, out dummy);
            uint curThread = GetCurrentThreadId();
            if (fgThread != curThread && fgThread != 0)
            {
                AttachThreadInput(curThread, fgThread, true);
                BringWindowToTop(target);
                SetForegroundWindow(target);
                AttachThreadInput(curThread, fgThread, false);
            }
            else
            {
                SetForegroundWindow(target);
            }
        }

        private static Color GetRegionColor(string area)
        {
            if (string.IsNullOrEmpty(area)) return Color.FromArgb(0, 175, 215);
            string a = area.ToLowerInvariant();
            if (a.Contains("amakna")) return Color.FromArgb(76, 175, 80);
            if (a.Contains("astrub")) return Color.FromArgb(212, 160, 23);
            if (a.Contains("cania")) return Color.FromArgb(210, 115, 45);
            if (a.Contains("bonta")) return Color.FromArgb(45, 145, 245);
            if (a.Contains("brâkmar") || a.Contains("brakmar") || a.Contains("sidimote")) return Color.FromArgb(215, 55, 55);
            if (a.Contains("frigost") || a.Contains("valonia")) return Color.FromArgb(70, 195, 245);
            if (a.Contains("pandala") || a.Contains("grobe")) return Color.FromArgb(40, 180, 140);
            if (a.Contains("sufokia")) return Color.FromArgb(20, 155, 200);
            if (a.Contains("otoma") || a.Contains("moon")) return Color.FromArgb(135, 190, 45);
            if (a.Contains("wabbit")) return Color.FromArgb(235, 130, 35);
            if (a.Contains("koalak")) return Color.FromArgb(165, 95, 65);
            if (a.Contains("incarnam")) return Color.FromArgb(145, 115, 235);
            if (a.Contains("saharach")) return Color.FromArgb(225, 175, 55);
            return Color.FromArgb(0, 180, 215);
        }

        private static string ShortenAreaName(string area)
        {
            if (string.IsNullOrEmpty(area)) return "DOFUS";
            return area
                .Replace("Plaines de Cania", "CANIA")
                .Replace("Baie de Sufokia", "SUFOKIA")
                .Replace("Île de Frigost", "FRIGOST")
                .Replace("Île de Pandala", "PANDALA")
                .Replace("Île d'Otomaï", "OTOMAÏ")
                .Replace("Île des Wabbits", "WABBITS")
                .Replace("Île de Moon", "MOON")
                .Replace("Île de Grobe", "GROBE")
                .Replace("Montagne des Koalaks", "KOALAKS")
                .Replace("Landes de Sidimote", "SIDIMOTE")
                .Replace("Archipel des Écailles", "CROCUZKO")
                .Replace("Archipel de Valonia", "VALONIA")
                .Replace("Profondeurs d'Astrub", "ASTRUB PROF.")
                .ToUpperInvariant();
        }

        public static string RenderZaapButton(string name, string area, string coords, string status)
        {
            const int W = 144;
            const int H = 144;
            using (Bitmap bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Color accent = GetRegionColor(area);

                using (LinearGradientBrush bg = new LinearGradientBrush(
                    new Rectangle(0, 0, W, H),
                    Color.FromArgb(18, 24, 34),
                    Color.FromArgb(10, 14, 20),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(bg, 0, 0, W, H);
                }

                using (Pen portalOuter = new Pen(Color.FromArgb(35, 0, 225, 255), 5f))
                using (Pen portalInner = new Pen(Color.FromArgb(45, accent.R, accent.G, accent.B), 2.5f))
                {
                    g.DrawEllipse(portalOuter, 26, 26, 92, 92);
                    g.DrawEllipse(portalInner, 34, 34, 76, 76);
                }

                using (SolidBrush headerBrush = new SolidBrush(Color.FromArgb(220, accent.R, accent.G, accent.B)))
                {
                    FillRoundedRect(g, headerBrush, 6, 6, W - 12, 26, 7);
                }

                string shortArea = ShortenAreaName(area);
                using (Font areaFont = new Font("Segoe UI", 11.5f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(shortArea, areaFont, whiteBrush, new RectangleF(6, 6, W - 12, 26), sf);
                }

                RectangleF titleRect = new RectangleF(6, 33, W - 12, 66);
                float fontSize = 20.5f;
                if (name.Length > 21) fontSize = 15.5f;
                else if (name.Length > 14) fontSize = 18.0f;

                using (Font nameFont = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(190, 0, 0, 0)))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(250, 252, 255)))
                {
                    StringFormat sfCenter = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    RectangleF shadowRect = new RectangleF(titleRect.X + 1, titleRect.Y + 2, titleRect.Width, titleRect.Height);
                    g.DrawString(name, nameFont, shadowBrush, shadowRect, sfCenter);
                    g.DrawString(name, nameFont, textBrush, titleRect, sfCenter);
                }

                bool hasStatus = !string.IsNullOrEmpty(status);
                Color pillBg = hasStatus
                    ? Color.FromArgb(230, 22, 163, 74)
                    : Color.FromArgb(235, 22, 34, 48);
                Color pillBorder = hasStatus
                    ? Color.FromArgb(255, 74, 222, 128)
                    : Color.FromArgb(215, 0, 215, 255);
                Color pillText = hasStatus
                    ? Color.White
                    : Color.FromArgb(0, 238, 255);

                using (SolidBrush pBg = new SolidBrush(pillBg))
                using (Pen pPen = new Pen(pillBorder, 2.0f))
                {
                    FillRoundedRect(g, pBg, 8, 101, W - 16, 35, 8);
                    DrawRoundedRect(g, pPen, 8, 101, W - 16, 35, 8);
                }

                string bottomLabel = hasStatus ? status : coords;
                float bottomFontSize = (bottomLabel.Length > 11) ? 17.5f : 20.0f;
                using (Font coordFont = new Font("Consolas", bottomFontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush coordBrush = new SolidBrush(pillText))
                {
                    StringFormat sfBottom = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(bottomLabel, coordFont, coordBrush, new RectangleF(8, 102, W - 16, 34), sfBottom);
                }

                using (Pen borderPen = new Pen(Color.FromArgb(95, accent.R, accent.G, accent.B), 2f))
                {
                    DrawRoundedRect(g, borderPen, 2, 2, W - 4, H - 4, 12);
                }

                return BitmapToBase64(bmp);
            }
        }

        private static void GenerateAllZaapIcons(string jsonFile, string outDir)
        {
            Directory.CreateDirectory(outDir);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string json = File.ReadAllText(jsonFile, Encoding.UTF8);
            var list = ser.Deserialize<List<Dictionary<string, object>>>(json);
            foreach (var z in list)
            {
                string id = GetStr(z, "id", "");
                string name = GetStr(z, "name", "");
                string area = GetStr(z, "area", "");
                string coords = GetStr(z, "coords", "");
                string b64 = RenderZaapButton(name, area, coords, "");
                SaveBase64Png(b64, Path.Combine(outDir, id + ".png"));
                SaveBase64Png(b64, Path.Combine(outDir, id + "@2x.png"));
            }
        }

        private static void GenerateStaticAssets(string outDir)
        {
            Directory.CreateDirectory(outDir);

            SaveBase64Png(RenderZaapButton("Cité d'Astrub", "Astrub", "[5, -18]", ""), Path.Combine(outDir, "default-zaap.png"));
            SaveBase64Png(RenderZaapButton("Cité d'Astrub", "Astrub", "[5, -18]", ""), Path.Combine(outDir, "default-zaap@2x.png"));

            GeneratePortalIcon(Path.Combine(outDir, "category.png"), 72, "Z");
            GeneratePortalIcon(Path.Combine(outDir, "category@2x.png"), 144, "Z");
            GeneratePortalIcon(Path.Combine(outDir, "action-zaap.png"), 72, "Z");
            GeneratePortalIcon(Path.Combine(outDir, "action-zaap@2x.png"), 144, "Z");
        }

        private static void GeneratePortalIcon(string path, int size, string symbol)
        {
            using (Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                using (LinearGradientBrush bg = new LinearGradientBrush(
                    new Rectangle(0, 0, size, size),
                    Color.FromArgb(18, 28, 42),
                    Color.FromArgb(10, 15, 24),
                    LinearGradientMode.ForwardDiagonal))
                {
                    FillRoundedRect(g, bg, 2, 2, size - 4, size - 4, size / 6);
                }

                using (Pen outer = new Pen(Color.FromArgb(220, 0, 220, 255), size * 0.07f))
                using (Pen inner = new Pen(Color.FromArgb(160, 56, 189, 248), size * 0.035f))
                {
                    int pad = (int)(size * 0.14);
                    g.DrawEllipse(outer, pad, pad, size - pad * 2, size - pad * 2);
                    int pad2 = (int)(size * 0.23);
                    g.DrawEllipse(inner, pad2, pad2, size - pad2 * 2, size - pad2 * 2);
                }

                using (Font f = new Font("Segoe UI", size * 0.34f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(240, 253, 255)))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(symbol, f, b, new RectangleF(0, 0, size, size), sf);
                }

                bmp.Save(path, ImageFormat.Png);
            }
        }

        private static void SaveBase64Png(string b64, string filePath)
        {
            byte[] bytes = Convert.FromBase64String(b64);
            File.WriteAllBytes(filePath, bytes);
        }

        private static string BitmapToBase64(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private static void FillRoundedRect(Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            using (GraphicsPath path = GetRoundedRectPath(x, y, width, height, radius))
            {
                g.FillPath(brush, path);
            }
        }

        private static void DrawRoundedRect(Graphics g, Pen pen, float x, float y, float width, float height, float radius)
        {
            using (GraphicsPath path = GetRoundedRectPath(x, y, width, height, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath GetRoundedRectPath(float x, float y, float width, float height, float radius)
        {
            float d = radius * 2f;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + width - d, y, d, d, 270, 90);
            path.AddArc(x + width - d, y + height - d, d, d, 0, 90);
            path.AddArc(x, y + height - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
