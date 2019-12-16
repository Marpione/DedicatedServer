
[System.Serializable]
public class Net_OnAddFriend : NetMessage
{
    public Net_OnAddFriend()
    {
        OP = NetOP.OnAddFriend;
    }

    public byte Success { set; get; }

    public Account FriendAccount { set; get; }
}
