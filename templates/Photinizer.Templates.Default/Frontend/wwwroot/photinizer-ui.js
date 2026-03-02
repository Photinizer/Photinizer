/* 
PHOTINIZER REACTIVE FRAMEWORK 

It is recommended for frontend to use VS Code with 'es6-string-html' plugin.

EXAMPLE:
<button onclick="${x.self()}.counter++">+</button>
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