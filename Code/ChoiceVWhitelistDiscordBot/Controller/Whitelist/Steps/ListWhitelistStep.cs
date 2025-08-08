using DiscordBot.Model.Database;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Controller.Whitelist.Steps {
    public class ListWhitelistStep : WhitelistStep {
        private class WhitelistSelection {
            public DiscordSelectComponentOption Selection;
            public string DatabaseValue;

            public WhitelistSelection(DiscordSelectComponentOption selection, string databaseValue) {
                Selection = selection;
                DatabaseValue = databaseValue;
            }
        }

        private List<WhitelistSelection> Selections;

        private string DatabaseName;

        private int MinOptions;
        private int MaxOptions;

        public ListWhitelistStep(string title, string startText, string databaseName, int minOptions, int maxOptions, TimeSpan infoDelay) : base(title, startText, infoDelay) {
            Selections = new List<WhitelistSelection>();
            DatabaseName = databaseName;
            MinOptions = minOptions;
            MaxOptions = maxOptions;
        }

        public ListWhitelistStep addSelection(string dbValue, string name, string description = null, DiscordComponentEmoji emoji = null) {
            Selections.Add(new WhitelistSelection(new DiscordSelectComponentOption(name, $"selection_{Selections.Count}", description, false, emoji), dbValue));
            return this;
        }

        public override void onEndStep(DiscordChannel channel, DiscordMember member) { }

        public async override Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) {
            if(eventType != "COMPONENT_INTERACTION") {
                return false;
            }

            var interaction = (DiscordInteraction)data["INTERACTION"];
            if(data["COMPONENT_ID"] == $"list_dropdown_{UniqueId}") {
                ulong messageId = 0;

                var selection = Selections.Where(s => interaction.Data.Values.Contains(s.Selection.Value));
                using(var db = new WhitelistDb()) {
                    var procedure = db.whitelist_procedures.Include(p => p.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                    if(procedure != null) {
                        var already = procedure.whitelist_procedure_data.FirstOrDefault(d => d.name == DatabaseName);

                        if(already != null) {
                            var prev = already.data;
                            already.data = JsonConvert.SerializeObject(selection.Select(s => s.DatabaseValue));
                            already.isInEdit = true;

                            messageId = already.messageId;

                            db.SaveChanges();

                            log(LogLevel.Debug, member, channel, "Auswahl geändert", $"Die Auswahl wurde von `{prev}` zu `{already.data}` geändert");

                            var builder = getEmbedMessageBuilder(Title, Text, null, new List<DiscordEmbedFieldAbstract> {
                                getSelectionField(selection)
                            });
                            builder.AddComponents(getSelectComponent());
                            builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, $"list_select_{UniqueId}", "Fortfahren", false));
                            await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));
                            return true;
                        }
                    }
                }
            } else if(data["COMPONENT_ID"] == $"list_select_{UniqueId}") {
                var find = ((List<whitelist_procedure_datum>)data["DATA"]).FirstOrDefault(wd => wd.name == DatabaseName);

                if(find != null) {
                    var list = JsonConvert.DeserializeObject<List<string>>(find.data);
                    var selection = Selections.Where(s => list.Contains(s.DatabaseValue));
                    var builder = getEmbedMessageBuilder(Title, Text, null, new List<DiscordEmbedFieldAbstract> {
                        getSelectionField(selection)
                    });

                    builder.AddComponents(getSelectComponent(true));
                    builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, $"list_select_{UniqueId}", "Auswahl bestätigen (Fortfahren)", true));
                    await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(builder));

                    using(var db = new WhitelistDb()) {
                        var procedure = db.whitelist_procedures.Include(p => p.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                        if(procedure != null) {
                            var already = procedure.whitelist_procedure_data.FirstOrDefault(d => d.name == DatabaseName);

                            if(already != null) {
                                already.finished = true;

                                db.SaveChanges();

                                log(LogLevel.Good, member, channel, "Auswahl akzeptiert", $"Die Auswahl wurde akzeptiert: `{already.data}` ");

                                finishStep(channel, member);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private DiscordSelectComponent getSelectComponent(bool disabled = false) {
            return new DiscordSelectComponent($"list_dropdown_{UniqueId}", null, Selections.Select(s => s.Selection), disabled, MinOptions, MaxOptions);
        }

        private DiscordEmbedFieldAbstract getSelectionField(IEnumerable<WhitelistSelection> selection = null) {
            var stringBuilder = new StringBuilder();

            foreach (var el in Selections) {
                if(selection != null && selection.Contains(el)) {
                    stringBuilder.AppendLine($"☒ {el.Selection.Label}");
                } else {
                    stringBuilder.AppendLine($"☐ {el.Selection.Label}");
                }
            }

            return new DiscordEmbedFieldAbstract(
                "Auswahlmöglichkeiten",
                stringBuilder.ToString(),
                false) {
            };
        }

        public async override void onStartStep(DiscordChannel channel, DiscordMember member) {
            await Task.Delay((int)InfoDelay.TotalMilliseconds);

            var builder = getEmbedMessageBuilder(Title, Text, null, new List<DiscordEmbedFieldAbstract> { getSelectionField() });
            builder.AddComponents(getSelectComponent());
            //builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, $"list_select_{UniqueId}", "Eingabe festlegen", false));

            var message = await builder.SendAsync(channel);

            using(var db = new WhitelistDb()) {
                var procedure = db.whitelist_procedures.FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                if(procedure != null) {
                    procedure.whitelist_procedure_data.Add(new whitelist_procedure_datum {
                        name = DatabaseName,
                        data = " ",
                        isInEdit = true,
                        messageId = message.Id
                    });
                    db.SaveChanges();
                }
            }

            await Task.Delay((int)InfoDelay.TotalMilliseconds);
        }
    }
}
