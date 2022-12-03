using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class TanksConnectionManager : ConnectionManager
{
    public override void OnConnected(ConnectionInfo info)
    {
        base.OnConnected(info);
        Debug.Log($"[Client] Connected to {info.Identity.SteamId}");
    }

    public override void OnConnecting(ConnectionInfo info)
    {
        base.OnConnecting(info);
        Debug.Log($"[Client] Connecting to ${info.Identity.SteamId}");
    }

    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        Debug.Log($"[Client] Disconnected from {info.Identity.SteamId}");
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        // Message received from socket server, delegate to method for processing
        SteamManager.Instance.ProcessMessageFromSocketServer(data, size);
        Debug.Log($"[Client] Connection Got A Message from server");
    }
}
