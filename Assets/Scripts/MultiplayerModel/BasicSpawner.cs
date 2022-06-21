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
    public Player redPlayerPrefab;
    public Player bluePlayerPrefab;
    public SpectatorPlayer spectatorPlayerPrefab;
    public Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public Player SpawnPlayer(NetworkRunner runner, PlayerRef playerRef, PlayerRole role, Transform spawnPoint) 
    {
        Player playerToSpawn = NetworkGameState.Instance.GetPlayerTeamNumber(playerRef) == 1 ? redPlayerPrefab : bluePlayerPrefab;

        Player player = runner.Spawn(playerToSpawn, spawnPoint.position, spawnPoint.rotation, playerRef, (_, obj) => {
            var newPlayer = obj.GetComponent<Player>();
            newPlayer.TeleportTo(spawnPoint);
            newPlayer.RPC_SwitchPlayerRole(role);
        });

        spawnedCharacters[playerRef] = player.Object;
        return player;
    }


    public async void SpawnAfterDelay(PlayerRef playerRef, PlayerRole curPlayerRole, float delay)
    {
        await Task.Delay(TimeSpan.FromSeconds(delay));

        // Randomizing respawn location to prevent spawn killing
        Transform spawnPoint;
        if (UnityEngine.Random.Range(0, 10) < 5)
        {
            spawnPoint = MapGenerator.Instance.curSkeleton.player1Spawn;
        } else
        {
            spawnPoint = MapGenerator.Instance.curSkeleton.player2Spawn;
        }

        Debug.Log("Respawning!");
        var teamNum = NetworkGameState.Instance.GetPlayerTeamNumber(playerRef);
        SpawnPlayer(NetworkManager.Instance.networkRunner, playerRef, curPlayerRole, spawnPoint);
    }

    public SpectatorPlayer SpawnSpectatorPlayer(NetworkRunner runner, PlayerRef playerRef, Transform spawnPoint)
    {
        SpectatorPlayer spectatorPlayer = runner.Spawn(spectatorPlayerPrefab, spawnPoint.position, spawnPoint.rotation, playerRef, (_, obj) =>
        {
            var newPlayer = obj.GetComponent<SpectatorPlayer>();
            newPlayer.spawnPoint = spawnPoint;
        });

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
