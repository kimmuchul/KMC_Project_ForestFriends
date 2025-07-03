using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

    public class MenuManager : MonoBehaviourPunCallbacks
    {
        public static MenuManager Instance;
        [Header("Lobby Panel")]
        public Button previousButton;
        public Button nextButton;


        [Header("Game Room Panel")]
        public Button[] GameRoom;

        [Header("Character Name Panel")]
        public TMP_Text characterNameText;
        public TMP_InputField setCharacterNameInputField;
        public Button applyNameButton;

        [Header("Room Button Panel")]
        public Button createButton;
        public Button joinButton;

        [Header("Room Create Panel")]
        public GameObject roomCreatePanel;
        public TMP_InputField roomNameCreateInputField;
        public Button createButtonInPanel;
        public Button cancelButtonInPanel;

        [Header("etc")]
        public GameObject setCharacterNameWarning;
        public GameObject nonGameRoom;
        public GameObject nonSelectGameRoom;
        public Button disconnectButton;

        int currentPage = 1, maxPage, multiple;
        List<RoomInfo> myList = new List<RoomInfo>();
        public TMP_Text playerInLobbyText;

        public string selectedRoomName = "";





        int selectedRoomIndex = -3;

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
                //DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            setCharacterNameWarning.SetActive(false);
            nonGameRoom.SetActive(false);
            nonSelectGameRoom.SetActive(false);

            roomCreatePanel.SetActive(false);

            previousButton.onClick.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            applyNameButton.onClick.RemoveAllListeners();
            createButton.onClick.RemoveAllListeners();
            joinButton.onClick.RemoveAllListeners();
            createButtonInPanel.onClick.RemoveAllListeners();
            cancelButtonInPanel.onClick.RemoveAllListeners();
            for (int i = 0; i < GameRoom.Length; i++)
            { GameRoom[i].onClick.RemoveAllListeners(); }
            disconnectButton.onClick.RemoveAllListeners();

            previousButton.onClick.AddListener(() => MyListClick(-2));
            nextButton.onClick.AddListener(() => MyListClick(-1));
            createButton.onClick.AddListener(() => CreateButton());
            cancelButtonInPanel.onClick.AddListener(() => CancelButton());
            applyNameButton.onClick.AddListener(() => CharacterName());
            createButtonInPanel.onClick.AddListener(() => OnCreateRoom());
            joinButton.onClick.AddListener(() => OnJoinRoom());
            for (int i = 0; i < GameRoom.Length; i++)
            {
                int index = i;
                GameRoom[i].onClick.AddListener(() => OnGameRoom());
                GameRoom[i].onClick.AddListener(() => OnClickRoomButton(index));
            }
            disconnectButton.onClick.AddListener(() => DisconnectButton());



            roomCreatePanel.SetActive(false);
            createButton.interactable = false;
            joinButton.interactable = false;
            characterNameText.text = "";
            createButtonInPanel.interactable = false;
        }
        void OnClickRoomButton(int index)
        {
            selectedRoomIndex = index;
            selectedRoomName = myList[multiple + index].Name;


            UpdateRoomButtonHighlights();
        }
        void UpdateRoomButtonHighlights()
        {
            for (int i = 0; i < GameRoom.Length; i++)
            {
                Transform highlight = GameRoom[i].transform.Find("highlight");

                if (highlight != null)
                {
                    highlight.gameObject.SetActive(i == selectedRoomIndex);
                }
            }
        }
        private void Update()
        {
            //if (currentPage < 1) { currentPage = 1; }
            //if (maxPage < 1) { maxPage = 1; }
            //curPage.text = $"{currentPage}/{maxPage}"; //Room Count
            playerInLobbyText.text =
                $"현재 접속중인 인원 :{PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms}/{PhotonNetwork.CountOfPlayers}";

            CreateRoomName();
        }

        public void MyListClick(int num)
        {
            if (num == -2) --currentPage;
            else if (num == -1) ++currentPage;
            else
            {
                selectedRoomName = myList[multiple + num].Name;
            }
            MyListRenewal();
        }
        public void MyListRenewal()
        {
            // 최대페이지
            maxPage = (myList.Count % GameRoom.Length == 0) ? myList.Count / GameRoom.Length : myList.Count / GameRoom.Length + 1;


            // 이전, 다음버튼
            previousButton.interactable = (currentPage <= 1) ? false : true;
            nextButton.interactable = (currentPage >= maxPage) ? false : true;

            // 페이지에 맞는 리스트 대입
            multiple = (currentPage - 1) * GameRoom.Length;
            for (int i = 0; i < GameRoom.Length; i++)
            {
                GameRoom[i].interactable = (multiple + i < myList.Count) ? true : false;
                GameRoom[i].transform.GetChild(0).GetComponent<TMP_Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
                GameRoom[i].transform.GetChild(1).GetComponent<TMP_Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
            }
            UpdateRoomButtonHighlights();
        }
        void CreateButton()
        {
            roomCreatePanel.SetActive(true);
        }
        void CancelButton()
        {
            roomCreatePanel.SetActive(false);
        }
        void CharacterName()
        {
            if (string.IsNullOrWhiteSpace(setCharacterNameInputField.text))
            {
                setCharacterNameWarning.SetActive(true);
                Invoke("DisapearNameWarning", 1f);
                return;
            }
            else
            {
                createButton.interactable = true;
                joinButton.interactable = true;
                characterNameText.text = setCharacterNameInputField.text;
                PhotonNetwork.NickName = characterNameText.text;
            }
        }
        void CreateRoomName()
        {

            if (!string.IsNullOrEmpty(roomNameCreateInputField.text))
            {
                createButtonInPanel.interactable = true;
            }
            else
            {
                createButtonInPanel.interactable = false;
            }
        }
        void DisconnectButton()
        {
            //Destroy(gameObject);
            PhotonNetwork.Disconnect();
        }
        public void OnCreateRoom()
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameCreateInputField.text, roomOptions);
        }
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            int roomCount = roomList.Count;
            for (int i = 0; i < roomCount; i++)
            {
                if (!roomList[i].RemovedFromList)
                {
                    if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                    else myList[myList.IndexOf(roomList[i])] = roomList[i];
                }
                else if (myList.IndexOf(roomList[i]) != -1)
                    myList.RemoveAt(myList.IndexOf(roomList[i]));
            }
            MyListRenewal();
        }
        void OnJoinRoom()
        {
            int index = multiple + selectedRoomIndex;

            if (index < 0 || index >= myList.Count)
            {
                nonGameRoom.SetActive(true);
                Invoke("DisapearNameWarning", 1f);
                return;
            }
            if (selectedRoomIndex < 0 || selectedRoomIndex >= myList.Count)
            {
                nonSelectGameRoom.SetActive(true);
                Invoke("DisapearNameWarning", 1f);
                return;
            }
            // 정상적인 인덱스 범위 내일 경우
            if (string.IsNullOrWhiteSpace(myList[index].Name))
            {
                nonSelectGameRoom.SetActive(true);
                Invoke("DisapearNameWarning", 1f);
            }
            else
            {
                PhotonNetwork.JoinRoom(myList[index].Name);
            }
        }
        void OnGameRoom()
        {
            if (string.IsNullOrWhiteSpace(characterNameText.text))
            {
                setCharacterNameWarning.SetActive(true);
                Invoke("DisapearNameWarning", 1f);
            }
        }

        void DisapearNameWarning()
        {
            setCharacterNameWarning.SetActive(false);
            nonGameRoom.SetActive(false);
            nonSelectGameRoom.SetActive(false);
        }
    }
