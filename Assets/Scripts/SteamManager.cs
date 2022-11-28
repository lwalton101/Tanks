using System;
using TMPro;
using System.Linq;
using System.Text;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;

public class SteamManager : MonoBehaviour
{
    private static uint gameID = 480;
    public static SteamManager Instance;

    private bool activeSteamSocketServer = false;
    private bool activeSteamSocketConnection = false;

    private TanksServer tanksSocketManager;
    private TanksConnectionManager tanksConnectionManager;
    
    private bool isAppQuitting = false;
    // Start is called before the first frame update
    void Awake()
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
        
        SteamNetworkingUtils.InitRelayNetworkAccess();
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
        Debug.Log("Creating Socket Server and joining Socket Server");
        tanksSocketManager = SteamNetworkingSockets.CreateRelaySocket<TanksServer>();
        tanksConnectionManager = SteamNetworkingSockets.ConnectRelay<TanksConnectionManager>(SteamClient.SteamId);
        activeSteamSocketServer = true;
        activeSteamSocketServer = true;
    }

    private void JoinTanksSocketServer(SteamId lobbySteamId)
    {
        Debug.Log("Joining socket server");
        tanksConnectionManager = SteamNetworkingSockets.ConnectRelay<TanksConnectionManager>(lobbySteamId);
        activeSteamSocketServer = false;
        activeSteamSocketConnection = true;
    }
    
    private void LeaveTanksSocketServer()
    {
        activeSteamSocketServer = false;
        activeSteamSocketConnection = false;
        try
        {
            // Shutdown connections/sockets. I put this in try block because if player 2 is leaving they don't have a socketManager to close, only connection
            tanksSocketManager.Close();
            tanksConnectionManager.Close();
        }
        catch
        {
            Debug.Log("Error closing socket server / connection manager");
        }
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
    
    public bool SendMessageToSocketServer(string message)
    {
        byte[] messageToSendBytes = Encoding.ASCII.GetBytes(message);
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
        foreach (GameObject child in transform)
        {
            Destroy(child);
        }

        GameObject lobbyPrefab = Resources.Load<GameObject>("LobbyListing");
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
        foreach (var lobby in lobbies)
        {
            GameObject lobbyListing = Instantiate(lobbyPrefab, contentPanel.transform);
            lobbyListing.GetComponentInChildren<TextMeshProUGUI>().text = lobby.MaxMembers + "'s Lobby";
        }
    }
    

    public void CreateLobby()
    {
        SceneManager.LoadScene(1);
        CreateTanksSocketServer();
    }
    
    
}
