using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using UnityEngine;
using System.Collections.Generic;
using System;

public class MongoDataBase
{
    private const string MONGO_URI = "mongodb://Marpione:843749Kom@lobbydb-shard-00-00-4obkx.mongodb.net:27017,lobbydb-shard-00-01-4obkx.mongodb.net:27017,lobbydb-shard-00-02-4obkx.mongodb.net:27017/lobbydb?ssl=true&replicaSet=lobbydb-shard-0&authSource=admin&w=majority";
    //private const string MONGO_URI = "mongodb://Marpione:843749Kom@lobbydb-4obkx.mongodb.net/lobbydb";
    private const string DATABASE_NAME = "lobbydb";


    private MongoClient client;
    private MongoServer server;
    private MongoDB.Driver.MongoDatabase database;

    private MongoCollection<AccountModel> accounts;
    private MongoCollection<FriendModel> friends;

    public void Initilize()
    {
        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        database = server.GetDatabase(DATABASE_NAME);
        UnityEngine.Debug.Log(database);


        //This is where we would initilize collections
        accounts = database.GetCollection<AccountModel>("account");
        friends = database.GetCollection<FriendModel>("friend");
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

    public bool InsertFriend(string token, string usernameOrEmail)
    {
        FriendModel newFriend = new FriendModel();
        newFriend.Sender = new MongoDBRef("account", FindAccountByToken(token)._id);

        //Getting Reference to friend
        if(!Utility.IsEmail(usernameOrEmail))
        {
            //If Username
            string[] data = usernameOrEmail.Split('#');
            if(data[1] != null)
            {
                AccountModel friend = FindAccountByUsernameAndDiscriminator(data[0], data[1]);
                if(friend != null)
                {
                    newFriend.Reciver = new MongoDBRef("account", friend._id);
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            //If Email
            AccountModel friend = FindAccountByEmail(usernameOrEmail);
            if (friend != null)
            {
                newFriend.Reciver = new MongoDBRef("account", friend._id);
            }
            else return false;
        }

        if(newFriend.Reciver != newFriend.Sender)
        {
            //Check if the friend exist?
            var query = Query.And(
                Query<FriendModel>.EQ(u => u.Sender, newFriend.Sender),
                Query<FriendModel>.EQ(u => u.Reciver, newFriend.Reciver));



            //If friend not added, create one
            if(friends.FindOne(query) == null)
            {
                friends.Insert(newFriend);
                return true;
            }
            
        }
        return false;
    }

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
        newAccount.CreateOn = System.DateTime.Now;

        //Roll for unique Discriminator
        int rollCount = 0;
        while(FindAccountByUsernameAndDiscriminator(newAccount.Username, newAccount.Discriminator) != null)
        {
            newAccount.Discriminator = UnityEngine.Random.Range(0, 999).ToString("0000");
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

    public AccountModel FindAccounById(ObjectId id)
    {
        return accounts.FindOne(Query<AccountModel>.EQ(u => u._id, id));
    }

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

    public AccountModel FindAccountByToken(string token)
    {
        return accounts.FindOne(Query<AccountModel>.EQ(u => u.Token, token));
    }

    public FriendModel FindFriendByUsername(string token, string username)
    {
        try
        {
            string[] data = username.Split('#');
            if (data[1] != null)
            {
                var sender = new MongoDBRef("account", FindAccountByToken(token)._id);
                var reciver = new MongoDBRef("account", FindAccountByUsernameAndDiscriminator(data[0], data[1])._id);

                var query = Query.And(
                    Query<FriendModel>.EQ(f => f.Sender, sender),
                    Query<FriendModel>.EQ(f => f.Reciver, reciver));

                return friends.FindOne(query);
            }

            return null;
        }
        catch (System.Exception)
        {

            throw;
        }
    }
    
    public List<Account> FindAllFriendsFrom(string token)
    {
        var self = new MongoDBRef("account", FindAccountByToken(token)._id);

        var query = Query<FriendModel>.EQ(f => f.Sender, self);

        List<Account> friendResponse = new List<Account>();
        foreach (var friend in friends.Find(query))
        {
            friendResponse.Add(FindAccounById(friend.Reciver.Id.AsObjectId).GetAccount());
        }

        return friendResponse;
    }

    public List<Account> FindAllFriendsBy(string email)
    {
        var self = new MongoDBRef("account", FindAccountByEmail(email)._id);

        var query = Query<FriendModel>.EQ(f => f.Reciver, self);

        List<Account> friendResponse = new List<Account>();
        foreach (var friend in friends.Find(query))
        {
            friendResponse.Add(FindAccounById(friend.Sender.Id.AsObjectId).GetAccount());
        }

        return friendResponse;
    }

    public AccountModel FindAccountByConnectionID(int connectionId)
    {
        return accounts.FindOne(Query<AccountModel>.EQ(u => u.ActiveConnection, connectionId));
    }

    #endregion

    #region Update
    internal void UpdateAccountOnDisconnect(string email)
    {
        var query = Query<AccountModel>.EQ(a => a.Email, email);
        var account = accounts.FindOne(query);

        account.Token = null;
        account.ActiveConnection = 0;
        account.Status = 0;

        accounts.Update(query, Update<AccountModel>.Replace(account));
    }
    #endregion

    #region Delete
    public void RemoveFriend(string token, string username)
    {
        ObjectId id = FindFriendByUsername(token, username)._id;
        friends.Remove(Query<FriendModel>.EQ(f => f._id, id));
    }
    #endregion
}
