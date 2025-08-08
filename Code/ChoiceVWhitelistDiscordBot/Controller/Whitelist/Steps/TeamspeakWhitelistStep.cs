using DiscordBot.Model.Database;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TeamSpeak3QueryApi.Net.Specialized;

namespace DiscordBot.Controller.Whitelist.Steps {
    public class TeamspeakWhitelistStep : WhitelistStep {
        public TeamspeakWhitelistStep() : base("\U0001faaa Teamspeak Identität \U0001faaa",
            "Hier wird deine Teamspeak Identität für das Ingame Voice System festgelegt. Diese wird wichtig für weitere Whitelist Schritte, sowie für den eigentlichen Server-Spielverlauf.", 
            TimeSpan.FromMilliseconds(0)) { }

        public override void onEndStep(DiscordChannel channel, DiscordMember member) { }

        public async override Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) { 
            if(eventType == "MODAL_SUBMIT") {
                var values = (List<whitelist_procedure_datum>)data["DATA"];
                var value = values.FirstOrDefault(v => v.name == "TEAMSPEAK_ID");
                if(value != null && value.finished) {
                    return false;
                }
                   
                var message = ((IReadOnlyDictionary<string, string>)data["VALUES"]).First().Value;
                var modal = ((IReadOnlyDictionary<string, string>)data["VALUES"]).First().Key;

                using(var db = new WhitelistDb()) {
                    var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);
                    
                    if(procedure != null) {
                        var already = procedure.whitelist_procedure_data.FirstOrDefault(d => d.name == "TEAMSPEAK_ID");

                        var interaction = (DiscordInteraction)data["INTERACTION"];
                        if(modal == "teamspeak_identity") {
                            var generator = new Random();
                            var number = generator.Next(0, 1000000).ToString("D6");

                            if(await TeamspeakController.sendTeamspeakMessage(message, $"Dein Zahlencode ist: {number}")) {
                                already.data = JsonConvert.SerializeObject(new List<string> { message, number });
                                already.isInEdit = false;

                                var dcMessage = await channel.GetMessageAsync(already.messageId);
                                await dcMessage.ModifyAsync(getEmbedMessageBuilderStep(message, false, true, false, true));

                                log(LogLevel.Normal, member, channel, "Identität hinzugefügt", $"Es wurde die Identität `{message}` hinzugefügt");
                            } else {
                                log(LogLevel.Bad, member, channel, "Identität nicht gefunden", $"Der Spieler hat eine Identität `{message}` angegeben, die nicht auf dem Server gefunden wurde");
                                Program.Bot.sendEmbedInChannel(channel, "Identität nicht gefunden", "Gib deine Identität erneut ein und bei weiterem Fehler \"Rufe eine Supporter\"");
                            }

                            await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            db.SaveChanges();

                            return true;
                        } else if( modal == "teamspeak_code") {
                            var list = JsonConvert.DeserializeObject<List<string>>(already.data);

                            if(message == list[1]) {
                                var dcMessage = await channel.GetMessageAsync(already.messageId);
                                await dcMessage.ModifyAsync(getEmbedMessageBuilderStep(list[0], false, false, true));

                                log(LogLevel.Normal, member, channel, "Richtigen Code eingegeben", $"Der Spieler hat einen richtigen Code eingegeben");
                            } else {
                                if(await TeamspeakController.sendTeamspeakMessage(list[0], $"Der angegebene Code ist falsch. Überprüfe ob du auch keine Leerzeichen etc. mitkopiert hast. Der Code ist: {list[1]}")) {
                                    log(LogLevel.Bad, member, channel, "Falschen Code eingegeben", $"Der Spieler hat einen falschen Code eingegeben");
                                } else {
                                    Program.Bot.sendEmbedInChannel(channel, "Fehler aufgetreten", "Versuche es erneut und bei weiterem Fehler \"Rufe eine Supporter\"");
                                }
                            }

                            await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            db.SaveChanges();
                            return true;
                        }
                    }
                }
            } else if(eventType == "COMPONENT_INTERACTION") {
                var interaction = (DiscordInteraction)data["INTERACTION"];

                using(var db = new WhitelistDb()) {
                    var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                    if(procedure != null) {
                        var find = procedure.whitelist_procedure_data.FirstOrDefault(wd => wd.name == "TEAMSPEAK_ID");

                        if(find != null) {
                            switch(data["COMPONENT_ID"]) {
                                case "accept_teamspeak":
                                    var list = JsonConvert.DeserializeObject<List<string>>(find.data);
                                    find.data = list[0];
                                    find.finished = true;

                                    await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getEmbedMessageBuilderStep(list[0], true, false, true, true)));

                                    log(LogLevel.Good, member, channel, "\"Fortfahren\" Button gedrückt", $"Der Spieler hat den \"Fortfahren\" Button gedrückt");

                                    finishStep(channel, member);
                                    return true;
                                case "edit_teamspeak":
                                    find.data = " ";
                                    find.isInEdit = true;

                                    log(LogLevel.Debug, member, channel, "\"Prozess starten\" Button gedrückt", $"Der Spieler hat den \"Prozess starten\" Button gedrückt");

                                    var modal = new DiscordInteractionResponseBuilder()
                                        .WithTitle("Teamspeak-Verbindung!")
                                        .WithCustomId("teamspeak_identity")
                                        .AddComponents(new TextInputComponent(label: "Teamspeak Identität", customId: "teamspeak_identity", placeholder: "z.B. LR896oo86hmlo7n1mik8/I8uqnl9=", max_length: 100));


                                    var message = await channel.GetMessageAsync(find.messageId);
                                    await message.ModifyAsync(getEmbedMessageBuilderStep("Bisher keine erhalten", false, false, false));

                                    await interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                                    return true;
                                case "approve_teamspeak":
                                    find.isInEdit = false;
                                    var findList = JsonConvert.DeserializeObject<List<string>>(find.data);

                                    log(LogLevel.Debug, member, channel, "\"Prozess starten\" Button gedrückt", $"Der Spieler hat den \"Prozess starten\" Button gedrückt");

                                    var modal2 = new DiscordInteractionResponseBuilder()
                                        .WithTitle("Teamspeak-Verbindung!")
                                        .WithCustomId("teamspeak_code")
                                        .AddComponents(new TextInputComponent(label: "Teamspeak Code", customId: "teamspeak_code", placeholder: "z.B. 876522", min_length: 6, max_length: 20));

                                    var message2 = await channel.GetMessageAsync(find.messageId);
                                    await message2.ModifyAsync(getEmbedMessageBuilderStep(findList[0], false, true, false, true));

                                    await interaction.CreateResponseAsync(InteractionResponseType.Modal, modal2);
                                    return true;
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }

            return false;
        }


        private DiscordMessageBuilder getEmbedMessageBuilderStep(string teamspeakId, bool disableEditButton, bool showCodeButton, bool showFinishButton, bool disableShowButton = false) {
            var builder = getEmbedMessageBuilder(Title, Text, null,
                new List<DiscordEmbedFieldAbstract> {
                    new DiscordEmbedFieldAbstract("🚪 Schritt 1", $"Verbinde dich auf unseren Teamspeak. Benutze dazu die Addresse: `{Config.TeamspeakAddress}`", false),
                    new DiscordEmbedFieldAbstract("❓ Schritt 2", "Finde deine Teamspeak Identität heraus und sende sie in das Formular, welches erscheint, wenn du \"Prozess starten\" drückst. Wie du deine Teamspeak ID herausfindest siehst du in dem unten angezeigten Video. Solltest du mehrere Identiäten haben wähle die aus, mit der du dich zu uns verbunden hast. (Meistens die **fett** gedruckte)", false),
                    new DiscordEmbedFieldAbstract("📬 Schritt 3", "Du erhälst nun einen 6 stelligen Zahlencode als Nachricht von unserem Whitelist Bot. Kopiere den Zahlencode und schicke ihn in das Formular welches erscheint wenn du \"Code eingeben\".", false),
                    new DiscordEmbedFieldAbstract("🔁 Optional: Schritt 4", "Sollte es ein Problem gegeben, oder du eine falsche Identität eingegeben haben, dann wähle: \"Eingabe zurücksetzen\" um den Prozess neu zu starten. Gib darauffolgend deine Teamspeak ID neu ein.", false),
                    new DiscordEmbedFieldAbstract("\U0001faaa Erhaltene ID:", $"{teamspeakId}", false),
            }, "http://choicev-cef.net/src/whitelist_bot/Teamspeak.gif");

            var compList = new List<DiscordComponent>();
            if(showFinishButton) {
                compList.Add(new DiscordButtonComponent(ButtonStyle.Success, $"accept_teamspeak", "Fortfahren", disableShowButton, new DiscordComponentEmoji("✅")));
            }

            if(showCodeButton) {
                compList.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"approve_teamspeak", "Code eingeben", disableEditButton,new DiscordComponentEmoji("📬")));
            }

            compList.Add(new DiscordButtonComponent(showCodeButton ? ButtonStyle.Secondary : ButtonStyle.Primary, $"edit_teamspeak", "Prozess starten", disableEditButton, new DiscordComponentEmoji("▶️")));
            builder.AddComponents(compList);

            return builder;
        }

        public async override void onStartStep(DiscordChannel channel, DiscordMember member) {
            var builder = getEmbedMessageBuilderStep("Bisher keine erhalten", false, false, false);

            var message = await builder.SendAsync(channel);

            using(var db = new WhitelistDb()) {
                var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                if(procedure != null) {
                    procedure.whitelist_procedure_data.Add(new whitelist_procedure_datum { 
                        name = "TEAMSPEAK_ID",
                        isInEdit = true,
                        data = " ",
                        messageId = message.Id,
                    });

                    db.SaveChanges();
                }
            }
        }
    }
}
