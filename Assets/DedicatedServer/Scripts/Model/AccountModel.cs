﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public enum AccountType { guest, facebook }

[System.Serializable]
public class AccountModel
{
    //[BsonId]
    public ObjectId _id;

    public int ActiveConnection { set; get; }
    public string userId;
    //public string Username { get; set; }
    //public string Discriminator { get; set; }
    //public string Email { get; set; }
    //public string ShaPassword { get; set; }

    //If bigger than 0 person is online, byte count means how many games he is playing from our server
    public byte Status { get; set; }
    public string Token { get; set; }

    public AccountType AccountType { set; get; }

    public DateTime CreateOn { get; set; }
    public DateTime LastLogin { get; set; }


    public Account GetAccount()
    {
        return new Account() { userId = this.userId,
            ActiveConnection = this.ActiveConnection,
            Status = this.Status
        };
    }
}
