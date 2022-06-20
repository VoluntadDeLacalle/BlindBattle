using BasicTools.ButtonInspector;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public bool singleTeam = false;
    public bool spectatorOnly = false;

    [Button("Reset Teams [Host Only]", "ResetTeams")]
    public bool btn_ResetTeams;

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

    public void ResetTeams()
    {
        NetworkGameState.Instance.ResetTeams(singleTeam);
    }

    void PostRunnerCreation()
    {
        RoundManager.Instance.RegisterRunnerCallbacks();
    }

    public void StartGame()
    {
        if (NetworkManager.Instance.IsHost)
        {
            RoundManager.Instance.StartRound(spectatorOnly);
        }
        else
        {
            Debug.LogWarning("Only the host can start the game");
        }
    }
}
