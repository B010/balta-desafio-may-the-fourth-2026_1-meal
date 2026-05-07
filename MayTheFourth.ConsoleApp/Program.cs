using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using meal1.Agents;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

UI.PrintBanner();

// --- Configuração ---
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .Build();

var modelId = config["OpenAI:ModelId"] ?? throw new InvalidOperationException("Configure OpenAI:ModelId");
var apiKey  = config["OpenAI:ApiKey"]  ?? throw new InvalidOperationException("Configure OpenAI:ApiKey");

// --- Kernel ---
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(modelId, apiKey)
    .Build();

// --- Entrada ---
UI.PrintModule(1, "INGREDIENTES");
Console.Write("  Ingredientes disponíveis: ");
var ingredientsInput = UI.ReadInput();

UI.PrintModule(2, "AGENDA");
Console.Write("  Agenda de hoje: ");
var scheduleInput = UI.ReadInput();

// --- Agentes ---
var ingredientsAgent = IngredientsAgent.Create(kernel);
var agendaAgent      = AgendaAgent.Create(kernel);
var recipeAgent      = RecipeAgent.Create(kernel);

// --- Processamento ---
UI.PrintAgentHeader("IngredientsAgent", "Categorizando ingredientes");
var ingredientsResult = await IngredientsAgent.InvokeAsync(ingredientsAgent, ingredientsInput);
UI.PrintAgentOutput(ingredientsResult);

UI.PrintAgentHeader("AgendaAgent", "Mapeando janelas livres");
var agendaResult = await AgendaAgent.InvokeAsync(agendaAgent, scheduleInput);
UI.PrintAgentOutput(agendaResult);

UI.PrintAgentHeader("RecipeAgent", "Cruzando dados e gerando receitas");
var recipePrompt = $"""
    Ingredientes disponíveis:
    {ingredientsResult}

    Períodos livres na agenda:
    {agendaResult}
    """;
var recipeResult = await RecipeAgent.InvokeAsync(recipeAgent, recipePrompt);

UI.PrintMissionResult(recipeResult);
UI.PrintExit();

// ─────────────────────────────────────────────
//  UI Helper
// ─────────────────────────────────────────────
static class UI
{
    private const int Width = 60;

    public static void PrintBanner()
    {
        try { Console.Clear(); } catch { /* terminal sem suporte a Clear (ex: Debug Console do VS Code) */ }
        var line = new string('═', Width - 2);

        Fg(ConsoleColor.DarkCyan);
        Console.WriteLine($"\n  ╔{line}╗");

        Fg(ConsoleColor.Cyan);
        Console.WriteLine($"  ║{"🚀  MEAL PLANNER  ·  MAY THE FOURTH 2026".PadCenter(Width - 2)}║");

        Fg(ConsoleColor.DarkCyan);
        Console.WriteLine($"  ║{"Agentes de IA para Planejamento de Refeições".PadCenter(Width - 2)}║");
        Console.WriteLine($"  ╚{line}╝");

        Fg(ConsoleColor.DarkGray);
        Console.WriteLine($"  modelo: gpt-4o  ·  semantic kernel  ·  .net 9");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintModule(int number, string title)
    {
        Console.WriteLine();
        Fg(ConsoleColor.Yellow);
        Console.Write($"  ▸ MÓDULO {number}");
        Fg(ConsoleColor.DarkYellow);
        Console.WriteLine($"  {title}");
        Fg(ConsoleColor.DarkGray);
        Console.WriteLine($"  {new string('─', Width - 4)}");
        Console.ResetColor();
    }

    public static string ReadInput()
    {
        Fg(ConsoleColor.White);
        var input = Console.ReadLine() ?? string.Empty;
        Console.ResetColor();
        return input;
    }

    public static void PrintAgentHeader(string agentName, string task)
    {
        Console.WriteLine();
        Fg(ConsoleColor.DarkGray);
        Console.Write("  ┌─ ");
        Fg(ConsoleColor.Magenta);
        Console.Write(agentName);
        Fg(ConsoleColor.DarkGray);
        Console.WriteLine($"  ·  {task}...");
        Console.ResetColor();
    }

    public static void PrintAgentOutput(string content)
    {
        Fg(ConsoleColor.DarkGray);
        Console.Write("  │ ");
        Fg(ConsoleColor.Green);

        foreach (var line in content.Split('\n'))
            Console.WriteLine($"  {line}");

        Fg(ConsoleColor.DarkGray);
        Console.WriteLine("  └" + new string('─', Width - 4));
        Console.ResetColor();
    }

    public static void PrintMissionResult(string content)
    {
        Console.WriteLine();
        var line = new string('═', Width - 2);

        Fg(ConsoleColor.Yellow);
        Console.WriteLine($"  ╔{line}╗");
        Console.WriteLine($"  ║{"✦  MISSÃO CULINÁRIA — SUAS RECEITAS  ✦".PadCenter(Width - 2)}║");
        Console.WriteLine($"  ╚{line}╝");
        Console.ResetColor();
        Console.WriteLine();

        Fg(ConsoleColor.White);
        foreach (var l in content.Split('\n'))
            Console.WriteLine($"  {l}");

        Console.ResetColor();
    }

    public static void PrintExit()
    {
        Console.WriteLine();
        Fg(ConsoleColor.DarkGray);
        Console.WriteLine($"  {'─'.Repeat(Width - 4)}");
        Console.Write("  FIM DA MISSÃO");
        Fg(ConsoleColor.DarkCyan);
        Console.Write("  ·  Pressione qualquer tecla para sair");
        Console.ResetColor();
        try { Console.ReadKey(intercept: true); } catch { Console.ReadLine(); }
        Console.WriteLine();
    }

    private static void Fg(ConsoleColor color) => Console.ForegroundColor = color;
}

static class StringExtensions
{
    public static string PadCenter(this string s, int width)
    {
        var padding = width - s.Length;
        if (padding <= 0) return s;
        var left  = padding / 2;
        var right = padding - left;
        return new string(' ', left) + s + new string(' ', right);
    }

    public static string Repeat(this char c, int count) => new(c, count);
}
