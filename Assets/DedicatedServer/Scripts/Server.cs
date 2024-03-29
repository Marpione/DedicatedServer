﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    private const int MAX_USER = 100;
    private const int port = 8080;
    private const int web_Port = 8081;
    private const int BYTE_SIZE = 1024;

    private byte relibleChannel;
    private int hostId;
    //private int webHostId;

    private byte error;
    private bool isStarted = false;

    private MongoDataBase mongoDataBase;
    #region MonoBehaviour
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Initilize();
    }

    private void Update()
    {
        UpdateMessageBumb();
    }

    #endregion
    public void Initilize()
    {
        mongoDataBase = new MongoDataBase();
        mongoDataBase.Initilize();
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        cc.AddChannel(QosType.Reliable);

        HostTopology hostTopology = new HostTopology(cc, MAX_USER);

        //Server Only Code
        hostId = NetworkTransport.AddHost(hostTopology, port);
        //Connecting to Browser
        //webHostId = NetworkTransport.AddHost(hostTopology, web_Port, null);

        isStarted = true;
        Debug.Log(string.Format("Opening Connection on port {0} and port {1}", port, web_Port));
    }

    public void ShutDown()
    {
        isStarted = false;
        NetworkTransport.Shutdown();
    }

    public void UpdateMessageBumb()
    {
        if (!isStarted)
            return;

        int recHostId; //Where is it from
        int connectionId; //Which User Sending this
        int channelId; //Which Lane is he sending that messageFrom

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);

        switch (type)
        {
            case NetworkEventType.DataEvent:
                Debug.Log("Data");
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMessage netMessage = (NetMessage)formatter.Deserialize(ms);
                OnData(connectionId, channelId, recHostId, netMessage);
                break;
            case NetworkEventType.ConnectEvent:
                //Update friend Here
                Debug.Log(string.Format("User {0} has connected trough {1} ", connectionId, recHostId));
                break;
            case NetworkEventType.DisconnectEvent:
                //Update friend here
                DisconnectEvent(recHostId, connectionId);
                break;
            case NetworkEventType.Nothing:
                break;
           
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network Event Type");
                break;
        }
    }

  

    private void CreateAccount(int connectionId, int channelId, int recHostId, Net_CreateAccount ca)
    {
        Net_OnCreateAccount oca = new Net_OnCreateAccount();

        if(mongoDataBase.InsertAccount(ca.UserId))
        {
            oca.Success = 1;
            oca.Information = "Account was created";
            oca.UserId = ca.UserId;
        }
        else
        {
            oca.Success = 0;
            oca.Information = "There was an error on create account!";
            oca.UserId = ca.UserId;
        }

        SendClient(recHostId, connectionId, oca);
    }

    private void LoginRequest(int connectionId, int channelId, int recHostId, Net_LoginRequest lr)
    {
        string randomToken = Utility.GenerateRandom(256);
        AccountModel account;
        account = mongoDataBase.LoginAccount(lr.UserId, connectionId, randomToken);

        Net_OnLoginRequest olr = new Net_OnLoginRequest();
        
        if(account != null)
        {
            olr.UserId = lr.UserId;
            olr.Success = 1;
            olr.Information = "Login success " + account.userId;
            olr.Token = randomToken;
            olr.ConnectionId = connectionId;

            //Update friend here
            // Prepare and send update message
            Net_FriendUpdate fu = new Net_FriendUpdate();
            fu.Friend = account.GetAccount();

            foreach (var f in mongoDataBase.FindAllFriendsBy(account.userId))
            {
                if (f.ActiveConnection == 0)
                    continue;

                SendClient(recHostId, f.ActiveConnection, fu);
            }

            //If this is a facebook login update the user id to facebook user id for this user
            if (lr.FacebookUserId != null)
            {
                olr.Information = "Login success user " + account.userId + "updated from guest to facebook with the id " + lr.FacebookUserId;
                mongoDataBase.UpdateUserFromGuestToFacebookUser(lr.UserId, lr.FacebookUserId);
            }
        }
        else
        {
            olr.Success = 0;
        }
        

        SendClient(recHostId, connectionId, olr);

        Debug.Log(olr.Information);

    }

    #region OnData
    private void OnData(int connectionId, int channelId, int recHostId, NetMessage netMessage)
    {
        Debug.Log("Recived a message of type" + netMessage.OP);
        switch(netMessage.OP)
        {
            case NetOP.None:
                Debug.LogError("Unexpected Message from client");
                break;
            case NetOP.CreateAccount:
                CreateAccount(connectionId, channelId, recHostId, (Net_CreateAccount)netMessage);
                break;
            case NetOP.LoginRequest:
                LoginRequest(connectionId, channelId, recHostId, (Net_LoginRequest)netMessage);
                break;
            case NetOP.AddFriend:
                AddFriend(connectionId, channelId, recHostId, (Net_AddFriend)netMessage);
                break;
            case NetOP.RemoveFriend:
                RemoveFriend(connectionId, channelId, recHostId, (Net_RemoveFriend)netMessage);
                break;
            case NetOP.RequestFriend:
                RequestFriend(connectionId, channelId, recHostId, (Net_RequestFriend)netMessage);
                break;
        }
    }

    private void DisconnectEvent(int recHostId, int connectionId)
    {
        Debug.Log(string.Format("User {0} has disconnected ", connectionId));

        //Get a referans to the connected acount
        AccountModel account = mongoDataBase.FindAccountByConnectionID(connectionId);
        if (account == null)
            return;

        mongoDataBase.UpdateAccountOnDisconnect(account.userId);

        // Prepare and send update message
        Net_FriendUpdate fu = new Net_FriendUpdate();
        AccountModel updatedAccount = mongoDataBase.FindAccountByUserId(account.userId);
        fu.Friend = updatedAccount.GetAccount();

        foreach (var f in mongoDataBase.FindAllFriendsBy(account.userId))
        {
            if (f.ActiveConnection == 0)
                continue;

            SendClient(recHostId, f.ActiveConnection, fu);
        }
    }

    #endregion

    #region Send
    public void SendClient(int recHost, int connectionID, NetMessage netMessage)
    {
        //This is where we hold our data
        byte[] buffer = new byte[BYTE_SIZE];

        //This is where you would crush your data into bytes
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, netMessage);


        //if(recHost == 0)
            NetworkTransport.Send(hostId, connectionID, relibleChannel, buffer, BYTE_SIZE, out error);
        //else
        //    NetworkTransport.Send(webHostId, connectionID, relibleChannel, buffer, BYTE_SIZE, out error);
    }

    private void AddFriend(int connectionId, int channelId, int recHostId, Net_AddFriend netMessage)
    {
        Net_OnAddFriend oaf = new Net_OnAddFriend();

        if (mongoDataBase.InsertFriend(netMessage.Token, netMessage.UserId))
        {
            oaf.Success = 1;
            oaf.FriendAccount = mongoDataBase.FindAccountByUserId(netMessage.UserId).GetAccount();
            Debug.Log("Adding Friend" + oaf.FriendAccount.userId);
        }
        else oaf.Success = 0;
        SendClient(recHostId, connectionId, oaf);
    }

    private void RequestFriend(int connectionId, int channelId, int recHostId, Net_RequestFriend netMessage)
    {
        Net_OnRequestFriend orf = new Net_OnRequestFriend();
        orf.FriendRequests = mongoDataBase.FindAllFriendsFrom(netMessage.Token);
        Debug.Log("Sending a list of frineds to client ");
        SendClient(recHostId, connectionId, orf);
    }

    private void RemoveFriend(int connectionId, int channelId, int recHostId, Net_RemoveFriend netMessage)
    {
        mongoDataBase.RemoveFriend(netMessage.Token, netMessage.UserId);
    }

    #endregion
}
