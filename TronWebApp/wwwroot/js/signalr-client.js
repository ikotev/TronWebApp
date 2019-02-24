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

        this.connection.on("playerDirectionChanged",
            data => {

            });


        this.connection.on("gameStateChanged",
            data => {

            });

        this.connection.onclose(async () => {
            console.log('connection closed');
            await this.connect();
        });
    }

    async connect() {
        try {
            await this.connection.start();
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

    async findGame(playerName) {
        try {
            let result = await this.connection.invoke("FindGame", { playerName });
        } catch (err) {
            console.log(err);
        }
    }

    async directionChanged(direction) {
        try {
            let result = await this.connection.invoke("DirectionChanged", { direction });
        } catch (err) {
            console.log(err);
        }
    }

    async gameFinished(winnerName) {
        try {
            let result = await this.connection.invoke("GameFinished", { winnerName });
        } catch (err) {
            console.log(err);
        }
    }
}
