"use strict";

const defaultBoardRows = 25;
const defaultBoardCols = 25;
const defaultFrameSize = 3;
const defaultGridSize = 1;

const defaultPlayerWidthRatio = 0.7;
const defaultPlayerHeightRatio = 0.7;
const defaultBoardWidth = 300;
const defaultBoardHeight = 300;
const defaultGameSpeedInMs = 500;

const defaultWinnerColor = 'green';
const defaultLoserColor = 'orange';
const defaultDrawColor = 'yellow';

const defaultPlayerColor = 'black';
const defaultEnemyColor = 'red';

const defaultFrameColor = 'red';
const defaultGridColor = 'silver';

const directionEnum = { none: 0, left: 1, up: 2, right: 3, down: 4 };
const gameStateEnum = { none: 0, pending: 1, playing: 2, finished: 3 };
const playerGameResultEnum = { none: 0, winner: 1, loser: 2, draw: 3 };
const collisionDetectionEnum = { none: 0, board: 1, player: 2 };

class Player {
    constructor({ name }) {
        this.name = name;
        this.direction = directionEnum.none;
        this.canChangeDirection = false;
        this.isPlaying = false;
        this.trail = null;
        this.gameResult = playerGameResultEnum.none;
    }

    init(boardPosition, direction) {
        this.gameResult = playerGameResultEnum.none;
        this.direction = directionEnum.none;
        this.canChangeDirection = true;

        this.initTrail(new TrailPart(boardPosition, direction));
        this.setDirection(direction);

        this.isPlaying = true;
    }

    initTrail(trailPart) {
        this.trail = [];
        this.trail.push(trailPart);
    }

    setDirection(newDirection) {
        if (this.canChangeDirection && this.direction !== newDirection) {
            let changeDirection = false;

            switch (newDirection) {
                case directionEnum.left:
                    changeDirection = this.direction !== directionEnum.right;
                    break;
                case directionEnum.up:
                    changeDirection = this.direction !== directionEnum.down;
                    break;
                case directionEnum.right:
                    changeDirection = this.direction !== directionEnum.left;
                    break;
                case directionEnum.down:
                    changeDirection = this.direction !== directionEnum.up;
                    break;
            }

            if (changeDirection) {
                this.direction = newDirection;
                this.canChangeDirection = false;

                return true;
            }
        }

        return false;
    }

    move() {
        if (!this.isPlaying) {
            return;
        }

        const lastPosition = this.trail[this.trail.length - 1].position;
        let x = lastPosition.col;
        let y = lastPosition.row;

        switch (this.direction) {
            case directionEnum.left:
                x--;
                break;
            case directionEnum.up:
                y--;
                break;
            case directionEnum.right:
                x++;
                break;
            case directionEnum.down:
                y++;
                break;
        }

        const newPosition = new BoardPosition(x, y);

        this.trail.push(new TrailPart(newPosition, this.direction));
        this.canChangeDirection = true;
    }

    undoMove() {
        this.trail.pop();
    }

    setGameResult(gameResult) {
        this.isPlaying = false;
        this.gameResult = gameResult;
    }
}

class PlayerLayer {
    constructor({ playerModel, boardLayer, color,
        widthRatio = defaultPlayerWidthRatio, heightRatio = defaultPlayerHeightRatio,
        drawColor = defaultDrawColor, loserColor = defaultLoserColor, winnerColor = defaultWinnerColor }) {

        this.playerModel = playerModel;
        this.boardLayer = boardLayer;
        this.widthRatio = widthRatio;
        this.heightRatio = heightRatio;
        this.color = color;

        this.drawColor = drawColor;
        this.loserColor = loserColor;
        this.winnerColor = winnerColor;

        this.setDimensions();
    }

    setDimensions() {
        this.width = this.widthRatio * this.boardLayer.squareWidth;
        this.height = this.heightRatio * this.boardLayer.squareHeight;

        this.headRadius = Math.min(this.width, this.height) / 2;
        this.headXOffset = this.boardLayer.squareWidth / 2;
        this.headYOffset = this.boardLayer.squareHeight / 2;
        this.squareXOffset = (this.boardLayer.squareWidth - this.width) / 2;
        this.squareYOffset = (this.boardLayer.squareHeight - this.height) / 2;
    }

