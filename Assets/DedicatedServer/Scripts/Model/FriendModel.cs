using MongoDB.Bson;
using MongoDB.Driver;

[System.Serializable]
public class FriendModel
{
    public ObjectId _id;

    public MongoDBRef Sender;
    public MongoDBRef Reciver;
}
