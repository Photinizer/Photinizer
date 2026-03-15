class MainView extends Component {
    constructor() {
        super({
            render: x =>
                /*html*/`
                <img class="logo center" src="assets/photino-logo-full.png">
                <button class="primary random center" onclick="${x.self()}.random()">Open Random Window</button>
                <button class="close center" onclick="${x.self()}.close()">Close Window</button>
                `
        });
    }

    close() {
        api.message("close-window", {});
    }

    random() {
        api.message("random-window", {});
    }

}