    resize() {
        this.setDimensions();
    }

    draw(ctx) {
        const playerModel = this.playerModel;

        if (playerModel.trail.length === 0) {
            return;
        }

        const boardLayer = this.boardLayer;
        const trail = playerModel.trail;
        const n = trail.length - 1;
        let previous = trail[n];

        for (let i = n; i >= 0; i--) {
            const x = boardLayer.xOffset + trail[i].position.col * boardLayer.squareWidth;
            const y = boardLayer.yOffset + trail[i].position.row * boardLayer.squareHeight;

            if (i === n) {
                this.drawHead(ctx, x, y);
            } else {
                ctx.fillStyle = this.color;

                if (previous.isHorizontalMove() !== trail[i].isHorizontalMove()) {
                    this.drawJoint(ctx, x, y);
                } else {
                    this.drawContinuation(ctx, trail[i], x, y);
                }
            }

            previous = trail[i];
        }
    }

    drawContinuation(ctx, trailPart, x, y) {
        let width;
        let height;
        const boardLayer = this.boardLayer;

        if (!trailPart.isHorizontalMove()) {
            x += this.squareXOffset;
            width = this.width;
        } else {
            width = boardLayer.squareWidth;
        }

        if (!trailPart.isVerticalMove()) {
            y += this.squareYOffset;
            height = this.height;
        } else {
            height = boardLayer.squareHeight;
        }

        ctx.fillRect(x, y, width, height);
    }

    drawJoint(ctx, x, y) {
        const boardLayer = this.boardLayer;

        ctx.fillRect(x, y, boardLayer.squareWidth, boardLayer.squareHeight);
    }

    drawHead(ctx, x, y) {
        ctx.fillStyle = this.getHeadColor();
        ctx.beginPath();
        ctx.arc(x + this.headXOffset, y + this.headYOffset, this.headRadius, 0, Math.PI * 2, true);
        ctx.closePath();
        ctx.fill();
    }

    getHeadColor() {
        let color = this.color;

        switch (this.playerModel.gameResult) {
            case playerGameResultEnum.loser:
                color = this.loserColor;
                break;
            case playerGameResultEnum.draw:
                color = this.drawColor;
                break;
            case playerGameResultEnum.winner:
                color = this.winnerColor;
                break;
            default:
                color = this.color;
                break;
        }

        return color;
    }
}

class Collision {
    constructor(player, detectionType) {
        this.player = player;
        this.detectionType = detectionType;
    }
}

class CollisionDetection {
    constructor(collisionDetectors) {
        this.collisionDetectors = collisionDetectors;        
    }

    detect(activePlayers) {
        let collisions = [];

        for (let i = 0; i < activePlayers.length; i++) {
            const collisionType = this.detectCollisionForPlayer(activePlayers[i]);
            if (collisionType !== collisionDetectionEnum.none) {
                collisions.push(new Collision(activePlayers[i], collisionType));
            }                            
        }

        return collisions;
    }

    detectCollisionForPlayer(player) {
        for (let i = 0; i < this.collisionDetectors.length; i++) {
            const collisionDetector = this.collisionDetectors[i];
            if (collisionDetector.detect(player)) {
                return collisionDetector.collisionType;
            }
        }

        return collisionDetectionEnum.none;
    }
}

class BoardCollisionDetection {
    constructor(board) {
        this.board = board;
        this.collisionType = collisionDetectionEnum.board;
    }

    detect(player) {
        let head = player.trail[player.trail.length - 1].position;
        return !this.board.isPositionInside(head);
    }
}

class PlayerCollisionDetection {
    constructor(players) {
        this.players = players;
        this.collisionType = collisionDetectionEnum.player;
    }

    detect(player) {
        const head = player.trail[player.trail.length - 1].position;

        if (this.detectTrailCollision(head, player.trail, player.trail.length - 2)) {
            return true;
        }

        for (let i = 0; i < this.players.length; i++) {
            if (this.players[i] !== player &&
                this.detectTrailCollision(head, this.players[i].trail, this.players[i].trail.length - 1)) {
                return true;
            }
        }

        return false;
    }

