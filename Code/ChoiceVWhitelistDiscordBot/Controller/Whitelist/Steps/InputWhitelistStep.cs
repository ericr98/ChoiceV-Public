using DiscordBot.Base;
using DiscordBot.Model.Database;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Controller.Whitelist.Steps {
    public class InputWhitelistStep : WhitelistStep {
        //private const int MAX_MESSAGE_LENGTH = 3000;

        private readonly string Footer;

        private class WhitelistStepInput {
            public TextInputStyle Style;
            public string InputName;
            public string InputPlaceholder;
            public string DatabaseName;

            public int MinLength;
            public int MaxLength;

            public WhitelistStepInput(TextInputStyle style, string inputName, string inputPlaceholder, string databaseName, int minLength, int maxLength) {
                Style = style;
                InputName = inputName;
                InputPlaceholder = inputPlaceholder;
                DatabaseName = databaseName;
                MinLength = minLength;
                MaxLength = maxLength;
            }
        }

        private List<WhitelistStepInput> Inputs;

        public InputWhitelistStep(string title, string startText, TimeSpan infoDelay, string footer = null) : base(title, startText, infoDelay) {
            Footer = footer;

            Inputs = new List<WhitelistStepInput>();
        }

        public InputWhitelistStep addTextInput(TextInputStyle style, string inputName, string inputPlaceholder, string databaseName, int minLength = 0, int maxLength = 1024) {
            Inputs.Add(new WhitelistStepInput(style, inputName, inputPlaceholder, databaseName, minLength, maxLength));
            return this;
        }

        public override void onEndStep(DiscordChannel channel, DiscordMember member) { }

        public async override Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) {
            var values = (List<whitelist_procedure_datum>)data["DATA"];
            //if(eventType == "MESSAGE_SENT") {
            //    var value = values.FirstOrDefault(i => i.name == DatabaseName);
            //    if(value != null && value.finished) {
            //        return false;
            //    }

            //    var message = (DiscordMessage)data["MESSAGE"];

            //    var messageChanged = false;
            //    ulong messageId = 0;

            //    if(values != null) {
            //        if(value != null) {
            //            //Else just messages in channel
            //            if(value.isInEdit??false) {
            //                var prev = " ";
            //                using(var db = new WhitelistDb()) {
            //                    var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);
            //                    var find = procedure.whitelist_procedure_data.FirstOrDefault(wd => wd.name == DatabaseName);
            //                    prev = find.data;
            //                    find.data = message.Content;
            //                    find.isInEdit = false;
            //                    db.SaveChanges();

            //                    messageChanged = true;
            //                    messageId = ulong.Parse(find.messageId);
            //                }

            //                log(LogLevel.Debug, member, channel, "Inhalt geändert", $"Inhalt des Schrittes von `{prev}` zu `{message.Content}` geändert");
            //            }
            //        }
            //    }

            //    if(messageChanged) {
            //        var dcMessage = await channel.GetMessageAsync(messageId);
            //        await dcMessage.ModifyAsync(getEmbedMessageBuilderStep(false, message.Content));
            //        await message.DeleteAsync();
            //    }

            //    return true;
            //} else if(eventType == "COMPONENT_INTERACTION") {
            //    var interaction = (DiscordInteraction)data["INTERACTION"];
            //    if(data["COMPONENT_ID"] == $"edit_input_{UniqueId}" || data["COMPONENT_ID"] == $"edit_stop_{UniqueId}") {
            //        using(var db = new WhitelistDb()) {
            //            var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);
            //            var find = procedure.whitelist_procedure_data.FirstOrDefault(wd => wd.name == DatabaseName);

            //            find.isInEdit = data["COMPONENT_ID"] == $"edit_input_{UniqueId}";
            //            db.SaveChanges();

            //            await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getEmbedMessageBuilderStep(find.isInEdit?? false, find.data)));

            //            log(LogLevel.Debug, member, channel, "Inhalt zurückgesetzt", $"Inhalt des Schrittes für das Überschreiben markiert");

            //            return true;
            //        }
            //    } else if(data["COMPONENT_ID"] == $"accept_input_{UniqueId}") {
            //        using(var db = new WhitelistDb()) {
            //            var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);
            //            var find = procedure.whitelist_procedure_data.FirstOrDefault(wd => wd.name == DatabaseName);

            //            find.isInEdit = false;
            //            find.finished = true;
            //            db.SaveChanges();

            //            log(LogLevel.Good, member, channel, "Inhalt akzeptiert", $"Den Schritt abgeschlossen mit Inhalt: `{find.data}` ");

            //            finishStep(channel, member);
            //            await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getEmbedMessageBuilderStep(false, find.data, true)));

            //            return true;
            //        }
            //    }
            //}

            if(eventType == "COMPONENT_INTERACTION") {
                var interaction = (DiscordInteraction)data["INTERACTION"];
                if(data["COMPONENT_ID"] == $"accept_input_{UniqueId}") {

                    using(var db = new WhitelistDb()) {
                        var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                        var fields = new Dictionary<string, string>();
                        foreach(var input in Inputs) {
                            var find = procedure.whitelist_procedure_data.FirstOrDefault(wd => wd.name == input.DatabaseName);

                            find.isInEdit = false;
                            find.finished = true;
                            db.SaveChanges();

                            fields.Add(input.DatabaseName, find.data);
                        }
                        log(LogLevel.Good, member, channel, "Inhalt akzeptiert", $"Den Schritt abgeschlossen");

                        finishStep(channel, member);
                        await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getEmbedMessageBuilderStep(fields, true)));
                    }
                    return true;
                } else if(data["COMPONENT_ID"] == $"edit_input_{UniqueId}") {
                    var response = new DiscordInteractionResponseBuilder();

                    response
                      .WithTitle(Title)
                      .WithCustomId($"input_edit_modal_{UniqueId}");
                      
                    for(int i = 0; i < Inputs.Count; i++) {
                        var input = Inputs[i];
                        //response.AddComponents(new TextInputComponent("TEst", $"t_{i}", "Test", null, true, TextInputStyle.Paragraph, 0, 100));
                        response.AddComponents(new TextInputComponent(input.InputName, $"input_{i}", input.InputPlaceholder, null, true, input.Style, input.MinLength, input.MaxLength));
                    }

                    await interaction.CreateResponseAsync(InteractionResponseType.Modal, response);

                    return true;
                }

            } else if(eventType == "MODAL_SUBMIT") {
                var dbValues = values.Where(i => Inputs.Select(i => i.DatabaseName).Contains(i.name)).ToList();
                if(dbValues != null && dbValues.Count > 0 && dbValues.Any(v => v.finished)) {
                    return false;
                }

                if(values != null) {
                    if(dbValues != null && dbValues.Count > 0) {
                        using(var db = new WhitelistDb()) {
                            var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedure_data).FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                            var modalValues = (IReadOnlyDictionary<string, string>)data["VALUES"];

                            var fields = new Dictionary<string, string>();
                            for(int i = 0; i < Inputs.Count; i++) {
                                var input = Inputs[i];

                                var dbField = procedure.whitelist_procedure_data.FirstOrDefault(d => d.name == input.DatabaseName);
                                if(dbField != null && modalValues.ContainsKey($"input_{i}")) {
                                    var modalValue = modalValues[$"input_{i}"];

                                    var prev = dbField.data;
                                    dbField.data = modalValue;

                                    fields.Add(input.DatabaseName, modalValue);

                                    log(LogLevel.Debug, member, channel, "Inhalt geändert", $"Inhalt des Felds {input.InputName} von `{prev.TakeWithDots(100)}` zu `{modalValue.TakeWithDots(100)}` geändert");
                                }
                            }

                            var interaction = (DiscordInteraction)data["INTERACTION"];
                            await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getEmbedMessageBuilderStep(fields, false)));

                            db.SaveChanges();

                            return true;
                        }
                    }
                }

            }
            return false;
        }

        public async override void onStartStep(DiscordChannel channel, DiscordMember member) {
            await Task.Delay((int)InfoDelay.TotalMilliseconds);

            var message = await channel.SendMessageAsync(getEmbedMessageBuilderStep());

            using(var db = new WhitelistDb()) {
                var procedure = db.whitelist_procedures.FirstOrDefault(w => w.userId == member.Id && w.channelId == channel.Id);

                if(procedure != null) {
                    foreach(var input in Inputs) {
                        procedure.whitelist_procedure_data.Add(new whitelist_procedure_datum {
                            name = input.DatabaseName,
                            data = " ",
                            isInEdit = true,
                            messageId = message.Id
                        });
                        db.SaveChanges();
                    }
                }
            }

            await Task.Delay((int)InfoDelay.TotalMilliseconds);
        }

        private DiscordMessageBuilder getEmbedMessageBuilderStep(Dictionary<string, string> userInput = null, bool deactivateAllButtons = false) {

            var list = new List<DiscordEmbedFieldAbstract>();

            foreach(var input in Inputs) {
                if(userInput != null && userInput.ContainsKey(input.DatabaseName)) {
                    var value = userInput[input.DatabaseName];
                    if(value.Length > 1024) {
                        var chunks = value.Chunk(1024).ToList();

                        for(var i = 0; i < chunks.Count; i++) {
                            list.Add(new DiscordEmbedFieldAbstract($"{input.InputName} ({i + 1}/{chunks.Count})", new string(chunks[i]), false));
                        }
                    } else {
                        list.Add(new DiscordEmbedFieldAbstract(input.InputName, value, false));
                    }
                } else {
                    list.Add(new DiscordEmbedFieldAbstract(input.InputName, $"_{input.InputPlaceholder}_", false));
                }
            }
            

            var builder = getEmbedMessageBuilder(Title, Text, Footer, list);


            var components = new List<DiscordButtonComponent> {
                    new DiscordButtonComponent(ButtonStyle.Success, $"accept_input_{UniqueId}", "Fortfahren", deactivateAllButtons || userInput == null),
                    new DiscordButtonComponent(ButtonStyle.Primary, $"edit_input_{UniqueId}", "Eingabe öffnen", deactivateAllButtons)};

            builder.AddComponents(components);

            return builder;
        }
    }
}
