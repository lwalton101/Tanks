using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;
    [SerializeField] private GameObject playerScrollViewContent;
    private Dictionary<Friend, GameObject> playerListingDictionary = new Dictionary<Friend, GameObject>();
    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            Debug.LogError("There were 2 Lobby UI Managers!");
            return;
        }

        Instance = this;
        
        Lobby lobby = SteamManager.Instance.lobby;
        
        foreach (Friend player in lobby.Members)
        {
            AddPlayerToListing(player);
        }   
        
        
        
    }

    public void AddPlayerToListing(Friend player)
    {
        GameObject playerLobbyListingPrefab = Resources.Load<GameObject>("PlayerLobbyListing");
        GameObject playerLobbyListing = Instantiate(playerLobbyListingPrefab, playerScrollViewContent.transform);
        PlayerLobbyListingContainer playerLobbyListingContainer =
            playerLobbyListing.GetComponent<PlayerLobbyListingContainer>();

        playerLobbyListingContainer.player = player;
        playerLobbyListingContainer.SetListingInfo();
        playerListingDictionary.Add(player, playerLobbyListing);
    }

    public void RemovePlayerFromListing(Friend player)
    {
        Destroy(playerListingDictionary[player]);
        playerListingDictionary.Remove(player);
    }

    public void LeaveLobby()
    {
        SteamManager.Instance.LeaveLobby();
    }
}
