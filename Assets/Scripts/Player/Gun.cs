using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    public float shootDistance = 50f;
    public SimpleTimer shootTimer = new SimpleTimer(4f);
    public string gunFireSFXName;
    public string gunEmptySFXName;

    [Header("Prefabs")]
    [SerializeField] private GunRicochet gunRicochetPrefab;

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
        return shootTimer.expired;
    }

    private void Update()
    {
        shootTimer.Update();
        //Debug.Log($"{gameObject.name} can shoot: {CanShoot()}");
    }

    public bool ShootGun(NetworkObject playerNetworkObject, out LagCompensatedHit raycastHit)
    {
        if (shootTimer.expired)
        {
            shootTimer.Reset();
        }
        else
        {
            raycastHit = new LagCompensatedHit { };
            return false;
        }

        return Runner.LagCompensation.Raycast(shootTransform.position, ((shootTransform.position + shootTransform.forward) - shootTransform.position).normalized, shootDistance, playerNetworkObject.InputAuthority, out raycastHit, -1, HitOptions.IncludePhysX);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_SpawnGunRicochet(NetworkObject playerNetworkObject, Vector3 hitPosition)
    {
        NetworkManager.Instance.networkRunner.Spawn(gunRicochetPrefab, hitPosition, Quaternion.identity, null, (Runner, o) =>
        {
            o.GetComponent<GunRicochet>().Init();
        });
    }
}
