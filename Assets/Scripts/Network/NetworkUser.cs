using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUser : NetworkBehaviour
{
    [Networked] public string userName { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        if (this.Object.HasInputAuthority)
        {
            gameObject.name += $" (Local)";
            RPC_SetUserName(NetworkManager.Instance.localUserName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SetUserName(string name)
    {
        userName = name;
    }
}
