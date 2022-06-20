using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : NetworkBehaviour
{
    public int value = 100;
    public string destructibleSFXName;

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
        NetworkGameState.Instance.RPC_PlayAt(destructibleSFXName, transform.position, 100);

        if (!isDestroyedInHost)
        {
            Runner.Despawn(Object);
            isDestroyedInHost = true;

            NetworkGameState.Instance.AddScoreToPlayer(value, destroyedBy.Object.InputAuthority);

            MapGenerator.Instance.PlaceRandomDestructible();
        }
    }
}
