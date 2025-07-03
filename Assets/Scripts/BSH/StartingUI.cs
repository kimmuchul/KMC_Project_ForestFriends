using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace BSH.V2
{
    public class StartingUI : MonoBehaviourPunCallbacks
    {
        public Button startButton;
        private void Start()
        {
            startButton.onClick.RemoveAllListeners();

            startButton.onClick.AddListener(() => OnStart());
        }
        public void OnStart()
        {
            if(!PhotonNetwork.IsConnected)
            { PhotonNetwork.LoadLevel("Product_2_Lobby"); }
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}