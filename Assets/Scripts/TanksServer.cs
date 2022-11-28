using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class TanksServer : SocketManager
{
    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        base.OnConnecting(connection, info);
        connection.Accept();
        Debug.Log($"{info.Identity.SteamId} is connecting");
    }

    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);
        Debug.Log($"{info.Identity.SteamId} has connected");
    }
    
    public override void OnDisconnected( Connection connection, ConnectionInfo data )
    {
        Debug.Log( $"{data.Identity.SteamId} is out of here" );
    }

    public override void OnMessage( Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel )
    {
        Debug.Log( $"We got a message from {identity.SteamId}!" );

        // Send it right back
        SteamManager.Instance.RelaySocketMessageReceived(data, size, connection.Id);
    }
}
