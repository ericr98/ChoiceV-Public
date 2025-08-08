using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot {
    public delegate void MemberJoinDelegate(GuildMemberAddEventArgs evt);
    public delegate void MemberLeaveDelegate(GuildMemberRemoveEventArgs evt);
    public delegate void MemberUpdatedDelegate(GuildMemberUpdateEventArgs evt);
    public delegate void ComponentInteractionDelegate(DiscordClient sender, ComponentInteractionCreateEventArgs e);
    public delegate void MessageCreatedDelegate(DiscordClient sender, MessageCreateEventArgs e);
    public delegate void ModalSubmittedDelegate(DiscordClient sender, ModalSubmitEventArgs e);

    public delegate void BotReadyDelegate();

    public class EventController : BotScript {
        public static MemberJoinDelegate MemberJoinDelegate;
        public static MemberLeaveDelegate MemberLeaveDelegate;
        public static MemberUpdatedDelegate MemberUpdatedDelegate;
        public static BotReadyDelegate BotReadyDelegate;
        public static ComponentInteractionDelegate ComponentInteractionDelegate;
        public static MessageCreatedDelegate MessageCreatedDelegate;
        public static ModalSubmittedDelegate ModalSubmittedDelegate;

        public EventController() {
            var bot = Program.Bot;

            bot.GuildMemberAdded += onGuildMemberAdded;
            bot.GuildMemberRemoved += onGuildMemberRemoved;
            bot.GuildMemberUpdated += onGuildMemberUpdated;

            bot.ComponentInteractionCreated += onComponentInteraction;
            bot.MessageCreated += onMessageCreated;

            bot.ModalSubmitted += onModalSubmitted;

        }

        private Task onModalSubmitted(DiscordClient sender, ModalSubmitEventArgs evt) {
            if(!makeGuildCheck(evt.Interaction.Guild)) { return Task.CompletedTask; }

            try {
                ModalSubmittedDelegate?.Invoke(sender, evt);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            return Task.CompletedTask;
        }

        private Task onMessageCreated(DiscordClient sender, MessageCreateEventArgs evt) {
            if(!makeGuildCheck(evt.Guild)) { return Task.CompletedTask; }

            try {
                MessageCreatedDelegate?.Invoke(sender, evt);
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            }
            return Task.CompletedTask;
        }

        private Task onComponentInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs evt) {
            if(!makeGuildCheck(evt.Guild)) { return Task.CompletedTask; }

            try {
                ComponentInteractionDelegate?.Invoke(sender, evt);
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            }
            return Task.CompletedTask;
        }

        private Task onGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs evt) {
            if(!makeGuildCheck(evt.Guild)) { return Task.CompletedTask; }

            try {
                MemberJoinDelegate?.Invoke(evt);
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            }
            return Task.CompletedTask;
        }

        private Task onGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs evt) {
            if(!makeGuildCheck(evt.Guild)) { return Task.CompletedTask; }

            try {
                MemberLeaveDelegate?.Invoke(evt);
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            }
            return Task.CompletedTask;
        }

        private Task onGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs evt) {
            if(!makeGuildCheck(evt.Guild)) { return Task.CompletedTask; }

            try {
                MemberUpdatedDelegate?.Invoke(evt);
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
            }

            return Task.CompletedTask;
        }

        private static bool makeGuildCheck(DiscordGuild guild) {
            return guild != null && guild.Id == Config.DiscordServerId;
        }
    }
}
