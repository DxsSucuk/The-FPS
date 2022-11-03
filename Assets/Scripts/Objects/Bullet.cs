using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks
{
    public float damageValue;

    private void Awake()
    {
        Invoke("Destroy_Bullet",5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController controller = collision.gameObject.GetComponentInParent<PlayerController>();
            if (controller != null)
            {
                GameManager gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
                gameManager.damage(controller, damageValue);
            }
            else
            {
            }
        }
        Destroy_Bullet();
    }

    private void Destroy_Bullet()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
