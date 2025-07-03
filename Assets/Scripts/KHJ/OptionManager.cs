using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Photon.Pun;
using Photon.Voice.Unity;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; }

    public UIManager uiManager;
    public PlayerController playerController;
    public GameManager gameManager;

    [Header("Volume Sliders")]
    public Slider masterVolumeSlider;
    public Slider BGMSlider;
    public Slider SFXSlider;
    public Slider myMicSlider;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Toggles")]
    public Toggle toggleWindowed;
    public Toggle toggleFullscreen;
    public Toggle toggleBorderless;
    public Toggle masterToggle;
    public Toggle bgmToggle;
    public Toggle sfxToggle;
    public Toggle myVoiceToggle;

    private float currentMasterVolume = 0.2f;
    private float savedMasterVolume = 0.2f;

    private float currentBGMVolume = 1.0f;
    private float savedBGMVolume = 1.0f;

    private float currentSFXVolume = 1.0f;
    private float savedSFXVolume = 1.0f;

    private float currentMicVolume = 1.0f;
    private float savedMicVolume = 1.0f;

    public bool masterMute = false;
    public bool sfxMute = false;
    public bool bgmMute = false;

    private FullScreenMode currentScreenMode, savedScreenMode;
    private Vector2Int currentResolution, savedResolution;

    private Dictionary<int, Speaker> playerSpeakers = new Dictionary<int, Speaker>();
    public Recorder recorder;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        currentResolution = savedResolution = new Vector2Int(Screen.width, Screen.height);
        currentScreenMode = savedScreenMode = Screen.fullScreenMode;

        savedMasterVolume = currentMasterVolume = masterVolumeSlider.value;
        savedBGMVolume = currentBGMVolume = BGMSlider.value;
        savedSFXVolume = currentSFXVolume = SFXSlider.value;
        savedMicVolume = currentMicVolume = myMicSlider.value;

        myMicSlider.onValueChanged.AddListener(OnMicSliderChanged);
    }

    private void OnEnable()
    {
        if (GameManager.Instance.LocalPlayerInstance != null)
        {
            playerController = GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>();
            SetRecorder();
        }

        uiManager.UpdatePlayerListFromPhoton();

        // 복원
        currentMasterVolume = savedMasterVolume;
        currentBGMVolume = savedBGMVolume;
        currentSFXVolume = savedSFXVolume;
        currentMicVolume = savedMicVolume;

        masterVolumeSlider.value = currentMasterVolume;
        BGMSlider.value = currentBGMVolume;
        SFXSlider.value = currentSFXVolume;
        myMicSlider.value = currentMicVolume;

        ApplyVolume();
        ApplyMicVolumeTemp();
        ApplyToggle(savedScreenMode);
    }

    public void ContinueGame()
    {
        playerController?.SetPlayerInputBlocked(false);
    }

    // === 슬라이더 핸들러 ===
    public void OnMasterSliderChanged()
    {
        currentMasterVolume = masterVolumeSlider.value;
        SetVolume("Master", currentMasterVolume, masterMute);
    }

    public void OnBgmSliderChanged()
    {
        currentBGMVolume = BGMSlider.value;
        SetVolume("BGM", currentBGMVolume, bgmMute);
    }

    public void OnSfxSliderChanged()
    {
        currentSFXVolume = SFXSlider.value;
        SetVolume("SFX", currentSFXVolume, sfxMute);
    }

    public void OnMicSliderChanged(float value)
    {
        currentMicVolume = value;
        ApplyMicVolumeTemp();
    }

    private void ApplyMicVolumeTemp()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "MicGain", currentMicVolume } });
    }

    private void SetVolume(string parameterName, float value, bool mute)
    {
        float dB = (value <= 0.0001f || mute) ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameterName, dB);
    }

    // === 화면 모드 ===
    public void OnWindowedToggle(bool isOn)
    {
        if (isOn)
        {
            currentScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(currentResolution.x, currentResolution.y, currentScreenMode);
        }
    }

    public void OnFullscreenToggle(bool isOn)
    {
        if (isOn)
        {
            currentScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.SetResolution(currentResolution.x, currentResolution.y, currentScreenMode);
        }
    }

    public void OnBorderlessToggle(bool isOn)
    {
        if (isOn)
        {
            currentScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(currentResolution.x, currentResolution.y, currentScreenMode);
        }
    }

    public void SetResolution(int width, int height)
    {
        currentResolution = new Vector2Int(width, height);
        Screen.SetResolution(width, height, currentScreenMode);
    }

    public void SetResolution_2560x1440() => SetResolution(2560, 1440);
    public void SetResolution_1920x1080() => SetResolution(1920, 1080);
    public void SetResolution_1600x900() => SetResolution(1600, 900);

    private void ApplyToggle(FullScreenMode mode)
    {
        toggleWindowed.isOn = (mode == FullScreenMode.Windowed);
        toggleFullscreen.isOn = (mode == FullScreenMode.ExclusiveFullScreen);
        toggleBorderless.isOn = (mode == FullScreenMode.FullScreenWindow);
    }

    private void ApplyVolume()
    {
        SetVolume("Master", currentMasterVolume, masterMute);
        SetVolume("BGM", currentBGMVolume, bgmMute);
        SetVolume("SFX", currentSFXVolume, sfxMute);
    }

    // === 적용 / 취소 ===
    public void OnApply()
    {
        savedMasterVolume = currentMasterVolume;
        savedBGMVolume = currentBGMVolume;
        savedSFXVolume = currentSFXVolume;
        savedMicVolume = currentMicVolume;

        savedScreenMode = currentScreenMode;
        savedResolution = currentResolution;

        ApplyVolume();
        ApplyMicVolumeTemp();
        ApplyToggle(savedScreenMode);

        Screen.SetResolution(savedResolution.x, savedResolution.y, savedScreenMode);

        playerController?.SetPlayerInputBlocked(false);
        gameObject.SetActive(false);
    }

    public void OnCancel()
    {
        currentMasterVolume = savedMasterVolume;
        currentBGMVolume = savedBGMVolume;
        currentSFXVolume = savedSFXVolume;
        currentMicVolume = savedMicVolume;

        masterVolumeSlider.value = currentMasterVolume;
        BGMSlider.value = currentBGMVolume;
        SFXSlider.value = currentSFXVolume;
        myMicSlider.value = currentMicVolume;

        ApplyVolume();
        ApplyMicVolumeTemp();

        currentScreenMode = savedScreenMode;
        currentResolution = savedResolution;

        Screen.SetResolution(savedResolution.x, savedResolution.y, savedScreenMode);

        playerController?.SetPlayerInputBlocked(false);
        gameObject.SetActive(false);
    }

    // === 음소거 ===
    public void MasterMute()
    {
        masterMute = masterToggle.isOn;
        SetVolume("Master", currentMasterVolume, masterMute);
    }

    public void BGMMute()
    {
        bgmMute = bgmToggle.isOn;
        SetVolume("BGM", currentBGMVolume, bgmMute);
    }

    public void SFXMute()
    {
        sfxMute = sfxToggle.isOn;
        SetVolume("SFX", currentSFXVolume, sfxMute);
    }

    public void SetMyVoiceMute()
    {
        if (recorder != null)
        {
            recorder.TransmitEnabled = !myVoiceToggle.isOn;
            //Debug.Log($"[Voice] Transmit Enabled: {recorder.TransmitEnabled}");
        }
    }

    private void SetRecorder()
    {
        if (recorder == null && GameManager.Instance.LocalPlayerInstance != null)
        {
            recorder = GameManager.Instance.LocalPlayerInstance.GetComponent<Recorder>();
        }
    }

    public void RegisterSpeaker(int actorNumber, Speaker speaker)
    {
        if (!playerSpeakers.ContainsKey(actorNumber))
        {
            playerSpeakers.Add(actorNumber, speaker);
        }
    }
}
