using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySceneManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public string arenaSceneName = "ArenaScene";

    public TMP_InputField createUserNameField;
    public TMP_InputField createRoomNameField;
    public Button createButton;
    public TMP_InputField joinUserNameField;
    public TMP_InputField joinRoomNameField;
    public Button joinButton;

    public TMP_Text messageText;

    public MenuPage roomScreen;
    public TMP_Text roomTitleText;
    public TMP_Text team1TitleText;
    public TMP_Text team2TitleText;
    public TMP_Text team1ListText;
    public TMP_Text team2ListText;
    public TMP_Text clientMessageText;

    public List<GameObject> HostOnlyItems;
    public List<GameObject> ClientOnlyItems;

    NetworkRunner networkRunner => NetworkManager.Instance.networkRunner;

    private string originalClientMessageText;

    // Start is called before the first frame update
    void Start()
    {
        originalClientMessageText = clientMessageText.text;
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkGameState.Instance)
        {
            RefreshTeamList(NetworkGameState.Instance.team1, team1ListText);
            RefreshTeamList(NetworkGameState.Instance.team2, team2ListText);
        }
    }

    void RefreshTeamList(NetworkArray<int> team, TMP_Text teamList)
    {
        teamList.text = "";
        for (int i=0; i<team.Length; i++)
        {
            var playerId = team.Get(i);
            if (playerId >= 0 && NetworkUser.allNetworkUsers.ContainsKey(playerId))
            {
                teamList.text += NetworkUser.allNetworkUsers[playerId].userName;
                teamList.text += "\n";
            }
        }
    }

    public async void CreateRoom()
    {
        var userName = createUserNameField.text.Trim();
        var roomName = createRoomNameField.text.Trim();
        if (userName.Length == 0 || roomName.Length == 0)
        {
            return;
        }

        try
        {
            createButton.interactable = false;
            messageText.text = "Creating room...";
            await NetworkManager.Instance.CreateRoom(roomName, userName);
            messageText.text = "Room created successfully!";

            MoveToRoom();
        }
        catch (Exception err)
        {
            messageText.text = $"Error: {err.Message}";
        }
        finally
        {
            createButton.interactable = true;
        }
    }

    public async void JoinRoom()
    {
        var userName = joinUserNameField.text.Trim();
        var roomName = joinRoomNameField.text.Trim();
        if (userName.Length == 0 || roomName.Length == 0)
        {
            return;
        }

        try
        {
            joinButton.interactable = false;
            messageText.text = "Joining room...";
            await NetworkManager.Instance.JoinRoom(roomName, userName);
            messageText.text = "Joined room successfully!";

            MoveToRoom();
        }
        catch (Exception err)
        {
            messageText.text = $"Error: {err.Message}";
        }
        finally
        {
            joinButton.interactable = true;
        }
    }

    public void MoveToRoom()
    {
        networkRunner?.RemoveCallbacks(this);
        networkRunner?.AddCallbacks(this);

        roomTitleText.text = $"Room: {networkRunner?.SessionInfo.Name ?? "N/A"}";
        clientMessageText.text = originalClientMessageText;
        for (int i = 0; i < HostOnlyItems.Count; i++)
        {
            HostOnlyItems[i].SetActive(networkRunner?.GameMode == GameMode.Host);
        }
        for (int i = 0; i < ClientOnlyItems.Count; i++)
        {
            ClientOnlyItems[i].SetActive(networkRunner?.GameMode == GameMode.Client);
        }

        roomScreen.Show();
    }

    public void LeaveRoom()
    {
        networkRunner?.Shutdown();
    }

    public void ResetTeams()
    {
        NetworkGameState.Instance?.ResetTeams();
    }

    public void StartGame()
    {
        if (networkRunner.GameMode != GameMode.Host)
        {
            return;
        }

        networkRunner.SetActiveScene(arenaSceneName);
    }

    void OnDestroy()
    {
        networkRunner?.RemoveCallbacks(this);
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
        clientMessageText.text = "Disconnected from server";
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
