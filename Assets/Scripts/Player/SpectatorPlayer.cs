using Fusion;
using Fusion.Sockets;
using LincolnCpp.HUDIndicator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorPlayer : NetworkBehaviour, INetworkRunnerCallbacks
{
    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    [SerializeField] private AudioSource droneHoverAudioSource;
    [SerializeField] private float droneVelocityThreshold = 0.2f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    [SerializeField] private new Camera camera;
    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    [Header("Indicators")]
    public IndicatorIconStyle team1IndicatorStyle;
    public IndicatorArrowStyle team1IndicatorArrowStyle;
    public IndicatorIconStyle team2IndicatorStyle;
    public IndicatorArrowStyle team2IndicatorArrowStyle;

    [Networked] public NetworkBool canMove { get; set; }
    [Networked] public float speed { get; set; }

    private CharacterController charCon;
    private Rigidbody rb;

    private void Awake()
    {
        charCon = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        droneHoverAudioSource.Play();
        droneHoverAudioSource.Pause();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (speed > droneVelocityThreshold)
        {
            if (!droneHoverAudioSource.isPlaying)
            {
                droneHoverAudioSource.UnPause();
            }
        }
        else
        {
            if (droneHoverAudioSource.isPlaying)
            {
                droneHoverAudioSource.Pause();
            }
        }
    }

    public override void Spawned()
    {
        NetworkManager.Instance.networkRunner.AddCallbacks(this);
        RPC_SwitchCanMove(true);

        if (Runner.LocalPlayer != Object.InputAuthority)
        {
            indicatorOnScreen.visible = false;
            indicatorOffScreen.visible = false;
            RenderSettings.fog = false;
            HUD.Instance.SetCameraForIndicator(camera);
        }
        else
        {
            camera.gameObject.SetActive(false);
            SetupIndicators();
        }
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        runner.RemoveCallbacks(this);
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

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_SwitchCanMove(NetworkBool val)
    {
        canMove = val;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && canMove)
        {
            rotationX += data.mouseInput.x * cameraSensitivity * Runner.DeltaTime;
            rotationY += data.mouseInput.y * cameraSensitivity * Runner.DeltaTime;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

            float factor = 1;

            if ((data.buttons & NetworkInputData.FAST) != 0)
            {
                factor = fastMoveFactor;
            }
            else if ((data.buttons & NetworkInputData.SLOW) != 0)
            {
                factor = slowMoveFactor;
            }

            //transform.position += transform.forward * normalMoveSpeed * factor * data.direction.z * Runner.DeltaTime;
            //transform.position += transform.right * normalMoveSpeed * factor * data.direction.x * Runner.DeltaTime;

            //if ((data.buttons & NetworkInputData.CLIMB) != 0)
            //{
            //    transform.position += transform.up * climbSpeed * Runner.DeltaTime;
            //}

            //if ((data.buttons & NetworkInputData.DESCEND) != 0)
            //{
            //    transform.position -= transform.up * climbSpeed * Runner.DeltaTime;
            //}

            var moveDir = data.direction;
            if ((data.buttons & NetworkInputData.CLIMB) != 0)
            {
                moveDir += Vector3.up;
            }

            if ((data.buttons & NetworkInputData.DESCEND) != 0)
            {
                moveDir -= Vector3.up;
            }

            Vector3 worldDir = transform.TransformDirection(moveDir.normalized);
            charCon.Move(normalMoveSpeed * factor * worldDir * Runner.DeltaTime);
            speed = charCon.velocity.magnitude;
        }
    }


    public void OnConnectedToServer(NetworkRunner runner)
    {
        //
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        //
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        //
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        //
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        //
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        //
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        data.direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        data.mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (Input.GetButton("Run"))
        {
            data.buttons |= NetworkInputData.FAST;
        }

        if (Input.GetButton("Crouch"))
        {
            data.buttons |= NetworkInputData.SLOW;
        }

        if (Input.GetButton("Climb"))
        {
            data.buttons |= NetworkInputData.CLIMB;
        }

        if (Input.GetButton("Descend"))
        {
            data.buttons |= NetworkInputData.DESCEND;
        }

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        //
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        //
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        //
    }
}
