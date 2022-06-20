using Fusion;
using LincolnCpp.HUDIndicator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : NetworkBehaviour
{
    public int value = 100;
    public string destructibleSFXName;

    public bool isDestroyedInHost = false;

    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Spawned()
    {
        SetupIndicators();
    }

    void SetupIndicators()
    {
        indicatorOnScreen.renderers.Add(HUD.Instance.indicatorRenderer);
        indicatorOnScreen.ResetCanvas();

        indicatorOffScreen.renderers.Add(HUD.Instance.indicatorRenderer);
        indicatorOffScreen.ResetCanvas();
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
