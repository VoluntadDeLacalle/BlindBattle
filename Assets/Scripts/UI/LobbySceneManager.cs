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

    NetworkRunner networkRunner => NetworkManager.Instance.networkRunner;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        roomScreen.Show();

        if (networkRunner?.GameMode == GameMode.Host)
        {

        }
    }

    public void LeaveRoom()
    {
        networkRunner?.Shutdown();
    }

    public void RefreshTeams()
    {

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
