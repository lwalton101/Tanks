using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;
    [SerializeField] private GameObject playerScrollViewContent;
    [SerializeField] private GameObject chatScrollViewContent;
    [SerializeField] private TextMeshProUGUI chatInput;

    [SerializeField] private Button startButton;
    
    private Dictionary<SteamId, PlayerLobbyListingContainer> playerListingDictionary = new();
    private void Awake()
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
    
    private void Update(){
        if (SteamManager.Instance.lobby.IsOwnedBy(SteamClient.SteamId) && SteamManager.Instance.IsEveryoneReady)
        {
            startButton.interactable = true;
        }
        else
        {
            startButton.interactable = false;
        }
        
    }

    public void AddPlayerToListing(Friend player)
    {
        GameObject playerLobbyListingPrefab = Resources.Load<GameObject>("PlayerLobbyListing");
        GameObject playerLobbyListing = Instantiate(playerLobbyListingPrefab, playerScrollViewContent.transform);
        PlayerLobbyListingContainer playerLobbyListingContainer =
            playerLobbyListing.GetComponent<PlayerLobbyListingContainer>();

        SteamManager.Instance.playerReadyDict.Add(player.Id, false);
        playerListingDictionary.Add(player.Id, playerLobbyListingContainer);
        playerLobbyListingContainer.player = player;
        playerLobbyListingContainer.SetListingInfo();
        
    }

    public void RemovePlayerFromListing(Friend player)
    {
        Destroy(playerListingDictionary[player.Id].gameObject);
        playerListingDictionary.Remove(player.Id);
    }

    public void UpdatePlayerListing(Friend player)
    {
        playerListingDictionary[player.Id].SetListingInfo();
    }

    public void LeaveLobby()
    {
        SteamManager.Instance.LeaveLobby();
    }

    public void ChatMessageRecieve(Friend player, string message)
    {
        string newMessage = $"<{player.Name}> {message}";
        AddChatMessage(newMessage);
    }

    public void SendChatMessage()
    {
        SteamManager.Instance.SendLobbyChatMessage(chatInput.text);
        chatInput.text = "";
    }

    private void AddChatMessage(string message)
    {
        GameObject chatMessagePrefab = Resources.Load<GameObject>("ChatMessage");
        
        var chatMessage = Instantiate(chatMessagePrefab, chatScrollViewContent.transform);
        chatMessage.GetComponent<TextMeshProUGUI>().text = message;
    }

    public void SendReadyMessage()
    {
        SteamManager.Instance.SendLobbyReady();
    }

    public void StartLobbyGame()
    {
        SteamManager.Instance.StartLobbyGame();
    }
}
