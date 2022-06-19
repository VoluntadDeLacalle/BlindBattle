using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameState : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static NetworkGameState Instance;

    [Networked, Capacity(GameManager.MaxPlayersPerTeam)]
    public NetworkArray<int> team1 { get; } = MakeInitializer(new int[] { -1, -1, -1, -1, -1 });
    [Networked, Capacity(GameManager.MaxPlayersPerTeam)]
    public NetworkArray<int> team2 { get; } = MakeInitializer(new int[] { -1, -1, -1, -1, -1 });

    private void Awake()
    {
        if (NetworkManager.Instance.IsHost)
        {
            if (Instance != null)
            {
                NetworkManager.Instance.networkRunner.Despawn(Instance.Object);
            }
        }

        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetTeams();
        NetworkManager.Instance.networkRunner.AddCallbacks(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetTeams()
    {
        if (!NetworkManager.Instance.IsHost)
        {
            return;
        }

        ClearTeam(team1);
        ClearTeam(team2);

        var users = FindObjectsOfType<NetworkUser>();
        Debug.Log("Users Length: " + users.Length);
        var userRandomizer = new Randomizer<NetworkUser>(users);

        for (int i = 0; i < userRandomizer.items.Count; i++)
        {
            AddPlayerToRandomTeam(userRandomizer.GetRandomItem().belongsTo);
        }
    }

    void ClearTeam(NetworkArray<int> team)
    {
        for (int i = 0; i < team.Length; i++)
        {
            team.Set(i, -1);
        }
    }

    void RemovePlayerFromTeam(NetworkArray<int> team, PlayerRef playerRef)
    {
        for (int i = 0; i < team.Length; i++)
        {
            if (team.Get(i) == playerRef)
            {
                team.Set(i, -1);
            }
        }
    }

    int GetPlayerTeamNumber(PlayerRef playerRef)
    {
        // TODO: Use a dictionary

        for (int i = 0; i < team1.Length; i++)
        {
            if (team1.Get(i) == playerRef)
            {
                return 1;
            }
        }

        for (int i = 0; i < team2.Length; i++)
        {
            if (team2.Get(i) == playerRef)
            {
                return 2;
            }
        }

        return -1;
    }

    void AddPlayerToRandomTeam(PlayerRef playerRef)
    {
        Debug.Log(playerRef);

        // THIS IS SOOO UGLY!!
        int existsInTeam = GetPlayerTeamNumber(playerRef);
        if (existsInTeam != -1)
        {
            Debug.Log("Exists in team already");
            return;
        }

        for (int i = 0; i < GameManager.MaxPlayersPerTeam; i++)
        {
            if (team1.Get(i) < 0)
            {
                Debug.Log("Assigning to team 1");
                team1.Set(i, playerRef);
                return;
            }

            if (team2.Get(i) < 0)
            {
                Debug.Log("Assigning to team 2");
                team2.Set(i, playerRef);
                return;
            }
        }
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.networkRunner.RemoveCallbacks(this);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (NetworkManager.Instance.IsHost)
        {
            Debug.Log("Player joined NGS: " + player);
            AddPlayerToRandomTeam(player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (NetworkManager.Instance.IsHost)
        {
            RemovePlayerFromTeam(team1, player);
            RemovePlayerFromTeam(team2, player);
        }
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
        //
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        //
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        //
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        //
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        //
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
