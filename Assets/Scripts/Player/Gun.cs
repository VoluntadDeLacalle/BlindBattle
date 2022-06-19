using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    public float shootDistance = 50f;
    public float shootTimer = 4f;

    [Networked] private TickTimer shotLife { get; set; }


    [Header("Prefabs")]
    [SerializeField] private NetworkPrefabRef gunRicochetPrefab;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (shootTransform != null)
        {
            Gizmos.DrawSphere(shootTransform.position, 0.08f);
            Gizmos.DrawRay(shootTransform.position, ((shootTransform.position + shootTransform.forward) - shootTransform.position).normalized * shootDistance);
        }
    }

    public bool CanShoot()
    {
        return shotLife.ExpiredOrNotRunning(Runner);
    }
    
    public bool ShootGun(NetworkObject playerNetworkObject, out LagCompensatedHit raycastHit)
    {
        if (shotLife.ExpiredOrNotRunning(Runner))
        {
            shotLife = TickTimer.CreateFromSeconds(Runner, shootTimer);
        }
        else
        {
            raycastHit = new LagCompensatedHit { };
            return false;
        }

        return Runner.LagCompensation.Raycast(shootTransform.position, ((shootTransform.position + shootTransform.forward) - shootTransform.position).normalized, shootDistance, playerNetworkObject.InputAuthority, out raycastHit, -1, HitOptions.IncludePhysX);
    }

    public void SpawnGunRicochet(NetworkObject playerNetworkObject, Vector3 hitPosition)
    {
        Runner.Spawn(gunRicochetPrefab, hitPosition, Quaternion.identity, playerNetworkObject.InputAuthority,
          (runner, o) =>
          {
              o.GetComponent<GunRicochet>().Init();
          });
    }
}
