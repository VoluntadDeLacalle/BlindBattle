using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour, INetworkRunnerCallbacks
{
    public enum PlayerRole
    {
        Disabled,
        Fighter,
        Spectator
    };

    [Networked(OnChanged = nameof(OnSwitchPlayerRole))] public PlayerRole playerRole { get; set; }

    [Header("Player Children Assets")]
    [SerializeField] private GameObject avatar;
    [SerializeField] private PlayerCamera FPCameraRef;
    [SerializeField] private Gun playerGun;

    [Header("Player Movement Variables")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float mouseSensitivity = 60;
    [SerializeField] private float maxPitchValue = 30;
    [SerializeField] private float minPitchValue = -20;

    private bool canMove = false;

    private float xRotation = 0f;
    private Vector2 mouseInput = Vector2.zero;
    private bool spacebar = false;

    private NetworkCharacterControllerPrototype networkCharCon;
    private NetworkObject playerNetworkObject;

    private Dictionary<Vector3, Vector3> hitPositionNormalPair = new Dictionary<Vector3, Vector3>(); 

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        foreach(KeyValuePair<Vector3, Vector3> entry in hitPositionNormalPair)
        {
            Gizmos.DrawLine(entry.Key, (entry.Value - entry.Key));
        }
    }

    private void Awake()
    {
        networkCharCon = GetComponent<NetworkCharacterControllerPrototype>();
        playerNetworkObject = GetComponent<NetworkObject>();
    }

    public override void Spawned()
    {
        NetworkManager.Instance.networkRunner.AddCallbacks(this);
    }

    private void Start()
    {
        if (playerNetworkObject.HasInputAuthority)
        {
            FPCameraRef.GetPlayerCamera().gameObject.SetActive(true);
            avatar.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SwitchPlayerRole(PlayerRole newPlayerRole)
    {
        if (newPlayerRole == playerRole)
        {
            return;
        }

        playerRole = newPlayerRole;
    }

    static void OnSwitchPlayerRole(Changed<Player> changed)
    {
        changed.LoadNew();
        var newPlayerRole = changed.Behaviour.playerRole;

        switch (newPlayerRole)
        {
            case PlayerRole.Disabled:
                changed.Behaviour.canMove = false;

                break;
            case PlayerRole.Fighter:
                changed.Behaviour.canMove = true;
                changed.Behaviour.FPCameraRef.GetPlayerCamera().gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
                changed.Behaviour.xRotation = 0;

                changed.Behaviour.FPCameraRef.ToggleCameraBlindness(true);

                break;
            case PlayerRole.Spectator:
                changed.Behaviour.canMove = true;

                changed.Behaviour.FPCameraRef.ToggleCameraBlindness(false);

                break;
        }
    }

    void Update()
    {
        spacebar = spacebar || Input.GetKeyDown(KeyCode.Space);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && canMove)
        {
            ///Movement and Looking
            data.direction.Normalize();
            networkCharCon.Move(moveSpeed * data.direction * Runner.DeltaTime);

            float mouseX = data.mouseInput.x * mouseSensitivity * Runner.DeltaTime;
            float mouseY = data.mouseInput.y * mouseSensitivity * Runner.DeltaTime;

            transform.Rotate(Vector3.up * mouseX);

            if (playerRole != PlayerRole.Fighter) ///CHANGE THIS TO BE ONLY SPECTATOR ONCE WE HAVE DISABLED FUNCTIONALITY IN.
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, minPitchValue, maxPitchValue);
            }

            ///Firing the gun
            if ((data.buttons & NetworkInputData.SPACEBAR) != 0 && playerNetworkObject.HasInputAuthority && playerRole == PlayerRole.Fighter)
            {
                LagCompensatedHit raycastHit;
                if (!playerGun.ShootGun(playerNetworkObject, out raycastHit))
                {
                    return;
                }

                if (!hitPositionNormalPair.ContainsKey(raycastHit.Point))
                {
                    hitPositionNormalPair.Add(raycastHit.Point, raycastHit.Normal);
                }

                if (raycastHit.Collider != null)
                {
                    if (raycastHit.Hitbox != null) //Hit a player with a hitbox
                    {
                        NetworkObject hitPlayerObject = raycastHit.Hitbox.transform.root.gameObject.GetComponent<Player>().playerNetworkObject;
                        //TODO: Call RPC to switch role to disabled

                    }
                    else //Hit a static object with a normal collider
                    {

                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        FPCameraRef.GetPlayerCamera().gameObject.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        runner.RemoveCallbacks(this);   
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
            data.direction += transform.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += -transform.forward;

        if (Input.GetKey(KeyCode.A))
            data.direction += -transform.right;

        if (Input.GetKey(KeyCode.D))
            data.direction += transform.right;

        data.mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (spacebar)
        {
            data.buttons |= NetworkInputData.SPACEBAR;
        }
        spacebar = false;

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
