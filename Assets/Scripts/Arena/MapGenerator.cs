using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : SingletonMonoBehaviour<MapGenerator>
{
    public Randomizer<MapSkeleton> availableMapPrefabs;
    public Randomizer<Destructible> destructiblePrefabs;

    [HideInInspector]
    public MapSkeleton curSkeleton;

    private new void Awake()
    {
        base.Awake();
        availableMapPrefabs.Shuffle();
        destructiblePrefabs.Shuffle();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {
        var skeletonPrefab = availableMapPrefabs.GetRandomItem();
        curSkeleton = NetworkManager.Instance.networkRunner.Spawn(skeletonPrefab, Vector3.zero, Quaternion.identity);

        var destructiblePrefab = destructiblePrefabs.GetRandomItem();
        PlaceDestructible(destructiblePrefab, curSkeleton.firstDestructibleSpawn);
    }

    public void PlaceRandomDestructible()
    {
        var destructiblePrefab = destructiblePrefabs.GetRandomItem();
        var spawnPoint = curSkeleton.destructibleSpawnsRandomizer.GetRandomItem();
        PlaceDestructible(destructiblePrefab, spawnPoint);
    }

    void PlaceDestructible(Destructible destructiblePrefab, Transform spawnPoint)
    {
        if (NetworkGameState.Instance.gameStyle == GameStyle.Battle)
        {
            return;
        }

        NetworkManager.Instance.networkRunner.Spawn(destructiblePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
