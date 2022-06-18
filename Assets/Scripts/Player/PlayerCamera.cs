using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    public LayerMask nothingLayerMask;
    public LayerMask everythingLayerMask;
    public Color defaultBackgroundColor;
    public Color blindBackgroundColor;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (transform.position + transform.forward) * 1.5f);
    }

    public Camera GetPlayerCamera()
    {
        return playerCamera;
    }

    public void ToggleCameraBlindness(bool shouldBlind)
    {
        playerCamera.cullingMask = shouldBlind ? nothingLayerMask : everythingLayerMask;
        playerCamera.clearFlags = shouldBlind ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
        playerCamera.backgroundColor = shouldBlind ? blindBackgroundColor : defaultBackgroundColor;
    }
}
