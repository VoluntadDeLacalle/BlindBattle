using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUser : NetworkBehaviour
{
    public static Dictionary<PlayerRef, NetworkUser> allNetworkUsers = new Dictionary<PlayerRef, NetworkUser>();

    [Networked] public string userName { get; set; }

    public PlayerRef belongsTo => Object.InputAuthority;

    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (this.Object.HasInputAuthority)
        {
            gameObject.name += $" (Local)";
            RPC_SetUserName(NetworkManager.Instance.localUserName);
        }

        allNetworkUsers[belongsTo] = this;
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

    private void OnDestroy()
    {
        if (allNetworkUsers.ContainsKey(belongsTo) && allNetworkUsers[belongsTo] == this)
        {
            allNetworkUsers.Remove(belongsTo);
        }
    }
}
