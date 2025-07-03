using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon; // Photon�� Hashtable

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    public float micGain = 1.0f;

    [Header("Voice Volume")]
    public Slider micVolumeSlider;
    [Range(0f, 3f)]
    private float currentMicVolume = 1.0f;
    [Range(0f, 3f)]
    private float savedMicVolume = 1.0f;

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
        SetMicGain();
    }

    public void SetMicGain()
    {
        micGain = micVolumeSlider.value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "MicGain", micGain } });
        //OptionManager.Instance.myMicVolume = micGain;
    }
}