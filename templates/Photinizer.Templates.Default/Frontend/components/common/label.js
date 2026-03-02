class Label extends Component {
    constructor() {
        super({
            text: '',
            render: x => /*html*/`<p>${x.text}</p>`,
        })
    }
}