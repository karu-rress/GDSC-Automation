using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.Commands;
using System.Reflection;
using Octokit;
using Discord.Rest;
using Octokit.Webhooks.Models;

namespace githubstats;

internal class GitHubStats
{
    private DiscordSocketClient client;
    private DiscordSocketConfig config = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
    };
    private Dictionary<string, Func<SocketUserMessage, Task<RestUserMessage>>> commands;
    private GitHubClient github = new(new ProductHeaderValue("GDSC-Automation"));
    private Octokit.User gdsc;

    public GitHubStats()
    {
        client = new(config);
        gdsc = github.User.Get("GDSC-CAU").GetAwaiter().GetResult();
        github.Credentials = new(File.ReadAllText("./token.txt"));
        var repos = github.Repository.GetAllForCurrent().GetAwaiter().GetResult();
        string result = "";
        foreach (var item in repos)
        {
            result += item.FullName + "\n";
        }
        commands = new()
        {
            ["!test"] = (msg) => SendAsync(msg, "Hi, Karu! How are you doing now?"),
            ["!ping"] = (msg) => SendAsync(msg, "pong!"),
            ["!debug"] = PrintDebugInfoAsync,
            ["!followers"] = (msg) => SendAsync(msg, $"Whoa, you have {gdsc.Followers} followers!"),
            ["!bio"] = (msg) => SendAsync(msg, gdsc.Bio),
            ["!created"] = (msg) => SendAsync(msg, gdsc.CreatedAt),
            ["!repos"] = (msg) => SendAsync(msg, result)
        };
    }

    private Task<RestUserMessage> PrintDebugInfoAsync(SocketUserMessage message)
    {
        return message.Channel.SendMessageAsync(
            $@"Content: {message.Content}
Author username: {message.Author.Username}
Author: {message.Author}
Author Mention: {message.Author.Mention}
Current User: {client.CurrentUser}
Current username: {client.CurrentUser.Username}");

        
    }

    private Task<RestUserMessage> SendAsync<T>(SocketUserMessage message, T text)
        where T: notnull
    {
        return message.Channel.SendMessageAsync(text.ToString());
    }

    public async Task RunAsync()
    {
        client.Log += (log) =>
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        };

        client.Ready += () =>
        {
            Console.WriteLine($"{client.CurrentUser} 연결됨!");
            return Task.CompletedTask;
        };

        client.MessageReceived += HandleCommandAsync;
        //  You can assign your bot token to a string, and pass that in to connect.
        //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
        // var token = "token";

        // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
        // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
        // var token = File.ReadAllText("token.txt");
        // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

        await client.LoginAsync(TokenType.Bot, File.ReadAllText("botauth.txt"));
        await client.StartAsync();

        await Task.Delay(-1);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message is null) return;

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix
        // and make sure no bots trigger commands
        if (!(message.HasCharPrefix('!', ref argPos) ||
            message.HasMentionPrefix(client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        // Create a WebSocket-based command context based on the message
        // var context = new SocketCommandContext(client, message);

        await commands[message.Content](message);
    }
}