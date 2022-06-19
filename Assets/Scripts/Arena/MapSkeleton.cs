using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSkeleton : NetworkBehaviour
{
    public Transform player1Spawn;
    public Transform player2Spawn;

    public Transform team1SpecSpawnHolder;
    public Transform team2SpecSpawnHolder;

    public Transform firstDestructibleSpawn;
    public Transform destructibleSpawnHolder;
    public Transform trapSpawnHolder;

    [HideInInspector]
    public Randomizer<Transform> team1SpecSpawnRandomizer;
    [HideInInspector]
    public Randomizer<Transform> team2SpecSpawnRandomizer;

    [HideInInspector]
    public Randomizer<Transform> destructibleSpawnsRandomizer;
    [HideInInspector]
    public Randomizer<Transform> trapSpawnsRandomizer;

    void Awake()
    {
        var team1Spawns = new List<Transform>();
        for (int i = 0; i < team1SpecSpawnHolder.childCount; i++)
        {
            team1Spawns.Add(team1SpecSpawnHolder.GetChild(i));
        }
        team1SpecSpawnRandomizer = new Randomizer<Transform>(team1Spawns);

        var team2Spawns = new List<Transform>();
        for (int i = 0; i < team2SpecSpawnHolder.childCount; i++)
        {
            team2Spawns.Add(team2SpecSpawnHolder.GetChild(i));
        }
        team2SpecSpawnRandomizer = new Randomizer<Transform>(team2Spawns);

        var destructibleSpawns = new List<Transform>();
        for (int i = 0; i < destructibleSpawnHolder.childCount; i++)
        {
            destructibleSpawns.Add(destructibleSpawnHolder.GetChild(i));
        }
        destructibleSpawnsRandomizer = new Randomizer<Transform>(destructibleSpawns);

        var trapSpawns = new List<Transform>();
        for (int i = 0; i < trapSpawnHolder.childCount; i++)
        {
            trapSpawns.Add(trapSpawnHolder.GetChild(i));
        }
        trapSpawnsRandomizer = new Randomizer<Transform>(trapSpawns);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
