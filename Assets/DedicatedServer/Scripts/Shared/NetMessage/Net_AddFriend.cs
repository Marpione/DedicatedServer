
[System.Serializable]
public class Net_AddFriend : NetMessage
{
    public Net_AddFriend()
    {
        OP = NetOP.AddFriend;
    }

    public string Token { set; get; }
    public string UserId { set; get; }
}
