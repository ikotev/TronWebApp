"use strict";

class SignalRClient {
    constructor(hubUrl = "/tron-game-hub") {
        this.hubUrl = hubUrl;
        this.connection = null;        
        this.init();
    }

    init() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl)
            .build();

        this.onReceivePlayerDirectionChanged = () => { };
        this.connection.on("receivePlayerDirectionChanged",
            model => {
                this.onReceivePlayerDirectionChanged(model);
            });
        
        this.onReceiveGameStarted = () => {};
        this.connection.on("receiveGameStarted",
            model => {
                this.onReceiveGameStarted(model);
            });

        this.onReceiveGameFinished = () => { };
        this.connection.on("receiveGameFinished",
            model => {
                this.onReceiveGameFinished(model);
            });

        this.onConnectionChanged = () => { };

        this.connection.onclose(async () => {            
            this.onConnectionChanged(false);
            console.log('connection closed');
            await this.connect();
        });        
    }

    async connect() {
        try {
            await this.connection.start();            
            this.onConnectionChanged(true);
            console.log('connected');
        } catch (err) {
            console.log(err);
            setTimeout(() => this.connect(), 5000);
        }
    }
    
    async disconnect() {
        try {
            await this.connection.stop();
            console.log('disconnected');
        } catch (err) {
            console.log(err);
        }
    }

    async forfeitGame(playerName) {
        try {
            const model = { playerName: playerName };
            const result = await this.connection.invoke("ForfeitGame", model);
        } catch (err) {
            console.log(err);
        }
    }

    async findGame(playerName, playerBoard) {
        try {
            const model = { playerName: playerName, playerBoard: playerBoard };
            const result = await this.connection.invoke("FindGame", model);
        } catch (err) {
            console.log(err);
        }
    }
    
    async changePlayerDirection(direction) {
        try {
            const result = await this.connection.invoke("ChangePlayerDirection", { direction });
        } catch (err) {
            console.log(err);
        }
    }

    async finishGame(winnerName) {
        try {
            const result = await this.connection.invoke("FinishGame", { winnerName });
        } catch (err) {
            console.log(err);
        }
    }
}
