
[System.Serializable]
public class Net_RequestFriend : NetMessage
{
    public Net_RequestFriend()
    {
        OP = NetOP.RequestFriend;
    }

    public string Token;

}
