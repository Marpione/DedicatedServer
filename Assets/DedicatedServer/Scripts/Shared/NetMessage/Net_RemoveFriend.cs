
[System.Serializable]
public class Net_RemoveFriend : NetMessage
{
    public Net_RemoveFriend()
    {
        OP = NetOP.RemoveFriend;
    }

    public string Token { set; get; }
    public string UserId { set; get; }

}
