using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using System;
//using DesignPatterns.Singleton;

public class GPTBasicController : MonoBehaviour
{
    private OpenAIApi openAI = new OpenAIApi(/*"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE3NDA1Mzg4MDIsIm5iZiI6MTc0MDUzODgwMiwiZXhwIjoxNzUwNDYzOTk5LCJrZXlfaWQiOiJiYTJhNGNmYS04NWI4LTQ4MDUtYTAzZi05MDQ3ZGRlODdmZDMifQ.YcRHfdX4ZTer9E2EmApVKYdtJ1Gl9q1MYJ0eyN8HmsM"*/"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");
    public int like_value = 0;

    public string npcname;
    public string personality;
    public string restrictions;
    public string knowledge;

    public void SetChatAISystem(string name, string personality, string restrictions, string knowledge)
    {
        this.npcname = name;
        this.personality = personality;
        this.restrictions = restrictions;
        this.knowledge = knowledge;

    }
    public async Task<string> SendMessageToGPTAndGetAnswer(string message)
    {
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        List<ChatMessage> messages = new List<ChatMessage>();
        messages.Add(AddMessage("system", $"name : {npcname}"));
        messages.Add(AddMessage("system", $"personality :{personality}"));
        messages.Add(AddMessage("system", $"restrictions : {restrictions}"));
        messages.Add(AddMessage("system", $"knowledge : {knowledge}"));
        messages.Add(AddMessage("user", message));

        request.Messages = messages;
        //request.Model = "helpy-pro";
        request.Model = "gpt-4o-mini";
        request.Temperature = 0.8f;
        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            return chatResponse.Content;

        }
        return "응답을 받지 못했습니다.";
    }
    private ChatMessage AddMessage(string role, string content)
    {
        ChatMessage message = new ChatMessage();
        message.Role = role;
        message.Content = content;
        return message;
    }
}