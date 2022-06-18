using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
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

    [Header("Player Movement Variables")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float mouseSensitivity = 60;
    [SerializeField] private float maxPitchValue = 30;
    [SerializeField] private float minPitchValue = -20;

    private float xRotation = 0f;
    private Vector2 mouseInput = Vector2.zero;

    private NetworkCharacterControllerPrototype networkCharCon;
    private NetworkObject playerNetworkObject;

    private void Awake()
    {
        networkCharCon = GetComponent<NetworkCharacterControllerPrototype>();
        playerNetworkObject = GetComponent<NetworkObject>();
    }

    private void Start()
    {
        if (playerNetworkObject.HasInputAuthority)
        {
            FPCameraRef.GetPlayerCamera().gameObject.SetActive(true);
            avatar.SetActive(false);
        }

        ///REMOVE THIS LATER
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

                break;
            case PlayerRole.Fighter:
                changed.Behaviour.FPCameraRef.GetPlayerCamera().gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
                changed.Behaviour.xRotation = 0;

                changed.Behaviour.FPCameraRef.ToggleCameraBlindness(true);

                break;
            case PlayerRole.Spectator:
                changed.Behaviour.FPCameraRef.ToggleCameraBlindness(false);

                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SwitchPlayerRole(PlayerRole.Fighter);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            networkCharCon.Move(moveSpeed * data.direction * Runner.DeltaTime);

            float mouseX = data.mouseInput.x * mouseSensitivity * Runner.DeltaTime;
            float mouseY = data.mouseInput.y * mouseSensitivity * Runner.DeltaTime;

            transform.Rotate(Vector3.up * mouseX);

            if (playerRole != PlayerRole.Fighter) ///CHANGE THIS TO BE ONLY SPECTATOR ONCE WE HAVE DISABLED FUNCTIONALITY IN.
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, minPitchValue, maxPitchValue);

                FPCameraRef.GetPlayerCamera().gameObject.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            }
        }
    }
}
