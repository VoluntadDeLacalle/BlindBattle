using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Player : NetworkBehaviour, INetworkRunnerCallbacks
{
    public enum PlayerRole
    {
        Disabled,
        Fighter,
        Spectator,
        Dead,
    };

    [Networked(OnChanged = nameof(OnSwitchPlayerRole))] public PlayerRole playerRole { get; set; }

    [Header("Player Children Assets")]
    [SerializeField] private GameObject avatar;
    [SerializeField] private PlayerCamera FPCameraRef;
    [SerializeField] private Gun playerGun;
    [SerializeField] private NetworkMecanimAnimator networkAnimator;

    [Header("Player Movement Variables")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float mouseSensitivity = 60;
    [SerializeField] private float maxPitchValue = 30;
    [SerializeField] private float minPitchValue = -20;

    private bool canMove = false;

    private float xRotation = 0f;
    private Vector2 mouseInput = Vector2.zero;
    private bool shootKeyPressed = false;

    private NetworkCharacterControllerPrototype networkCharCon;
    private NetworkObject playerNetworkObject;

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

            if (NetworkManager.Instance.IsHost)
            {
                FPCameraRef.RemoveHostLayer();
            }
            else
            {
                avatar.SetActive(false);
            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_SwitchPlayerRole(PlayerRole newPlayerRole)
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
            case PlayerRole.Dead:
                changed.Behaviour.canMove = false;
                changed.Behaviour.networkAnimator.SetTrigger("DeathTrigger");

                break;
        }
    }

    void Update()
    {
        shootKeyPressed = shootKeyPressed || Input.GetKeyDown(KeyCode.Space);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && canMove)
        {
            ///Movement and Looking
            networkAnimator.Animator.SetFloat("InputX", data.direction.x);
            networkAnimator.Animator.SetFloat("InputZ", data.direction.z);

            data.direction.Normalize();
            Vector3 worldData = transform.localToWorldMatrix * data.direction;
            networkCharCon.Move(moveSpeed * worldData * Runner.DeltaTime);

            float mouseX = data.mouseInput.x * mouseSensitivity * Runner.DeltaTime;
            float mouseY = data.mouseInput.y * mouseSensitivity * Runner.DeltaTime;

            transform.Rotate(Vector3.up * mouseX);

            if (playerRole == PlayerRole.Spectator)
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, minPitchValue, maxPitchValue);
            }

            ///Firing the gun
            if ((data.buttons & NetworkInputData.SPACEBAR) != 0 && playerNetworkObject.HasInputAuthority && playerRole == PlayerRole.Fighter)
            {
                if (playerGun.CanShoot())
                {
                    networkAnimator.SetTrigger("ShootTrigger");
                }

                LagCompensatedHit raycastHit;
                if (!playerGun.ShootGun(playerNetworkObject, out raycastHit))
                {
                    return;
                }
                
                if (raycastHit.Collider != null)
                {
                    if (raycastHit.Hitbox != null) //Hit a player with a hitbox
                    {
                        Player player = raycastHit.Hitbox.transform.root.gameObject.GetComponent<Player>();
                        if (player)
                        {
                            // TODO: Wait, and then respawn
                            player.RPC_Respawn();
                        }
                    }
                    else //Hit a static object with a normal collider
                    {
                        var destructible = raycastHit.Collider.GetComponentInParent<Destructible>();
                        if (destructible)
                        {
                            destructible.RPC_Destroy(this);
                        }
                    }

                    Debug.Log("Hit");
                    playerGun.RPC_SpawnGunRicochet(Object, raycastHit.Point);
                }
            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    void RPC_Respawn()
    {
        DieAndRespawn();
    }

    private async void DieAndRespawn()
    {
        var playerRef = Object.InputAuthority;
        var curPlayerRole = playerRole;

        RPC_SwitchPlayerRole(PlayerRole.Dead);

        await Task.Delay(TimeSpan.FromSeconds(3));

        Runner.Despawn(Object);

        BasicSpawner.Instance.SpawnAfterDelay(playerRef, curPlayerRole, 1);
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
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += -Vector3.forward;

        if (Input.GetKey(KeyCode.A))
            data.direction += -Vector3.right;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;

        data.mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (shootKeyPressed)
        {
            data.buttons |= NetworkInputData.SPACEBAR;
        }
        shootKeyPressed = false;

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
