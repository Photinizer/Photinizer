/* ONCLICK EXAMPLE
<button onclick="${x.self()}.counter++">+</button>
*/
/* BRIDGE EXAMPLE
<button onclick="api.call('hello backend!', {}, (data, error) => { if (!error) alert(data); })">Say hello to backend!</button>
*/
/*
It is recommended for frontend to use VS Code with 'es6-string-html' plugin.
 */

/* 
PHOTINIZER REACTIVE FRAMEWORK 
*/
let lastId = 0;
const components = new Map();

class UI {
    constructor(root, appId = 'app') {
        const app = document.getElementById(appId)
        const styles = `
        <style>
            ${[...components.values()].filter(x => x.stylize).map(x => x.stylize(x)).join('\n')}
        </style>`
        app.innerHTML = styles + root.render()
    }
}

class Component {
    constructor(proto) {
        this.id = lastId++;
        for (const key in proto) {
            if (key === 'private') {
                for (const privateKey in proto[key])
                    this[privateKey] = proto[key][privateKey];
                continue;
            }
            if (key === 'stylize') {
                this[key] = proto[key]
                continue; 
            }
            const fieldName = `_${key}`;
            this[fieldName] = proto[key];
            if (key === 'render') {
                this[key] = function () {
                    return this[fieldName](this).replace(/(<[a-zA-Z0-9\-]+)/, `$1 data-id="${this.id}"`);
                }
                continue;
            }
            
            Object.defineProperty(this, key, {
                set: function (value) {
                    this[fieldName] = value;
                    const thiss = this;
                    if (this.bindings.has(key))
                        this.bindings.get(key).forEach(onChange => { onChange(thiss); });
                    else
                        this.update();
                },
                get: function () {
                    return this[fieldName];
                }
            });
        }
        components.set(this.id, this);
        this.bindings = new Map(); // property -> callbacks
    }
    update() {
        const el = document.querySelector(`[data-id="${this.id}"]`);
        if (el) el.outerHTML = this.render();
    }
    self() { return `components.get(${this.id})`; }
    find(id) { return `components.get(${id})` };
    onChange(property, callback) {
        if (!this.bindings.has(property))
            this.bindings.set(property, [callback]);
        else
            this.bindings.get(property).push(callback);
        return this;
    }
    bindProp(property, source, sourceProperty, transform) {
        if (!transform)
            transform = source => source[sourceProperty];
        this[property] = transform(source);
        source.onChange(sourceProperty, x => this[property] = transform(x));
        return this;
    }
}

/* 
PHOTINIZER MESSAGE BRIDGE aka API
*/
class PhotinizerMessageBridge {
    constructor() {
        this.pendingRequests = new Map(); // requestId -> callback

        window.external.receiveMessage(rawMsg => {
            const { requestId, data, error } = JSON.parse(rawMsg);

            if (this.pendingRequests.has(requestId)) {
                const callback = this.pendingRequests.get(requestId);
                callback(data, error);
                this.pendingRequests.delete(requestId);
            }
        });
    }

    call(command, args = {}, callback) {
        const requestId = Math.random().toString(36).substring(2, 9);

        if (callback) {
            this.pendingRequests.set(requestId, callback);
        }

        window.external.sendMessage(JSON.stringify({
            requestId,
            command,
            args
        }));
    }
}

const api = new PhotinizerMessageBridge();