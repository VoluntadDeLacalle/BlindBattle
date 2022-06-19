using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : SingletonMonoBehaviour<RoundManager>
{
    public int currentRound => NetworkGameState.Instance.currentRound;

    private new void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager.Instance.networkRunner)
        {
            StartRound();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartRound()
    {
        if (!NetworkManager.Instance.IsHost)
        {
            return;
        }

        NetworkManager.Instance.networkRunner.SessionInfo.IsOpen = false;

        if (NetworkGameState.Instance.currentRound == 1)
        {
            NetworkGameState.Instance.ComputeTotalRounds();
        }

        var team1 = NetworkGameState.Instance.team1;
        var team2 = NetworkGameState.Instance.team2;

        SpawnTeam(team1);
        SpawnTeam(team2);
    }

    void SpawnTeam(NetworkArray<int> team)
    {
        var runner = NetworkManager.Instance.networkRunner;
        int substitutePlayerRef = -1;

        // Spawning fighter
        int fighterPlayerRef = team.Get(currentRound - 1);
        if (fighterPlayerRef >= 0)
        {
            BasicSpawner.Instance.SpawnPlayer(runner, fighterPlayerRef, Player.PlayerRole.Fighter);
        }
        else
        {
            substitutePlayerRef = SelectRandomValidPlayerFromTeam(team);
            if (substitutePlayerRef >= 0)
            {
                BasicSpawner.Instance.SpawnPlayer(runner, substitutePlayerRef, Player.PlayerRole.Fighter);
            }
            else
            {
                Debug.Log("Not enough players");
            }
        }

        // Spawning spectators
        for (int i = 0; i < team.Length; i++)
        {
            // Skipping the fighter
            if (i != currentRound - 1)
            {
                int playerRef = team.Get(i);

                // Also skipping any substitute fighter
                if (playerRef >= 0 && playerRef != substitutePlayerRef)
                {
                    BasicSpawner.Instance.SpawnPlayer(runner, playerRef, Player.PlayerRole.Spectator);
                }
            }
        }
    }

    int SelectRandomValidPlayerFromTeam(NetworkArray<int> team)
    {
        List<int> validUsers = new List<int>();

        for (int i = 0; i < team.Length; i++)
        {
            int playerRef = team.Get(i);
            if (playerRef >= 0)
            {
                validUsers.Add(playerRef);
            }
        }

        if (validUsers.Count <= 0)
        {
            return -1;
        }

        var userRandomizer = new Randomizer<int>(validUsers);
        return userRandomizer.GetRandomItem();
    }

    public void EndRound()
    {
        if (!NetworkManager.Instance.IsHost)
        {
            return;
        }

        foreach (var kvp in BasicSpawner.Instance.spawnedCharacters)
        {
            var player = kvp.Value;
            if (player.playerRole == Player.PlayerRole.Fighter)
            {
                player.SwitchPlayerRole(Player.PlayerRole.Disabled);
            }
        }

        // TODO: Show Round End Screen
    }
}
