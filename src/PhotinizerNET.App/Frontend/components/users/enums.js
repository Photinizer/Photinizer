const GameStates = {
    xTurn: 0,
    oTurn: 1,
    xWin: 2,
    oWin: 3,
    tie: 4,
    isTurn: state => state < 2
}

const CellStates = {
    empty: 0,
    x: 1,
    o: 2
}

function getEnumString(enumObj, val) {
    for (const key in enumObj)
        if (enumObj[key] === val)
            return key;
}