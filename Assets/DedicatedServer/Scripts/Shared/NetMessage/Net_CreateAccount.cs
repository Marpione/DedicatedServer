
[System.Serializable]
public class Net_CreateAccount : NetMessage
{
    public Net_CreateAccount()
    {
        OP = NetOP.CreateAccount;
    }

    public string UserId { get; set; }
    public string Email { get; set; }
}
