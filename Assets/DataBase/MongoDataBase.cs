using MongoDB.Driver;

public class MongoDataBase
{
    private const string MONGO_URI = "mongodb://Marpione843749Kom@lobbydb-shard-00-00-4obkx.mongodb.net:27017,lobbydb-shard-00-01-4obkx.mongodb.net:27017,lobbydb-shard-00-02-4obkx.mongodb.net:27017/test?ssl=true&replicaSet=LobbyDB-shard-0&authSource=admin&retryWrites=true&w=majority";
    private const string DATABASE_NAME = "lobbydb";


    private MongoClient client;
    private MongoServer server;
    private MongoDatabase database;

    public void Initilize()
    {
        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        database = server.GetDatabase(DATABASE_NAME);

        //This is where we would initilize collections
        UnityEngine.Debug.Log("Database has been initilized");
    }

    public void ShutDown()
    {
        client = null;
        server.Shutdown();
        database = null;
    }
}
