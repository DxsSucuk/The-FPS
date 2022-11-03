using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    PlayerCameraController cameraController;
    public Camera playerCamera;

    public float yaw;
    public float pitch;

    public float playerFOV = 120;
    public float maxLookAngle = 90f;

    public float sensitivityX, sensitivityY;

    public Transform orientation;

    void Awake()
    {
        playerCamera = GetComponent<Camera>();
        SetFov(playerFOV);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void CameraMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivityY;

        pitch += mouseX;

        yaw -= mouseY;
        yaw = Mathf.Clamp(yaw, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(yaw, pitch, 0);

        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, pitch, 0);
    }

    // Update is called once per frame
    void Update()
    {
        CameraMovement();
    }

    public void SetFov(float fov)
    {
        playerCamera.fieldOfView = fov;
    }

    public void resetFov()
    {
        SetFov(playerFOV);
    }
}