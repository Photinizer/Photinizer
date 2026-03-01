//using label
//using section
class Greetings extends Component {
    constructor() {
        super({
            name: 'dear friend',
            time: '??:??:??',
            stylize: x => /*css*/`
                .greetings { position: relative; }
                .greetings fieldset { margin-top: 2rem; padding-top: 1rem; }
                .greetings .timer { position: fixed; top: 1rem; right: 1rem; }
            `,
            render: x =>
            /*html*/`
            <div class="greetings">
                <h1>Greetings from Photinizer</h1>
                ${x.timerSection.render()}
                <button onclick="${x.self()}.hello()">Say hello to backend!</button>
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
        this.label = new Label().bindProp('text', this, 'name', x => `Hello, ${x.name}!`)
        this.section = new Section(this, ['name'], x => /*html*/`<p>Nice to meet you, ${x.name}!</p>`)
        this.timerSection = new Section(this, ['time'], x => /*html*/`<p class="timer">Time is ${x.time}!</p>`)
 
        api.query('get username', {}).then(data => {
            this.name = data
            this.update()
        })
        api.onTask('update timer', data => this.time = data);
    }
    async hello() {
        alert(await api.query('Hello, backend!', {}))
    }
    async remember() {
        await api.task('save username', { username: this.name })
        alert('Username saved')
    }
    async forget() {
        await api.task('delete username', { username: this.name })
        this.name = 'dear friend'
        this.update()
    }
}