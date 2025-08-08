using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.SlashCommands;
using DiscordBot.Controller.Whitelist;
using DiscordBot.Commands;
using DiscordBot.Controller;

namespace DiscordBot {
    public class Program {
        public static DiscordClient Bot { get; private set; }
        public static Thread BotThread { get; private set; }
        public static BaseCommandModule CommandsModule { get; private set; }


        private static List<BotScript> LoadedTypes = new List<BotScript>();

        static void Main(string[] args) {
            loadConfig();

       
            runBot().GetAwaiter().GetResult();

        }

        private static void loadScripts() {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach(var item in allTypes) {
                //Load WorldController first
                if(item.IsSubclassOf(typeof(BotScript)) && !item.IsAbstract) {
                    var o = (BotScript)Activator.CreateInstance(item);

                    // Important! Prevent Garbage Collection
                    LoadedTypes.Add(o);
                }
            }
        }

        private static void loadConfig() {
            var document = XDocument.Load("config.xml");

            var root = document.Root;
            var results =
              root
                .Elements()
                .ToDictionary(element => element.Name.ToString(), element => element.Value);


            Type type = typeof(Config);
            foreach(var key in results.Keys) {
                PropertyInfo property = type.GetProperty(key);

                PropertyInfo propertyInfo = type.GetProperty(key);
                Type propertyType = propertyInfo.PropertyType;
                var value = Convert.ChangeType(results[key], propertyType);
                property.SetValue(type, value);
            }
        }

        public static async Task runBot() {
            var config = new DiscordConfiguration {
                Token = Config.BotToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true,
            };

            Bot = new DiscordClient(config);

            var commands = Bot.UseCommandsNext(new CommandsNextConfiguration() {
                StringPrefixes = new[] { Config.BotCommandsPrefix }
            });

            //commands.RegisterCommands<Commands>();

            var slash = Bot.UseSlashCommands();
            slash.RegisterCommands<DiscordSlashCommands>(Config.DiscordServerId);
            slash.RegisterCommands<DiscordServerCommands>(Config.DiscordServerId);

            Bot.UseInteractivity(new InteractivityConfiguration() {
                Timeout = TimeSpan.FromHours(1),
                AckPaginationButtons = true,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
            });

            try {
                await Bot.ConnectAsync();
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            }

            loadScripts();


            EventLoggingController.log("@everyone Discord-Bot ist online!", DiscordColor.Green, "Der Whitelist Bot ist nun wieder online!");

            while(true) {
                Console.WriteLine("Processing onTick..");
                try {
                    WhitelistProcedureManager.onTick();
                    Console.WriteLine("Processed onTick!");
                } catch (Exception ex) {
                    Console.WriteLine($"onTick failed! {ex.ToString()}");
                }
                await Task.Delay(1000 * 60);
            }

            //await Task.Delay(-1);
        }
    }
}
