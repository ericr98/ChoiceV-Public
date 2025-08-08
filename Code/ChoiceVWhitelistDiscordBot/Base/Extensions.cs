using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Base {
    public static class ListExtensions {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize) {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }

    public static class StringExtensions {
        public static string TakeWithDots(this string s, int count) {
            if(s != null && s.Length > count) {
                return new string(s.Take(count).ToArray()) + "...";
            } else { 
                return s; 
            }
        }

        static IEnumerable<string> ChunksUpto(this string str, int maxChunkSize) {
            for(int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
    }

    public static class DiscordExtensions {
        public async static void RespondeToSlashCommand(this InteractionContext ctx, bool worked) {
            var msg = new DiscordMessageBuilder();
            if(worked) {
                msg.WithContent("Anfrage erfolgreich!");
            } else {
                msg.WithContent("Anfrage fehlgeschlagen!");
            }

            try {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(msg).AsEphemeral(true));
            } catch(Exception) { }
        }
    }
}
