using UnityEngine;
using Photon.Pun;

public class CameraManager : MonoBehaviourPun
{

    public static CameraManager Instance { get; private set; }

    PlayerController playerController;

    public Transform focusSun;
    public Transform focusSunPosition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // // Update is called once per frame
    // void Update()
    // {
    //     if(Input.GetKeyDown(KeyCode.Space))
    //     {
    //         FocusAllCamerasToTarget();
    //     }
    // }

    public void FocusAllCamerasToTarget()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_FocusCamera", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_FocusCamera()
    {
        var localPlayer = GameManager.Instance.LocalPlayerInstance;
        if (localPlayer != null)
        {
            var playerController = localPlayer.GetComponent<PlayerController>();
            playerController.isMouseOn = false;

            playerController.focusPosition = focusSunPosition;
            playerController.focusTarget = focusSun;

            playerController?.SunWatch();
        }
    }

}