    detectTrailCollision(pos, trail, length) {
        for (let i = 0; i < length; i++) {
            if (trail[i].position.col === pos.col && trail[i].position.row === pos.row) {
                return true;
            }
        }

        return false;
    }
}

class TrailPart {
    constructor(boardPosition, direction) {
        this.position = boardPosition;
        this.direction = direction;
    }

    isHorizontalMove() {
        return this.direction === directionEnum.left ||
            this.direction === directionEnum.right;
    }

    isVerticalMove() {
        return this.direction === directionEnum.up ||
            this.direction === directionEnum.down;
    }
}

class BoardPosition {
    constructor(col, row) {
        this.col = col;
        this.row = row;
    }
}

class Board {
    constructor({ cols = defaultBoardCols, rows = defaultBoardRows }) {
        this.cols = cols;
        this.rows = rows;
    }

    isPositionInside(boardPosition) {
        const isPositionInside = boardPosition.col >= 0 && boardPosition.col <= this.cols - 1 &&
            boardPosition.row >= 0 && boardPosition.row <= this.rows - 1;

        return isPositionInside;
    }
}

class BoardLayer {
    constructor({ boardModel, width, height,
        isFrameVisible = true, isGridVisible = true,
        frameSize = defaultFrameSize, gridSize = defaultGridSize,
        frameColor = defaultFrameColor, gridColor = defaultGridColor }) {

        this.boardModel = boardModel;

        this.width = width;
        this.height = height;

        this.isFrameVisible = isFrameVisible;
        this.isGridVisible = isGridVisible;

        this.frameSize = frameSize;
        this.gridSize = gridSize;

        this.frameColor = frameColor;
        this.gridColor = gridColor;

        this.setDimensions();
    }

    setDimensions() {
        const boardModel = this.boardModel;

        const frameSize = this.isFrameVisible ? this.frameSize : 0;

        this.clientWidth = this.width - 2 * frameSize;
        this.clientHeight = this.height - 2 * frameSize;

        this.xPadding = (this.clientWidth % boardModel.cols) / 2;
        this.yPadding = (this.clientHeight % boardModel.rows) / 2;

        this.squareWidth = Math.floor(this.clientWidth / boardModel.cols);
        this.squareHeight = Math.floor(this.clientHeight / boardModel.rows);

        this.xOffset = this.xPadding + frameSize;
        this.yOffset = this.yPadding + frameSize;
    }

    resize(width, height) {
        this.width = width;
        this.height = height;

        this.setDimensions();
    }

    draw(ctx) {
        const boardModel = this.boardModel;
        const frameSize = this.isFrameVisible ? this.frameSize : 0;

        const height = this.clientHeight - this.yPadding + frameSize;
        const width = this.clientWidth - this.xPadding + frameSize;

        if (this.isGridVisible && this.gridSize > 0) {
            ctx.lineWidth = this.gridSize;
            ctx.strokeStyle = this.gridColor;
            ctx.beginPath();

            for (let i = 0; i <= boardModel.cols; i++) {
                let x = this.xOffset + i * this.squareWidth;
                ctx.moveTo(x, this.yOffset);
                ctx.lineTo(x, height);
            }

            for (let i = 0; i <= boardModel.rows; i++) {
                let y = this.yOffset + i * this.squareHeight;
                ctx.moveTo(this.xOffset, y);
                ctx.lineTo(width, y);
            }

            ctx.closePath();
            ctx.stroke();
        }

        if (frameSize > 0) {
            ctx.lineWidth = this.frameSize;
            ctx.strokeStyle = this.frameColor;
            ctx.beginPath();

            ctx.moveTo(this.xOffset, this.yOffset);
            ctx.lineTo(width, this.yOffset);
            ctx.lineTo(width, height);
            ctx.lineTo(this.xOffset, height);

            ctx.closePath();
            ctx.stroke();
        }
    }
}

