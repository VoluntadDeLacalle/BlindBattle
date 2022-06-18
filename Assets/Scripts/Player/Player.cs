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

    [SerializeField] private GameObject avatar;
    [SerializeField] private Camera FPCamera;

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
        }
    }
}
