using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordDatabase
{
    public List<string> words;
}

public class WordLoader : MonoBehaviour
{
    private WordDatabase wordDatabase;
    public string category;
    public string randomWord;

    void Start()
    {
        randomWord = GetRandomWord();
    }

    public void LoadWords()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(category); // Resources/words.json
        if (jsonFile != null)
        {
            wordDatabase = JsonUtility.FromJson<WordDatabase>(jsonFile.text);
        }
        else
        {
            Debug.LogError("json 파일을 찾을 수 없습니다.");
        }
    }

    public string GetRandomWord()
    {
        LoadWords();
        if (wordDatabase != null && wordDatabase.words.Count > 0)
        {
            int index = Random.Range(0, wordDatabase.words.Count);
            return wordDatabase.words[index];
        }
        return "단어 없음";
    }
}