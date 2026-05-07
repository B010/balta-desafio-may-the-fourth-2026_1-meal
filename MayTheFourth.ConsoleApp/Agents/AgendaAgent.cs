using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace meal1.Agents;

public static class AgendaAgent
{
    private static readonly string Instructions =
        File.ReadAllText(Path.Combine("agentes", "agenda-agent.md"));

    public static ChatCompletionAgent Create(Kernel kernel) => new()
    {
        Name = "AgendaAgent",
        Instructions = Instructions,
        Kernel = kernel
    };

    public static async Task<string> InvokeAsync(ChatCompletionAgent agent, string userMessage)
    {
        var history = new ChatHistory();
        history.AddUserMessage(userMessage);

        var result = new System.Text.StringBuilder();

        await foreach (var response in agent.InvokeAsync(history))
            result.Append(response.Message.Content);

        return result.ToString();
    }
}
