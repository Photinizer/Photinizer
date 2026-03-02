// using enums
class XoxCell extends Component {
    constructor(ownerId, i) {
        super({ cellState: 0, private: { ownerId: ownerId, i: i }, render: x =>
            /*html*/`
            <div class="xox-cell xox-cell-${getEnumString(CellStates, this.cellState)}"
                 onclick="${x.find(x.ownerId)}.cellClicked(${x.i})">
            </div>`
        })
    }
}