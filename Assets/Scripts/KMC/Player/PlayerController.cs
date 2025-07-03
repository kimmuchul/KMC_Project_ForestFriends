using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public TMP_Text playerNameText;
    public int actorNumber;
    public Player photonPlayer;

    public float mouseSensitivity = 100f;
    public GameObject camera;//실제 카메라 instance사용 시 카메라 오프를 위한 것
    public GameObject Forlookcamera;//IK 전용 카메라 대신 사용되는 게임 오브젝트
    public Transform lookPosition;
    float xRotation = 0f;
    float yRotation = 0f;
    private bool playerInputBlocked = false; //플레이어 키입력 가능 여부 변수
    public GameObject[] votePanel;

    private Animator animator;
    private float expressionResetTime = -3f;

    #region 카메라 설정
    public Camera playerCamera;
    public Transform focusTarget; // 문 등 바라볼 대상
    public Transform focusPosition;

    public float camMoveSpeed = 2f;
    public float camRotateSpeed = 2f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public bool isFocusing = false;
    public bool isReturning = false;
    public Vector3 syncedLookAtPos;
    #endregion
    
    public bool isMouseOn = false;
    public bool isReady = false;
    public bool isMyTurn = false;
    public bool isSendTurn = false;
    public int VotedPlayers = -1;
    public bool isLiar = false;
    public int votedCount = 0;
    public bool isWin = false;
    private bool endFocusRequested = false;

    void Awake()
    {
        if (photonView.IsMine)
        {
            GameManager.Instance.LocalPlayerInstance = this.gameObject;
        }
    }


    public void SetPlayerInputBlocked(bool b)
    {
        playerInputBlocked = b;
    }
    public bool GetPlayerInputBlocked()
    {
        return playerInputBlocked;
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;

        actorNumber = player.ActorNumber;

        if (playerNameText != null)
        {
            playerNameText.text = player.NickName;
        }

        GameManager.Instance.spawning.playerControllers.Add(this);
        SystemManager.instance.points.Add(0);
        GameManager.Instance.spawning.playerControllers = GameManager.Instance.spawning.playerControllers.OrderBy(p => p.actorNumber).ToList();
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        photonView.RPC("TurnOnCamera", RpcTarget.All);

        if (Microphone.devices.Length == 0)
        {
            //Debug.Log("마이크를 찾을 수 없습니다");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)//KMC 플레이어 프리팹 움직임 구현현
        {
            if (isMouseOn)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                xRotation -= mouseY;
                yRotation += mouseX;
                xRotation = Mathf.Clamp(xRotation, -30f, 30f); // 상하 제한
                yRotation = Mathf.Clamp(yRotation, -80f, 80f); // 좌우 제한

                camera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
                Forlookcamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isMouseOn = !isMouseOn;
                if (isMouseOn)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Confined;
                }
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                UIreset();
            }


            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
            SetExpression(1); 
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
            SetExpression(2); 
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
            SetExpression(3); 
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
            SetExpression(4); 
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
            SetExpression(5); 
            }
            
            if (expressionResetTime > 0 && Time.time > expressionResetTime)
            {
                SetExpression(0);
                expressionResetTime = -3f;

            }


            if (SystemManager.instance.systemphase == Phase.Vote)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                        {
                            PhotonView targetView = hit.collider.GetComponent<PhotonView>();
                            if (targetView != null && !targetView.IsMine)
                            {
                                if (VotedPlayers == targetView.ViewID)
                                {
                                    targetView.RPC("MinusVote", RpcTarget.AllBuffered);
                                    VotedPlayers = -1;
                                }
                                else if (VotedPlayers == -1)
                                {
                                    targetView.RPC("AddVote", RpcTarget.AllBuffered);
                                    VotedPlayers = targetView.ViewID;
                                }
                                else
                                    return;
                            }
                        }
                    }
                }
        }
        if (!photonView.IsMine)
        {
            lookPosition.position = Vector3.Lerp(lookPosition.position, syncedLookAtPos, Time.deltaTime * 10f);
        }
        SmoothMove();
    }
    void UIreset()
    {
        GameManager.Instance.suggestAndAnswerUI.WarningVotePanel.SetActive(false);
        GameManager.Instance.suggestAndAnswerUI.AnswerPanel.SetActive(false);
        if (SystemManager.instance.systemphase == Phase.Answer)
        {
        if (!GameManager.Instance.suggestAndAnswerUI.AnswerPanel.activeSelf)
            {
                GameManager.Instance.suggestAndAnswerUI.AnswerPanel.SetActive(true);
            }
        }
    }
    void SetExpression(int value)
    {
        animator.SetInteger("Expression", value);
        photonView.RPC("PlayAnimationRPC", RpcTarget.Others, value);

        if (value != 0)
            expressionResetTime = Time.time + 3f;
    }
    [PunRPC]
    void PlayAnimationRPC(int index)
    {
        animator.SetInteger("Expression", index);
    }
    [PunRPC]
    public void AddVote()
    {
        votedCount++;
        votePanel[votedCount - 1].SetActive(true);
    }
    [PunRPC]
    public void MinusVote()
    {
        votedCount--;
        votePanel[votedCount].SetActive(false);
    }

    public void SunWatch()
    {
        GameManager.Instance.keywordPrompt.OnClickRequestKeywords();
        StartFocus();
        GameManager.Instance.uIManager.SunTextUI();
    }

    [PunRPC]
    void TurnOnCamera()
    {
        if (photonView.IsMine)
        {
            camera.SetActive(true);
        }
    }
    private void OnAnimatorIK(int layerIndex)//KMC IK애니메이션 구현현
    {
        animator.SetLookAtWeight(1f);
        animator.SetLookAtPosition(lookPosition.position);
    }


    public void StartFocus()
    {
        if (isFocusing || isReturning) return;
        SetPlayerInputBlocked(true);
        // 타겟 위치/회전 계산
        originalPosition = playerCamera.transform.position;
        originalRotation = playerCamera.transform.rotation;
        Vector3 dir = (focusTarget.position - focusPosition.transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);

        // 저장
        targetPosition = focusPosition.position;
        targetRotation = lookRot;

        isFocusing = true;
        isReturning = false;
    }

    public void EndFocus()
    {
        if (isFocusing || isReturning) return;
        SetPlayerInputBlocked(true);
        targetPosition = originalPosition;
        targetRotation = originalRotation;
        

        isFocusing = false;
        isReturning = true;

        endFocusRequested = true;
        
        SystemManager.instance.systemphase = Phase.Present;
    }

    private void SmoothMove()
    {
        if (isFocusing || isReturning)
        {
            animator.speed = 0;
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPosition, camMoveSpeed * Time.deltaTime);
            playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, targetRotation, camRotateSpeed * Time.deltaTime);

            // 거의 도달했으면 정확하게 고정
            if (Vector3.Distance(playerCamera.transform.position, targetPosition) < 0.01f &&
                Quaternion.Angle(playerCamera.transform.rotation, targetRotation) < 0.5f)
            {
                playerCamera.transform.position = targetPosition;
                playerCamera.transform.rotation = targetRotation;

                if (isReturning && endFocusRequested)
                {
                    SetPlayerInputBlocked(false);
                    //IKLookController.Instance.playerInputBlocked = false;
                    endFocusRequested = false;
                    animator.speed = 1;
                }
                isFocusing = false;
                isReturning = false;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(lookPosition.position);
        }
        else if (stream.IsReading)
        {
            syncedLookAtPos = (Vector3)stream.ReceiveNext();
        }
    }

}
