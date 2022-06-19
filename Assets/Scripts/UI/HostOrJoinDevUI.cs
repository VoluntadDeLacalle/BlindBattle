using BasicTools.ButtonInspector;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostOrJoinDevUI : MonoBehaviour
{
    public string roomName = "LocalDevRoom";
    public string yourName = "CoolDev42";

    [ReadOnly]
    public string currentMode;

    [Button("Host", "Host")]
    public bool btn_Host;

    [Button("Join", "Join")]
    public bool btn_Join;

    [Button("Start Game [Host Only]", "StartGame")]
    public bool btn_StartGame;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Instance.networkRunner) {
            currentMode = NetworkManager.Instance.IsHost ? "Host" : "Client";
        } else
        {
            currentMode = "";
        }
    }

    public async void Host()
    {
        await NetworkManager.Instance.CreateRoom(roomName, yourName);
        PostRunnerCreation();
    }

    public async void Join()
    {
        await NetworkManager.Instance.JoinRoom(roomName, yourName);
        PostRunnerCreation();
    }

    void PostRunnerCreation()
    {
        RoundManager.Instance.RegisterRunnerCallbacks();
    }

    public void StartGame()
    {
        if (NetworkManager.Instance.IsHost)
        {
            RoundManager.Instance.StartRound();
        }
        else
        {
            Debug.LogWarning("Only the host can start the game");
        }
    }
}
