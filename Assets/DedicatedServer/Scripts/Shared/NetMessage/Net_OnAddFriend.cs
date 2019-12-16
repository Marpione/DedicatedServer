
[System.Serializable]
public class Net_OnAddFriend : NetMessage
{
    public Net_OnAddFriend()
    {
        OP = NetOP.OnAddFriend;
    }

    public Account FriendAccount { set; get; }
}
