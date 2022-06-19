using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRicochet : NetworkBehaviour
{
    public float particleTimer = 0;

    [Networked] private TickTimer life { get; set; }

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, particleTimer);
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}
