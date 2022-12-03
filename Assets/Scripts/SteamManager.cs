using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class SteamManager : MonoBehaviour
{
    private readonly uint gameID = 480;
    public static SteamManager Instance;

    private bool activeSteamSocketServer = false;
    private bool activeSteamSocketConnection = false;
    private bool inLobby = false;

    private TanksServer tanksSocketManager;
    private TanksConnectionManager tanksConnectionManager;

    public Lobby lobby;
    public Dictionary<SteamId, bool> playerReadyDict = new();

    private bool isAppQuitting = false;
    
    [Header("Ready Colors")] 
    [SerializeField] public Color readyColor = Color.red;
    [SerializeField] public Color notReadyColor = Color.green;

    public bool IsEveryoneReady
    {
        get
        {
            foreach (var id in playerReadyDict.Keys)
            {
                if (!playerReadyDict[id])
                {
                    return false;
                }
            }

            return true;
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        DontDestroyOnLoad(this);
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            Debug.LogError("There were 2 steam managers!");
            return;
        }

        Instance = this;
        
        SteamClient.Init(gameID);
        Debug.Log($"Steam user {SteamClient.Name} initiated");
        
        SteamNetworkingUtils.InitRelayNetworkAccess();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnChatMessage += OnChatMessage;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
    }


    // Update is called once per frame
    void Update()
    {
        SteamClient.RunCallbacks();
        
        try
        {
            if (activeSteamSocketServer)
            {
                tanksSocketManager.Receive();
            }
            if (activeSteamSocketConnection)
            {
                tanksConnectionManager.Receive();
            }
        }
        catch
        {
            Debug.Log("Error receiving data on socket/connection");
        }
    }

    private void CreateTanksSocketServer()
    {
        Debug.Log("Creating Socket Server");
        tanksSocketManager = SteamNetworkingSockets.CreateRelaySocket<TanksServer>();
        //tanksConnectionManager = SteamNetworkingSockets.ConnectRelay<TanksConnectionManager>(SteamClient.SteamId);
        activeSteamSocketServer = true;
        lobby.SetGameServer(SteamClient.SteamId);
    }

    private void JoinTanksSocketServer(SteamId lobbySteamId)
    {
        Debug.Log("Joining socket server");
        tanksConnectionManager = SteamNetworkingSockets.ConnectRelay<TanksConnectionManager>(lobbySteamId);
        activeSteamSocketConnection = true;
    }
    
    private void LeaveTanksSocketServer()
    {
        if (activeSteamSocketServer)
        {
            tanksSocketManager.Close();
            Debug.Log("Closed Server");
        }
        if (activeSteamSocketConnection)
        {
            tanksConnectionManager.Close();
            Debug.Log("Closed connection");
        }
        activeSteamSocketServer = false;
        activeSteamSocketConnection = false;
    }
    
    public void RelaySocketMessageReceived(IntPtr message, int size, uint connectionSendingMessageId)
    {
        try
        {
            // Loop to only send messages to socket server members who are not the one that sent the message
            for (int i = 0; i < tanksSocketManager.Connected.Count; i++)
            {
                if (tanksSocketManager.Connected[i].Id != connectionSendingMessageId)
                {
                    Result success = tanksSocketManager.Connected[i].SendMessage(message, size);
                    if (success != Result.OK)
                    {
                        Result retry = tanksSocketManager.Connected[i].SendMessage(message, size);
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Unable to relay socket server message");
        }
    }
    
    public bool SendMessageToSocketServer(Message message)
    {
        string messageString = message.ToString();
        byte[] messageToSendBytes = Encoding.ASCII.GetBytes(message.ToString());
        try
        {
            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = messageToSendBytes.Length;
            IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
            System.Runtime.InteropServices.Marshal.Copy(messageToSendBytes, 0, intPtrMessage, sizeOfMessage);
            Result success = tanksConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
            if (success == Result.OK)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                return true;
            }
            else
            {
                // RETRY
                Result retry = tanksConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                if (retry == Result.OK)
                {
                    return true;
                }
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("Unable to send message to socket server");
            return false;
        }
    }

    public void ProcessMessageFromSocketServer(IntPtr messageIntPtr, int dataBlockSize)
    {
        try
        {
            byte[] message = new byte[dataBlockSize];
            System.Runtime.InteropServices.Marshal.Copy(messageIntPtr, message, 0, dataBlockSize);
            string messageString = System.Text.Encoding.UTF8.GetString(message);

            Debug.Log($"Message Recieved is {messageString}");

        }
        catch
        {
            Debug.Log("Unable to process message from socket server");
        }
    }
    private void OnApplicationQuit()
    {
        isAppQuitting = true;
        SteamClient.Shutdown();
    }

    private void OnDisable()
    {
        if(!isAppQuitting)
            Debug.LogError("Steam Manager should not be disabled!");
    }

    private void OnDestroy()
    {
        if(!isAppQuitting)
            Debug.LogError("Steam Manager should not be destroyed!");
    }

    public Friend GetFriendByName(String name)
    {
        return SteamFriends.GetFriends().First(friend => friend.Name == name);
    }

    public void SetupFriendsPanel(GameObject lobbyPanel)
    {
        GameObject steamFriendPrefab = Resources.Load<GameObject>("SteamFriend");
        foreach (var friend in SteamFriends.GetFriends().Where(friend => friend.IsOnline))
        {
            GameObject steamFriendObject = Instantiate(steamFriendPrefab, lobbyPanel.transform);
            steamFriendObject.GetComponentInChildren<TextMeshProUGUI>().text = friend.Name;
            Image image = steamFriendObject.GetComponentInChildren<Image>();

            Texture2D profilePicture = new Texture2D(256, 256);
            
        }
    }

    public async void RefreshLobbies(GameObject contentPanel)
    {
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject lobbyPrefab = Resources.Load<GameObject>("LobbyListing");
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithKeyValue("test", "data").RequestAsync();
        if(lobbies == null)
            return;
        foreach (var lobby in lobbies)
        {
            GameObject lobbyListing = Instantiate(lobbyPrefab, contentPanel.transform);
            lobbyListing.GetComponentInChildren<TextMeshProUGUI>().text = lobby.GetData("name") + "'s Lobby";
            lobbyListing.GetComponent<LobbyListingContainer>().lobby = lobby;
        }
    }
    

    public void CreateLobby()
    {
        SteamMatchmaking.CreateLobbyAsync(4);
        inLobby = true;
    }
    
    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        Debug.Log("Lobby Created");
        lobby.SetPublic();
        lobby.SetData("test", "data");
        lobby.SetData("name", SteamClient.Name);
    }
    
    private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        Debug.Log($"Someone left the lobby with name {friend.Name}");
        LobbyUIManager.Instance.RemovePlayerFromListing(friend);
        playerReadyDict.Remove(friend.Id);
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"Someone joined the lobby with name {friend.Name}");
        LobbyUIManager.Instance.AddPlayerToListing(friend);
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        Debug.Log("Lobby joined");
        this.lobby = lobby;
        SceneManager.LoadScene(1);
    }

    public void JoinLobby(Lobby lobby)
    {
        SteamMatchmaking.JoinLobbyAsync(lobby.Id);
    }

    public void LeaveLobby()
    {
        lobby.Leave(); 
        
        Debug.Log("Leaving Lobby");
        SceneManager.LoadScene(0);
        playerReadyDict.Clear();
    }

    public void SendLobbyChatMessage(string message)
    {
        lobby.SendChatString($"[{LobbyPacketHeader.Chat.ToString()}]" + message);
    }
    
    //TODO: Fix "]" not sending
    private void OnChatMessage(Lobby lobby, Friend player, string message)
    {
        var splitMessage = message.Split("]");
        LobbyPacketHeader header = Enum.Parse<LobbyPacketHeader>(splitMessage[0].Replace("[", ""));
        var data = String.Join("", splitMessage.Skip(1).ToArray());
        switch (header)
        {
            case LobbyPacketHeader.Chat:
                LobbyUIManager.Instance.ChatMessageRecieve(player, data);
                break;
            case LobbyPacketHeader.Ready:
                Debug.Log("Recieved Lobby Ready Message");
                OnReady(player);
                break;
        }
        
    }

    public void SendLobbyReady()
    {
        lobby.SendChatString($"[{LobbyPacketHeader.Ready}]");
        Debug.Log("Sent ready packet");
    }
    private void OnReady(Friend player)
    {
        playerReadyDict[player.Id] = !playerReadyDict[player.Id];
        LobbyUIManager.Instance.UpdatePlayerListing(player);
    }

    public void StartLobbyGame()
    {
        if (lobby.IsOwnedBy(SteamClient.SteamId))
        {
            CreateTanksSocketServer();
        }
    }
    
    private void OnLobbyGameCreated(Lobby lobby, uint arg2, ushort arg3, SteamId lobbyId)
    {
        JoinTanksSocketServer(lobbyId);
        LeaveLobby();
        SceneManager.LoadScene(2);
    }
}
