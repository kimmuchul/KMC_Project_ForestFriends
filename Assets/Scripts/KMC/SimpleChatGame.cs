// ============= SimpleChatGame.cs =============
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEditor;

public class SimpleChatGame : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField inputField;      // 메시지 입력창
    public Button sendButton;             // 전송 버튼
    public TextMeshProUGUI chatDisplay;   // 채팅 표시 (하나의 텍스트로)
    public TextMeshProUGUI turnInfo;      // 현재 턴 정보

    // 게임 상태
    [SerializeField]
    private List<Player> players = new List<Player>();
    [SerializeField] private int currentTurn = 1;
    private string chatHistory = "";

    void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
        inputField.onEndEdit.AddListener(OnEnterPressed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            
            if (PhotonNetwork.IsMasterClient)
            {
                
                photonView.RPC("SetupPlayers", RpcTarget.All);
                StartGame();
                
            }

        }
    }
   [PunRPC]
    void SetupPlayers()
    {
        players.Clear();
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            players.Add(p);
        }
        players.Sort((a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
    }
    void StartGame()
    {
        photonView.RPC("UpdateTurn", RpcTarget.All, 0);
        photonView.RPC("AddToChatHistory", RpcTarget.All, "=== 대화 시작 ===");
    }
    public void SendMessage()
    {
        string message = inputField.text.Trim();

        if (string.IsNullOrEmpty(message)) return;
        if (!IsMyTurn()) return;

        photonView.RPC("ShowMessage", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber.ToString(), message);

        inputField.text = "";
                       
        NextTurn();
    }

    void NextTurn()
    {
        currentTurn = (currentTurn + 1) % players.Count;
        photonView.RPC("UpdateTurn", RpcTarget.All, currentTurn);
    }
    void OnEnterPressed(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendMessage();
        }
    }
    bool IsMyTurn()
    {
        if (currentTurn >= players.Count) return false;
        return players[currentTurn] == PhotonNetwork.LocalPlayer;
    }

    [PunRPC]
    void UpdateTurn(int turnIndex)
    {
        currentTurn = turnIndex;

        if (turnIndex < players.Count)
        {
            Player currentPlayer = players[turnIndex];
            turnInfo.text = $"현재 턴: {currentPlayer.NickName}";

            // 내 턴이면 초록색, 아니면 흰색
            turnInfo.color = IsMyTurn() ? UnityEngine.Color.green : Color.white;

            // 입력창 활성화/비활성화
            inputField.interactable = IsMyTurn();
            sendButton.interactable = IsMyTurn();

            if (IsMyTurn())
            {
                inputField.Select();
            }
        }
    }
    [PunRPC]
    void ShowMessage(string playerName, string message)
    {
        // 현재 시간 추가
        string timeStamp = System.DateTime.Now.ToString("HH:mm");
    
        // 메시지 포맷
        string formattedMessage;
        if (playerName == PhotonNetwork.LocalPlayer.NickName)
        {
            formattedMessage = $"<color=blue>[{timeStamp}] {playerName}: {message}</color>";
        }
        else
        {
            formattedMessage = $"<color=gray>[{timeStamp}] {playerName}: {message}</color>";
        }
    
        // 채팅에 추가
        AddToChatHistory(formattedMessage);
    }
    [PunRPC]
    void AddToChatHistory(string message)
    {
        chatHistory += message + "\n";
        chatDisplay.text = chatHistory;

        Canvas.ForceUpdateCanvases();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetupPlayers();
        photonView.RPC("AddToChatHistory", RpcTarget.All, $">>> {newPlayer.NickName}님이 입장했습니다.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetupPlayers();
        photonView.RPC("AddToChatHistory", RpcTarget.All, $">>> {otherPlayer.NickName}님이 퇴장했습니다.");

        if (PhotonNetwork.IsMasterClient && players.Count > 0)
        {
            int newTurn = currentTurn % players.Count;
            photonView.RPC("UpdateTurn", RpcTarget.All, newTurn);
        }
    }
}