const net = require("net");
const crypto = require("crypto");
const { EventEmitter } = require("events");

class SimpleWebSocket extends EventEmitter {
  constructor(url) {
    super();
    const parsed = new URL(url);
    this.host = parsed.hostname || "127.0.0.1";
    this.port = parseInt(parsed.port, 10);
    this.path = (parsed.pathname || "/") + (parsed.search || "");
    this.readyState = 0; // CONNECTING
    this.buffer = Buffer.alloc(0);
    this.fragments = [];
    this._connect();
  }

  _connect() {
    const key = crypto.randomBytes(16).toString("base64");
    this.socket = net.connect(this.port, this.host, () => {
      const req = [
        `GET ${this.path} HTTP/1.1`,
        `Host: ${this.host}:${this.port}`,
        `Upgrade: websocket`,
        `Connection: Upgrade`,
        `Sec-WebSocket-Key: ${key}`,
        `Sec-WebSocket-Version: 13`,
        `\r\n`
      ].join("\r\n");
      this.socket.write(req);
    });

    let handshakeDone = false;

    this.socket.on("data", (chunk) => {
      this.buffer = Buffer.concat([this.buffer, chunk]);

      if (!handshakeDone) {
        const headerEnd = this.buffer.indexOf("\r\n\r\n");
        if (headerEnd !== -1) {
          handshakeDone = true;
          this.buffer = this.buffer.slice(headerEnd + 4);
          this.readyState = 1; // OPEN
          this.emit("open");
          this._processFrames();
        }
      } else {
        this._processFrames();
      }
    });

    this.socket.on("close", () => {
      this.readyState = 3;
      this.emit("close");
    });

    this.socket.on("error", (err) => {
      this.emit("error", err);
    });
  }

  _processFrames() {
    while (this.buffer.length >= 2) {
      const firstByte = this.buffer[0];
      const secondByte = this.buffer[1];
      const fin = (firstByte & 0x80) !== 0;
      const opcode = firstByte & 0x0f;
      const isMasked = (secondByte & 0x80) !== 0;
      let payloadLen = secondByte & 0x7f;
      let offset = 2;

      if (payloadLen === 126) {
        if (this.buffer.length < 4) return;
        payloadLen = this.buffer.readUInt16BE(2);
        offset = 4;
      } else if (payloadLen === 127) {
        if (this.buffer.length < 10) return;
        payloadLen = Number(this.buffer.readBigUInt64BE(2));
        offset = 10;
      }

      const maskLen = isMasked ? 4 : 0;
      if (this.buffer.length < offset + maskLen + payloadLen) return;

      let maskKey = null;
      if (isMasked) {
        maskKey = this.buffer.slice(offset, offset + 4);
        offset += 4;
      }

      const payload = Buffer.from(this.buffer.slice(offset, offset + payloadLen));
      this.buffer = this.buffer.slice(offset + payloadLen);

      if (isMasked && maskKey) {
        for (let i = 0; i < payload.length; i++) {
          payload[i] ^= maskKey[i % 4];
        }
      }

      if (opcode === 0x08) {
        // Close frame
        this.close();
        return;
      } else if (opcode === 0x09) {
        // Ping -> send Pong
        this._sendFrame(0x0a, payload);
      } else if (opcode === 0x01 || opcode === 0x02 || opcode === 0x00) {
        this.fragments.push(payload);
        if (fin) {
          const full = Buffer.concat(this.fragments);
          this.fragments = [];
          this.emit("message", full.toString("utf8"));
        }
      }
    }
  }

  send(data) {
    if (this.readyState !== 1) return;
    const payload = Buffer.from(typeof data === "string" ? data : JSON.stringify(data), "utf8");
    this._sendFrame(0x01, payload);
  }

  _sendFrame(opcode, payload) {
    if (!this.socket || this.socket.destroyed) return;
    const mask = crypto.randomBytes(4);
    const len = payload.length;
    let header;

    if (len < 126) {
      header = Buffer.alloc(2);
      header[0] = 0x80 | opcode;
      header[1] = 0x80 | len;
    } else if (len < 65536) {
      header = Buffer.alloc(4);
      header[0] = 0x80 | opcode;
      header[1] = 0x80 | 126;
      header.writeUInt16BE(len, 2);
    } else {
      header = Buffer.alloc(10);
      header[0] = 0x80 | opcode;
      header[1] = 0x80 | 127;
      header.writeBigUInt64BE(BigInt(len), 2);
    }

    const maskedPayload = Buffer.alloc(len);
    for (let i = 0; i < len; i++) {
      maskedPayload[i] = payload[i] ^ mask[i % 4];
    }

    this.socket.write(Buffer.concat([header, mask, maskedPayload]));
  }

  close() {
    this.readyState = 3;
    if (this.socket && !this.socket.destroyed) {
      this.socket.end();
    }
  }
}

module.exports = SimpleWebSocket;

