using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRicochet : NetworkBehaviour
{
    [SerializeField] private NetworkObject gunRicochetNetworkObject;
    public float particleTimer = 0;

    [Networked] private TickTimer life { get; set; }

    public void Init()
    {
        Debug.Log("Gun ricochet initialized");
        life = TickTimer.CreateFromSeconds(Runner, particleTimer);
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
        {
            Debug.Log("test");
            Runner.Despawn(gunRicochetNetworkObject);
        }
    }
}