class TronModel {
    constructor({ boardModel, playerModels }) {
        this.boardModel = boardModel;
        this.playerModels = playerModels;
    }
}

class TronLayer {
    constructor({ boardLayer, playerLayers }) {
        this.boardLayer = boardLayer;
        this.playerLayers = playerLayers;
    }

    resize(width, height) {
        this.boardLayer.resize(width, height);

        for (let i = 0; i < this.playerLayers.length; i++) {
            this.playerLayers[i].resize();
        }
    }

    draw(ctx) {
        ctx.clearRect(0, 0, this.boardLayer.width, this.boardLayer.height);

        this.boardLayer.draw(ctx);

        for (let i = 0; i < this.playerLayers.length; i++) {
            this.playerLayers[i].draw(ctx);
        }
    }
}

class TronGame {
    constructor(canvas, commClient, playerName = '') {
        this.canvas = canvas;
        this.ctx = this.canvas.getContext('2d');

        this.commClient = commClient;
        this.commClient.onReceiveGameStarted = (model) => this.gameStarted(model);
        this.commClient.onReceiveGameFinished = (model) => this.gameFinished(model);
        this.commClient.onReceivePlayerDirectionChanged = (model) => this.changePlayerDirection(model);
        this.commClient.onConnectionChanged = (isOnline) => this.connectionChanged(isOnline);
        this.commClient.connect();

        const boardModel = new Board({});
        const boardLayer = new BoardLayer({ boardModel: boardModel, width: this.canvas.width, height: this.canvas.height });

        this.model = new TronModel({ boardModel: boardModel, playerModels: [] });
        this.layer = new TronLayer({ boardLayer: boardLayer, playerLayers: [] });

        this.state = gameStateEnum.none;

        if (!playerName) {
            playerName = 'Player.' + Math.floor(Math.random() * (9999 - 1000) + 1000);
        }
        this.playerName = playerName;

        this.engineTimer = null;

        this.collisionDetection = new CollisionDetection([
            new BoardCollisionDetection(this.model.boardModel),
            new PlayerCollisionDetection(this.model.playerModels)]);

        this.onGameStarted = () => { };
        this.onGameFinished = () => { };
        this.onGameConnectionChanged = () => { };

        this.invalidate();
    }

    gameStarted(model) {
        if (this.state !== gameStateEnum.pending) {
            return;
        }

        for (let i = 0; i < model.players.length; i++) {
            let player = model.players[i];

            let playerColor;
            if (player.name === this.playerName) {
                playerColor = defaultPlayerColor;
            } else {
                playerColor = defaultEnemyColor;
            }

            this.addPlayer(player.name, player.position, playerColor);
        }

        this.start();
        this.onGameStarted();
    }

    changePlayerDirection(model) {
        if (this.state !== gameStateEnum.playing) {
            return;
        }

        this.setPlayerDirection(model.playerName, model.direction);
    }

    connectionChanged(isOnline) {
        if (!isOnline) {
            if (this.state === gameStateEnum.pending || this.state === gameStateEnum.playing) {
                if (this.state === gameStateEnum.playing) {
                    this.stop();
                }
            }

            this.state = gameStateEnum.none;
        }

        this.onGameConnectionChanged(isOnline);
    }

    gameFinished(model) {
        if (this.state !== gameStateEnum.playing) {
            return;
        }

        this.stop();
        this.onGameFinished(this.getGameResult(model.winnerName));

        for (let i = 0; i < this.model.playerModels.length; i++) {
            let player = this.model.playerModels[i];
            let result;

            if (model.winnerName) {
                if (player.name === model.winnerName) {
                    result = playerGameResultEnum.winner;
                } else {
                    result = playerGameResultEnum.loser;
                }
            } else {
                result = playerGameResultEnum.draw;
            }

            player.setGameResult(result);
        }

        this.invalidate();
    }

    forfeitGame() {
        if (this.state !== gameStateEnum.pending && this.state !== gameStateEnum.playing) {
            return false;
        }

        if (this.state === gameStateEnum.pending) {
            this.state = gameStateEnum.none;
        } else if (this.state === gameStateEnum.playing) {
            this.stop();
        }

        this.commClient.forfeitGame(this.playerName);
        this.onGameFinished('Game cancelled');

        return true;
    }

