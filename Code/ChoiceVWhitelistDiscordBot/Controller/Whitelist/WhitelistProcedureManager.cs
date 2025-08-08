using DiscordBot.Controller.Whitelist.Steps;
using DiscordBot.Model.Database;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using TeamSpeak3QueryApi.Net.Specialized;

namespace DiscordBot.Controller.Whitelist {
    public class WhitelistProcedureManager {
        private static List<WhitelistStep> Steps;     
        public static int StepCount { get => Steps.Count; }

        private static TimeSpan WHITELIST_TICKET_TIMEOUT = TimeSpan.FromDays(14);

        public WhitelistProcedureManager() {
            Steps = new List<WhitelistStep>();

            EventController.ComponentInteractionDelegate += onComponentInteraction;
            EventController.MessageCreatedDelegate += onMessageCreated;
            EventController.ModalSubmittedDelegate += onModalSubmit;

            EventController.MemberLeaveDelegate += onMemberLeave;

            createTicketCreationIfNecessary();
        }

        private async void createTicketCreationIfNecessary() {
            var mainChannel = await Program.Bot.GetChannelAsync(Config.WhitelistStartChannel);

            if((await mainChannel.GetMessagesAsync(10)).Count <= 0) {
                var sentMessage = await getTicketMessage(ConfigController.getConfigData("WHITELIST_STATE") == "ACTIVE").SendAsync(mainChannel);

                ConfigController.setConfigData("START_TICKET_MESSAGE", sentMessage.Id.ToString());
            }
        }

        private static DiscordMessageBuilder getTicketMessage(bool isActive) {
            var embed = new DiscordEmbedBuilder()
                .WithColor(MessageController.CHOICEV_COLOR)
                .WithTitle("Whitelistverfahren starten")
                .WithDescription("Starte ein Whitelistverfahren indem du unten auf \"Whitelist starten!\" klickst. Alles weitere wird in der Whitelist selbst erklärt! Bei Fragen und Unklarheiten erstelle ein Supportticket.")
                .WithTimestamp(DateTime.Now)
                .WithImageUrl("http://choicev-cef.net/src/whitelist_bot/ChoiceVLogo.png")
                .WithFooter("ChoiceV - Deine Community!", "http://choicev-cef.net/src/whitelist_bot/ChoiceV.png")
                .WithAuthor("Whitelist-Bot", "https://www.choicev.net");

            if(!isActive) {
                embed.AddField(":x: Whitelist aktuell deaktiviert! :x:", "Die Whitelist ist aktuell deaktiviert. Falls unklar ist warum erstelle ein Ticket oder kommen auf unseren Teamspeak!");
            } else {
                embed.AddField(":white_check_mark:  Whitelist aktuell aktivert! :white_check_mark: ", "Die Whitelist ist aktuell aktiviert. Erstelle ein Ticket, und erzähle uns deine Geschichte!");
            }

            var message = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "create_new_whitelist", "Starte deine Whitelist!", !isActive, new DiscordComponentEmoji("🛫")));

