
public static class NetOP
{
    public const int None = 0;
    public const int CreateAccount = 1;
    public const int LoginRequest = 2;

    public const int OnCreateAccount = 3;
    public const int OnLoginRequest = 4;

    public const int AddFriend = 5;
    public const int RemoveFriend = 6;
    public const int RequestFriend = 7;

    public const int OnAddFriend = 8;
    public const int OnRequestFriend = 9;
}

[System.Serializable]
public abstract class NetMessage
{
    public byte OP { get; set; }

    public NetMessage()
    {
        OP = NetOP.None;
    }
}
