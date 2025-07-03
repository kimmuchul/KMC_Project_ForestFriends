using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SuggestAndAnswerUI : MonoBehaviourPunCallbacks
{
    [Header("Suggest UI Panel")]

    public TMP_Text[] characterNameText;
    public TMP_Text[] AnswerText;
    public Image[] readyCheck;
    //bool isclicked = false;

    [Header("Description UI Panel")]
    public GameObject descriptionUIPanel;
    public TMP_Text keywordText;
    //public TMP_Text writeADescriptionText;
    public TMP_InputField descriptionInputField;
    public Button enterButton;
    public Button xMarkButton;

    public Button gameStartButton;
    public Button ReadyButton;
    public bool isGameStart = false;
    [Header("Answer")]
    public GameObject AnswerPanel;
    public TMP_Text AnswerkeywordText;
    public TMP_InputField AnswerField;
    public Button enterAnswerButton;

    [Header("Vote")]
    public Button yesButton;
    public Button noButton;
    public GameObject voteCanvas;
    private enum Vote { None, Yes, No }
    private Vote currentVote = Vote.None;
    public TMP_Text VoteText;
    public GameObject WarningVotePanel;

    [Header("etc")]
    public bool isOpenDes;
    public GameObject NondescriptionText;
    int nameWidthIndex = 8;
    public PhotonView pv;
    public PhotonView pvSystem;
    public TMP_Text TurnText;
    public TMP_Text startCount;
    public GameObject WinPanel;
    public TMP_Text WinText;
    CanvasGroup cg;
    public float fadeTime = 1f;
    Coroutine fadeCor;

    public Image readyimage;
    Dictionary<int, int> playerUIIndex = new Dictionary<int, int>();
    Dictionary<int, bool> hasSubmitAnswer = new Dictionary<int, bool>();

    private void Start()
    {
        readyimage.color = Color.red;
        descriptionUIPanel.SetActive(false);
        NondescriptionText.SetActive(false);
        xMarkButton.gameObject.SetActive(false);


        enterButton.transform.gameObject.SetActive(false);

        enterButton.onClick.RemoveAllListeners();
        xMarkButton.onClick.RemoveAllListeners();
        gameStartButton.onClick.RemoveAllListeners();
        ReadyButton.onClick.RemoveAllListeners();

        enterButton.onClick.AddListener(() => OnEnterButtonClick());
        xMarkButton.onClick.AddListener(() => SystemManager.instance.XmarkButtonClick());
        //xMarkButton.onClick.AddListener(() => EscButton());
        gameStartButton.onClick.AddListener(() => GameStartButtonClick());
        ReadyButton.onClick.AddListener(() => ReadyButtonClick());

        if (PhotonNetwork.IsMasterClient)
        {
            gameStartButton.interactable = true;
        }
        else
        {
            gameStartButton.interactable = false;
        }

        keywordText.text = "";
        keywordText.alignment = TextAlignmentOptions.Center;

        descriptionInputField.text = "";
        descriptionInputField.transform.gameObject.SetActive(false);

        VoteText.gameObject.SetActive(false);

        for (int i = 0; i < characterNameText.Length; i++)
        {
            characterNameText[i].text = "";
        }
        for (int j = 0; j < AnswerText.Length; j++)
        {
            AnswerText[j].text = "";
        }
        pv = GetComponent<PhotonView>();

        cg = GetComponent<CanvasGroup>();
        startCount.text = "";
        TurnText.text = "Turn " + (SystemManager.instance.turnindex + 1);
        //KeywordDescription();//���� ���� ��ư ������ �װŷ� �̺�Ʈ ���� ����ϸ� ��.
        
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
            {
                if (player.isReady)
                {
                    gameStartButton.interactable = true;
                }
                break;
            }
        }
        else
        {
            gameStartButton.interactable = false;
        }
    }
    // private void Update()
    // {
    //     GameStart();
    // }
    // void GameStart()
    // {
    //     if (PhotonNetwork.IsMasterClient)
    //     {
    //         foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
    //         {
    //             if (player.isReady)
    //             {
    //                 gameStartButton.interactable = true;
    //             }
    //             break;
    //         }
    //     }
    //     else
    //     {
    //         gameStartButton.interactable = false;
    //     }
    // }
    void ReadyButtonClick()
    {
        readyimage.color = Color.green;
        if (PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            pv.RPC("ImageReadyCheck", RpcTarget.AllBuffered, GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().actorNumber);
        }
    }

    void GameStartButtonClick()
    {
        if (!PhotonNetwork.IsMasterClient)
        { return; }

        if (PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            bool isReadytoStart = true;
            foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
            {
                if (!player.isReady)
                {
                    Debug.Log("모든 플레이어가 Ready를 눌러야 합니다.");
                    isReadytoStart = false;
                }
            }
            if (isReadytoStart)
                pv.RPC("RPC_StartCountdown", RpcTarget.AllBuffered);
        }
    }
    
    public void ResetText()
    {
        for (int i = 0; i < characterNameText.Length; i++)
        {
            characterNameText[i].text = "";
        }
        for (int j = 0; j < AnswerText.Length; j++)
        {
            AnswerText[j].text = "";
        }
    }

    [PunRPC]
    void RPC_StartCountdown()
    {
        if (fadeCor != null)
        {
            StopAllCoroutines();
            fadeCor = null;
        }

        fadeCor = StartCoroutine(StartFadeOut());
    }

    IEnumerator StartFadeOut()
    {
        string[] messages = { "3", "2", "1", "Game Start!" };

        for (int i = 0; i < messages.Length; i++)
        {
            //sound추가
            startCount.text = messages[i];

            cg.alpha = 1f;

            yield return StartCoroutine(FadeOut());
        }
        startCount.text = "";
        gameStartButton.interactable = false;

        isGameStart = true;
    }
    
    IEnumerator FadeOut()
    {
        //cg.alpha = 1f;

        float ftime = 0f;
        while (ftime < fadeTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, ftime / fadeTime);
            ftime += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0f;
    }

    void EscButton()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        { descriptionUIPanel.SetActive(false); }
    }
    //IEnumerator

    public void PanelActive()//
    {

        if (!descriptionUIPanel.activeSelf)
        {
            descriptionUIPanel.SetActive(true);
            if (!GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().isMyTurn)
            {
                descriptionInputField.transform.gameObject.SetActive(true);
                descriptionInputField.interactable = true;

                enterButton.transform.gameObject.SetActive(true);
                enterButton.interactable = true;

                xMarkButton.gameObject.SetActive(true);
                xMarkButton.interactable = true;

                descriptionInputField.Select();
                descriptionInputField.ActivateInputField();
                descriptionInputField.text = "";
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().isMyTurn = true;
                SystemManager.instance.isTurnActive = true;
            }
        }
        else
            return;
    }
    public void OnEnterButtonClick()
    {
        string description = descriptionInputField.text;

        if (!string.IsNullOrWhiteSpace(description))
        {
            descriptionUIPanel.SetActive(false);
            isOpenDes = false;
        }
        else
        {
            NondescriptionText.SetActive(true);
            Invoke("DisapearText", 1f);
            return;
        }
        pv.RPC("SendDes", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.NickName, description);
        if (!GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().isSendTurn)
        {
            //SystemManager.instance.EndPlayerTurn();
            pvSystem.RPC("EndPlayerTurn", RpcTarget.All);
            GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().isSendTurn = true;
        }
    }

    [PunRPC]
    void SendDes(string senderNickname, string msg)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].NickName == senderNickname)
            {
                string displayName = senderNickname;
                if (displayName.Length > nameWidthIndex)
                {
                    displayName = displayName.Substring(0, nameWidthIndex - 3) + "...";
                }
                characterNameText[i].text = $"{displayName}:";
                AnswerText[i].text = msg;
                break;
            }
        }
    }
    [PunRPC]
    void ImageReadyCheck(int actorIndex)
    {
        foreach (var player in GameManager.Instance.spawning.playerControllers)
        {
            if (player.actorNumber == actorIndex)
            {
                player.isReady = true;
                readyCheck[actorIndex - 1].color = Color.green;
            }
        }
    }
    public void OnClickAnswerButton()
    {
        string description = AnswerField.text;

        if (!string.IsNullOrWhiteSpace(description))
        {
            GameManager.Instance.systemManager.answer = description;
            pv.RPC("PutAnswer", RpcTarget.All, description);
        }
        else
        {
            NondescriptionText.SetActive(true);
            Invoke("DisapearText", 1f);
            return;
        }
        //pv.RPC("PutAnswer", RpcTarget.All);
        enterAnswerButton.interactable = false;
        StartCoroutine(WaitTimeAnswer(5f));
    }
    IEnumerator WaitTimeAnswer(float time)
    {
        yield return new WaitForSeconds(time);
        pv.RPC("submitanswerTrue", RpcTarget.All);
    }
    [PunRPC]
    void PutAnswer(string Text)
    {
        AnswerkeywordText.text = $"제시어는 {SystemManager.instance.currentKeyword}입니다.\n 입력하신 키워드는{Text}입니다.";
    }
    [PunRPC]
    void submitanswerTrue()
    {
        SystemManager.instance.isSubmitAnswer = true;
    }

    void DisapearText()
    {
        NondescriptionText.SetActive(false);
    }


    #region Voted
    public void OnDisable()
    {
        currentVote = Vote.None;
        yesButton.interactable = true;
        noButton.interactable = true;
        VoteText.text = "0 : 0";
    }

    [PunRPC]
    void RPC_ToggleVoteCanvas(bool isActive)
    {
        voteCanvas.SetActive(isActive);
        VoteText.gameObject.SetActive(isActive);
    }

    public void SetVoteCanvasActive(bool isActive)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("RPC_ToggleVoteCanvas", RpcTarget.All, isActive);
        }
    }

    public void OnYesButtonClick()
    {
        OnVoteButtonClick(Vote.Yes);
    }

    public void OnNoButtonClick()
    {
        OnVoteButtonClick(Vote.No);
    }

    void OnVoteButtonClick(Vote vote)
    {
        if (vote == currentVote) return;

        Vote previousVote = currentVote;
        currentVote = vote;

        pv.RPC("RPC_VoteChanged", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, (int)previousVote, (int)vote);
    }

    [PunRPC]
    void RPC_VoteChanged(int actorNumber, int prevVote, int newVote)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if ((Vote)prevVote == Vote.Yes) SystemManager.instance.yesCount--;
        else if ((Vote)prevVote == Vote.No) SystemManager.instance.noCount--;

        if ((Vote)newVote == Vote.Yes) SystemManager.instance.yesCount++;
        else if ((Vote)newVote == Vote.No) SystemManager.instance.noCount++;

        pv.RPC("RPC_UpdateVoteUI", RpcTarget.All, actorNumber, newVote, SystemManager.instance.yesCount, SystemManager.instance.noCount);
    }

    [PunRPC]
    void RPC_UpdateVoteUI(int actorNumber, int vote, int updatedYesCount, int updatedNoCount)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            currentVote = (Vote)vote;

            yesButton.interactable = (currentVote != Vote.Yes);
            noButton.interactable = (currentVote != Vote.No);
        }
        UpdateVoteCountUI(updatedYesCount, updatedNoCount);
    }

    void UpdateVoteCountUI(int yes, int no) //?ㅼ떆媛꾩쑝濡??ы몴 ?뺣낫 蹂댁씠怨??띠쓣 ???닿굅 ?ъ슜?섎㈃ ??
    {
        VoteText.text = $"{yes} : {no}";
    }

    #endregion

}
