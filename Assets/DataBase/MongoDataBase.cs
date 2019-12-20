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

    public bool InsertFriend(string token, string firendUserId)
    {
        FriendModel newFriend = new FriendModel();
        newFriend.Sender = new MongoDBRef("account", FindAccountByToken(token)._id);

        AccountModel friend = FindAccountByUserId(firendUserId);
        if (friend != null)
        {
            newFriend.Reciver = new MongoDBRef("account", friend._id);
        }
        else
        {
            return false;
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

    public bool InsertAccount(string userId)
    {
        //Check if the account is already exist
        if (FindAccountByUserId(userId) != null)
        {
            UnityEngine.Debug.Log(userId + " Account is already in use");
            return false;
        }

        AccountModel newAccount = new AccountModel();
        newAccount.userId = userId;
        newAccount.CreateOn = System.DateTime.Now;

        accounts.Insert(newAccount);
        return true;
    }

    //public AccountModel LoginAccount(string usernameOrEmail, string password, int connectionID, string token)
    //{
    //    AccountModel myAccount = null;
    //    IMongoQuery query = null;

    //    //Find my acount
    //    if(Utility.IsEmail(usernameOrEmail))
    //    {
    //        query = Query.And(
    //            Query<AccountModel>.EQ(u => u.Email, usernameOrEmail),
    //            Query<AccountModel>.EQ(u => u.ShaPassword, password));

    //        myAccount = accounts.FindOne(query);
    //    }else
    //    {
    //        string[] data = usernameOrEmail.Split('#');
    //        if(data[1] != null)
    //        {
    //            query = Query.And(
    //                Query<AccountModel>.EQ(u => u.Username, data[0]),
    //                Query<AccountModel>.EQ(u => u.Discriminator, data[1]),
    //                Query<AccountModel>.EQ(u => u.ShaPassword, data[2]));

    //            myAccount = accounts.FindOne(query);
    //        }
    //    }
    //    if (myAccount != null)
    //    {
    //        //Login
    //        myAccount.ActiveConnection = connectionID;
    //        myAccount.Token = token;
    //        myAccount.Status = 1;
    //        myAccount.LastLogin = System.DateTime.Now;

    //        accounts.Update(query, Update<AccountModel>.Replace(myAccount));
    //    }
    //    else
    //    {
    //        Debug.Log("No account Found");
    //    }

    //    return myAccount;
    //}

    public AccountModel LoginAccount(string userID, int connectionID, string token)
    {
        AccountModel myAccount = null;
        IMongoQuery query = null;

        //Find my acount
        myAccount = FindAccountByUserId(userID);

        query = Query.And(
                    Query<AccountModel>.EQ(u => u.userId, userID));

        myAccount = accounts.FindOne(query);

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
    public AccountModel FindAccountByUserId(string userId)
    {
        return accounts.FindOne(Query<AccountModel>.EQ(u => u.userId, userId));
    }
    public AccountModel FindAccounById(ObjectId id)
    {
        return accounts.FindOne(Query<AccountModel>.EQ(u => u._id, id));
    }
    #region Fetch Extra
    //public AccountModel FindAccountByEmail(string email)
    //{
    //    return accounts.FindOne(Query<AccountModel>.EQ(u => u.Email, email));
    //}

    //public AccountModel FindAccountByUsernameAndDiscriminator(string username, string discriminator)
    //{
    //    var query = Query.And(
    //        Query<AccountModel>.EQ(u => u.Username, username),
    //        Query<AccountModel>.EQ(u => u.Discriminator, discriminator));
    //    return accounts.FindOne(query);
    //}
    #endregion

    public FriendModel FindFriendByUsername(string token, string userId)
    {
        try
        {
            var sender = new MongoDBRef("account", FindAccountByToken(token)._id);
            var reciver = new MongoDBRef("account", FindAccountByUserId(userId)._id);

            var query = Query.And(
                Query<FriendModel>.EQ(f => f.Sender, sender),
                Query<FriendModel>.EQ(f => f.Reciver, reciver));

            return friends.FindOne(query);
        }
        catch (System.Exception)
        {

            throw;
        }
    }


    public AccountModel FindAccountByToken(string token)
    {
        return accounts.FindOne(Query<AccountModel>.EQ(u => u.Token, token));
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

    public List<Account> FindAllFriendsBy(string userId)
    {
        var self = new MongoDBRef("account", FindAccountByUserId(userId)._id);

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
    internal void UpdateAccountOnDisconnect(string userId)
    {
        var query = Query<AccountModel>.EQ(a => a.userId, userId);
        var account = accounts.FindOne(query);

        account.Token = null;
        account.ActiveConnection = 0;
        account.Status = 0;

        accounts.Update(query, Update<AccountModel>.Replace(account));
    }

    public void UpdateUserFromGuestToFacebookUser(string currentUserId, string facebookUserId)
    {
        try
        {
            var query = Query<AccountModel>.EQ(a => a.userId, currentUserId);
            var account = accounts.FindOne(query);
            account.userId = facebookUserId;

            accounts.Update(query, Update<AccountModel>.Replace(account));
        }
        catch (Exception e)
        {
            Debug.LogError("Can't find user with id " + currentUserId);
        }
        
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
