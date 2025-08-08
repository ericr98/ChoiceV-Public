using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot.Controller.Whitelist.Steps {
    public class ReactWhitelistStep : WhitelistStep {
        protected string Footer;
        protected List<DiscordEmbedFieldAbstract> Fields;
        private List<DiscordButtonComponent> Buttons;

        public ReactWhitelistStep(string title, string startText, TimeSpan infoDelay, string footer = null, List<DiscordEmbedFieldAbstract> fields = null, bool allowsExternalInteraction = false) : base(title, startText, infoDelay, allowsExternalInteraction) { 
            Footer = footer;
            if(fields != null) {
                Fields = fields;
            } else {
                Fields = new List<DiscordEmbedFieldAbstract>();
            }

            Buttons = new List<DiscordButtonComponent>();
        }

        public void addField(DiscordEmbedFieldAbstract field) {
            Fields.Add(field);
        }

        public ReactWhitelistStep addButton(DiscordButtonComponent buttonComponent) {
            Buttons.Add(buttonComponent);

            return this;
        }

        public async override void onStartStep(DiscordChannel channel, DiscordMember member) {
            var builder = getEmbedMessageBuilder(Title, Text, Footer, Fields);
            builder.AddComponents(Buttons.ToArray());
            await channel.SendMessageAsync(builder);
        }

        public override void onEndStep(DiscordChannel channel, DiscordMember member) { }

        public override Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) { return Task.FromResult(false); }
    }


    public class InitialWhitelistReactStep : ReactWhitelistStep {
        private List<DiscordButtonComponent> UpdatedButtons;

        public InitialWhitelistReactStep() : base(
            $"{Config.ServerEmojiName} ChoiceV Whitelistverfahren gestartet! {Config.ServerEmojiName}",
            "Danke, dass du das ChoiceV Whitelist Verfahren gestartet hast!\nDu kannst mit den Reaktionen auf dieser Nachricht fortfahren oder das Verfahren jederzeit abbrechen. Deine Daten werden dann aus Datenschutzgründen unverzüglich gelöscht. Du kannst mit einer Reaktion auch einen Supporter in das Ticket anfordern. Dieser wird dir bei Fragen oder Problemen zur Seite stehen.", 
            TimeSpan.FromSeconds(0.75)) {

            addButton(new DiscordButtonComponent(ButtonStyle.Success, "start_whitelist", "Starte die Whitelist", false, new DiscordComponentEmoji("▶️")));
            addButton(new DiscordButtonComponent(ButtonStyle.Primary, "call_supporter", "Rufe einen Supporter", false, new DiscordComponentEmoji("ℹ️")));
            addButton(new DiscordButtonComponent(ButtonStyle.Danger, "stop_whitelist", "Brich die Whitelist ab", false, new DiscordComponentEmoji("✖️")));

            UpdatedButtons = new() {
                new DiscordButtonComponent(ButtonStyle.Success, "start_whitelist", "Starte die Whitelist", true, new DiscordComponentEmoji("▶️")),
                new DiscordButtonComponent(ButtonStyle.Primary, "call_supporter", "Rufe einen Supporter", false, new DiscordComponentEmoji("ℹ️")),
                new DiscordButtonComponent(ButtonStyle.Danger, "stop_whitelist", "Brich die Whitelist ab", false, new DiscordComponentEmoji("✖️"))
            };
        }

        public override async Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) {
            if(eventType != "COMPONENT_INTERACTION") {
                return false;
            }

            var interaction = (DiscordInteraction)data["INTERACTION"];
            switch(data["COMPONENT_ID"]) {
                case "start_whitelist":
                    var builder = getEmbedMessageBuilder(Title, Text, Footer);
                    builder.AddComponents(UpdatedButtons.ToArray());
                    await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));

                    log(LogLevel.Normal, member, channel, "Button gedrückt", $"Es wurde der \"Starte die Whitelist\" Button gedrückt");

                    finishStep(channel, member);
                    return true;
                case "stop_whitelist":
                    await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    WhitelistProcedureManager.cancelWhiteList(channel, member);
                    return true;
                case "call_supporter":
                    log(LogLevel.Normal, member, channel, "Button gedrückt", $"Es wurde der \"Rufe einen Supporter\" Button gedrückt");
                    SupportController.sendJoinRequestToSupportChannel(member, channel, "Supporter angefordert", $"In einem Whitelistantrag wurde ein Support angefordert. Der Spieler: {member.Mention}. Die Channel-ID ist: `{channel.Id}`");
    
                    //TODO MAYBE THINK ABOUT PEOPLE WHO MAKE TICKETS AND INSTANTLY SPAM CALL SUPPORTER!

                    await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    Program.Bot.sendEmbedInChannel(channel, "Support angefordert", "Es wurde soeben ein Supporter in das Ticket angefordert. Bitte habe etwas Geduld!");
                    return true;
            }

            return false;
        }
    }

    public class AcceptWhitelistReactStep : ReactWhitelistStep {
        protected string AcceptText;
        protected DiscordComponentEmoji Emoji;

        public AcceptWhitelistReactStep(string title, string description, TimeSpan delay, string footer = null, List<DiscordEmbedFieldAbstract> fields = null) : base(title, description, delay, footer, fields) { }

        public AcceptWhitelistReactStep addAcceptButton(string text, string acceptText, DiscordComponentEmoji emoji) {
            AcceptText = acceptText;
            Emoji = emoji;

            addButton(new DiscordButtonComponent(ButtonStyle.Success, $"accept_react_{UniqueId}", text, false, emoji));

            return this;
        }

        public override async Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) {
            if(eventType != "COMPONENT_INTERACTION") {
                return false;
            }

            var interaction = (DiscordInteraction)data["INTERACTION"];
            if(data["COMPONENT_ID"] == $"accept_react_{UniqueId}") {
                var builder = getEmbedMessageBuilder(Title, Text, Footer, Fields);
                builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "NOT_EVALUATED", AcceptText, true, Emoji));
                await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));

                log(LogLevel.Normal, member, channel, "Button gedrückt", $"Es wurde der \"{AcceptText}\" Button gedrückt");

                finishStep(channel, member);
                return true;
            }

            return false;
        }
    }

    public class TeamApprovalWhitelistStep : ReactWhitelistStep {
        private string Reason;
        private string AcceptText;
        private string TeamText;

        private DiscordComponentEmoji ApprovalEmoji;

        public TeamApprovalWhitelistStep(string title, string startText, string reason, TimeSpan infoDelay) : base(title, startText, infoDelay, null, null, true) {
            Reason = reason;
        }

        public override void onEndStep(DiscordChannel channel, DiscordMember member) { }

        public TeamApprovalWhitelistStep addApprovalButton(string text, string teamText, string acceptText, DiscordComponentEmoji emoji = null, DiscordComponentEmoji approvalEmoji = null) {
            AcceptText = acceptText;
            TeamText = teamText;
            ApprovalEmoji = approvalEmoji;

            addButton(new DiscordButtonComponent(ButtonStyle.Success, $"initial_team_approval_{UniqueId}", text, false, emoji));

            return this;
        }

        public async override Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) {
            if(eventType != "COMPONENT_INTERACTION") {
                return false;
            }

            var interaction = (DiscordInteraction)data["INTERACTION"];
            if(data["COMPONENT_ID"] == $"initial_team_approval_{UniqueId}") {
                var builder = getEmbedMessageBuilder(Title, Text, Footer, Fields);
                builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, $"secondary_team_approval_{UniqueId}", TeamText, false, ApprovalEmoji));
                await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));

                Program.Bot.sendEmbedInChannel(channel, "Teammitglied angefordert!", "Es wurde ein Teammitglied angefordert um den aktuellen Stand deiner Whitelist zu überprüfen. Gedulde dich etwas");

                SupportController.sendJoinRequestToSupportChannel(member, channel, "Whitelist-Ticket benötigt einen Supporter!", $"Der Whitelist Bot hat Überprüfung für ein Ticket vom Ersteller {member.Mention} mit dem Grund: \"{Reason}\" angefordert!");

                log(LogLevel.Normal, member, channel, "Team-Überprüfung akzeptiert", $"Es wurde der Button des Team Approval Steps gedrückt");
                return true;
            } else if(data["COMPONENT_ID"] == $"secondary_team_approval_{UniqueId}") {
                if(member.Roles.Any(r => r.Id == Config.SupportRoleId)) {
                    var builder = getEmbedMessageBuilder(Title, Text, Footer, Fields);
                    builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, $"NOT_EVALUATED", AcceptText, true, ApprovalEmoji));
                    await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));

                    finishStep(channel, await channel.Guild.GetMemberAsync((ulong)data["RESULT_USER_ID"]));
                } else {
                    Program.Bot.sendEmbedInChannel(channel, "Kein Berechtigung!", "Ein Teammitglied muss diesen Schritt akzeptieren. Es wurde bereits eine Anfrage gesendet, gedulde dich etwas.");
                }
            }

            return false;
        }
    }
}
