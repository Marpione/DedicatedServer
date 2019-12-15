
using System.Collections.Generic;

public class Net_OnRequestFriend : NetMessage
{
    public Net_OnRequestFriend()
    {
        OP = NetOP.RequestFriend;
    }


    public List<Account> FriendRequests { set; get; }
}
