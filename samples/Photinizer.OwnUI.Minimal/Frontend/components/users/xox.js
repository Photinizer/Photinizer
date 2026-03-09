// using enums
class XOX extends Component {
    constructor() {
        super({
            gameState: 0, private: { cells: [], remainCells: 9 },
            stylize: x =>
            /*css*/`
                .xox-game { width: min(80vh, 80vw); }
                .xox-header { display: flex; justify-content: space-between; align-items: center; }
                .xox-label { font-size: 2rem; }
                .xox-field { display: grid; grid-template-rows: repeat(3, 1fr); grid-template-columns: repeat(3, 1fr); gap: .5rem; height: min(80vh, 80vw); }
                .xox-cell { display: flex; justify-content: center; align-items: center; border: 2px solid black; font-size: 5rem; background: #333; }
                .xox-cell-x:after { content: "x"; color: red; }
                .xox-cell-o:after { content: "o"; color: blue; }`,
            render: x =>
            /*html*/`
            <div class="xox-game">
                <div class="xox-header">
                    <p class="xox-label" style="color: ${x.labelColors[x.gameState]}">${getEnumString(GameStates, x.gameState)}</p>
                    <button onclick="${x.self()}.restart()">Restart</button>
                </div>
                <div class="xox-field">
                    ${x.cells.map(x => x.render()).join('')}
                </div>
            </div>`
        })
        this.labelColors = ['darkred', 'darkblue', 'red', 'blue', 'unset'];
        for (let i = 0; i < 9; i++)
            this.cells.push(new XoxCell(this.id, i))
    }
    getStateStr() {
        return;
    }
    cellClicked(i) {
        if (GameStates.isTurn(this.gameState)) {
            const cell = this.cells[i];
            if (cell.cellState == CellStates.empty) {
                cell.cellState = this.gameState == GameStates.xTurn ? CellStates.x : CellStates.o

                if (this.checkWin(i, cell.cellState))
                    this.gameState = this.gameState == GameStates.xTurn ? GameStates.xWin : GameStates.oWin;
                else if (--this.remainCells == 0)
                    this.gameState = GameStates.tie;
                else
                    this.gameState = this.gameState == GameStates.xTurn ? GameStates.oTurn : GameStates.xTurn
            }
        }
    }
    checkWin(i, newState) {
        return this.checkRow(i, newState) || this.checkCol(i, newState) || this.checkDiag(i, newState);
    }
    checkRow(i, newState) {
        for (let j = 0; j < 3; j++)
            if (this.cells[Math.floor(i / 3) * 3 + j].cellState != newState)
                return false;
        return true;
    }
    checkCol(i, newState) {
        for (let j = 0; j < 3; j++)
            if (this.cells[i % 3 + j * 3].cellState != this.cells[i].cellState)
                return false;
        return true;
    }
    checkDiag(i, newState) {
        if (this.cells[4].cellState == newState) {
            let success = true;
            for (let j = 0; j < 3; j++)
                if (this.cells[j * 4].cellState != newState) {
                    success = false;
                    break;
                }
            if (success)
                return true;
            for (let j = 0; j < 3; j++)
                if (this.cells[j * 2 + 2].cellState != newState)
                    return false;
            return true;
        }
    }
    restart() {
        for (let i = 0; i < 9; i++)
            this.cells[i].cellState = CellStates.empty;
        this.gameState = GameStates.xTurn;
        this.remainCells = 9;
    }
}