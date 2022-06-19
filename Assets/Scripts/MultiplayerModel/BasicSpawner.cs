using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Player;

public class BasicSpawner : SingletonMonoBehaviour<BasicSpawner>, INetworkRunnerCallbacks
{
    public Randomizer<Player> playerPrefabs;
    public SpectatorPlayer spectatorPlayerPrefab;
    public Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public Player SpawnPlayer(NetworkRunner runner, PlayerRef playerRef, PlayerRole role, Transform spawnPoint) 
    {
        Player player = runner.Spawn(playerPrefabs.GetRandomItem(), spawnPoint.position, spawnPoint.rotation, playerRef, (_, obj) => {
            obj.GetComponent<Player>().RPC_SwitchPlayerRole(role);
        });

        spawnedCharacters[playerRef] = player.Object;
        return player;
    }


    public async void SpawnAfterDelay(PlayerRef playerRef, PlayerRole curPlayerRole, float delay)
    {
        await Task.Delay(TimeSpan.FromSeconds(delay));

        Debug.Log("Respawning!");
        var teamNum = NetworkGameState.Instance.GetPlayerTeamNumber(playerRef);
        if (teamNum == 1)
        {
            SpawnPlayer(NetworkManager.Instance.networkRunner, playerRef, curPlayerRole, MapGenerator.Instance.curSkeleton.player1Spawn);
        }
        else if (teamNum == 2)
        {
            SpawnPlayer(NetworkManager.Instance.networkRunner, playerRef, curPlayerRole, MapGenerator.Instance.curSkeleton.player2Spawn);
        }
    }

    public SpectatorPlayer SpawnSpectatorPlayer(NetworkRunner runner, PlayerRef playerRef, Transform spawnPoint)
    {
        SpectatorPlayer spectatorPlayer = runner.Spawn(spectatorPlayerPrefab, spawnPoint.position, spawnPoint.rotation, playerRef);

        spawnedCharacters[playerRef] = spectatorPlayer.Object;
        return spectatorPlayer;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
    {
        if(spawnedCharacters.TryGetValue(playerRef, out NetworkObject obj))
        {
            runner.Despawn(obj);
            spawnedCharacters.Remove(playerRef);
        }
    }

    private void Start()
    {
        playerPrefabs.Shuffle();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    //private void OnGUI()
    //{
    //    if (networkRunner == null)
    //    {
    //        if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
    //        {
    //            StartGame(GameMode.Host);
    //        }
    //        if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
    //        {
    //            StartGame(GameMode.Client);
    //        }
    //    }
    //}

    //async void StartGame(GameMode mode)
    //{
    //    networkRunner = gameObject.AddComponent<NetworkRunner>();
    //    networkRunner.ProvideInput = true;

    //    await networkRunner.StartGame(new StartGameArgs()
    //    {
    //        GameMode = mode,
    //        SessionName = "TestRoom",
    //        Scene = SceneManager.GetActiveScene().buildIndex,
    //        SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
    //    });
    //}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //
    }
}
