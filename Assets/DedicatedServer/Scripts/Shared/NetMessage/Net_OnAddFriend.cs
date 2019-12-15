

public class Net_OnAddFriend : NetMessage
{
    public Net_OnAddFriend()
    {
        OP = NetOP.AddFriend;
    }

    public Account FriendAccount { set; get; }
}
