import * as signalR from "@microsoft/signalr";

let connection;

export const startConnection = async (token) => {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5286/todohub", {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

    await connection.start();
    return connection;
};

export const getConnection = () => connection;
