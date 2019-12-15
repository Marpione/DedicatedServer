using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Net_LoginRequest : NetMessage
{
    public Net_LoginRequest()
    {
        OP = NetOP.LoginRequest;
    }

    public string UsernameOrEmail { get; set; }
    public string Password { get; set; }
}
