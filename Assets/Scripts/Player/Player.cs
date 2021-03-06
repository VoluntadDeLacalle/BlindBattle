using Fusion;
using Fusion.Sockets;
using LincolnCpp.HUDIndicator;
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
    [Networked] public float speed { get; set; }

    [Header("Player Children Assets")]
    [SerializeField] private GameObject avatar;
    [SerializeField] private PlayerCamera FPCameraRef;
    [SerializeField] private Gun playerGun;
    [SerializeField] private NetworkMecanimAnimator networkAnimator;
    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    [Header("Player Movement Variables")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float mouseSensitivity = 60;
    [SerializeField] private float maxPitchValue = 30;
    [SerializeField] private float minPitchValue = -20;
    [SerializeField] private float footstepMagnitudeThreshold = 0.2f;


    [Header("Indicators")]
    public IndicatorIconStyle team1IndicatorStyle;
    public IndicatorArrowStyle team1IndicatorArrowStyle;
    public IndicatorIconStyle team2IndicatorStyle;
    public IndicatorArrowStyle team2IndicatorArrowStyle;

    [Header("Player SFX Names & Varibles")]
    [SerializeField] private AudioSource footstepAudioSource;

    [SerializeField] private int playerKillScore;

    private Transform spawnPoint = null;

    private bool canMove = false;

    private float xRotation = 0f;
    private Vector2 mouseInput = Vector2.zero;

    private NetworkCharacterControllerPrototype networkCharCon;
    private NetworkObject playerNetworkObject;

    private void Awake()
    {
        networkCharCon = GetComponent<NetworkCharacterControllerPrototype>();
        playerNetworkObject = GetComponent<NetworkObject>();

        footstepAudioSource.Play();
        footstepAudioSource.Pause();
    }

    public override void Spawned()
    {
        NetworkManager.Instance.networkRunner.AddCallbacks(this);

        if (spawnPoint != null)
        {
            networkCharCon.TeleportToPositionRotation(spawnPoint.position, spawnPoint.rotation);
        }

        if (Object.HasStateAuthority && Object.HasInputAuthority)
        {
            avatar.SetLayerRecursively(LayerMask.NameToLayer("Host"));
        }

        var camera = FPCameraRef.GetPlayerCamera();
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            RenderSettings.fog = true;

            indicatorOnScreen.enabled = false;
            indicatorOffScreen.enabled = false;

            camera.gameObject.SetActive(true);
            HUD.Instance.SetCameraForIndicator(camera);
            // We don't show indicators if the current player is in the pit
            HUD.Instance.ToggleIndicators(false);

            RoundManager.Instance.ToggleDirectionalLightsShadows(false);
        }
        else
        {
            camera.gameObject.SetActive(false);
            SetupIndicators();
        }
    }

    void SetupIndicators()
    {
        IndicatorIconStyle selectedStyle = default;
        IndicatorArrowStyle selectedArrowStyle = default;
        var teamNum = NetworkGameState.Instance.GetPlayerTeamNumber(Object.InputAuthority);
        switch (teamNum)
        {
            case 1:
                selectedStyle = team1IndicatorStyle;
                selectedArrowStyle = team1IndicatorArrowStyle;
                break;
            case 2:
                selectedStyle = team2IndicatorStyle;
                selectedArrowStyle = team1IndicatorArrowStyle;
                break;
        }

        indicatorOnScreen.renderers.Add(HUD.Instance.indicatorRenderer);
        indicatorOnScreen.style = selectedStyle;
        indicatorOnScreen.ResetCanvas();

        indicatorOffScreen.renderers.Add(HUD.Instance.indicatorRenderer);
        indicatorOffScreen.style = selectedStyle;
        indicatorOffScreen.arrowStyle = selectedArrowStyle;
        indicatorOffScreen.ResetCanvas();
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
                changed.Behaviour.RPC_AnimationTrigger("DeathTrigger");

                break;
        }
    }

    void Update()
    {
        if (speed > footstepMagnitudeThreshold)
        {
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.UnPause();
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Pause();
            }
        }

        if(Object.HasInputAuthority && RoundManager.Instance.playerInput.actions["Fire"].triggered)
        {
            bool gunFireStatus = playerGun.CanShoot();
            if (!gunFireStatus)
            {
                SoundEffectsManager.Instance.Play(playerGun.gunEmptySFXName);
            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_AnimationTrigger(string triggerName)
    {
        networkAnimator.SetTrigger(triggerName);
    }

    public void TeleportTo(Transform spawnPoint)
    {
        networkCharCon.TeleportToPositionRotation(spawnPoint.position, spawnPoint.rotation);
        this.spawnPoint = spawnPoint;
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
            speed = networkCharCon.Velocity.magnitude;

            float mouseX = data.mouseInput.x * mouseSensitivity * Runner.DeltaTime;
            float mouseY = data.mouseInput.y * mouseSensitivity * Runner.DeltaTime;

            transform.Rotate(Vector3.up * mouseX);

            if (playerRole == PlayerRole.Spectator)
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, minPitchValue, maxPitchValue);
            }

            ///Firing the gun
            if ((data.buttons & NetworkInputData.SHOOT) != 0 && playerNetworkObject.HasInputAuthority && playerRole == PlayerRole.Fighter)
            {
                bool gunFireStatus = playerGun.CanShoot();
                if (gunFireStatus)
                {
                    RPC_AnimationTrigger("ShootTrigger");
                    NetworkGameState.Instance.RPC_PlayAt(playerGun.gunFireSFXName, transform.position, 300);
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
                            player.RPC_PlayerDied(Object.InputAuthority);
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
    void RPC_PlayerDied(PlayerRef killedBy)
    {
        NetworkGameState.Instance.AddScoreToPlayer(playerKillScore, killedBy);

        switch(NetworkGameState.Instance.gameStyle)
        {
            case GameStyle.Classic:
                DieAndRespawn();
                break;
            case GameStyle.Battle:
                DieAndMoveToNextRound();
                break;
        }
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

    private async void DieAndMoveToNextRound()
    {
        var playerRef = Object.InputAuthority;
        var curPlayerRole = playerRole;

        RPC_SwitchPlayerRole(PlayerRole.Dead);

        await Task.Delay(TimeSpan.FromSeconds(3));

        RoundManager.Instance.EndRound();
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
        //
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
