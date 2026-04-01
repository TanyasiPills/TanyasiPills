using UnityEngine;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Networker : MonoBehaviour
{
    public static Networker Instance { get; private set; } = null;

    public Lobby? lobby { get; private set; } = null;

    private FacepunchTransport transport;

    public TMP_Text log;

    public GameObject playerPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }

    public async void StartHost()
    {
        if (NetworkManager.Singleton.IsListening) return;

        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

        if (NetworkManager.Singleton.StartHost()) log.text = "started server";

        lobby = await SteamMatchmaking.CreateLobbyAsync(10);
    }

    public void Disconnect()
    {
        lobby?.Leave();

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.Shutdown();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void StartClient(SteamId id)
    {
        if (NetworkManager.Singleton.IsListening) return;

        transport.targetSteamId = id;

        if (NetworkManager.Singleton.StartClient()) log.text = "started client";
    }

    private void SpawnForClient(ulong clientId)
    {
        GameObject obj = Instantiate(playerPrefab, new Vector3(0, 4, 0), Quaternion.identity);
        obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }


    private void OnServerStarted()
    {
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            SpawnForClient(clientId);
        }
    }






    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        this.lobby = lobby;
        await SteamMatchmaking.JoinLobbyAsync(lobby.Id);
    }

    private void OnLobbyGameCreated(Lobby arg1, uint arg2, ushort arg3, SteamId arg4)
    {
    }

    private void OnLobbyInvite(Friend arg1, Lobby arg2)
    {
    }

    private void OnLobbyMemberLeave(Lobby arg1, Friend arg2)
    {
    }

    private void OnLobbyMemberJoined(Lobby arg1, Friend arg2)
    {
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost) return;

        this.lobby = lobby;
        StartClient(lobby.Owner.Id);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError($"{result}");
            return;
        }

        lobby.SetFriendsOnly();
        lobby.SetData("name", "i will wack your pp");
        lobby.SetJoinable(true);
    }
}
