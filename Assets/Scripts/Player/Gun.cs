using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    public float shootDistance = 50f;

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
    
    public bool ShootGun(NetworkObject playerNetworkObject, out LagCompensatedHit raycastHit)
    {
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
