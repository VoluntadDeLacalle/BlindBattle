using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : SingletonMonoBehaviour<NetworkManager>, INetworkRunnerCallbacks
{
    public NetworkRunner networkRunnerPrefab;
    public NetworkUser networkUserPrefab;
    public NetworkGameState networkGameStatePrefab;

    [HideInInspector]
    public NetworkRunner networkRunner;

    [HideInInspector]
    public string localUserName;
    
    public bool IsHost => networkRunner?.GameMode == GameMode.Host;

    private new void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public async Task CreateRoom(string roomName, string userName)
    {
        // TODO: Check if name is taken
        var res = await InitializeNetworkRunner(roomName, userName, GameMode.Host);
        if (res.Ok)
        {
            Debug.Log("Joined as server");
        }
        else
        {
            Debug.Log("Failed to join");
            throw new Exception(res.ShutdownReason.ToString());
        }
    }

    public async Task JoinRoom(string roomName, string userName)
    {
        var res = await InitializeNetworkRunner(roomName, userName, GameMode.Client);
        if (res.Ok)
        {
            Debug.Log("Joined as client");
        }
        else
        {
            Debug.Log("Failed to join");
            throw new Exception(res.ShutdownReason.ToString());
        }
    }

    Task<StartGameResult> InitializeNetworkRunner(string sessionName, string userName, GameMode mode)
    {
        if (networkRunner)
        {
            DestroyImmediate(networkRunner.gameObject);
        }

        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network Runner";
        networkRunner.ProvideInput = true;
        networkRunner.AddCallbacks(this);

        localUserName = userName;

        Debug.Log("Trying to start..");
        return networkRunner.StartGame(new StartGameArgs
        {
            // other args...
            SessionName = sessionName,
            GameMode = mode,
            SessionProperties = { },
            DisableClientSessionCreation = true,
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        var networkUser = networkRunner.Spawn(networkUserPrefab, Vector3.zero, Quaternion.identity, player);

        if (player == runner.LocalPlayer && runner.GameMode == GameMode.Host)
        {
            var networkGameState = networkRunner.Spawn(networkGameStatePrefab, Vector3.zero, Quaternion.identity, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        //
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        networkRunner = null;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        networkRunner = null;
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        //
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        networkRunner = null;
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        //
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        //
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        //
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        //
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //
    }
}
