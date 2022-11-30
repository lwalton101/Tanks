using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class LobbyListingContainer : MonoBehaviour
{
    public Lobby lobby;

    public void JoinLobby()
    {
        SteamManager.Instance.JoinLobby(lobby);
    }
}
