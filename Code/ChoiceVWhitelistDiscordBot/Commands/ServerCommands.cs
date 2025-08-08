using DiscordBot.Base;
using DiscordBot.Controller;
using DiscordBot.Controller.Whitelist;
using DiscordBot.Model.Database;
using DSharpPlus.SlashCommands;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tommy;

namespace DiscordBot.Commands {
    [SlashCommandGroup("Server", "Commands für den Server")]
    public class DiscordServerCommands : ApplicationCommandModule {
        [SlashCommand("start", "Startet den Server, wenn er aktuell nicht läuft")]
        public Task startServer(InteractionContext ctx) {
            var channel = ctx.Guild.GetChannel(Config.ServerCommandInfoChannel);

            var process = Process.GetProcessesByName("altv-server");
            if(process.Length <= 0) {
                Process.Start($"{Config.AltVServerPath}server-start.bat");
                Program.Bot.sendEmbedInChannel(channel, "Server gestartet", $"Der Server wurde von {ctx.Member.Mention} gestartet! Er könnte ein paar Minuten zum hochfahren benötigen!");
            } else {
                Program.Bot.sendEmbedInChannel(channel, "Server läuft bereits", $"{ctx.Member.Mention} wollte den Server starten, er lief aber bereits!");
            }

            ctx.RespondeToSlashCommand(true);

            return Task.CompletedTask;
        }

        [SlashCommand("stop", "Stoppe den Server, wenn er aktuell läuft")]
        public Task stopServer(InteractionContext ctx) {
            var channel = ctx.Guild.GetChannel(Config.ServerCommandInfoChannel);

            var process = Process.GetProcessesByName("altv-server");
            if(process.Length > 0) {
                Process.Start($"{Config.AltVServerPath}server-stop.bat");
                Program.Bot.sendEmbedInChannel(channel, "Server gestoppt", $"Der Server wurde von {ctx.Member.Mention} gestoppt!");
            } else {
                Program.Bot.sendEmbedInChannel(channel, "Server lief nicht", $"{ctx.Member.Mention} wollte den Server stoppen, er lief aber nicht! Er könnte ein paar Minuten zum hochfahren benötigen!");
            }

            ctx.RespondeToSlashCommand(true);

            return Task.CompletedTask;
        }

        [SlashCommand("restart", "Restarte den Server. Startet ihn wenn er nicht läuft")]
        public async Task restartServer(InteractionContext ctx) {
            var channel = ctx.Guild.GetChannel(Config.ServerCommandInfoChannel);

            Program.Bot.sendEmbedInChannel(channel, "Game-Server restartet", $"Der Game-Server wurde von {ctx.Member.Mention} restartet! Er könnte ein paar Minuten zum hochfahren benötigen!");
            ctx.RespondeToSlashCommand(true);

            Process.Start($"{Config.AltVServerPath}server-stop.bat");
            await Task.Delay(5000);
            Process.Start($"{Config.AltVServerPath}server-start.bat");

            return;
        }

        [SlashCommand("git-pull", "Pulle Modding Repo Updates")]
        public Task pullModding(InteractionContext ctx) {
            var channel = ctx.Guild.GetChannel(Config.ServerCommandInfoChannel);
            ctx.RespondeToSlashCommand(true);

            using(var repo = new Repository($"{Config.AltVServerPath}resources/ChoiceVModding")) {
                // Credential information to fetch
                var options = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials() {
                            Username = Config.GitUser,
                            Password = Config.GitPassword,
                        });

                // User information to create a merge commit
                var signature = new LibGit2Sharp.Signature(
                    new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

                // Pull
                var result = LibGit2Sharp.Commands.Pull(repo, signature, options);
                switch(result.Status) {
                    case MergeStatus.UpToDate:
                        Program.Bot.sendEmbedInChannel(channel, "Modding Repo gepullt", $"Der Ordner war Up-To-Date!");
                        break;
                    case MergeStatus.FastForward:
                        Program.Bot.sendEmbedInChannel(channel, "Modding Repo gepullt", $"Es hat funktioniert: Die Ausgabe war: ```{result.Commit.Message}```");
                        break;
                    case MergeStatus.Conflicts:
                        Program.Bot.sendEmbedInChannel(channel, "Modding Repo gepullt", $"Es gab einen Konflikt!");
                        break;
                }
            }

            using(var readerGit = File.OpenText($"{Config.AltVServerPath}resources/ChoiceVModding/resources.toml")) {
                var tableGit = TOML.Parse(readerGit);

                using(var readerServer = File.OpenText($"{Config.AltVServerPath}/server.toml")) {
                    var tableServer = TOML.Parse(readerServer);

                    tableServer["resources"] = tableGit["resources"];

                    using(StreamWriter writer = File.CreateText($"{Config.AltVServerPath}/server.toml")) {
                        tableServer.WriteTo(writer);
                        writer.Flush();
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
