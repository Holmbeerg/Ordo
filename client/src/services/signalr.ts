import * as signalR from '@microsoft/signalr';
import {ref} from 'vue';
import type {TimeControl} from "@/types/TimeControl.ts";

export const connectionState = ref<'disconnected' | 'connecting' | 'connected'>('disconnected');
export const playerId = ref<string>(localStorage.getItem('playerId') || '');

let connection: signalR.HubConnection | null = null;

export const createConnection = (): signalR.HubConnection => {
    if (connection) {
        return connection;
    }

    const url = `http://localhost:5172/gameHub?playerId=${playerId.value}`;

    connection = new signalR.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .build(); // doesn't start the connection yet

    connection.onclose(() => {
        connectionState.value = 'disconnected';
        console.log('SignalR disconnected');
    });

    connection.onreconnecting(() => {
        connectionState.value = 'connecting';
        console.log('SignalR reconnecting...');
    });

    connection.onreconnected(() => {
        connectionState.value = 'connected';
        console.log('SignalR reconnected');
    });

    connection.on('PlayerConnected', (playerIdFromServer: string) => {
        if (playerId.value !== playerIdFromServer) {
            playerId.value = playerIdFromServer;
            localStorage.setItem('playerId', playerIdFromServer);
            console.log('Player connected and assigned new ID:', playerIdFromServer);
        } else {
            console.log('Player reconnected with existing ID:', playerIdFromServer);
        }
    });

    connection.on('JoinedMatchmaking', (timeControl: TimeControl) => {
        console.log('Joined matchmaking for:', timeControl);
    });

    // TODO: need gameData or something here to initialize the game state, update game store etc?, notification?
    connection.on('MatchFound', (gameId: string) => {
        console.log('Match found! Game ID:', gameId);
        // router.push(`/game/${gameId}`);
    });

    return connection;
};

export const connectToHub = async (): Promise<void> => {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        console.log('Already connected to SignalR hub');
        return;
    }

    connection = createConnection();

    try {
        connectionState.value = 'connecting';
        await connection.start();
        connectionState.value = 'connected';
        console.log('Connected to SignalR hub');
    } catch (error) {
        connectionState.value = 'disconnected';
        console.error('Failed to connect to SignalR hub:', error);
        throw error;
    }
};

export const disconnectFromHub = async (): Promise<void> => {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        await connection.stop();
        console.log('Disconnected from SignalR hub');
    }
};

export const joinMatchmaking = async (timeControl: TimeControl): Promise<void> => {
    if (connection && playerId.value) {
        await connection.invoke('JoinMatchmaking', timeControl, playerId.value);
    }
};

export const leaveMatchmaking = async (timeControl: TimeControl): Promise<void> => {
    if (connection && playerId.value) {
        await connection.invoke('LeaveMatchmaking', timeControl, playerId.value);
    }
};