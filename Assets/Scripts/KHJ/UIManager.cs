using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Voice.Unity;
using Photon.Voice.Unity.Demos;
using ExitGames.Client.Photon;

public class UIManager : MonoBehaviourPunCallbacks
{
    public static UIManager Instance { get; private set; }

    public PlayerController playerController;

    public GameObject suntextUI;
    public GameObject optionPanel;
    public GameObject HowToPlayPanel;

    public Transform playerListContent;
    public GameObject playerNameItemPrefab;
    public TMP_Text myVoice;

    private bool isSunTextActive = false;
    private bool isOptionOpen = false;

    private List<GameObject> currentPlayerNameItems = new List<GameObject>();
    private Dictionary<int, float> personalMics = new Dictionary<int, float>();
    private Dictionary<int, float> personalVolumes = new Dictionary<int, float>();
    private List<GameObject> pooledItems = new List<GameObject>();
    private int maxPlayers = 8;

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

    private void Start()
    {
        InitPool();
        if (GameManager.Instance.LocalPlayerInstance != null)
        {
            playerController = GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            OptionPannelSetActive();
        }
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void InitPool()
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            GameObject item = Instantiate(playerNameItemPrefab, playerListContent);
            item.SetActive(false);
            pooledItems.Add(item);
        }
    }

    public void UpdatePlayerListFromPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Photon is not connected.");
            return;
        }

        string myName = PhotonNetwork.LocalPlayer.NickName;
        myVoice.text = myName;

        List<Player> otherPlayers = PhotonNetwork.PlayerList
            .Where(p => !p.IsLocal)
            .ToList();

        UpdatePlayerNameList(otherPlayers);
    }
public void UpdatePlayerNameList(List<Player> otherPlayers)
    {
        foreach (GameObject obj in pooledItems)
            obj.SetActive(false);

        for (int i = 0; i < otherPlayers.Count; i++)
        {
            GameObject item = pooledItems[i];
            item.SetActive(true);

            TMP_Text text = item.GetComponentInChildren<TMP_Text>();
            Toggle toggle = item.GetComponentInChildren<Toggle>();
            Slider slider = item.GetComponentInChildren<Slider>();

            Player player = otherPlayers[i];
            int actorNumber = player.ActorNumber;
            int index = actorNumber - 1;

            if (text != null)
                text.text = player.NickName;

            if (index >= 0 && index < GameManager.Instance.spawning.playerControllers.Count)
            {
                PlayerController controller = GameManager.Instance.spawning.playerControllers[index];

                if (controller != null)
                {
                    Speaker speaker = controller.GetComponentInChildren<Speaker>();
                    if (speaker != null)
                    {
                        AudioSource audioSource = speaker.GetComponent<AudioSource>();
                        if (audioSource != null)
                        {
                            if (slider != null)
                            {
                                slider.onValueChanged.RemoveAllListeners();
                                if (personalVolumes.TryGetValue(actorNumber, out float savedVolume))
                                    slider.value = savedVolume;
                                else
                                    slider.value = 1f;
                                slider.onValueChanged.AddListener((value) =>
                                {
                                    personalVolumes[actorNumber] = value;
                                    if (personalMics.TryGetValue(actorNumber, out float savedVolume))
                                    {
                                        audioSource.volume = value * savedVolume;
                                    }
                                    else
                                        audioSource.volume = value;
                                });
                            }

                            if (toggle != null)
                            {
                                toggle.onValueChanged.RemoveAllListeners();
                                toggle.isOn = !speaker.enabled;
                                toggle.onValueChanged.AddListener((isMuted) =>
                                {
                                    speaker.enabled = !isMuted;
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("MicGain"))
        {
            float micGain = (float)changedProps["MicGain"];
            int actorNumber = targetPlayer.ActorNumber;
            int index = actorNumber - 1;

            personalMics[actorNumber] = micGain;

            if (index >= 0 && index < GameManager.Instance.spawning.playerControllers.Count)
            {
                PlayerController controller = GameManager.Instance.spawning.playerControllers[index];

                if (controller != null)
                {
                    Speaker speaker = controller.GetComponentInChildren<Speaker>();
                    if (speaker != null)
                    {
                        AudioSource audioSource = speaker.GetComponent<AudioSource>();
                        if (audioSource != null)
                        {
                            if (personalVolumes.TryGetValue(actorNumber, out float savedVolume))
                            {
                                audioSource.volume = micGain * savedVolume;
                            }
                            else
                                audioSource.volume = micGain;
                        }
                    }
                }
            }
        }
    }
    public void HowToPlayButton()
    {
        if (HowToPlayPanel.activeSelf)
        {
            HowToPlayPanel.SetActive(false);
        }
        else
        {
            HowToPlayPanel.SetActive(true);
        }
    }


    public void SunTextUI()
    {
        suntextUI.SetActive(!isSunTextActive);
        isSunTextActive = !isSunTextActive;
    }

    public void OptionPannelSetActive()
    {
        if (personalMics.Count == 0)
        {
            //Debug.Log("personalMics is empty");
        }
        else
        {
            foreach (KeyValuePair<int, float> pair in personalMics)
            {
                //Debug.Log($"personalMics Key: {pair.Key}, Value: {pair.Value}");
            }
        }

        if (personalVolumes.Count == 0)
        {
            //Debug.Log("personalVolumes is empty");
        }
        else
        {
            foreach (KeyValuePair<int, float> pair in personalVolumes)
            {
                //Debug.Log($"personalVolumes Key: {pair.Key}, Value: {pair.Value}");
            }
        }

        isOptionOpen = !isOptionOpen;
        playerController = GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>();
        playerController?.SetPlayerInputBlocked(isOptionOpen);

        optionPanel.SetActive(isOptionOpen);
    }
}