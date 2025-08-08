using DiscordBot.Model.Database;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DiscordBot.Controller.Whitelist.Steps {
    public class QuestionWhitelistStep : WhitelistStep {
        private static float WHITELIST_QUESTION_COUNT = 20; //20
        private static float WHITELIST_PASSING_PERCENTAGE = 0.75f; //0.75

        private static TimeSpan WHITELIST_FAIL_TIMEOUT = TimeSpan.FromDays(3);

        public QuestionWhitelistStep() : base("Whitelist Fragen", "", TimeSpan.FromMilliseconds(0)) { }

        public override void onEndStep(DiscordChannel channel, DiscordMember member) { }

        public async override Task<bool> onRelayData(DiscordChannel channel, DiscordMember member, string eventType, Dictionary<string, dynamic> data) {
            if(eventType != "COMPONENT_INTERACTION") {
                return false;
            }

            var interaction = (DiscordInteraction)data["INTERACTION"];
            if(data["COMPONENT_ID"] == $"questions_accept_rules") {
                startQuestions(channel, member);
                await interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getInitialEmbedBuilder(true)));

                return true;
            } else if(data["COMPONENT_ID"] == "whitelist_select_question") {
                using(var db = new WhitelistDb()) {
                    var test = db.whitelist_questions_tests.Include(wqt => wqt.whitelist_question_test_answers).ThenInclude(wqta => wqta.question).FirstOrDefault(t => t.userId == member.Id && t.channelId == channel.Id && !t.finished);

                    if(test != null) {
                        var answer = test.whitelist_question_test_answers.FirstOrDefault(a => !a.answered);

                        if(answer != null) {
                            answer.answer_1 = false; answer.answer_2 = false; answer.answer_3 = false; answer.answer_4 = false; answer.answer_5 = false;
                            foreach(var value in interaction.Data.Values) {
                                switch(value) {
                                    case "answer1":
                                        answer.answer_1 = true;
                                        break;
                                    case "answer2":
                                        answer.answer_2 = true;
                                        break;
                                    case "answer3":
                                        answer.answer_3 = true;
                                        break;
                                    case "answer4":
                                        answer.answer_4 = true;
                                        break;
                                    case "answer5":
                                        answer.answer_5 = true;
                                        break;
                                }
                            }

                            db.SaveChanges();

                            await interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getQuestionMessage(test.whitelist_question_test_answers.Count, QuestionState.NotAnswered, answer.question, answer, true)));

                            return true;
                        }
                    }
                }
            } else if(data["COMPONENT_ID"] == "whitelist_submit_question") {
                using(var db = new WhitelistDb()) {
                    var test = db.whitelist_questions_tests.Include(wqt => wqt.whitelist_question_test_answers).ThenInclude(wqta => wqta.question).FirstOrDefault(t => t.userId == member.Id && t.channelId == channel.Id && !t.finished);

                    if(test != null) {
                        var answer = test.whitelist_question_test_answers.FirstOrDefault(a => !a.answered);

                        if(answer != null) {
                            answer.answered = true;

                            QuestionState questionState;

                            if(answer.question.answer1Right == answer.answer_1 &&
                               answer.question.answer2Right == answer.answer_2 &&
                               answer.question.answer3Right == answer.answer_3 &&
                               answer.question.answer4Right == answer.answer_4 &&
                               answer.question.answer5Right == answer.answer_5) {
                                
                                log(LogLevel.Good, member, channel, "Frage richtig beantwortet", $"Der Spieler hat die Whitelistfrage mit Id: `{answer.questionId}` richtig beantwortet");

                                test.rightQuestions += 1;
                                questionState = QuestionState.Right;
                            } else {
                                var wrongString = "";

                                if(answer.question.answer1Right != answer.answer_1) wrongString += "Antwort **1** falsch ausgewählt, ";
                                if(answer.question.answer2Right != answer.answer_2) wrongString += "Antwort **2** falsch ausgewählt, ";
                                if(answer.question.answer3Right != answer.answer_3) wrongString += "Antwort **3** falsch ausgewählt, ";
                                if(answer.question.answer4Right != answer.answer_4) wrongString += "Antwort **4** falsch ausgewählt, ";
                                if(answer.question.answer5Right != answer.answer_5) wrongString += "Antwort **5** falsch ausgewählt, ";

                                wrongString.Substring(0, wrongString.Length - 3);

                                log(LogLevel.Bad, member, channel, "Frage falsch beantwortet", $"Der Spieler hat die Whitelistfrage mit Id: `{answer.questionId}` falsch beantwortet.\nEs wurde: {wrongString}");

                                test.wrongQuestions += 1;
                                questionState = QuestionState.Wrong;
                            }

                            await db.SaveChangesAsync();

                            await interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getQuestionMessage(test.whitelist_question_test_answers.Count, questionState, answer.question, answer, true, true)));

                            if((test.wrongQuestions / WHITELIST_QUESTION_COUNT) > (1 - WHITELIST_PASSING_PERCENTAGE)) {
                                var procedure = db.whitelist_procedures.FirstOrDefault(wd => wd.channelId == channel.Id && wd.userId == member.Id);

                                if(procedure != null) {
                                    procedure.notCanceable = true;

                                    test.finished = true;
         
                                    var sentMessage = await channel.SendMessageAsync(getWhitelistRedoMessage(DateTime.Now + WHITELIST_FAIL_TIMEOUT, false));

                                    procedure.whitelist_procedure_data.Add(new whitelist_procedure_datum {
                                        messageId = sentMessage.Id,
                                        name = "WHITELIST_REDO_DATE",
                                        data = JsonConvert.SerializeObject(DateTime.Now + WHITELIST_FAIL_TIMEOUT),
                                        finished = true,
                                        isInEdit = false,
                                    });

                                    db.SaveChanges();
                                }

                                log(LogLevel.Bad, member, channel, "Zu viele Fragen falsch beantwortet", $"Damit hat der Spieler zu viele Fragen falsch beantwortet und ist vorerst blockiert! Er darf die Whitelist wiederholen am {DateTime.Now + WHITELIST_FAIL_TIMEOUT}");

                                return true;
                            } else if(test.rightQuestions + test.wrongQuestions >= WHITELIST_QUESTION_COUNT) {
                                test.finished = true;
                                var procedure = db.whitelist_procedures.FirstOrDefault(wd => wd.channelId == channel.Id && wd.userId == member.Id);
                                if(procedure != null) {
                                    procedure.notCanceable = true;

                                    channel.sendChannelUnBlockMessage("Whitelist bestanden");
                                }
                                
                                await db.SaveChangesAsync();

                                var message = getEmbedMessageBuilder("Whitelist-Fragen bestanden!", "Herzlichen Glückwunsch! Du hast die Whitelist Fragen bestanden! Du kannst nun mit deiner Whitelist fortfahren.");
                                await channel.SendMessageAsync(message);

                                finishStep(channel, member);
                                return true;
                            } else {
                                var random = new Random();
                                var nextQuestion = db.whitelist_questions.OrderBy(r => EF.Functions.Random()).First();

                                while(test.whitelist_question_test_answers.Any(a => a.questionId == nextQuestion.id)) {
                                    nextQuestion = db.whitelist_questions.OrderBy(r => EF.Functions.Random()).First();
                                }

                                var message = getQuestionMessage(test.whitelist_question_test_answers.Count + 1, QuestionState.NotAnswered, nextQuestion, null);
                                var sentMessage = await channel.SendMessageAsync(message);

                                var newAnswer = new whitelist_question_test_answer {
                                    whitelistTestId = test.id,
                                    questionId = nextQuestion.id,
                                    answered = false,
                                    messageId = sentMessage.Id.ToString(),
                                };

                                db.whitelist_question_test_answers.Add(newAnswer);
                                await db.SaveChangesAsync();

                                return true;
                            }
                        }
                    }
                }
            } else if(data["COMPONENT_ID"] == "whitelist_redo_whitelist") {
                var values = (List<whitelist_procedure_datum>)data["DATA"];

                var value = values.FirstOrDefault(v => v.name == "WHITELIST_REDO_DATE");
                if(value != null) {
                    var date = JsonConvert.DeserializeObject<DateTime>(value.data);

                    if(DateTime.Now > date) {
                        using(var db = new WhitelistDb()) {
                            var procedure = db.whitelist_procedures.Include(wp => wp.whitelist_procedure_data).FirstOrDefault(wd => wd.channelId == channel.Id && wd.userId == member.Id && !wd.blocked && wd.notCanceable);

                            if(procedure != null) {
                                procedure.notCanceable = false;
                                procedure.cancelStartTime = procedure.cancelStartTime + WHITELIST_FAIL_TIMEOUT;

                                var datum = procedure.whitelist_procedure_data.FirstOrDefault(d => d.name == "WHITELIST_REDO_DATE");

                                procedure.whitelist_procedure_data.Remove(datum);

                                await db.SaveChangesAsync();
                            }
                        }

                        startQuestions(channel, member);

                        await interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(getWhitelistRedoMessage(date, true)));

                        log(LogLevel.Normal, member, channel, "Whitelist neu gestartet", $"Der Spieler hat die Whitelistfragerunde neu gestartet!");
                    }

                    return true;
                }
            }
            
            return false;
        }

        public async Task finishQuestions(int procedureId, DiscordMember member, DiscordChannel channel) {
            using(var db = new WhitelistDb()) {
                var procedure = db.whitelist_procedures.Include(wp => wp.whitelist_questions_tests).FirstOrDefault(wp => wp.id == procedureId);

                var test = procedure.whitelist_questions_tests.FirstOrDefault(wt => !wt.finished);

                if(test != null) {
                    test.finished = true;
                }

                if(procedure != null) {
                    procedure.notCanceable = true;

                    channel.sendChannelUnBlockMessage("Whitelist bestanden");
                }

                await db.SaveChangesAsync();

                var message = getEmbedMessageBuilder("Whitelist-Fragen bestanden!", "Herzlichen Glückwunsch! Du hast die Whitelist Fragen bestanden! Du kannst nun mit deiner Whitelist fortfahren.");
                await channel.SendMessageAsync(message);

                finishStep(channel, member);

            }
        }

        private async void startQuestions(DiscordChannel channel, DiscordMember member) {
            using(var db = new WhitelistDb()) {
                var newTest = new whitelist_questions_test {
                    channelId = channel.Id,
                    userId = member.Id,
                    rightQuestions = 0,
                    wrongQuestions = 0,
                    finished = false,
                };

                db.whitelist_questions_tests.Add(newTest);

                var procedure = db.whitelist_procedures.Include(wp => wp.whitelist_procedure_data).FirstOrDefault(wd => wd.channelId == channel.Id && wd.userId == member.Id && !wd.blocked);

                if(procedure != null) {
                    procedure.notCanceable = true;

                    channel.sendChannelBlockMessage("Erste Whitelistfrage erhalten");
                }

                await db.SaveChangesAsync();

                var random = new Random();
                var firstQuestion = db.whitelist_questions.OrderBy(r => EF.Functions.Random()).First();

                var message = getQuestionMessage(1, QuestionState.NotAnswered, firstQuestion, null);
                var sentMessage = await channel.SendMessageAsync(message);

                var newAnswer = new whitelist_question_test_answer {
                    whitelistTestId = newTest.id,
                    questionId = firstQuestion.id,
                    answered = false,
                    messageId = sentMessage.Id.ToString(),
                };

                db.whitelist_question_test_answers.Add(newAnswer);
                await db.SaveChangesAsync();
            }
        }

        private DiscordMessageBuilder getWhitelistRedoMessage(DateTime redoDate, bool disableButton) {
            var message = getEmbedMessageBuilder("Whitelist nicht bestanden!", $"Du hast leider zu viel Fragen falsch beantwortet und damit die Whitelist nicht bestanden. Aber keine Sorge, du kannst die Whitelist-Fragen in spätestens {WHITELIST_FAIL_TIMEOUT.TotalDays} Tagen wiederholen. Du kannst jedoch \"Einen Supporter rufen\" und mit ihm die Whitelistfragen die du beantwortet hast durchlesen. Gegebenenfalls kann dieser dann deine Whitelist verfrüht wieder aktivieren"
                                        , null,
                                        new List<DiscordEmbedFieldAbstract> {
                                            new DiscordEmbedFieldAbstract("Wiederholung", $"Du kannst die Whitelistfragen am {redoDate} wiederholen.", false)
                                        });
            message.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, $"whitelist_redo_whitelist", "Whitelist wiederholen (Erst ab Datum oben möglich)", disableButton));

            return message;
        }

        private enum QuestionState {
            NotAnswered,
            Right,
            Wrong,
        }

        private DiscordMessageBuilder getQuestionMessage(int questionNumber, QuestionState questionState, whitelist_question question, whitelist_question_test_answer alreadyAnswer = null, bool showButton = false, bool disabled = false) {
            var fields = new List<DiscordEmbedFieldAbstract>();
            var selectOptions = new List<DiscordSelectComponentOption>();

            //I didnt design the row for the database. Would have made it relational :/
            if(question.answer1 != null && question.answer1 != "") {
                var str = "☐";
                if(alreadyAnswer != null && (alreadyAnswer.answer_1 ?? false)) str = "☒"; 

                fields.Add(new DiscordEmbedFieldAbstract($"{str} Anwort 1", question.answer1, false));
                selectOptions.Add(new DiscordSelectComponentOption("Antwort 1", "answer1", null, str == "☒", new DiscordComponentEmoji("1️⃣")));
            }

            if(question.answer2 != null && question.answer2 != "") {
                var str = "☐";
                if(alreadyAnswer != null && (alreadyAnswer.answer_2 ?? false)) str = "☒";

                fields.Add(new DiscordEmbedFieldAbstract($"{str} Anwort 2", question.answer2, false));
                selectOptions.Add(new DiscordSelectComponentOption("Antwort 2", "answer2", null, str == "☒", new DiscordComponentEmoji("2️⃣")));
            }
            
            if(question.answer3 != null && question.answer3 != "") {
                var str = "☐";
                if(alreadyAnswer != null && (alreadyAnswer.answer_3 ?? false)) str = "☒";

                fields.Add(new DiscordEmbedFieldAbstract($"{str} Anwort 3", question.answer3, false));
                selectOptions.Add(new DiscordSelectComponentOption("Antwort 3", "answer3", null, str == "☒", new DiscordComponentEmoji("3️⃣")));
            }

            if(question.answer4 != null && question.answer4 != "") {
                var str = "☐";
                if(alreadyAnswer != null && (alreadyAnswer.answer_4 ?? false)) str = "☒";

                fields.Add(new DiscordEmbedFieldAbstract($"{str} Anwort 4", question.answer4, false));
                selectOptions.Add(new DiscordSelectComponentOption("Antwort 4", "answer4", null, str == "☒", new DiscordComponentEmoji("4️⃣")));
            }

            if(question.answer5 != null && question.answer5 != "") {
                var str = "☐";
                if(alreadyAnswer != null && (alreadyAnswer.answer_5 ?? false)) str = "☒";

                fields.Add(new DiscordEmbedFieldAbstract($"{str} Anwort 5", question.answer5, false));
                selectOptions.Add(new DiscordSelectComponentOption("Antwort 5", "answer5", null, str == "☒", new DiscordComponentEmoji("5️⃣")));
            }

            var color = MessageController.CHOICEV_COLOR;
            switch(questionState) {
                case QuestionState.Right:
                    color = new DiscordColor(87, 242, 135);
                    break;
                case QuestionState.Wrong:
                    color = new DiscordColor(237, 66, 69); 
                    break;
            }

            var message = getEmbedColorMessageBuilder(color, $"Frage {questionNumber}/{WHITELIST_QUESTION_COUNT}", question.question, "", fields);
            message.AddComponents(new DiscordSelectComponent($"whitelist_select_question", null, selectOptions, disabled, 1, selectOptions.Count));

            if(showButton) {
                message.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, $"whitelist_submit_question", $"Eingabe bestätigen", disabled));
            }

            return message;
        }

        private DiscordMessageBuilder getInitialEmbedBuilder(bool disableButton) {
            var initialEmbed = getEmbedMessageBuilder("❔ Whitelist Fragen ❔",
                                      $"Du musst jetzt {WHITELIST_QUESTION_COUNT} unserer Whitelist-Fragen beantworten. Dazu gibt es einige Regeln und Anmerkungen zu beachten. Lies sie dir bitte GENAU durch, und akzeptiere sie mit einem Klick auf den \"Regeln akzeptieren\". Falls es Fragen gibt \"Rufe eine Supporter\". Viel Erfolg!",
                                      null, new List<DiscordEmbedFieldAbstract> {
                                        new DiscordEmbedFieldAbstract("👤 Die Fragen alleine beantworten!", "Beantworte die Fragen alleine und ohne Hilfe! Es ist nicht erlaubt mit anderen Spielern zusammenzuarbeiten und bei der Festellung droht ein unwiderruflicher Ban!", false),
                                        new DiscordEmbedFieldAbstract("🏁 Bestehen der Fragen", $"Zum Bestehen der Fragen müssen {WHITELIST_PASSING_PERCENTAGE * 100}% der Fragen richtig beantwortet werden. Solltest du zu viele Fragen falsch beantworten bricht der Test ab. Du hast dann die Möglichkeit entweder {WHITELIST_FAIL_TIMEOUT.TotalDays} Tage zu warten und den Test erneut zu probieren, oder du \"Rufst einen Supporter\". Dieser kann den Test ggf. früher wieder für dich freischalten.", false),
                                        new DiscordEmbedFieldAbstract("❓ Unklarheiten", $"Sollten Unklarheiten für den Grund des Nichtbestehens aufkommen \"Rufe einen Supporter\" damit Fragen geklärt werden können.", false),
                                        new DiscordEmbedFieldAbstract("❌ Whitelist nicht mehr ohne Supporter abbrechbar", $"Sobald die erste Frage erscheint ist die Whitelist nicht mehr abbrechbar. Dies ist erforderlich um zu gewähleisten, dann die Whitelist nicht mehrmals widerholt werden kann.", false),
                                        new DiscordEmbedFieldAbstract("▶️ Durchführung", $"Du wird immer eine Frage nach der anderen erhalten. Bei jeder Frage ist mindestens EINE aber es können auch MEHRERE Antwortmöglichkeiten richtig sein. Es ist jedoch immer mindestens eine Antwort korrekt. Solltest du zu viele Fragen falsch beantwortet haben endet der Test automatisch und du wirst informiert wann du den Test erneut wahrnehmen kannst.", false),
                                        new DiscordEmbedFieldAbstract("🤫 Tipps zu den Fragen", $"Der Test besteht aus Regelfragen, welche nach dem Durchlesen und Verstehen des Regelwerkes beantwortet werden können, und aus RP-Verständnisfragen. Die RP-Verständisfragen sind in einem Format wie z.B: \"Welche Handlungen sind RP konform?\" formuliert. Diese Fragen sind **nicht nur** aus dem Blickwinkel des eigenen Charakters zu beantworten, sondern verlangen das **Hineinversetzen in alle möglichen Charaktere** und aus der Sicht dieser die Handlungsarten auf RP-Konformität zu überprüfen.\nAußerdem sind bei den Fragen nicht alle möglichen Handlungsarten aufgezeigt. Es kann besser oder alternative Möglichkeiten geben die Fragen zu beantworten, es geht jedoch um die Auswahl an Antworten die gegeben ist!", false),
                                      });

            initialEmbed.AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, $"questions_accept_rules", $"✅ Regeln akzeptieren", disableButton));

            return initialEmbed;
        }

        public async override void onStartStep(DiscordChannel channel, DiscordMember member) {
            await Task.Delay(InfoDelay);

            var initialEmbed = getInitialEmbedBuilder(false);

            log(LogLevel.Normal, member, channel, "Fragen gestartet", "Der Spieler hat den Fragenkatalog gestartet");
            await channel.SendMessageAsync(initialEmbed);
        }

        public async void resetQuestionTimeoutSpan(DiscordMember sender, DiscordChannel channel, int hours) {
            using(var db = new WhitelistDb()) {
                var prodedure = db.whitelist_procedures.Include(wp => wp.whitelist_procedure_data).FirstOrDefault(w => w.channelId == channel.Id && !w.blocked && w.notCanceable);

                if(prodedure != null) {
                    var data = prodedure.whitelist_procedure_data.FirstOrDefault(d => d.name == "WHITELIST_REDO_DATE");

                    if(data != null) {
                        var newDate = DateTime.Now + TimeSpan.FromHours(hours);
                        data.data = JsonConvert.SerializeObject(newDate);

                        await db.SaveChangesAsync();

                        var message = await channel.GetMessageAsync(data.messageId);
                        await message.ModifyAsync(getWhitelistRedoMessage(newDate, false));

                        Program.Bot.sendEmbedInChannel(channel, "Neues Fragen Wiederholungsdatum gesetzt!", $"Das Datum für die Wiederholung der Fragen wurde neue gesetzt auf den: {newDate}");
                    } else {
                        Program.Bot.sendEmbedToUser(sender.Id, "Fehler aufgetreten!", "Gib bitte dem Entwicklungsteam Bescheid!");
                    }
                } else {
                    Program.Bot.sendEmbedToUser(sender.Id, "Keine passende Whitelist gefunden!", "Es wurde keine passene Whitelist in diesem Channel gefunden!");
                }
            }
        }

        public override bool getHaltedBlocked(List<whitelist_procedure_datum> data) {
            var questionBlock = data.FirstOrDefault(d => d.name == "WHITELIST_REDO_DATE");

            if(questionBlock != null) {
                var date = JsonConvert.DeserializeObject<DateTime>(questionBlock.data);

                if(DateTime.Now < date) {
                    return true;
                } else {
                    return false;
                }
            }

            return false;
        }
    }
}
