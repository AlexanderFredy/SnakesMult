import { Room, Client } from "colyseus";
import { Schema, type, MapSchema, ArraySchema } from "@colyseus/schema";

export class Vector2Float extends Schema {
    @type("uint32") id = 0;
    @type("number") x = Math.floor(Math.random() * 256) - 128;
    @type("number") z = Math.floor(Math.random() * 256) - 128;
}

export class Player extends Schema {
    @type("string") login = "";
    @type("number") x = Math.floor(Math.random() * 128) - 64;
    @type("number") z = Math.floor(Math.random() * 128) - 64;
    @type("uint8") d = 0;
    @type("uint16") score = 0;
    @type("uint16") appleCollected = 0;
    @type("uint8") c = 0; 
}

export class State extends Schema {
    @type({ map: Player }) players = new MapSchema<Player>();
    @type([ Vector2Float ]) apples = new ArraySchema<Vector2Float>();

    appleLastId = 0;

    CreateApple(){
        const apple = new Vector2Float();
        apple.id = this.appleLastId;
        this.appleLastId++;
        this.apples.push(apple);
    }

    collectApple(player: Player, data: any){
        const apple = this.apples.find((value) => value.id === data.id);
        if (apple === undefined) return;

        apple.x = Math.floor(Math.random() * 256) - 128;
        apple.z = Math.floor(Math.random() * 256) - 128;

        player.score += Math.round(player.d/2);
        player.appleCollected++;
        if (player.appleCollected >= player.d)
        {
            player.d++;
            player.appleCollected -= player.d;
        }
           
    }

    createPlayer(sessionId: string,skin:number,login) {
        const player = new Player();
        player.login = login;
        player.c = skin;

        this.players.set(sessionId, player);
    }

    removePlayer(sessionId: string) {
        this.players.delete(sessionId);
    }

    movePlayer (sessionId: string, movement: any) {
        this.players.get(sessionId).x = movement.x;
        this.players.get(sessionId).z = movement.z;
    }
}

export class StateHandlerRoom extends Room<State> {
    maxClients = 6;
    skinIndexes: number[] = [0];
    startAppleCount = 200;
    gameDuration = 600;

    mixArray(arr:any){
        var currentIndex = arr.length;
        var tempValue, randomIndex;

        while (currentIndex !== 0){
            randomIndex = Math.floor(Math.random()*currentIndex);
            currentIndex -= 1;
            tempValue = arr[currentIndex];
            arr[currentIndex] = arr[randomIndex];
            arr[randomIndex] = tempValue;
        }
    }

    Delay(timeInMillis: number): Promise<void> {
        return new Promise((resolve) => setTimeout(() => resolve(), timeInMillis));
    }

    async StartCountDown(duration:number): Promise<void> {
        var lastTime = duration;
        while (lastTime >= 0) {
            this.broadcast("time",lastTime);
            await this.Delay(1000);
            lastTime--;         
        }
        this.broadcast("GameOver");
      }

    onCreate (options) {
        this.maxClients = options.skinsCount;
        for (var i = 1; i < options.skinsCount; i++){
            this.skinIndexes.push(i);
        }
        this.mixArray(this.skinIndexes);

        this.StartCountDown(this.gameDuration);
        console.log("StateHandlerRoom created!", options);
        

        this.setState(new State());

        this.onMessage("move", (client, data) => {
            this.state.movePlayer(client.sessionId, data);
        });

        this.onMessage("collect", (client, data) => {
            const player = this.state.players.get(client.sessionId);
            this.state.collectApple(player, data);
        });

        this.onMessage("respawn", (client,data) => {
            this.broadcast("enemyDead",data.id,{except: client});
            const player = this.state.players.get(client.sessionId);
            player.d = 0;
        });

        this.onMessage("enemyAlive", (client,data) => {
            this.broadcast("enemyAlive",data.id,{except: client});
        });

        for (var i = 0; i < this.startAppleCount; i++){
            this.state.CreateApple();
        }
    }

    onAuth(client, options, req) {
        return true;
    }

    onJoin (client: Client,data) {
        const skin = this.skinIndexes[this.clients.length-1];
        this.state.createPlayer(client.sessionId,skin,data.login);
    }

    onLeave (client) {
        this.state.removePlayer(client.sessionId);
    }

    onDispose () {
        console.log("Dispose StateHandlerRoom");
    }

}
