using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    private void Update()
    {
        //if (!PhotonNetwork.InLobby || MenuManager.Instance.playerInLobbyText == null) return;
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnCreatedRoom()
    {

    }
    public override void OnJoinedRoom()
    {
        if (string.IsNullOrWhiteSpace(MenuManager.Instance.characterNameText.text))
        {
            PhotonNetwork.LeaveRoom();
            return;
        }
        PhotonNetwork.LoadLevel("Game");

    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {

    }
    public override void OnJoinedLobby()
    {
        //PhotonNetwork.LoadLevel("Product_1_Lobby");
        MenuManager.Instance.MyListRenewal();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Product_3_Start");
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //PV.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{newPlayer.NickName}���� �����ϼ̽��ϴ�</color>");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //RoomRenewal();
        //PV.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{otherPlayer.NickName}���� �����ϼ̽��ϴ�</color>");
    }

}
