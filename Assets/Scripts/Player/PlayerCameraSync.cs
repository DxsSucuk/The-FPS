using Photon.Pun;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerCameraSync : MonoBehaviour
{
    public PlayerCameraController playerCameraController;
    public Transform playerCamera;
    public Transform weaponModel;

    private void Awake()
    {
        playerCameraController = GetComponentInChildren<PlayerCameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCamera == null) return;

        transform.position = playerCamera.position;

        playerCamera.rotation = playerCameraController.playerCamera.transform.rotation;

        if (weaponModel != null)
        {
            weaponModel.rotation = playerCameraController.playerCamera.transform.rotation;
        }
    }
}
    