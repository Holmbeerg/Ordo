import * as signalR from '@microsoft/signalr';
import { ref } from 'vue';
import type { TimeControl } from '@/types/TimeControl.ts';
import type { GameOverReason, Tile, TilePlacement } from '@/types/game.ts';
import { useGameStore } from '@/stores/gameStore.ts';

export const connectionState = ref<'disconnected' | 'connecting' | 'connected'>('disconnected');
export const playerId = ref<string>(localStorage.getItem('playerId') || '');

let connection: signalR.HubConnection | null = null;

export const createConnection = (): signalR.HubConnection => {
    if (connection) {
        return connection;
    }

    const baseUrl = import.meta.env.VITE_API_URL || '';
    const url = `${baseUrl}/gameHub?playerId=${playerId.value}`;

    connection = new signalR.HubConnectionBuilder().withUrl(url).withAutomaticReconnect().build(); // doesn't start the connection yet

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

    connection.on('ReceiveGameState', (gameStateDto) => {
        console.log('Received updated game state:', gameStateDto);
        useGameStore().setGameState(gameStateDto);
    });

    connection.on('OpponentDisconnected', () => {
        console.log('Opponent disconnected');
        useGameStore().setOpponentConnected(false);
    });

    connection.on('OpponentReconnected', () => {
        console.log('Opponent reconnected');
        useGameStore().setOpponentConnected(true);
    });

    connection.on('ReceiveError', (errorMessage: string) => {
        console.error('Game Error:', errorMessage);
        const gameStore = useGameStore();
        gameStore.error = errorMessage;
        gameStore.recallTiles();
    });

    connection.on('GameOver', (winnerId: string, reason: GameOverReason) => {
        console.log(`Game over. Winner: ${winnerId}, Reason: ${reason}`);
        useGameStore().setWinner(winnerId, reason);
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

export const submitWord = async (gameId: string, placements: TilePlacement[]): Promise<void> => {
    if (connection) {
        const payload = placements.map(({ row, col, tile: { letter, isBlank } }) => ({
            row,
            col,
            tile: { letter, isBlank },
        }));
        await connection.invoke('SubmitMove', gameId, payload);
    }
};

export const passTurn = async (gameId: string): Promise<void> => {
    if (connection) {
        await connection.invoke('PassTurn', gameId);
    }
};

export const exchangeTiles = async (gameId: string, tiles: Tile[]): Promise<void> => {
    if (connection) {
        await connection.invoke('ExchangeTiles', gameId, tiles);
    }
};

export const resignGame = async (gameId: string): Promise<void> => {
    if (connection) {
        await connection.invoke('ResignGame', gameId);
    }
};
