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
        }
        catch (Exception err)
        {
            createButton.interactable = true;
            messageText.text = $"Error: {err.Message}";
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
        }
        catch (Exception err)
        {
            joinButton.interactable = true;
            messageText.text = $"Error: {err.Message}";
        }
    }
}
