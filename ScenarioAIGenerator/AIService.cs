using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Azure.AI.OpenAI;
using Azure;
using System.Net;
using OpenAI.Chat;

namespace ScenarioAIGenerator
{
  internal class AIService
  {
    private string _API_KEY;
    private const string _RESOURCE = "https://management.azure.com/.default";
    private string _ENDPOINT;
    private readonly string _DEPLOYMENT_NAME;

    public AIService(string API_KEY, string ENDPOINT, string DEPLOYMENT_NAME)
    {
      _ENDPOINT = ENDPOINT;
      _DEPLOYMENT_NAME = DEPLOYMENT_NAME;
      _API_KEY = API_KEY;
    }

    public async Task<string> Completion(string systemMessage, string userMessage)
    {
      var client = new AzureOpenAIClient(new Uri(_ENDPOINT), new AzureKeyCredential(_API_KEY));
      var chatClient = client.GetChatClient(_DEPLOYMENT_NAME);

      ChatCompletion completion = chatClient.CompleteChat(
        [
          // System messages represent instructions or other guidance about how the assistant should behave
          new SystemChatMessage(systemMessage),
          // User messages represent user input, whether historical or the most recent input
          new UserChatMessage(userMessage)
        ]
      );

      return completion.Content[0].Text;
    }
  }
}
