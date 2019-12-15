using MongoDB.Bson;
using MongoDB.Driver;

public class FriendModel
{
    public ObjectId _id;

    public MongoDBRef Sender;
    public MongoDBRef Reciver;
}
