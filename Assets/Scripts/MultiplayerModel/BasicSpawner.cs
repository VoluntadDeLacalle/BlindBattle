using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Player;

public class BasicSpawner : SingletonMonoBehaviour<BasicSpawner>, INetworkRunnerCallbacks
{
    public Player playerPrefab;
    public Dictionary<PlayerRef, Player> spawnedCharacters = new Dictionary<PlayerRef, Player>();

    public Player SpawnPlayer(NetworkRunner runner, PlayerRef playerRef, PlayerRole role) 
    {
        // TODO: Use spawn points and role to spawn players in the right spots

        //player.RawEncoded%runner.Config.Simulation.DefaultPlayers This code gets the current player's queue I suppose from the ref. It is then used to multiply the x to give it a unique position.
        Vector3 spawnPosition = new Vector3((playerRef.RawEncoded%runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
        Player player = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, playerRef);
        player.RPC_SwitchPlayerRole(role);

        spawnedCharacters.Add(playerRef, player);
        return player;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
    {
        if(spawnedCharacters.TryGetValue(playerRef, out Player player))
        {
            runner.Despawn(player.Object);
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
