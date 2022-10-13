using System;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Slider _dashSlider;
    public Slider _healthSlider;
    public PlayerController _playerController;

    private void Awake()
    {
        PhotonNetwork.NickName = RandomString(8);
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.GetPhotonView(PhotonNetwork.SyncViewId) == null)
            {
                _playerController = PhotonNetwork
                    .Instantiate("Prefabs/Player/Player", new Vector3(0, 1, 0), Quaternion.identity)
                    .GetComponent<PlayerController>();
                _dashSlider.maxValue = _playerController.dashCooldown;
                _healthSlider.maxValue = _playerController.maxHealth;
            }
        }
        else
        {
            if (!PhotonNetwork.IsConnected) 
                PhotonNetwork.ConnectUsingSettings();
            
            if (!PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.JoinRandomOrCreateRoom(null, 0, Photon.Realtime.MatchmakingMode.RandomMatching, null, null, RandomString(4));
        }
    }

    private void Start()
    {
        if (_playerController != null)
        {
            _dashSlider.maxValue = _playerController.dashCooldown;
            _healthSlider.maxValue = _playerController.maxHealth;
        }
    }

    private void Update()
    {
        if (_playerController == null) return;
        
        Check_Health();
        Check_Dash();
    }

    void Check_Health()
    {
        _healthSlider.value = _playerController.health;
        if (_healthSlider.value == _healthSlider.maxValue)
        {
            _healthSlider.gameObject.SetActive(false);
        }
        else
        {
            _healthSlider.gameObject.SetActive(true);
        }
    }

    void Check_Dash()
    {
        float value = _dashSlider.maxValue - ((_playerController.lastDash + _playerController.dashCooldown) - Time.time);
        if (value < _dashSlider.minValue) value = 0;
        if (value > _playerController.dashCooldown) value = _playerController.dashCooldown;

        if (value == _playerController.dashCooldown)
        {
            _dashSlider.gameObject.SetActive(false);
        }
        else
        {
            _dashSlider.gameObject.SetActive(true);
        }

        _dashSlider.SetValueWithoutNotify(value);
    }

    public void damage(PlayerController playerController, float damage)
    {
        playerController.photonView.RPC("Player_Damage", RpcTarget.All, damage);
        Debug.Log("Damage dealt to " + playerController.photonView.Owner.NickName + "! " + damage);
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Connected to Master!");
        if (!PhotonNetwork.InRoom)
            PhotonNetwork.JoinRandomOrCreateRoom(null, 0, Photon.Realtime.MatchmakingMode.FillRoom, null, null, RandomString(4));
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        if (PhotonNetwork.GetPhotonView(PhotonNetwork.SyncViewId) == null)
        {
            _playerController = PhotonNetwork
                .Instantiate("Prefabs/Player/Player", new Vector3(0, 1, 0), Quaternion.identity)
                .GetComponent<PlayerController>();
            _dashSlider.maxValue = _playerController.dashCooldown;
            _healthSlider.maxValue = _playerController.maxHealth;
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + " joined!");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " left!");
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
    
    string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new System.Random().Next(s.Length)]).ToArray());
    }
}