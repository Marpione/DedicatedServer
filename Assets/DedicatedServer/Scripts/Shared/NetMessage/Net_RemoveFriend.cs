

public class Net_RemoveFriend : NetMessage
{
    public Net_RemoveFriend()
    {
        OP = NetOP.RemoveFriend;
    }

    public string Token;
    public string UsernameOrEmail { set; get; }

}