    findGame() {
        if (this.state !== gameStateEnum.none && this.state !== gameStateEnum.finished) {
            return false;
        }

        if (this.state === gameStateEnum.finished) {
            this.removeAllPlayers();
        }

        this.state = gameStateEnum.pending;

        let playerBoard = { rows: this.model.boardModel.rows, cols: this.model.boardModel.cols };
        this.commClient.findGame(this.playerName, playerBoard);

        return true;
    }

    addPlayer(name, positionModel, color = defaultPlayerColor) {
        const model = new Player({ name: name });
        const layer = new PlayerLayer({ playerModel: model, boardLayer: this.layer.boardLayer, color: color });

        model.init(new BoardPosition(positionModel.col, positionModel.row), positionModel.direction);

        this.model.playerModels.push(model);
        this.layer.playerLayers.push(layer);

        this.invalidate();

        return model;
    }

    findPlayerIndex(name) {
        return this.model.playerModels.findIndex(p => p.name === name);
    }

    removeAllPlayers() {
        this.model.playerModels.splice(0);
        this.layer.playerLayers.splice(0);
        this.invalidate();
    }

    removePlayer(name) {
        const index = this.findPlayerIndex(name);
        if (index > -1) {
            this.model.playerModels.splice(index, 1);
            this.layer.playerLayers.splice(index, 1);
            this.invalidate();
        }
    }

    start() {
        this.state = gameStateEnum.playing;
        this.invalidate();
        this.engineTimer = setInterval(() => this.engine(), defaultGameSpeedInMs);
    }

    stop() {
        clearInterval(this.engineTimer);
        this.engineTimer = null;

        this.state = gameStateEnum.finished;
        this.invalidate();
    }

    engine() {
        const players = this.model.playerModels;
        const activePlayers = players.filter(p => p.isPlaying);

        for (let i = 0; i < activePlayers.length; i++) {
            activePlayers[i].move();
        }

        const collisions = this.detectCollisions(activePlayers);
        let winnerName = null;
        let gameFinished = false;

        if (collisions.length === activePlayers.length - 1) {
            let winner = activePlayers.find(p => p.isPlaying);
            winner.setGameResult(playerGameResultEnum.winner);
            winnerName = winner.name;
            gameFinished = true;
        } else if (collisions.length === activePlayers.length) {
            winnerName = '';
            gameFinished = true;
        }

        if (!gameFinished) {
            this.invalidate();
        } else {
            this.stop();
            this.commClient.finishGame(winnerName);
            this.onGameFinished(this.getGameResult(winnerName));
        }
    }

    getGameResult(winnerName) {
        if (winnerName) {
            return 'Winner ' + winnerName;
        } else {
            return 'Draw!';
        }
    }

    detectCollisions(activePlayers) {
        const collisions = this.collisionDetection.detect(activePlayers);

        if (collisions.length > 0) {
            const isDraw = collisions.length === activePlayers.length;
            const gameResult = isDraw ? playerGameResultEnum.draw : playerGameResultEnum.loser;

            for (let i = 0; i < collisions.length; i++) {
                const player = collisions[i].player;
                player.setGameResult(gameResult);
                player.undoMove();
            }
        }

        return collisions;
    }

    draw() {
        this.layer.draw(this.ctx);
    }

    resize(width, height) {
        this.layer.resize(width, height);
        this.invalidate();
    }

    invalidate() {
        this.draw();
    }

    changeDirection(newDirection) {
        const directionChanged = this.setPlayerDirection(this.playerName, newDirection);

        if (directionChanged) {
            this.commClient.changePlayerDirection(newDirection);
        }
    }

    setPlayerDirection(playerName, newDirection) {
        if (this.state === gameStateEnum.playing) {
            const index = this.findPlayerIndex(playerName);
            if (index > -1) {
                const player = this.model.playerModels[index];
                return player.setDirection(newDirection);
            }
        }

        return false;
    }
}