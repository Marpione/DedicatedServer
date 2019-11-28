using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UnityEngine;

public class MongoDataBase
{
    private const string MONGO_URI = "mongodb://Marpione:843749Kom@lobbydb-shard-00-00-4obkx.mongodb.net:27017,lobbydb-shard-00-01-4obkx.mongodb.net:27017,lobbydb-shard-00-02-4obkx.mongodb.net:27017/lobbydb?ssl=true&replicaSet=lobbydb-shard-0&authSource=admin&w=majority";
    //private const string MONGO_URI = "mongodb://Marpione:843749Kom@lobbydb-4obkx.mongodb.net/lobbydb";
    private const string DATABASE_NAME = "lobbydb";


    private MongoClient client;
    private MongoServer server;
    private MongoDB.Driver.MongoDatabase database;

    private MongoCollection<AccountModel> accounts;

    public void Initilize()
    {
        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        database = server.GetDatabase(DATABASE_NAME);
        UnityEngine.Debug.Log(database);


        //This is where we would initilize collections
        accounts = database.GetCollection<AccountModel>("account");
        UnityEngine.Debug.Log(accounts);
        UnityEngine.Debug.Log(accounts.Database);
        UnityEngine.Debug.Log("Database has been initilized");
    }

    public void ShutDown()
    {
        client = null;
        //server.();
        database = null;
    }

    #region Insert
    public bool InsertAccount(string username, string password, string email)
    {
        if(!Utility.IsEmail(email))
        {
            UnityEngine.Debug.Log(email + " Not an E-mail");
            return false;
        }

        if (!Utility.IsUsername(username))
        {
            UnityEngine.Debug.Log(username + " Not an E-mail");
            return false;
        }

        //Check if the account is already exist
        if (FindAccountByEmail(email) != null)
        {
            UnityEngine.Debug.Log(email + " Account is already in use");
            return false;
        }

        AccountModel newAccount = new AccountModel();
        newAccount.Username = username;
        newAccount.ShaPassword = password;
        newAccount.Email = email;
        newAccount.Discriminator = "0000";

        //Roll for unique Discriminator
        int rollCount = 0;
        while(FindAccountByUsernameAndDiscriminator(newAccount.Username, newAccount.Discriminator) != null)
        {
            newAccount.Discriminator = Random.Range(0, 999).ToString("0000");
            rollCount++;
            if (rollCount > 1000)
            {
                Debug.Log("We rolled too many times suggest a user name to change!");
                return false;
            }
                
        }

        accounts.Insert(newAccount);
        return true;
    }

    public AccountModel LoginAccount(string usernameOrEmail, string password, int connectionID, string token)
    {
        AccountModel myAccount = null;
        IMongoQuery query = null;

        //Find my acount
        if(Utility.IsEmail(usernameOrEmail))
        {
            query = Query.And(
                Query<AccountModel>.EQ(u => u.Email, usernameOrEmail),
                Query<AccountModel>.EQ(u => u.ShaPassword, password));

            myAccount = accounts.FindOne(query);
        }else
        {
            string[] data = usernameOrEmail.Split('#');
            if(data[1] != null)
            {
                query = Query.And(
                    Query<AccountModel>.EQ(u => u.Username, data[0]),
                    Query<AccountModel>.EQ(u => u.Discriminator, data[1]),
                    Query<AccountModel>.EQ(u => u.ShaPassword, data[2]));

                myAccount = accounts.FindOne(query);
            }
        }
        if (myAccount != null)
        {
            //Login
            myAccount.ActiveConnection = connectionID;
            myAccount.Token = token;
            myAccount.Status = 1;
            myAccount.LastLogin = System.DateTime.Now;

            accounts.Update(query, Update<AccountModel>.Replace(myAccount));
        }
        else
        {
            Debug.Log("No account Found");
        }

        return myAccount;
    }
    #endregion

    #region Fetch
    public AccountModel FindAccountByEmail(string email)
    {
        return accounts.FindOne(Query<AccountModel>.EQ(u => u.Email, email));
    }

    public AccountModel FindAccountByUsernameAndDiscriminator(string username, string discriminator)
    {
        var query = Query.And(
            Query<AccountModel>.EQ(u => u.Username, username),
            Query<AccountModel>.EQ(u => u.Discriminator, discriminator));
        return accounts.FindOne(query);
    }
    #endregion

    #region Update

    #endregion

    #region Delete

    #endregion
}
