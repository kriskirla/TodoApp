import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export const startConnection = async (token: string): Promise<signalR.HubConnection> => {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5286/todohub", {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

    await connection.start();
    return connection;
};

export const getConnection = (): signalR.HubConnection | null => {
    return connection;
};
