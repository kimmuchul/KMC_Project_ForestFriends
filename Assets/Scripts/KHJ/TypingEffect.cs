using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    public TMP_Text targetText;
    public float typingSpeed = 0.1f;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        targetText = GetComponent<TMP_Text>();
    }

    public void StartTyping(string fullText, Action onComplete = null)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(fullText, onComplete));
    }

    private IEnumerator TypeText(string fullText, Action onComplete)
    {
        targetText.text = "";
        for (int i = 0; i <= fullText.Length; i++)
        {
            targetText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }

        onComplete?.Invoke();
    }
}
