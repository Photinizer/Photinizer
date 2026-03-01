/* 
PHOTINIZER MESSAGE BRIDGE aka API

EXAMPLE:
// fire and forget
api.message('endpoint', { arg1: arg1Val });

// wait for completion
await api.task('endpoint', { arg1: arg1Val });

// wait for result
var result = await api.query('endpoint', { arg1: arg1Val });
*/
class PhotinizerMessageBridge {
    constructor() {
        this.pendingRequests = new Map();
        this.handlers = new Map();

        window.external.receiveMessage(rawMsg => {
            try {
                const packet = JSON.parse(rawMsg);
                const { requestId, endpoint, data, error } = packet;

                if (requestId && this.pendingRequests.has(requestId)) {
                    const { resolve, reject } = this.pendingRequests.get(requestId);
                    error ? reject(error) : resolve(data);
                    this.pendingRequests.delete(requestId);
                    return;
                }

                if (endpoint && this.handlers.has(endpoint)) {
                    this._handleInbound(packet);
                }
            } catch (e) {
                console.error("Photinizer RPC Error:", e);
            }
        });
    }

    message(endpoint, data = {}) { this._send(endpoint, data); }
    async task(endpoint, data = {}) { return this._send(endpoint, data, true); }
    async query(endpoint, data = {}) { return this._send(endpoint, data, true); }

    onMessage(endpoint, cb) { this.handlers.set(endpoint, { cb, reply: false }); }
    onTask(endpoint, cb)    { this.handlers.set(endpoint, { cb, reply: true }); }
    onQuery(endpoint, cb)   { this.handlers.set(endpoint, { cb, reply: true }); }

    _send(endpoint, data, expectResponse = false) {
        const requestId = expectResponse ? crypto.randomUUID() : null;
        const payload = { endpoint, data, requestId };

        if (!expectResponse) {
            window.external.sendMessage(JSON.stringify(payload));
            return;
        }

        return new Promise((resolve, reject) => {
            this.pendingRequests.set(requestId, { resolve, reject });
            window.external.sendMessage(JSON.stringify(payload));
        });
    }

    async _handleInbound({ endpoint, data, requestId }) {
        const handler = this.handlers.get(endpoint);
        
        try {
            const result = await handler.cb(data);
            
            if (handler.reply && requestId) {
                window.external.sendMessage(JSON.stringify({
                    requestId,
                    endpoint,
                    data: result
                }));
            }
        } catch (err) {
            if (requestId) {
                window.external.sendMessage(JSON.stringify({
                    requestId,
                    endpoint,
                    error: err.message
                }));
            }
        }
    }
}

const api = new PhotinizerMessageBridge();