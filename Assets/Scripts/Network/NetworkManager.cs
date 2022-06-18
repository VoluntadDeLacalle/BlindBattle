using Fusion;
using Fusion.Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : SingletonMonoBehaviour<NetworkManager>
{
    public NetworkRunner networkRunnerPrefab;

    [HideInInspector]
    public NetworkRunner networkRunner;

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

    public async Task CreateRoom(string name)
    {
        // TODO: Check if name is taken
        var res = await InitializeNetworkRunner(name, GameMode.Host);
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

    public async Task JoinRoom(string name)
    {
        var res = await InitializeNetworkRunner(name, GameMode.Client);
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

    Task<StartGameResult> InitializeNetworkRunner(string sessionName, GameMode mode)
    {
        if (networkRunner)
        {
            DestroyImmediate(networkRunner);
        }

        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network Runner";

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
}
