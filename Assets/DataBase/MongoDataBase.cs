using MongoDB.Driver;

public class MongoDataBase
{
    private const string MONGO_URI = "mongodb://Marpione:843749Kom@lobbydb-4obkx.mongodb.net/lobbydb";
    private const string DATABASE_NAME = "lobbydb";


    private MongoClient client;
    private MongoServer server;
    private MongoDatabase database;

    private MongoCollection accounts;

    public void Initilize()
    {
        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        database = server.GetDatabase(DATABASE_NAME);

        //This is where we would initilize collections
        accounts = database.GetCollection<AccountModel>("account");
        UnityEngine.Debug.Log("Database has been initilized");
    }

    public void ShutDown()
    {
        client = null;
        server.Shutdown();
        database = null;
    }

    #region Insert
    public bool InsertAccount(string username, string password, string email)
    {
        AccountModel newAccount = new AccountModel();
        newAccount.Username = username;
        newAccount.ShaPassword = password;
        newAccount.Email = email;
        newAccount.Discriminator = "0000";

        accounts.Insert(newAccount);

        return true;
    }
    #endregion

    #region Fetch

    #endregion

    #region Update

    #endregion

    #region Delete

    #endregion
}
