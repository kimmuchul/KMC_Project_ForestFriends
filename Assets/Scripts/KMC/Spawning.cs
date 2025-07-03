using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Photon.Pun;
using Photon.Pun.Demo.SlotRacer;
using UnityEngine;
using UnityEngine.UI;
public class Spawning : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public List<PlayerController> playerControllers = new List<PlayerController>();
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // 이 시점에서는 PhotonView를 통한 RPC 호출이 필요하지 않음

        int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
        //Transform spawnPoint = spawnPoints[index];
        Vector3 position = spawnPoints[index].position;
        Quaternion rotation = spawnPoints[index].rotation;

        // Player0, Player1 등의 프리팹 이름일 경우
        GameObject player = PhotonNetwork.Instantiate($"Player{index}", position, rotation);

        // 초기화는 RPC로 전송 (예: 이름, 점수 등 동기화용)
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
    }
}