
public class Net_FriendUpdate : NetMessage
{
    public Net_FriendUpdate()
    {
        OP = NetOP.FriendUpdate;
    }

    public Account Friend { set; get; }
}
