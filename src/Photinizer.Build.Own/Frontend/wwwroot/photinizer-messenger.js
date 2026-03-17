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

//#region  Models
const Ph_MessageTypes = Object.freeze({
    MESSAGE: 0,
    TASK: 1,
    QUERY: 2
});

class Ph_Request {
    constructor(type, endpoint, parameters){
        this.id = crypto.randomUUID();
        this.type = type; // Ph_MessageTypes
        this.endpoint = endpoint;
        this.parameters = parameters;
    }
}

class Ph_Response {
    constructor(requestId, result, error){
        this.id = crypto.randomUUID();
        this.requestId = requestId;
        this.result = result;
        this.error = error;
    }
}

class Ph_ErrorNotification {
    constructor(error){
        this.error = error;
    }
}

//  Ph_MessageDto { request, response, error } 
//#endregion

class PhotinizerMessenger {
    constructor() {
        this.pendingRequests = new Map();
        this.handlers = new Map();
        this.enableLogging = false;

        window.external.receiveMessage(rawMsg => {
            try {
                const packet = JSON.parse(rawMsg);
                if (this.enableLogging) console.log("Photinizer Messenger:", packet);
                const { request, response, error } = packet;

                if (request) this._handleRequest(request);
                else if (response) this._handleResponse(response);
                else if (error) this._handleBackendError(error);
            } catch (e) {
                this._handleFrontendError(e);
            }
        });
    }

    message(endpoint, parameters = {}) { this._sendRequest(new Ph_Request(Ph_MessageTypes.MESSAGE, endpoint, parameters)); }
    async task(endpoint, parameters = {}) { return this._sendRequest(new Ph_Request(Ph_MessageTypes.TASK, endpoint, parameters)); }
    async query(endpoint, parameters = {}) { return this._sendRequest(new Ph_Request(Ph_MessageTypes.QUERY, endpoint, parameters)); }

    onMessage(endpoint, callback) { this.handlers.set(endpoint, { type: Ph_MessageTypes.MESSAGE, callback }); }
    onTask(endpoint, callback)    { this.handlers.set(endpoint, { type: Ph_MessageTypes.TASK, callback }); }
    onQuery(endpoint, callback)   { this.handlers.set(endpoint, { type: Ph_MessageTypes.QUERY, callback }); }

    //#region Private functions: hanling incoming messages
    async _handleRequest(request) {
        const handler = this.handlers.get(request.endpoint);
        if (!handler){
            this._handleFrontendError({ message: `Endpoint '${request.endpoint}' is not registered on the frontend.`});
            return;
        }
        let result;
        let error;
        try {
            result = await handler.callback(request.parameters);
        } catch (err) {
            error = err;
        }
        if (request.type !== Ph_MessageTypes.MESSAGE) {
            const response = error
                ? new Ph_Response(request.id, undefined, error) 
                : (request.type === Ph_MessageTypes.TASK
                    ? new Ph_Response(request.id, "OK")
                    : new Ph_Response(request.id, result));
            this._sendResponseInternal(response);
        }
    }
    _handleResponse(response) {
        const request = this.pendingRequests.get(response.requestId);
        if (!request) {
            this._handleFrontendError({ message: `No pending request found for ID '${response.requestId}'.`});
            return;
        }
        this.pendingRequests.delete(response.requestId);
        response.error ? request.reject(response.error) : request.resolve(response.result);
    }
    _handleBackendError(error) {
        console.error("Backend error:", error);
    }
    _handleFrontendError(error) {
        console.error("Frontend error:", error);
        this._sendError(e);
    }
    //#endregion

    //#region Private functions: send messages to backend
    _sendRequest(request) {
        if (request.type == Ph_MessageTypes.MESSAGE) {
            this._sendRequestInternal(request);
            return;
        }

        return new Promise((resolve, reject) => {
            this.pendingRequests.set(request.id, { resolve, reject });
            this._sendRequestInternal(request);
        });
    }

    _sendError(error) {
        _sendErrorInternal(new Ph_ErrorNotification(error));
    }

    _sendRequestInternal(request) { this._sendPackage({ request }); }
    _sendResponseInternal(response) { this._sendPackage({ response }); }
    _sendErrorInternal(error) { this._sendPackage({ error }); }

    _sendPackage(package) { window.external.sendMessage(JSON.stringify(package)); }
    //#endregion
}

// TODO: to move
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