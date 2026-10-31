const path = require("path");
const { spawn } = require("child_process");
const readline = require("readline");
const SimpleWebSocket = require("./ws-client");
const { ZAAPS, getZaapById } = require("./zaaps");

function parseArgs(argv) {
  const res = {};
  for (let i = 2; i < argv.length; i++) {
    const arg = argv[i];
    if (arg.startsWith("-")) {
      const key = arg.replace(/^-+/, "");
      res[key] = argv[i + 1];
      i++;
    }
  }
  return res;
}

const args = parseArgs(process.argv);
const PORT = args.port || process.argv[3];
const PLUGIN_UUID = args.pluginUUID || process.argv[5];
const REGISTER_EVENT = args.registerEvent || process.argv[7] || "registerPlugin";

class HelperBridge {
  constructor() {
    this.reqId = 0;
    this.pending = new Map();
    this.imageCache = new Map();
    this._startProcess();
  }

  _startProcess() {
    const exePath = path.join(__dirname, "..", "bin", "DofusHelper.exe");
    this.proc = spawn(exePath, [], {
      stdio: ["pipe", "pipe", "ignore"],
      windowsHide: true
    });

    const rl = readline.createInterface({ input: this.proc.stdout });
    rl.on("line", (line) => {
      try {
        const msg = JSON.parse(line);
        if (msg.id && this.pending.has(msg.id)) {
          const resolve = this.pending.get(msg.id);
          this.pending.delete(msg.id);
          resolve(msg);
        }
      } catch (_) {}
    });

    this.proc.on("exit", () => {
      setTimeout(() => this._startProcess(), 1000);
    });
  }

  call(payload) {
    return new Promise((resolve) => {
      const id = String(++this.reqId);
      const req = Object.assign({}, payload, { id });
      this.pending.set(id, resolve);
      try {
        this.proc.stdin.write(JSON.stringify(req) + "\n", "utf8");
      } catch (_) {
        this.pending.delete(id);
        resolve({ ok: false });
      }
    });
  }

  async renderZaap({ name, area, coords, status = "" }) {
    const cacheKey = `zaap|${name}|${area}|${coords}|${status}`;
    if (!status && this.imageCache.has(cacheKey)) {
      return this.imageCache.get(cacheKey);
    }
    const res = await this.call({
      cmd: "renderZaap",
      name,
      area,
      coords,
      status
    });
    if (res && res.image) {
      if (!status) {
        this.imageCache.set(cacheKey, res.image);
      }
      return res.image;
    }
    return null;
  }

  async executeCommand({ text, mode = "autotype_space", focusDofus = true, delayMs = 60 }) {
    return this.call({
      cmd: "execute",
      text,
      mode,
      focusDofus: Boolean(focusDofus),
      delayMs: Number(delayMs) || 60
    });
  }
}

const helper = new HelperBridge();
const contexts = new Map();
let currentInspectorContext = null;

function getDefaultZaapIdFromAction(action = "") {
  const m = action.match(/\.(z\d+)$/);
  return m ? m[1] : "z01";
}

function resolveZaapInfo(settings = {}, action = "") {
  const defaultId = getDefaultZaapIdFromAction(action);
  const zaap = getZaapById(settings.zaapId || defaultId);
  return {
    zaap,
    execMode: settings.execMode || "autotype_space",
    focusDofus: settings.focusDofus !== false
  };
}

if (!PORT) {
  process.exit(0);
}

const ws = new SimpleWebSocket(`ws://127.0.0.1:${PORT}`);

function send(payload) {
  ws.send(payload);
}

function setImage(context, image) {
  if (!image) return;
  send({
    event: "setImage",
    context,
    payload: { target: 0, image }
  });
}

function setTitle(context, title) {
  send({
    event: "setTitle",
    context,
    payload: { target: 0, title: title || "" }
  });
}

function setSettings(context, settings) {
  send({
    event: "setSettings",
    context,
    payload: settings
  });
}

function sendToPropertyInspector(context, action, payload) {
  send({
    event: "sendToPropertyInspector",
    context,
    action,
    payload
  });
}

async function updateZaapButton(context, statusText = "") {
  const entry = contexts.get(context);
  if (!entry) return;

  const { zaap } = resolveZaapInfo(entry.settings, entry.action);

  const img = await helper.renderZaap({
    name: zaap.name,
    area: zaap.area,
    coords: zaap.coords,
    status: statusText
  });

  setImage(context, img);
  setTitle(context, "");
}

async function handleKeyDown(context) {
  const entry = contexts.get(context);
  if (!entry) return;

  const { zaap, execMode, focusDofus } = resolveZaapInfo(entry.settings, entry.action);
  const statusBadge = execMode === "clipboard" ? "COPIÉ !" : "ZAAP !";

  await helper.executeCommand({
    text: zaap.command,
    mode: execMode,
    focusDofus
  });

  await updateZaapButton(context, statusBadge);
  setTimeout(() => {
    updateZaapButton(context, "");
  }, 1100);
}

ws.on("open", () => {
  send({
    event: REGISTER_EVENT,
    uuid: PLUGIN_UUID
  });
});

ws.on("message", async (raw) => {
  let msg;
  try {
    msg = JSON.parse(raw);
  } catch (_) {
    return;
  }

  const { event, action, context, device, payload } = msg;

  if (event === "willAppear") {
    const settings = Object.assign({}, (payload && payload.settings) || {});
    let needsSave = false;
    const defaultZaapId = getDefaultZaapIdFromAction(action);

    if (!settings.zaapId) {
      settings.zaapId = defaultZaapId;
      needsSave = true;
    }
    if (!settings.execMode) {
      settings.execMode = "autotype_space";
      needsSave = true;
    }
    if (typeof settings.focusDofus !== "boolean") {
      settings.focusDofus = true;
      needsSave = true;
    }

    contexts.set(context, {
      action,
      device,
      settings
    });

    if (needsSave) {
      setSettings(context, settings);
    }

    await updateZaapButton(context);
  } else if (event === "willDisappear") {
    contexts.delete(context);
  } else if (event === "didReceiveSettings") {
    const entry = contexts.get(context);
    if (entry) {
      entry.settings = Object.assign({}, (payload && payload.settings) || {});
      if (!entry.settings.zaapId) {
        entry.settings.zaapId = getDefaultZaapIdFromAction(entry.action);
      }
      await updateZaapButton(context);
    }
  } else if (event === "keyDown") {
    await handleKeyDown(context);
  } else if (event === "propertyInspectorDidAppear") {
    currentInspectorContext = context;
    const entry = contexts.get(context);
    sendToPropertyInspector(context, action, {
      event: "initData",
      zaaps: ZAAPS,
      settings: entry ? entry.settings : {}
    });
  } else if (event === "sendToPlugin") {
    const targetCtx = context || currentInspectorContext;
    const entry = contexts.get(targetCtx);
    if (payload && payload.type === "updateSettings" && entry) {
      entry.settings = Object.assign({}, entry.settings, payload.settings || {});
      setSettings(targetCtx, entry.settings);
      await updateZaapButton(targetCtx);
    } else if (payload && payload.type === "testCommand") {
      const info = resolveZaapInfo(payload.settings || (entry && entry.settings) || {}, entry ? entry.action : action);
      await helper.executeCommand({
        text: info.zaap.command,
        mode: info.execMode,
        focusDofus: info.focusDofus
      });
    }
  }
});

ws.on("close", () => {
  process.exit(0);
});
