using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChattingSystem : MonoBehaviourPunCallbacks
{
    public static ChattingSystem instance;
    [Header("Chatting UI Panel")]
    public TMP_InputField chatInput;
    public Button enterButton;

    [Header("Chatting UI")]
    public TMP_Text[] chatDialogue;

    [Header("etc")]
    public GameObject chatPanel;
    public PhotonView PV;
    public bool isOpen = false;

    public TMP_Text roomName;

    public TMP_Text[] inPeople_name;
    public GameObject inPeople_nameGameObject;
    public TMP_Text[] inPeople_score;
    public GameObject inPeople_scoreGameObject;
    public Button exitRoomButton;
    public Button ChatUIxMarkButton;
    public Button settingButton;

    public GameObject exitRoomPanel;
    public bool isOpenExitRoomPanel = false;

    public GameObject roomSettingPanel;
    public bool isOpenRoomSettingPanel = false;

    public Button yesButton;
    public Button noButton;

    int nameFieldWidth = 8;

    private void Start()
    {
        chatPanel.SetActive(false);
        inPeople_nameGameObject.SetActive(false);
        inPeople_scoreGameObject.SetActive(false);
        exitRoomPanel.SetActive(false);
        roomSettingPanel.SetActive(false);

        enterButton.onClick.RemoveAllListeners();
        ChatUIxMarkButton.onClick.RemoveAllListeners();
        exitRoomButton.onClick.RemoveAllListeners();
        settingButton.onClick.RemoveAllListeners();
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        enterButton.onClick.AddListener(() => Send());
        ChatUIxMarkButton.onClick.AddListener(() => XMarkButton());
        exitRoomButton.onClick.AddListener(() => OpenExitPanel());
        settingButton.onClick.AddListener(() => OpenSettingPanel());
        noButton.onClick.AddListener(() => CloseExitPanel());
        yesButton.onClick.AddListener(() => ExitButton());

        roomName.text = "";

        for (int i = 0; i < chatDialogue.Length; i++)
        {
            chatDialogue[i].text = "";
        }
        PV = GetComponent<PhotonView>();
    }
    void OpenExitPanel()
    {
        isOpenExitRoomPanel = !isOpenExitRoomPanel;

        if (isOpenExitRoomPanel) { exitRoomPanel.SetActive(true); }
        else { exitRoomPanel.SetActive(false); }

    }
    void CloseExitPanel()
    {
        exitRoomPanel.SetActive(false);
    }
    void OpenSettingPanel()
    {
        isOpenRoomSettingPanel = !isOpenRoomSettingPanel;
        if (isOpenRoomSettingPanel) { roomSettingPanel.SetActive(true); }
        else { roomSettingPanel.SetActive(false); }

    }
    private void Update()
    {
        CurScoreText();
        RoomNameText();

        ChatPanelActive();
    }
    void ExitButton()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Product_3_Lobby");
        }
        else if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        else
        {
            PhotonNetwork.Disconnect();
        }
    }
    void CurScoreText()//점수 획득 시스템 표시
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            inPeople_nameGameObject.SetActive(true);
            inPeople_scoreGameObject.SetActive(true);
        }

        for (int i = 0; i < inPeople_name.Length; i++)
        {
            var players = PhotonNetwork.PlayerList;

            if (i < players.Length)
            {
                string disPlayName;
                string nick = players[i].NickName;

                if (nick.Length > nameFieldWidth)
                {
                    disPlayName = nick.Substring(0, nameFieldWidth - 3) + "...";
                }
                else
                {
                    disPlayName = nick.PadRight(nameFieldWidth);
                }
                inPeople_name[i].text = $"{disPlayName}";
            }
            else
            {
                inPeople_name[i].text = "";
            }
        }
        for (int i = 0; i < inPeople_score.Length; i++)
        {
            if (i < SystemManager.instance.points.Count)
            {
                inPeople_score[i].text = $": {SystemManager.instance.points[i]}";
            }
            else
            {
                inPeople_score[i].text = ": 0";
            }
        }
    }

    void RoomNameText()//방 이름 텍스트
    {
        if (string.IsNullOrWhiteSpace(roomName.text))
        {
            roomName.text = PhotonNetwork.CurrentRoom.Name;
            if (roomName.text.Length > nameFieldWidth)
            {
                roomName.text = roomName.text.Substring(0, nameFieldWidth - 3) + "...";
            }
            roomName.text =
                $"{PhotonNetwork.CurrentRoom.Name}" +
                $"\t{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }
        else return;
    }

    public void ChatPanelActive()//엔터, esc누르면 저절로 창 닫힘 c누르면 채팅창 표시
    {
        if (!chatInput.isFocused && Input.GetKeyDown(KeyCode.C))
        {
            isOpen = !isOpen;
            chatPanel.SetActive(isOpen);

            if (isOpen)
            {
                chatInput.text = "";
                chatInput.Select();
                chatInput.ActivateInputField();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if (chatPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = false;
            chatPanel.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Send();
            chatInput.Select();
            chatInput.ActivateInputField();
        }
    }
    public void Send()//텍스트 보내기
    {
        string nickname = PhotonNetwork.NickName;
        int maxLength = Mathf.Min(nickname.Length, nameFieldWidth - 3);

        string displayName = nickname.Substring(0, maxLength) + (nickname.Length > maxLength ? "..." : "");

        if (string.IsNullOrWhiteSpace(chatInput.text)) { return; }
        else
        {
            PV.RPC("ChatRPC", RpcTarget.All, displayName + " : " + chatInput.text, PhotonNetwork.LocalPlayer.ActorNumber);
            chatInput.text = "";
        }

    }

    [PunRPC] // 
    public void ChatRPC(string msg, int senderViewID)
    {
        bool isInput = false;
        for (int i = 0; i < chatDialogue.Length; i++)
            if (chatDialogue[i].text == "")
            {
                isInput = true;
                chatDialogue[i].text = msg;
                if (PhotonNetwork.LocalPlayer.ActorNumber == senderViewID)
                {
                    chatDialogue[i].color = Color.green;
                }
                else
                {
                    chatDialogue[i].color = Color.white;
                }
                break;
            }
        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            for (int i = 1; i < chatDialogue.Length; i++) chatDialogue[i - 1].text = chatDialogue[i].text;
            chatDialogue[chatDialogue.Length - 1].text = msg;
        }
    }
    void XMarkButton()
    {
        chatPanel.SetActive(false);
        isOpen = false;
    }
}
