
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using OpenAI;
using Photon.Pun;
using System.Collections;

public class KeywordPrompt : MonoBehaviourPun
{
    public GameObject keywordOutputText;        // GPT가 뽑은 설명 출력
    public GPTBasicController gptController;  // GPT 컨트롤러 연결
    public WordLoader wordLoader;

    private TypingEffect EffectText;

    public void OnClickRequestKeywords()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        string topic = wordLoader?.randomWord;
        //Debug.Log("사용자가 입력한 주제: " + topic);
        RequestKeywordHint(topic);
    }

    private async void RequestKeywordHint(string topic)
    {
        //Debug.Log("GPT 호출 시작, 주제: " + topic);

        gptController.SetChatAISystem(
            name: "라이어게임 설명 봇",
            personality: "힌트를 주는 말투",
            restrictions: "정답을 직접 말하지 않음. 설명은 짧고 헷갈려야 함.",
            knowledge: ""
        );

        string prompt = $"{topic}이라는 단어를 직접 말하지 말고, 그것을 연상할 수 있는 은유적이고 추상적인 짧은 설명을 해줘. " +
                 "듣는 사람이 바로 알 수 있을 정도로 명확하게 말하지 말고, " +
                 "그 단어와 관련된 분위기나 느낌, 비유적인 표현으로 아주 애매하게 한 문장으로만 설명해주고, " +
                 "예를 들어 키워드가 바나나면 '부드럽고 달아.', '길쭉해.' 이런 느낌으로 설명해줘" +
                 "이 키워드를 모르는 사람은 하나도 모르도록 설명해줘";

        string response = await gptController.SendMessageToGPTAndGetAnswer(prompt);

        //Debug.Log("GPT 응답: " + response);
        ShowKeywordAndDescription(response);

        photonView.RPC(nameof(ReceiveKeywordAndDescription), RpcTarget.OthersBuffered, response);

    }

    [PunRPC]
    void ReceiveKeywordAndDescription(string response)
    {
        ShowKeywordAndDescription(response);
    }

    void ShowKeywordAndDescription(string response)
    {
        keywordOutputText.SetActive(true);
        EffectText = keywordOutputText.GetComponent<TypingEffect>();
        EffectText.StartTyping(response, OnTypingComplete);
    }

    void OnTypingComplete()
    {
        var playerController = GameManager.Instance.LocalPlayerInstance.GetComponent<PlayerController>();

        StartCoroutine(DelayedEndFocus(playerController));
    }

    IEnumerator DelayedEndFocus(PlayerController playerController)
    {
        yield return new WaitForSeconds(1.5f); // 1.5초 대기
        keywordOutputText.SetActive(false);
        playerController.EndFocus();
    }
}