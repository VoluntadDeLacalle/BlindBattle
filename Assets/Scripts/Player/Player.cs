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
    [SerializeField] private Camera FPCamera;

    [Header("Player Look Movement Variables")]
    [SerializeField] private float mouseSensitivity = 60;
    [SerializeField] private float maxPitchValue = 30;
    [SerializeField] private float minPitchValue = -20;

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
            FPCamera.gameObject.SetActive(true);
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
                changed.Behaviour.FPCamera.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);

                break;
            case PlayerRole.Spectator:
                
                break;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            networkCharCon.Move(5 * data.direction * Runner.DeltaTime);

            data.mouseInput.Normalize();
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, transform.localRotation.eulerAngles.y + (mouseSensitivity * data.mouseInput.x), 0), Runner.DeltaTime);

            if (true/*playerRole == PlayerRole.Spectator*/)
            {
                float currentCameraEulerX = FPCamera.gameObject.transform.localRotation.eulerAngles.x;
                float finalEulerAngle = currentCameraEulerX - (mouseSensitivity * data.mouseInput.y * Runner.DeltaTime);
                if (finalEulerAngle < 0)
                {
                    finalEulerAngle = 360 + finalEulerAngle;
                }

                if (finalEulerAngle < 180)
                {
                    finalEulerAngle = Mathf.Clamp(finalEulerAngle, Mathf.Max(0, minPitchValue), maxPitchValue);
                }
                else
                {
                    finalEulerAngle = Mathf.Clamp(finalEulerAngle, 360 + Mathf.Min(0, minPitchValue), 360);
                }

                FPCamera.gameObject.transform.localRotation = Quaternion.Euler(finalEulerAngle, 0, 0);
            }
        }
    }
}