            return message;
        }

        public static async void changeWhitelistState(bool isActive) {
            var messageId = ulong.Parse(ConfigController.getConfigData("START_TICKET_MESSAGE"));
            var mainChannel = await Program.Bot.GetChannelAsync(Config.WhitelistStartChannel);
            
            var message = await mainChannel.GetMessageAsync(messageId);
            await message.ModifyAsync(getTicketMessage(isActive));
        }

        public static async void skipWhitelistQuestion(DiscordChannel channel) {
            var result = await getWhitelistProcedureDb(channel.Id, false);

            if(result != null) {
                var whitelistStep = (QuestionWhitelistStep)Steps.FirstOrDefault(s => s.GetType() == typeof(QuestionWhitelistStep));
                var member = await channel.Guild.GetMemberAsync(result.userId);

                if(whitelistStep != null && member != null) {
                    await whitelistStep.finishQuestions(result.id, member, channel);
                }
            }
        }

        public static async void setWhitelistTicketFinished(DiscordChannel channel, bool finished) {
            using(var db = new WhitelistDb()) {
                var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.channelId == channel.Id);

                if(procedure != null) {
                    procedure.blocked = true;

                    await db.SaveChangesAsync();
                }
            }
        }

        public static async void setWhitelistTicketCanceable(DiscordChannel channel, bool notCanceable) {
            using(var db = new WhitelistDb()) {
                var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.channelId == channel.Id);

                if(procedure != null) {
                    procedure.notCanceable = notCanceable;

                    await db.SaveChangesAsync();
                }
            }
        }

        public static bool interactionPrecheck(DiscordChannel channel) {
            if(channel != null && channel.Name != null) {
                return channel.Name.StartsWith(Config.WhitelistChannelNamePrefix);
            } else {
                return false;
            }
        }

        private async void onComponentInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e) {
            try {
                if(e.Id == "create_new_whitelist") {
                    var member = await e.Guild.GetMemberAsync(e.User.Id);
                    if(member != null && !member.Roles.Any(r => r.Id == Config.DiscordSucceedRoleId)) {
                        startProcedure(e.Guild, member);
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    } else if(member != null) {
                        Program.Bot.sendEmbedToUser(e.User.Id, "Whitelist starten fehlgeschlagen", "Du hast die Whitelist bereits erfolgreich abgeschlossen. Du brauchst sie nicht nochmal machen :relieved:");
                    }

                    return;
                }

                if(!interactionPrecheck(e.Channel)) { return; }

                var result = await getWhitelistProcedureDb(e.Channel.Id, e.User.Id, true);
                if(result != null) {
                    if(e.Guild.GetMemberAsync(result.userId) == null) {
                        Program.Bot.sendEmbedInChannel(e.Channel, "User nicht mehr auf Server", "Der Originale User des Tickets befindet sich nicht mehr auf dem Server!");
                        return;
                    }

                    var data = new Dictionary<string, dynamic> { { "COMPONENT_ID", e.Id }, { "INTERACTION", e.Interaction }, { "DATA", result.whitelist_procedure_data.ToList() }, { "RESULT_USER_ID", result.userId } };

                    var member = await e.Guild.GetMemberAsync(e.User.Id);

                    var count = result.currentStep;
                    while(count >= 0) {
                        var step = Steps[count];
                        if((step.AllowsExternalInteraction || (member.Id == result.userId && !step.AllowsExternalInteraction))) {
                            var outcome = await Steps[count].onRelayData(e.Channel, member, "COMPONENT_INTERACTION", data);
                            if(outcome) {
                                break;
                            }
                        }
                        count--;
                    }
                }
            } catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        private async void onMessageCreated(DiscordClient sender, MessageCreateEventArgs e) {
            if(!interactionPrecheck(e.Channel)) { return; }

            var member = await e.Guild.GetMemberAsync(e.Author.Id);
            var result = await getWhitelistProcedureDb(e.Channel.Id, e.Author.Id, false);

            if(!member.IsBot) {
                var text = $"Nachricht geschickt von {member.DisplayName}#{member.Discriminator}";
                if(result == null) {
                    result = await getWhitelistProcedureDb(e.Channel.Id, true);
                    if(result != null) {
                        member = await e.Guild.GetMemberAsync(result.userId);
                    }
                }
                WhitelistLoggingController.addWhiteListLog(LogLevel.Debug, member.Id, e.Channel, result?.currentStep ?? 0, text, $"{e.Message.Content}");
            }
        }

        private async void onModalSubmit(DiscordClient sender, ModalSubmitEventArgs e) {
            if(!interactionPrecheck(e.Interaction.Channel)) { return; }
            
            var result = await getWhitelistProcedureDb(e.Interaction.Channel.Id, e.Interaction.User.Id, true);
            if(result != null) {
                if(e.Interaction.Guild.GetMemberAsync(result.userId) == null) {
                    Program.Bot.sendEmbedInChannel(e.Interaction.Channel, "User nicht mehr auf Server", "Der Originale User des Tickets befindet sich nicht mehr auf dem Server!");
                    return;
                }

                var data = new Dictionary<string, dynamic> { { "INTERACTION", e.Interaction }, { "VALUES", e.Values }, { "DATA", result.whitelist_procedure_data.ToList() }, { "RESULT_USER_ID", result.userId } };

                var member = await e.Interaction.Guild.GetMemberAsync(e.Interaction.User.Id);

                var count = result.currentStep;
                while(count >= 0) {
                    var step = Steps[count];
                    if((step.AllowsExternalInteraction || (member.Id == result.userId && !step.AllowsExternalInteraction)) && await Steps[count].onRelayData(e.Interaction.Channel, member, "MODAL_SUBMIT", data)) {
                        break;
                    }
                    count--;
                }
            }
        }


        private async static Task<whitelist_procedure> getWhitelistProcedureDb(ulong channelId, ulong userId, bool searchByChannelOnly) {
            using(var db = new WhitelistDb()) {
                if(searchByChannelOnly) {
                    var list = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).Where(w => w.channelId == channelId && !w.blocked).ToList();
                    if(list.Count <= 0) {
                        return null;
                    } else {
                        return list.First();
                    }
                } else {
                    return await db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefaultAsync(w => w.channelId == channelId && w.userId == userId && !w.blocked);
                }
            }
        }

        private async static Task<whitelist_procedure> getWhitelistProcedureDb(ulong channelId, bool alsoBlockedTickets = false) {
            using(var db = new WhitelistDb()) {
                if(alsoBlockedTickets) {
                    return await db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefaultAsync(w => w.channelId == channelId);
                } else {
                    return await db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefaultAsync(w => w.channelId == channelId && !w.blocked);
                }
            }
        }

        public void addStep(WhitelistStep step) {
            step.setInfo(this, Steps.Count);
            Steps.Add(step);
        }

        public async void startProcedure(DiscordGuild guild, DiscordMember member) {
            using(var db = new WhitelistDb()) {
                var already = db.whitelist_procedures.FirstOrDefault(w => w.userId == member.Id && !w.blocked);
                if(already != null) {
                    Program.Bot.sendEmbedToUser(member.Id, "Bereits in einem Whitelistverfahren", "Du befindest dich bereits in einem Whitelistverfahren! Führe dieses erst fort. Solltest du keinen Zugriff mehr auf das Verfahren haben, dann melde dich im Support!", $"Erreichte Verfahrenstufe: {already.currentStep + 1}");
                    return;
                }

                var builder = new DiscordOverwriteBuilder(guild.EveryoneRole);
                builder.Deny(Permissions.All);

                var category = guild.GetChannel(Config.WhitelistTicketsCategory);
                var channel = await guild.CreateChannelAsync($"{Config.WhitelistChannelNamePrefix}-NOT-YET-ID", ChannelType.Text, category, default, null, null, new List<DiscordOverwriteBuilder> { builder });

                var supportRole = guild.GetRole(Config.SupportRoleId);
                await channel.AddOverwriteAsync(supportRole, Permissions.UseApplicationCommands);

                var newproc = new whitelist_procedure {
                    userId = member.Id,
                    channelId = channel.Id,
                    startTime = DateTime.Now,
                    cancelStartTime = DateTime.Now,
                    currentStep = 0,
                    blocked = false,
                };

                db.whitelist_procedures.Add(newproc);
                db.SaveChanges();

                Action<ChannelEditModel> action = new(x => x.Name = $"{Config.WhitelistChannelNamePrefix}-{newproc.id}");
                await channel.ModifyAsync(action);

                await channel.AddOverwriteAsync(member,
                    Permissions.AccessChannels | Permissions.ReadMessageHistory | Permissions.SendMessages
                );

                Program.Bot.sendEmbedInChannel(channel, $"Whitelistverfahren durch: {member.DisplayName}#{member.Discriminator}", $"Dieses Whitelistverfahren wurde gestartet durch: {member.Mention}");

                Steps[0].onStartStep(channel, member);
            }
        }

        public void finishStep(WhitelistStep finish, DiscordChannel channel, DiscordMember member) {
            if(finish.Order + 1 >= Steps.Count) {
                finishWhiteList(channel, member);
                return;
            }

            using(var db = new WhitelistDb()) {
                var row = db.whitelist_procedures.FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);
                row.currentStep = finish.Order + 1;
                db.SaveChanges();

                Steps[finish.Order + 1].onStartStep(channel, member);
            }

            //addWhiteListLog(member, channel, $"Whitelistschritt: {finish.Order + 1} abgeschlossen");
        }

        private static void finishWhiteList(DiscordChannel channel, DiscordMember member) {
            using(var db = new WhitelistDb()) {
                var row = db.whitelist_procedures.FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);
                row.blocked = true;
                db.SaveChanges();

                Program.Bot.sendEmbedInChannel(channel,
                    ":fireworks: Herzlichen Glückwunsch! :confetti_ball:",
                    "Du hast hiermit die Whitelist offiziell bestanden! Du solltest nun automatisch alle nötigen Ränge erhalten haben und kannst dich nun auf dem Server einloggen. Alle nötigen Infos findest du in den neu erschienenen Channels. Viel Spaß auf Choicev!",
                    "Whitelist bestanden");

                channel.sendChannelBlockMessage("Whitelist erfolgreich abgeschlossen!");

                WhitelistController.finishWhitelistProcedure(channel.Guild, channel, member);
            }

            WhitelistLoggingController.addWhiteListLog(LogLevel.Good, member.Id, channel, Steps.Count - 1, $"Whitelist erfolgreich abgeschlossen!", $"Der Spieler {member.Mention} hat die Whitelist erfolgreich abgeschlossen!");
        }

        public static async void cancelWhiteList(DiscordChannel channel, DiscordMember member) {
            using(var db = new WhitelistDb()) {
                var row = db.whitelist_procedures.Include(wd => wd.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id && !w.blocked);
                var dataList = row.whitelist_procedure_data.ToList();

                if(row != null && !(row.notCanceable && Steps.Any(s => s.getHaltedBlocked(dataList)))) {
                    db.whitelist_procedures.Remove(row);
                    db.SaveChanges();

                    await channel.DeleteAsync();

                    Program.Bot.sendEmbedToUser(member.Id, "Whitelist abgebrochen", "Schade, dass du unser Whitelistverfahren abgebrochen hast :confused:. Du kannst jederzeit ein weiteres Verfahren starten! Bei Fragen oder Problemen wende dich direkt an den Support!", $"Erreichte Verfahrenstufe: {row.currentStep + 1}");

                    SupportController.deactivateJoinRequestsForTicket(member.Id, channel.Id, "Die Whitelist wurde vom Ersteller abgebrochen!");
                    WhitelistController.cancelWhitelistProcedure(channel.Guild, channel, member);
                } else {
                    WhitelistLoggingController.addWhiteListLog(LogLevel.Bad, member.Id, channel, row.currentStep + 1, $"Whitelist-Abbruch Versuch", $"Spieler hat versucht die Whitelist abzubrechen, obwohl diese \"Blockiert war\"");
                    Program.Bot.sendEmbedToUser(member.Id, "Abbrechen nicht möglich!", "Dein Antrag ist auf \"Fertig\" gesetzt. Du kannst ihn nicht mehr abbrechen. Setze dich mit dem Support in Verbindung falls dir unklar ist warum!");
                }
            }
        }

        private async void onMemberLeave(GuildMemberRemoveEventArgs evt) {
            using(var db = new WhitelistDb()) {
                var rows = db.whitelist_procedures.Where(w => w.userId == evt.Member.Id && !w.blocked).ToList();
                if(rows != null && rows.Count > 0) {
                    foreach(var row in rows) {
                        db.whitelist_procedures.Remove(row);

                        var channel = evt.Guild.GetChannel(row.channelId);
                        if(channel != null) {
                            SupportController.deactivateJoinRequestsForTicket(evt.Member.Id, channel.Id, "Der Spieler hat den Discord-Server verlassen!");
                            EventLoggingController.log(evt.Member, "Whitelist abgebrochen", DiscordColor.IndianRed, "Der Spieler hat eine Whitelist abgebrochen, da er den Server verlassen hat");

                            WhitelistController.cancelWhitelistProcedure(channel.Guild, channel, evt.Member);
                            await channel.DeleteAsync();
                        } else {
                            EventLoggingController.log(evt.Member, "Whitelist abgebrochen (Channel war bereits gelöscht)", DiscordColor.IndianRed, "Der Spieler hat eine Whitelist abgebrochen, da er den Server verlassen hat. Der Channel zu der Whitelist war bereits gelöscht.");
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        public static void resetQuestionTimeoutSpan(DiscordMember sender, DiscordChannel channel, int hours) {
            var whitelistStep = (QuestionWhitelistStep)Steps.FirstOrDefault(s => s.GetType() == typeof(QuestionWhitelistStep));

            if(whitelistStep != null) {
                whitelistStep.resetQuestionTimeoutSpan(sender, channel, hours);
            }
        }

        public static async void onTick() {
            foreach(var guild in Program.Bot.Guilds.Values) {
                using(var db = new WhitelistDb()) {
                    var toRemove = new List<whitelist_procedure>();

                    foreach(var procedure in db.whitelist_procedures) {
                        if(procedure.cancelStartTime + WHITELIST_TICKET_TIMEOUT <= DateTime.Now) {
                            var channel = guild.GetChannel(procedure.channelId);
                            if(channel != null) {
                                await channel.DeleteAsync();
                            }
                            procedure.cancelStartTime = DateTime.Now;

                            if(!procedure.blocked && !procedure.notCanceable) {
                                toRemove.Add(procedure);
                            }
                        }
                    }

                    db.whitelist_procedures.RemoveRange(toRemove);

                    db.SaveChanges();
                }
            }
        }
    }
}
