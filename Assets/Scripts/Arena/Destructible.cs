using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : NetworkBehaviour
{
    public int value = 100;

    public bool isDestroyedInHost = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_Destroy(Player destroyedBy)
    {
        if (!isDestroyedInHost)
        {
            Runner.Despawn(Object);
            isDestroyedInHost = true;

            NetworkGameState.Instance.AddScoreToPlayer(value, destroyedBy.Object.InputAuthority);

            MapGenerator.Instance.PlaceRandomDestructible();
        }
    }
}
