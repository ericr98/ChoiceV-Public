using DiscordBot.Controller.Whitelist.Steps;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot.Controller.Whitelist {
    public class WhitelistController : BotScript {
        private static WhitelistProcedureManager ProcedureManager;
        private static TimeSpan MAX_WHITELIST_ACTIVE_TIME = TimeSpan.FromDays(7);

        public WhitelistController() {
            ProcedureManager = new WhitelistProcedureManager();

            ProcedureManager.addStep(new InitialWhitelistReactStep());
            ProcedureManager.addStep(new AcceptWhitelistReactStep("📔 Regeln akzeptieren 📔", "Mit dem Klicken des \"Regeln akzeptieren\" Knopfes bin ich mir folgender Regeln und Tatsachen bewusst und akzeptiere diese.", TimeSpan.FromMilliseconds(75), null,
                new List<DiscordEmbedFieldAbstract> {
                    new DiscordEmbedFieldAbstract("🗒️ Whitelist wird geloggt", "Auch wenn kein Supporter zu dem Whitelistticket hinzugefügt wird, so werden ALLE Aktionen innerhalb dieses Tickets auch nach beenden des Tickets für Supportzwecke geloggt. Es werden keine persönlichen Daten (bis auf die grundsätzlich anonyme ID des Discordaccounts) gespeichert, jedoch alle Aktionen und Nachrichten, welche in diesem Channel passieren. Diese Daten werden, sollte die Whitelist abgebrochen werden, gelöscht, es sei denn diese Whitelist wurde von einem Supporter als \"Blockiert\" gekennzeichnet oder wird vom Ticket Bot als \"Nicht abbrechbar\" markiert. Sollte dies passieren wird das in diesem Ticket gekennzeichnet. Solltest du die Whitelist dennoch abbrechen wollen bitten wir dich, einen \"Supporter zu rufen\" und ihm die Situation zu erklären.", false),
                    new DiscordEmbedFieldAbstract("☑️ Regeln werden akzeptiert", "Es werden unsere Serverregeln, welche [HIER](https://choicev.net/home/regeln/) zu finden sind zur Kentniss genommen und akzeptiert. Diese Regeln gelten für alle \"Kanäle\" (z.B. Discord, Teamspeak, Webseite, Gameserver) von ChoiceV. Bei Verstoß gegen diese Regeln können Sanktionen, welche sich wiederum auf unsere Kanäle beziehen, wie z.B. Timeouts oder Bans nach sich ziehen.", false),
                    new DiscordEmbedFieldAbstract("⏰ Maximale Bearbeitungszeit", $"Deine Whitelist ist nach Erstellung maximal {MAX_WHITELIST_ACTIVE_TIME.TotalDays} Tage gültig. Nach Ablauf dieser Zeit wird ein Supporter sich deine Whitelist anschauen und diese, falls kein Kontakt zu dir möglich ist, löschen.", false)
                }).addAcceptButton("Regeln akzeptieren", "Regeln akzeptiert", new DiscordComponentEmoji("✅"))
            );

            ProcedureManager.addStep(new InputWhitelistStep("👤 GTA Socialclub eintragen 👤",
                        "Trage hier den Socialclub des Rockstar Accounts ein, für welchen diese Whitelist gültig ist. Der Socialclub-Name kann später UNTER KEINEN UMSTÄNDEN mehr angepasst werden. Bei falscher Eingabe muss im schlimmsten Fall die KOMPLETTE Whitelist wiederholt werden. Achte auch auf Groß- und Kleinschreibung!"
                        , TimeSpan.FromMilliseconds(75))
                        .addTextInput(TextInputStyle.Short, "👤 Socialclub", "Hier den Socialclub Namen eintragen", "SOCIALCLUB", 0, 100));

            ProcedureManager.addStep(new TeamspeakWhitelistStep());

            ProcedureManager.addStep(new InputWhitelistStep("👤 Charakter-Daten eingeben 👤",
                "Gib nun die Daten deines Charakters ein.\n " +
                "🇺🇸 Beachte, dass der Charakter die **amerikanische Staatsbürgerschaft** besitzen muss. Dies ist unter anderem möglich, durch: Geburt auf Staatsgebiet der USA (einschließlich Außengebiete der USA z.B. Puerto Rico), Einbürgerung durch langjährige Arbeit in einem US Bundestaat, ein Elternteil ist US Staatsbürger, etc.\n" +
                "📝 Weiterhin sind die **Zeichenlimits** für die Charaktergeschichte zu beachten. Fokussiere dich bitte auf die Informationen, welche für den \"Charakter\" deines Charakters verantwortlich sind. Du brauchst hier also nicht anzugeben, dass dein Charakter Zebrabars mag."
                , TimeSpan.FromMilliseconds(75))
                .addTextInput(TextInputStyle.Short, "👤 Charakter-Name mit Titeln", "Hier den Charakternamen eingeben", "CHAR_NAME", 0, 50)
                .addTextInput(TextInputStyle.Short, "👵 Charakter-Alter", "Hier das Alter eingeben", "CHAR_AGE", 0, 50)
                .addTextInput(TextInputStyle.Paragraph, "📖 Charakter-Geschichte", "Füge hier die Geschichte deines Charakters ein", "CHAR_HISTORY", 500, 3000));


            ProcedureManager.addStep(new ListWhitelistStep("👥 Charakter Gesinnung 👥", "Wähle im folgenden die Gesinnung des Charakters aus. Soll der Charakter ein  **Zivilist** 👷 sein (arbeiten in z.B. Anwalt, Mechaniker, Stadtverwaltung, Autohaus, etc.), soll er eine **kriminelle Laufbahn** \U0001f977 eingeschlagen haben (z.B. Dealer, Hehler, Einbrecher) oder soll er für eine unserer **Fraktionen** \U0001f9d1‍⚖️ arbeiten (z.B. Polizei, Sheriff, Gericht, Arzt)? Wenn du dir nicht zu 100% sicher bist kannst du auch mehrere Gesinnungen auswählen. Beachte jedoch, dass die Anzahl an Personen die bei uns kriminellen Aktivitäten nachgehen können limitiert ist. Falls du dir unsicher bist unter was deine Vorstellung fällt \"Rufe einen Supporter\".", "CHAR_PROFESSION", 1, 3, TimeSpan.FromMilliseconds(50))
                                    .addSelection("civil", "Zivilist", "Spiele als Zivilist und erlebe Los Santos", new DiscordComponentEmoji("👷"))
                                    .addSelection("crime", "Kriminell", "Siehe die kriminelle Seite von Los Santos kennen", new DiscordComponentEmoji("\U0001f977"))
                                    .addSelection("faction", "Fraktion", "Helfe dabei Recht und Ordnung aufrecht zu erhalten", new DiscordComponentEmoji("🧑‍⚖️")));

            ProcedureManager.addStep(new ListWhitelistStep("🔍 Wie hast du uns gefunden? 🔍", "Die letzte Frage, bevor es zum Whitelist-Test geht, ist wie du uns gefunden hast. Diese Information hilft uns sehr um herauszufinden welche Kanäle am ansprechensten für euch Spieler ist, damit wir ein besseres Erlebnis bieten können.", "WHERE_FIND", 1, 1, TimeSpan.FromMilliseconds(50))
                                    .addSelection("website", "Webseite", "Über unsere Webseite/Suchmaschine (z.B. Google)", new DiscordComponentEmoji("🔍"))
                                    .addSelection("stream", "\"Fremde\" Stream", "Über einen Streamer der bei uns spielt", new DiscordComponentEmoji("🖥"))
                                    .addSelection("choicev_stream", "ChoiceV Streams", "Über einen unserer offiziellen Streams/Videos", new DiscordComponentEmoji(Config.ServerEmojiId))
                                    .addSelection("reference", "Referenz", "Jemand hat dir über uns erzählt (kein Streamer)", new DiscordComponentEmoji("🙋"))
                                    .addSelection("misc", "Sonstiges", "Alles was nicht gelistet ist", new DiscordComponentEmoji("❓")));

            ProcedureManager.addStep(new AcceptWhitelistReactStep($"{Config.ServerEmojiName} Whitelist Ablauf {Config.ServerEmojiName}", "Damit du den Überblick über die Whitelist behalten kannst, wollen wir dich hier noch einmal kurz über den weiteren Ablauf der Whitelist informieren.", TimeSpan.FromMilliseconds(100), null,
                new List<DiscordEmbedFieldAbstract> {
                    new DiscordEmbedFieldAbstract("1. ❔ Whitelist Fragen", "Du erhälst gleich Whitelist Fragen die RP- und Regelverständniss abfragen. Beachte: Sobald du die erste Frage erhalten hast wird dein Ticket auf \"Nicht abbrechbar\" gesetzt!", false),
                    new DiscordEmbedFieldAbstract("2. ℹ️ Support Überprüfung", "Bei erfolgreichem Beantworten der Fragen wird sich einer unserer Whitelist Supporter deine Bewerbung anschauen und dir ggf. Feedback und Änderungswünsche bei z.B. der Charaktergeschichte geben.", false),
                    new DiscordEmbedFieldAbstract("3. 🎭 Whitelist Szenario", "Um sicherzustellen, dass das RP Verständniss nicht nur theoretisch sondern auch praktisch bei dir besteht wirst du nun eines unserer Whitelist-Szenarien spielen. In diesen Szenarien wird dir für kurze Zeit (ca. 15 - 20min) ein Charakter zugewiesen und du spielst mit anderen eine kurze RP Situation. Ein Whitelist Supporter wird dies begleiten und anschauen. Dies stellt sicher, dass du auch in ungewohnten Situationen ein flüßiges RP Erlebniss für dich selbst und andere bieten kannst.", false),
                    new DiscordEmbedFieldAbstract("4. 🛫 Einreise!", "Hast du das alles bestanden können wir uns mehr als sicher sein, dass du ein ausreichendes RP und Regelverständnis (theoretisch UND praktisch) mitbringst und es bleibt nur noch die Einreise auf unserem Server! Steige also in den nächsten Flug nach Los Santos und schreibe deine Geschichte!", false)
                }).addAcceptButton("Fortfahren", "Fortfahren", new DiscordComponentEmoji("⏩"))
            );
            ProcedureManager.addStep(new QuestionWhitelistStep());

            ProcedureManager.addStep(new TeamApprovalWhitelistStep("ℹ️ Teamüberprüfung anfordern ℹ️", "Da du nun die Whitelist-Fragen bestanden hast, wird es Zeit, dass sich einer unserer Whitelist-Supporter deine Whitelist anschaut. Fordere einen Supporter mit einem Klick auf \"Überprüfung anfordern\" unten an. Ein Whitelist Supporter wird sich so schnell es geht bei dir melden. Alles weitere wird weiter in diesem Kanal besprochen.", "Whitelist-Fragen bestanden", TimeSpan.FromMilliseconds(75))
               .addApprovalButton("Überprüfung anfordern", "Whitelist bestätigen (für Teammitglied)", "Whitelist bestätigt", new DiscordComponentEmoji("ℹ️"), new DiscordComponentEmoji("✅"))
            );

            ProcedureManager.addStep(new TeamApprovalWhitelistStep("🎭 Whitelist Szenario bestanden 🎭", "Wenn du das Whitelist Szenario bestanden hast, dann fordere hier einen Whitelist Supporter an, der dir dies bestätigt. Mache dies auch wenn sich ein Supporter schon in deinem Ticket befindet. Dies ist wichtig für das Log und die Verständlichkeit.", "Whitelist Szenario bestanden", TimeSpan.FromMilliseconds(75))
               .addApprovalButton("Überprüfung anfordern", "\"Whitelist Szenario bestanden?\"-Anfrage", "Szenario bestätigt", new DiscordComponentEmoji("ℹ️"), new DiscordComponentEmoji("✅"))
            );

            ProcedureManager.addStep(new AcceptWhitelistReactStep("🛫 Bist du bereit für die Einreise? 🛫", "Du hast die Whitelist fast geschafft, es gibt nur noch eine einzige wichtige Frage zu beantworten.. Fühlst du dich bereit für die Einreise auf ChoiceV? Wenn die Antwort Ja! ist, dann hält dich nichts mehr ab, außer dies zu bestätigen und dich bei uns einzuloggen!", TimeSpan.Zero)
                .addAcceptButton("Ich bin bereit!", "Ich bin bereit!", new DiscordComponentEmoji("🛫")));
             
        }

        public static void startWhitelistProcedure(DiscordGuild guild, DiscordMember member) {
            ProcedureManager.startProcedure(guild, member);
        }

        public async static void  cancelWhitelistProcedure(DiscordGuild guild, DiscordChannel channel, DiscordMember member) {
            
        }

        public async static void finishWhitelistProcedure(DiscordGuild guild, DiscordChannel channel, DiscordMember member) {
            await member.GrantRoleAsync(guild.GetRole(Config.DiscordSucceedRoleId));

            //TODO Create Account in Root Database
            //TODO Use TeamspeakController.giveTeamspeakSuceedRank
        }
    }
}
