using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    public float shootDistance = 50f;
    public int maxAmmo = 15;
    private int currentAmmo = 0;

    [Header("Prefabs")]
    [SerializeField] private NetworkPrefabRef gunRicochetPrefab;

    private void Awake()
    {
        currentAmmo = maxAmmo;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (shootTransform != null)
        {
            Gizmos.DrawSphere(shootTransform.position, 0.08f);
            Gizmos.DrawRay(shootTransform.position, ((shootTransform.position + shootTransform.forward) - shootTransform.position).normalized * shootDistance);
        }
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public void SetCurrentAmmo(int ammoAmount)
    {
        if (ammoAmount <= 0)
        {
            currentAmmo = 0;
        }
        else if (ammoAmount >= maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
        else
        {
            currentAmmo = ammoAmount;
        }

    }
    
    public bool ShootGun(NetworkObject playerNetworkObject, out LagCompensatedHit raycastHit)
    {
        //if (currentAmmo <= 0)
        //{
        //    raycastHit = new LagCompensatedHit { };
        //    return false;
        //}
        //else
        //{
        //    currentAmmo--;
        //}

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
