using System.Collections.Generic;

[System.Serializable]
public class Net_OnRequestFriend : NetMessage
{
    public Net_OnRequestFriend()
    {
        OP = NetOP.OnRequestFriend;
    }


    public List<Account> FriendRequests { set; get; }
}
