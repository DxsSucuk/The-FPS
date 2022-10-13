using Photon.Pun;

public class SuicideScript : MonoBehaviourPunCallbacks
{
    public float delay;

    private void Awake()
    {
        Invoke("KillMe", delay);
    }

    void KillMe()
    {
        if (!photonView.IsMine) return;
        PhotonNetwork.Destroy(gameObject);
    }
}
