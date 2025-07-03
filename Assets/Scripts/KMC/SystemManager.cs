using System.Collections;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Photon.Voice.PUN;
using Unity.VisualScripting;
using Photon.Realtime;
using System.Runtime.InteropServices.WindowsRuntime;
public enum Phase { Ready, Allocate, Wait, Start, Check, Sun, Present, Vote, Refute, Answer, Complete };


public class SystemManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public TimeUI timeUI;
    public static SystemManager instance;
    public WordLoader wordLoader;
    public List<string> Keyword;
    public Phase systemphase;
    private Phase syncedphase;
    public UIManager uIManager;
    public SuggestAndAnswerUI suggestAndAnswerUI;
    public int currentTurnIndex = 1;
    public int turnindex = 0;
    public List<int> points;
    //public int ReceiveTurnIndex = 0;
    public bool isTurnActive = false;
    public bool checkStarted = false;
    public bool isPresentend = false;
    public bool isAllVoted = false;
    public bool isAllVotedRefute = false;
    public bool isOpenRefutePanel = false;
    public bool isPointed = false;
    public List<int> liarIndex;
    public PlayerController liarPlayer;
    public PlayerController votedLiar;
    public int yesCount = 0;
    public int noCount = 0;
    public string answer = "";
    public bool isResetAll = false;
    public bool isSubmitAnswer = false;
    public string currentKeyword;
    public int currentLiar;
    public bool isPointUp = false;
    public bool isCameraFocused = false;
    public bool isClosed = false;
    public PlayerController winPlayer;
    bool isAllocated = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        isSubmitAnswer = false;
        systemphase = Phase.Ready;
    }

    void Update()
    {
        switch (systemphase)
        {
            case Phase.Ready:
                timeUI.TimeTexting();
                checkStarted = false;
                if (suggestAndAnswerUI.isGameStart)
                {
                    isAllocated = true;
                    systemphase = Phase.Allocate;
                }
                break;
            case Phase.Allocate:

                if (isAllocated)
                {
                    suggestAndAnswerUI.isGameStart = false;
                    photonView.RPC("AllocateAllPlayer", RpcTarget.All);
                    isAllocated = false;
                }
                else
                {
                    systemphase = Phase.Start;
                }
                break;

            case Phase.Wait:
                isSubmitAnswer = false;
                GameManager.Instance.suggestAndAnswerUI.AnswerPanel.SetActive(false);
                StartCoroutine(WaitTimeToStart(2.5f));
                break;

            case Phase.Start:
                suggestAndAnswerUI.ReadyButton.gameObject.SetActive(false);
                suggestAndAnswerUI.readyimage.gameObject.SetActive(false);
                suggestAndAnswerUI.gameStartButton.gameObject.SetActive(false);
                photonView.RPC("ReceiveLiarInfo", RpcTarget.All, liarIndex?[turnindex], Keyword?[turnindex]);
                photonView.RPC("CheckInfo", RpcTarget.All);
                break;

            case Phase.Check:
                if (!string.IsNullOrWhiteSpace(timeUI.timeText.text))
                {
                    timeUI.PhaseCheckTime();
                }

                suggestAndAnswerUI.xMarkButton.gameObject.SetActive(true);
                if (isClosed == true)
                {
                    systemphase = Phase.Sun;
                }
                break;
            case Phase.Sun:

                timeUI.timeText.text = "";

                if (!isCameraFocused)
                {
                    CameraManager.Instance.FocusAllCamerasToTarget();
                    isCameraFocused = true;
                }
                break;
            case Phase.Present:

                if (currentTurnIndex <= GameManager.Instance.spawning.playerControllers.Count) // ������ UIâ ���� �ϴ� ��
                {

                    if (GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().actorNumber == currentTurnIndex)
                    {
                        //pv.RPC("AllocateCurPlayer", RpcTarget.AllBuffered, currentTurnIndex);

                        suggestAndAnswerUI.PanelActive();
                        timeUI.timeText.text = "설명 중...";
                    }
                }

                else
                {
                    GameManager.Instance.suggestAndAnswerUI.WarningVotePanel.SetActive(true);
                    systemphase = Phase.Vote;
                }
                break;

            case Phase.Vote:
                StartCoroutine(VotedPhaseCoroutine());
                int totalVote = 0;
                foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
                {
                    totalVote += player.votedCount;
                    if (totalVote == GameManager.Instance.spawning.playerControllers.Count)
                    {
                        GameManager.Instance.suggestAndAnswerUI.WarningVotePanel.SetActive(false);

                        isAllVoted = true;
                    }
                }
                
                break;

            case Phase.Refute:
                StartCoroutine(RefutePhaseCoroutine());

                if ((yesCount + noCount) == GameManager.Instance.spawning.playerControllers.Count)
                {
                    isAllVotedRefute = true;
                }

                break;
            case Phase.Answer:
                if (isResetAll)
                {
                    turnindex++;
                    systemphase = Phase.Wait;
                }
                if (!GameManager.Instance.suggestAndAnswerUI.AnswerPanel.activeSelf && !isSubmitAnswer)
                {
                    GameManager.Instance.suggestAndAnswerUI.AnswerPanel.SetActive(true);
                }

                if (GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().isLiar)
                {
                    GameManager.Instance.suggestAndAnswerUI.AnswerField.interactable = true;
                    GameManager.Instance.suggestAndAnswerUI.enterAnswerButton.interactable = true;
                }

                break;
            case Phase.Complete:
                GameManager.Instance.suggestAndAnswerUI.WinPanel.SetActive(true);
                StartCoroutine(WaitTimeToComplete(2.5f));
                break;
        }
    }
    void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (systemphase != syncedphase)
            {
                syncedphase = systemphase;
            }
        }
        if (systemphase == Phase.Vote)
        {
            if (isAllVoted)
            {
                List<PlayerController> sortedPlayers = GameManager.Instance.spawning.playerControllers.OrderByDescending(p => p.votedCount).ToList();
                if (sortedPlayers[0].votedCount > sortedPlayers[1].votedCount)
                {
                    ResetVoteSystem();
                    votedLiar = sortedPlayers[0];
                    systemphase = Phase.Refute;
                    StopCoroutine(VotedPhaseCoroutine());
                    return;
                }
                else if (sortedPlayers[0].votedCount == sortedPlayers[1].votedCount)
                {
                    ResetVoteSystem();
                    systemphase = Phase.Vote;
                    VotedPhaseCoroutine().Reset();

                    return;
                }
            }
        }

        if (systemphase == Phase.Refute)
        {
            GameManager.Instance.suggestAndAnswerUI.SetVoteCanvasActive(true);

            if (isAllVotedRefute)
            {
                GameManager.Instance.suggestAndAnswerUI.SetVoteCanvasActive(false);
                isPointUp = true;
                if (yesCount > noCount)
                {
                    if (votedLiar == liarPlayer)//when liar lose
                    {
                        //photonView.RPC("LiarWin", RpcTarget.All);
                        if (isPointUp)
                        {
                            LiarLose();
                        }
                    }
                    else//when liar win
                    {
                        //photonView.RPC("LiarLose", RpcTarget.All);
                        if (isPointUp)
                        {
                            LiarWin();
                        }
                    }
                }
                else
                {
                    StopCoroutine(RefutePhaseCoroutine());
                    ResetVoteSystem();
                    systemphase = Phase.Vote;
                    return;
                }
            }
        }
        if (systemphase == Phase.Answer)
        {
            if (!isSubmitAnswer) return;

            else
            {
                if (answer == currentKeyword)
                {
                    isPointed = true;
                }
                if (isPointed)
                {
                    points[currentLiar - 1] += 2;
                    answer = null;
                    photonView.RPC("SynchronizePoints", RpcTarget.All, points.ToArray());
                    isPointed = false;
                }
                for (int i = 0; i < GameManager.Instance.spawning.playerControllers.Count; i++)
                {
                    if (points[i] > 9)
                    {
                        GameManager.Instance.spawning.playerControllers[i].GetComponent<PlayerController>().isWin = true;
                    }
                }
                foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
                {
                    if (player.isWin)
                    {
                        GameManager.Instance.suggestAndAnswerUI.WinText.text = player.playerNameText.text + "님이 우승하였습니다";
                        systemphase = Phase.Complete;
                    }
                }
                photonView.RPC("ResetEverySystem", RpcTarget.AllBuffered);
                if (isResetAll)
                {
                    turnindex++;
                    systemphase = Phase.Wait;
                }
            }
        }
    }
    [PunRPC]
    void ResetEverySystem()
    {
        GameManager.Instance.suggestAndAnswerUI.AnswerField.text = "";
        GameManager.Instance.suggestAndAnswerUI.AnswerPanel.SetActive(false);
        GameManager.Instance.suggestAndAnswerUI.AnswerField.interactable = false;
        GameManager.Instance.suggestAndAnswerUI.descriptionInputField.text = "";
        GameManager.Instance.suggestAndAnswerUI.descriptionInputField.transform.gameObject.SetActive(false);
        GameManager.Instance.suggestAndAnswerUI.enterButton.transform.gameObject.SetActive(false);
        GameManager.Instance.suggestAndAnswerUI.AnswerkeywordText.text = "정답을 입력해주세요";
        ResetVoteSystem();
        isCameraFocused = false;
        if (isSubmitAnswer)
        {
            isTurnActive = false;
            checkStarted = false;
            isPresentend = false;
            GameManager.Instance.suggestAndAnswerUI.OnDisable();

            currentTurnIndex = 1;
            votedLiar = null;
            liarPlayer = null;
            foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
            {
                player.isMyTurn = false;
                player.isSendTurn = false;
                player.isLiar = false;
                player.isFocusing = false;
                player.isReturning = false;
            }
            isResetAll = true;
            turnindex++;
            systemphase = Phase.Wait;
        }
    }
    void ResetVoteSystem()
    {
        isAllVoted = false;
        isAllVotedRefute = false;
        foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
        {
            foreach (GameObject panel in player.votePanel)
            {
                panel.SetActive(false);
            }
            player.votedCount = 0;
            player.VotedPlayers = -1;
            yesCount = 0;
            noCount = 0;
            answer = null;
        }
        isPointUp = false;
    }
    IEnumerator WaitTime(float time)
    {
        yield return new WaitForSeconds(time);
    }
    IEnumerator WaitTimeToStart(float time)
    {
        yield return new WaitForSeconds(time);
        isResetAll = false;
        systemphase = Phase.Start;
    }
    IEnumerator WaitTimeToComplete(float time)
    {
        yield return new WaitForSeconds(time);
        GameManager.Instance.suggestAndAnswerUI.WinPanel.SetActive(false);
        systemphase = Phase.Ready;
    }

    IEnumerator VotedPhaseCoroutine()
    {
        float totalTime = 180f;
        while (totalTime > 0)
        {
            // UI 업데이트
            if (GameManager.Instance.timerText != null)
            {
                int minutes = Mathf.FloorToInt(totalTime / 60f);
                int seconds = Mathf.FloorToInt(totalTime % 60f);
                GameManager.Instance.timerText.text = $"{minutes:D2}:{seconds:D2}";
            }
            totalTime -= Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator RefutePhaseCoroutine()
    {
        float totalTime = 180f;

        while (totalTime > 0)
        {
            // UI 업데이트
            if (GameManager.Instance.timerText != null)
            {
                int minutes = Mathf.FloorToInt(totalTime / 60f);
                int seconds = Mathf.FloorToInt(totalTime % 60f);
                GameManager.Instance.timerText.text = $"{minutes:D2}:{seconds:D2}";
            }
            totalTime -= Time.deltaTime;
            yield return null;
        }
    }
    public void XmarkButtonClick()
    {
        suggestAndAnswerUI.descriptionUIPanel.SetActive(false);
        isClosed = true;
    }

    public void LiarWin()
    {
        for (int i = 1; i < GameManager.Instance.spawning.playerControllers.Count + 1; i++)
        {
            if (i == currentLiar)
                points[i - 1]++;
            // int actorNumber = PhotonNetwork.PlayerList[liarIndex[turnindex] - 1].ActorNumber;
            // photonView.RPC("SyncPoint", RpcTarget.All, actorNumber, points[liarIndex[turnindex] - 1]);
        }
        photonView.RPC("SynchronizePoints", RpcTarget.All, points.ToArray());
        StopCoroutine(RefutePhaseCoroutine());
        ResetVoteSystem();
        systemphase = Phase.Answer;
    }

    public void LiarLose()
    {
        for (int i = 1; i < GameManager.Instance.spawning.playerControllers.Count + 1; i++)
        {
            if (i != currentLiar)
                points[i - 1]++;
        }
        photonView.RPC("SynchronizePoints", RpcTarget.All, points.ToArray());
        StopCoroutine(RefutePhaseCoroutine());
        ResetVoteSystem();
        systemphase = Phase.Answer;
    }

    [PunRPC]
    public void EndPlayerTurn()
    {
        if (photonView.IsOwnerActive)
        {
            currentTurnIndex++;
        }

        GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().isMyTurn = false;
        isTurnActive = false;
    }

    [PunRPC]
    void AllocateAllPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 50; i++)
            {
                Keyword.Add(wordLoader.GetRandomWord());
                liarIndex.Add(Random.Range(1, GameManager.Instance.spawning.playerControllers.Count + 1));
            }
        }
    }

    [PunRPC]
    void ReceiveLiarInfo(int liarActorNumber, string randomWord)
    {
        suggestAndAnswerUI.TurnText.text = "Turn " + (SystemManager.instance.turnindex + 1);
        currentKeyword = randomWord;
        currentLiar = liarActorNumber;
        foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
        {
            if (player.actorNumber == currentLiar)
            {
                liarPlayer = player;
                player.isLiar = true;
                break;
            }
        }
    }

    [PunRPC]
    void CheckInfo()
    {
        if (GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>().isLiar)
        {
            suggestAndAnswerUI.keywordText.text = "당신은 라이어 입니다!";
        }
        else
        {
            suggestAndAnswerUI.keywordText.text = "제시어는 " + currentKeyword + " 입니다.";
        }

        suggestAndAnswerUI.descriptionUIPanel.SetActive(true);

        systemphase = Phase.Check;
    }

    IEnumerator AutoCloseDescriptionPanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        suggestAndAnswerUI.descriptionUIPanel.SetActive(false);

        checkStarted = true;
    }

    [PunRPC]
    void CheckRPC()
    {
        //suggestAndAnswerUI.descriptionUIPanel.SetActive(true);

        foreach (PlayerController player in GameManager.Instance.spawning.playerControllers)
        {
            suggestAndAnswerUI.keywordText.text = player.isLiar ?
        "당신은 라이어 입니다!" : "제시어는" + currentKeyword + "입니다.";
        }
        StartCoroutine(WaitTime(2.5f));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)systemphase);
        }
        else
        {
            systemphase = (Phase)(int)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void SyncPoint(int actorNumber, int newPoint)
    {
        int index = GetPlayerIndex(actorNumber);
        if (index >= 0 && index < points.Count)
        {
            points[index] = newPoint;
        }
    }
    [PunRPC]
    public void SynchronizePoints(int[] point)
    {
        // 받은 배열을 저장하거나 처리
        this.points = point.ToList();
    }
    int GetPlayerIndex(int actorNumber)
    {
        var players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].ActorNumber == actorNumber)
            {
                return i;
            }
        }
        return -1;
    }
}

