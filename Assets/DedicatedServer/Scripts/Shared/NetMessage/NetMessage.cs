
public static class NetOP
{
    public const int None = 0;
    public const int CreateAccount = 1;
    public const int LoginRequest = 2;
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
