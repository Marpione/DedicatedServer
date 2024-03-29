﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Net_OnLoginRequest : NetMessage
{
    public Net_OnLoginRequest()
    {
        OP = NetOP.OnLoginRequest;
    }

    public byte Success { set; get; }
    public string Information { set; get; }

    public int ConnectionId { set; get; }
    public string UserId { set; get; }
    public string Token { set; get; }
}
