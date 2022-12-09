using IMAP_server;

Server server = new Server("127.0.0.1", 143);

server.StartServer();

await server.ConnectClient();