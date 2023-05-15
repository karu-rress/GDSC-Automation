using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Webhook;
using Discord.Commands;
using System.Reflection;
using Octokit;

namespace githubstats
{
    class Program
    {
        static async Task Main(string[] _)
        {
            await new GitHubStats().RunAsync();
        }
    }
}