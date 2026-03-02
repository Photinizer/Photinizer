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
class PhotinizerMessenger {
    constructor() {
        this.pendingRequests = new Map();
        this.handlers = new Map();
        this.enableLogging = false;

        window.external.receiveMessage(rawMsg => {
            try {
                const packet = JSON.parse(rawMsg);
                if (this.enableLogging) console.log("Photinizer RPC:", packet);
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

    onMessage(endpoint, callback) { this.handlers.set(endpoint, { callback, reply: false }); }
    onTask(endpoint, callback)    { this.handlers.set(endpoint, { callback, reply: true }); }
    onQuery(endpoint, callback)   { this.handlers.set(endpoint, { callback, reply: true }); }

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
            data = await handler.callback(data);
            const payload = { endpoint, data, requestId };

            if (handler.reply && requestId) {
                window.external.sendMessage(JSON.stringify(payload));
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

class CrudController {
    constructor(entityName){
        this.entityName = entityName;
    }

    async create(entity) {
        return await api.query(`${this.entityName}.create`, entity);
    }

    async read(id) {
        return await api.query(`${this.entityName}.read`, id);
    }

    async readAll() {
        return await api.query(`${this.entityName}.readAll`);
    }

    async update(entity) {
        return await api.task(`${this.entityName}.update`, entity);
    }

    async delete(id) {
        return await api.task(`${this.entityName}.delete`, id);
    }
}

const api = new PhotinizerMessenger();