using Fusion;
using System.Collections.Generic;
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
    [SerializeField] private Gun playerGun;

    [Header("Player Movement Variables")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float mouseSensitivity = 60;
    [SerializeField] private float maxPitchValue = 30;
    [SerializeField] private float minPitchValue = -20;

    private float xRotation = 0f;
    private Vector2 mouseInput = Vector2.zero;

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

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
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

                FPCameraRef.GetPlayerCamera().gameObject.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            }

            ///Firing the gun
            if ((data.buttons & NetworkInputData.SPACEBAR) != 0)
            {
                Debug.Log("FIRED!");
                LagCompensatedHit raycastHit;
                if (!playerGun.ShootGun(playerNetworkObject, out raycastHit))
                {
                    Debug.Log(false);
                    return;
                }

                if (!hitPositionNormalPair.ContainsKey(raycastHit.Point))
                {
                    hitPositionNormalPair.Add(raycastHit.Point, raycastHit.Normal);
                }
                
                
                Debug.Log(raycastHit.Collider);///Collider will be null if hit a hit box so have an if statement for this.
                Debug.Log(raycastHit.Hitbox);
            }
        }
    }
}
