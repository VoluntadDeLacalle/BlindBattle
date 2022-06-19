using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSkeleton : NetworkBehaviour
{
    public Transform player1Spawn;
    public Transform player2Spawn;

    public Transform firstDestructibleSpawn;
    public Transform destructibleSpawnHolder;
    public Transform trapSpawnHolder;

    [HideInInspector]
    public List<Transform> destructibleSpawns;

    [HideInInspector]
    public List<Transform> trapSpawns;

    [HideInInspector]
    public Randomizer<Transform> destructibleSpawnsRandomizer;

    [HideInInspector]
    public Randomizer<Transform> trapSpawnsRandomizer;

    void Awake()
    {
        destructibleSpawns.Clear();
        for (int i=0; i< destructibleSpawnHolder.childCount; i++)
        {
            destructibleSpawns.Add(destructibleSpawnHolder.GetChild(i));
        }
        destructibleSpawnsRandomizer = new Randomizer<Transform>(destructibleSpawns);

        trapSpawns.Clear();
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
