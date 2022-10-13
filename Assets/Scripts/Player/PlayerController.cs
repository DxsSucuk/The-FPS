using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public float walkSpeed = 7f;
    public float playerFOV = 120;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;
    public float dashSpeed = 20f;
    public float dashCooldown = 5f;
    public float dashTime = 0.4f;
    public float lastDash;
    public float health = 100;
    public float maxHealth = 100;

    public Transform shootPoint;
    public GameObject bullet;

    private CharacterController characterController;
    private Camera playerCamera;
    private ParticleSystem _particleSystem;

    private float yaw;
    private float pitch;
    private bool onGround;
    private bool canShoot = true;
    private Vector3 velocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        _particleSystem = shootPoint.gameObject.GetComponentInChildren<ParticleSystem>();

        if (photonView.IsMine)
        {
            playerCamera.fieldOfView = playerFOV;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            playerCamera.enabled = false;
            playerCamera.gameObject.GetComponent<AudioListener>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            CameraMovement();
            GroundCheck();
            Movement();
            WeaponHandler();
        }
    }

    void GroundCheck()
    {
        Vector3 position = transform.position;
        Vector3 origin = new Vector3(position.x, position.y - (transform.localScale.y * .5f),
            position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            onGround = true;
        }
        else
        {
            onGround = false;
        }
    }

    void Movement()
    {
        Vector3 moveVector =
            transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
        
        characterController.Move(moveVector * Time.deltaTime * walkSpeed);
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDash + dashCooldown)
        {
            lastDash = Time.time;
            StartCoroutine(Dash());
        }

        if (velocity.y < 0 && onGround)
        {
            velocity.y = 0;
        }
        
        if (Input.GetKey(KeyCode.Space) && onGround && velocity.y <= 0)
        {
            velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y);
        }

        velocity.y += Physics.gravity.y * Time.deltaTime;
        
        characterController.Move(velocity * Time.deltaTime);
    }

    void CameraMovement()
    {
        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

        pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");

        // Clamp pitch between lookAngle
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.localEulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    void WeaponHandler()
    {
        if (Input.GetKey(KeyCode.Mouse0) && canShoot)
        {
            photonView.RPC("Weapon_Shoot", RpcTarget.All);
        }
    }

    [PunRPC]
    void Weapon_Shoot()
    {
        if (_particleSystem != null) _particleSystem.Play();
        
        if (photonView.IsMine)
        {
            if (!canShoot) return;

            canShoot = false;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(75);
            }

            Vector3 direction = targetPoint - shootPoint.position;

            Debug.DrawRay(shootPoint.position, direction, Color.red);

            GameObject bulletObject = PhotonNetwork.Instantiate("Prefabs/Projecttiles/" + bullet.name,
                shootPoint.position, Quaternion.identity);

            bulletObject.GetComponent<Rigidbody>().AddForce(direction.normalized * 250f, ForceMode.Impulse);
            bulletObject.GetComponent<Rigidbody>().AddForce(direction.normalized * 250f, ForceMode.Impulse);

            Invoke("Weapon_Shoot_Reset", 0.25f);
        }
    }

    void Weapon_Shoot_Reset()
    {
        canShoot = true;
    }


    [PunRPC]
    public void Player_Heal(float heal)
    {
        health = health + heal > maxHealth ? maxHealth : health + heal;
    }
    
    [PunRPC]
    public void Player_Damage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            
            if (photonView.IsMine)
            {
                photonView.RPC("Player_Death", RpcTarget.All);
            }
        }
    }
    
    [PunRPC]
    void Player_Death()
    {
        Debug.Log(photonView.Owner.NickName + " died!");
        if (photonView.IsMine)
        {
            PhotonNetwork.Instantiate("Prefabs/Player/Player_Corpse", gameObject.transform.position,
                Quaternion.identity);
                transform.position += new Vector3(0, 5, 0);
                photonView.RPC("Player_Heal", RpcTarget.All, 99999f);
        }
    }

    IEnumerator Dash()
    {
        float start = Time.time;
        while (Time.time < start + dashTime)
        {
            characterController.Move(transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))) * Time.deltaTime * dashSpeed);
            yield return null;
        }
    }
}