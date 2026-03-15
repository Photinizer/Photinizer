class SecondaryView extends Component {
    constructor() {
        super({
            render: x =>
                /*html*/`
                <img class="logo center" src="assets/photino-logo-full.png">
                <button class="close center" onclick="${x.self()}.close()">Close Window</button>
                `
        });
    }

    close() {
        api.message("close-window", {});
    }

}