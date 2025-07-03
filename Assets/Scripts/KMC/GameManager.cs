using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    //스폰지역
    // private List<int> usedIndices = new List<int>();
    public Spawning spawning;
    public KeywordPrompt keywordPrompt;
    public UIManager uIManager;
    public ChattingSystem chattingSystem;
    public SuggestAndAnswerUI suggestAndAnswerUI;
    public SystemManager systemManager;
    public TMP_Text timerText;
    public GameObject LocalPlayerInstance;

    private void Awake()
    {
        Instance = this;
    }
}
