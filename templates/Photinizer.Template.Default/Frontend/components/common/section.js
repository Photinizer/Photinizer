class Section extends Component {
    constructor(owner, updatedByProperties, render) {
        super({
            private: { owner },
            render: x => render(x.owner),
        })
        const thiss = this;
        updatedByProperties.forEach(x => owner.onChange(x, _ => thiss.update()));
    }
}