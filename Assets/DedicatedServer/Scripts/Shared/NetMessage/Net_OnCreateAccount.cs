using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Net_OnCreateAccount : NetMessage
{
    public Net_OnCreateAccount()
    {
        OP = NetOP.OnCreateAccount;
    }

    public string UserId { set; get; }
    public byte Success { set; get; }
    public string Information { set; get; }
}
