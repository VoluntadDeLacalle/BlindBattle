using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySceneManager : MonoBehaviour
{
    public TMP_InputField createRoomNameField;
    public Button createButton;
    public TMP_InputField joinRoomNameField;
    public Button joinButton;

    public TMP_Text messageText;

    public MenuPage roomScreen;
    public TMP_Text roomTitleText;
    public TMP_Text team1TitleText;
    public TMP_Text team2TitleText;
    public TMP_Text team1ListText;
    public TMP_Text team2ListText;


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
        var roomName = createRoomNameField.text.Trim();
        if (roomName.Length == 0)
        {
            return;
        }

        try
        {
            createButton.interactable = false;
            messageText.text = "Creating room...";
            await NetworkManager.Instance.CreateRoom(roomName);
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
        var roomName = joinRoomNameField.text.Trim();
        if (roomName.Length == 0)
        {
            return;
        }

        try
        {
            joinButton.interactable = false;
            messageText.text = "Joining room...";
            await NetworkManager.Instance.JoinRoom(roomName);
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
        roomTitleText.text = $"Room: {NetworkManager.Instance.networkRunner?.SessionInfo.Name}";
        roomScreen.Show();
    }

    public void LeaveRoom()
    {
        NetworkManager.Instance.networkRunner?.Shutdown();
    }
}
