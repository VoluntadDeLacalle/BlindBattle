using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : SingletonMonoBehaviour<RoundManager>, INetworkRunnerCallbacks
{
    public int roundDuration = 60;

    public int currentRound => NetworkGameState.Instance.currentRound;

    private new void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        RegisterRunnerCallbacks();
        HelperUtilities.UpdateCursorLock(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkManager.Instance.networkRunner || !NetworkGameState.Instance)
        {
            return;
        }

        if (NetworkManager.Instance.IsHost)
        {
            if (NetworkGameState.Instance.gameTimer.Expired(NetworkManager.Instance.networkRunner))
            {
                EndRound();
            }
        }
    }

    public void RegisterRunnerCallbacks()
    {
        NetworkManager.Instance.networkRunner?.AddCallbacks(this);
    }

    public void StartRound()
    {
        if (!NetworkManager.Instance.IsHost)
        {
            return;
        }

        NetworkManager.Instance.networkRunner.SessionInfo.IsOpen = false;

        if (NetworkGameState.Instance.currentRound == 1)
        {
            NetworkGameState.Instance.ComputeTotalRounds();
        }

        MapGenerator.Instance.Generate();

        var team1 = NetworkGameState.Instance.team1;
        var team2 = NetworkGameState.Instance.team2;

        SpawnTeam(team1, 1);
        SpawnTeam(team2, 2);

        NetworkGameState.Instance.ResetTimer(roundDuration);
    }

    void SpawnTeam(NetworkArray<int> team, int teamNum)
    {
        var runner = NetworkManager.Instance.networkRunner;
        int substitutePlayerRef = -1;

        Transform fighterSpawnPoint;
        Randomizer<Transform> spectatorSpawnPoints;
        if (teamNum == 1)
        {
            fighterSpawnPoint = MapGenerator.Instance.curSkeleton.player1Spawn;
            spectatorSpawnPoints = MapGenerator.Instance.curSkeleton.team1SpecSpawnRandomizer;
        }
        else
        {
            fighterSpawnPoint = MapGenerator.Instance.curSkeleton.player2Spawn;
            spectatorSpawnPoints = MapGenerator.Instance.curSkeleton.team2SpecSpawnRandomizer;
        }

        // Spawning fighter
        int fighterPlayerRef = team.Get(currentRound - 1);
        if (fighterPlayerRef >= 0)
        {
            BasicSpawner.Instance.SpawnPlayer(runner, fighterPlayerRef, Player.PlayerRole.Fighter, fighterSpawnPoint);
        }
        else
        {
            substitutePlayerRef = SelectRandomValidPlayerFromTeam(team);
            if (substitutePlayerRef >= 0)
            {
                BasicSpawner.Instance.SpawnPlayer(runner, substitutePlayerRef, Player.PlayerRole.Fighter, fighterSpawnPoint);
            }
            else
            {
                Debug.Log("Not enough players");
            }
        }

        // Spawning spectators
        for (int i = 0; i < team.Length; i++)
        {
            // Skipping the fighter
            if (i != currentRound - 1)
            {
                int playerRef = team.Get(i);

                // Also skipping any substitute fighter
                if (playerRef >= 0 && playerRef != substitutePlayerRef)
                {
                    BasicSpawner.Instance.SpawnPlayer(runner, playerRef, Player.PlayerRole.Spectator, spectatorSpawnPoints.GetRandomItem());
                }
            }
        }
    }

    int SelectRandomValidPlayerFromTeam(NetworkArray<int> team)
    {
        List<int> validUsers = new List<int>();

        for (int i = 0; i < team.Length; i++)
        {
            int playerRef = team.Get(i);
            if (playerRef >= 0)
            {
                validUsers.Add(playerRef);
            }
        }

        if (validUsers.Count <= 0)
        {
            return -1;
        }

        var userRandomizer = new Randomizer<int>(validUsers);
        return userRandomizer.GetRandomItem();
    }

    public void EndRound()
    {
        if (!NetworkManager.Instance.IsHost)
        {
            return;
        }

        foreach (var kvp in BasicSpawner.Instance.spawnedCharacters)
        {
            var player = kvp.Value;
            if (player.playerRole == Player.PlayerRole.Fighter)
            {
                player.RPC_SwitchPlayerRole(Player.PlayerRole.Disabled);
            }
        }
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.networkRunner?.RemoveCallbacks(this);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //
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
        // TODO: Show error screen
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
        if (NetworkManager.Instance.networkRunner)
        {
            StartRound();
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //
    }
}
