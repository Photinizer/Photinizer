//using label
//using section
class Greetings extends Component {
    constructor() {
        super({
            name: 'dear friend',
            stylize: x => /*css*/`
                .greetings fieldset { margin-top: 2rem; padding-top: 1rem; }
            `,
            render: x =>
            /*html*/`
            <div class="greetings">
                <h1>Greetings from Photinizer</h1>
                <button onclick="api.call('Hello, backend!', {}, (data, error) => { if (!error) alert(data); })">Say hello to backend!</button>
                <div>
                    <fieldset>
                        <label>Name:</label>
                        <input oninput="${x.self()}.name = this.value" value="${x.name}"/>
                        ${x.label.render()}
                        ${x.section.render()}
                        <button onclick="${x.self()}.remember()">Remember my name!</button>
                        <button onclick="${x.self()}.forget()">Forget my name!</button>
                    </fieldset>
                </div>
            </div>`
        })
        this.label = new Label().bindProp('text', this, 'name', x => `Hello, ${x.name}!`);
        this.section = new Section(this, ['name'], x => /*html*/`<p>Nice to meet you, ${x.name}!</p>`);
        api.call('get username', {}, (data, error) => { if (!error) this.name = data; this.update() });
    }
    remember() {
        api.call('save username', { username: this.name });
    }
    forget() {
        api.call('delete username', { username: this.name });
        this.name = 'dear friend';
        this.update();
    }
}