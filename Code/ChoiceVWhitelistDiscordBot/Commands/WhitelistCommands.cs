using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Model.Database;
using Microsoft.EntityFrameworkCore;
using DiscordBot.Controller.Whitelist;
using DiscordBot.Controller.Whitelist.Steps;
using DiscordBot.Controller;
using DiscordBot.Base;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.VisualBasic;
using System.Threading.Channels;
using System.Diagnostics;

namespace DiscordBot {
    [SlashCommandGroup("Whitelist", "Commands für den Whitelist Teil des Servers")]
    public class DiscordSlashCommands : ApplicationCommandModule {
        [SlashCommand("ticket-log-id", "Lasse dir das Log eines Tickets anzeigen. In dieser Variante benutzt du die Whitelist Ticket ID")]
        public async Task showTicketLogId(InteractionContext ctx, [Option("Ticket-ID", "Die ID des Tickets")] string ticketIdStr) {
            using(var db = new WhitelistDb()) {
                var ticketId = int.Parse(ticketIdStr);
                var procedure = db.whitelist_procedures.Include(w => w.whitelist_procedures_logs).FirstOrDefault(w => w.id == ticketId);

                WhitelistLoggingController.openLoggingChannel(ctx.Guild, await ctx.Guild.GetMemberAsync(ctx.User.Id), ticketId, procedure.whitelist_procedures_logs.ToList());
            }

            ctx.RespondeToSlashCommand(true);
        }

        [SlashCommand("ticket-log-user", "Lasse dir das Log von Tickets anzeigen. Es können mehrere Channel erstellt werden!")]
        public async Task showTicketLogUser(InteractionContext ctx, [Option("Ersteller", "Der Ersteller der Whitelist")] DiscordUser member) {
            using(var db = new WhitelistDb()) {
                var procedures = db.whitelist_procedures.Include(w => w.whitelist_procedures_logs).Where(w => w.userId == member.Id);

                foreach(var procedure in procedures) {
                    WhitelistLoggingController.openLoggingChannel(ctx.Guild, await ctx.Guild.GetMemberAsync(ctx.User.Id), procedure.id, procedure.whitelist_procedures_logs.ToList());
                }
            }

            ctx.RespondeToSlashCommand(true);
        }

        [SlashCommand("ticket-log-socialclub", "Lasse dir das Log von Tickets anzeigen. Hier mithilfe des Socialclubs")]
        public async Task showTicketLogSocialclub(InteractionContext ctx, [Option("Socialclub", "Socialclub des Tickets")] string socialclub) {
            using(var db = new WhitelistDb()) {
                var procedures = db.whitelist_procedures.Include(w => w.whitelist_procedures_logs).Where(w => w.whitelist_procedure_data.Any(w => w.name == "SOCIALCLUB" && w.data == socialclub));

                foreach(var procedure in procedures) {
                    WhitelistLoggingController.openLoggingChannel(ctx.Guild, await ctx.Guild.GetMemberAsync(ctx.User.Id), procedure.id, procedure.whitelist_procedures_logs.ToList());
                }
            }

            ctx.RespondeToSlashCommand(true);
        }

        [SlashCommand("lösche-ticket", "Löscht ein Ticket, behält das Whitelistverfahren in der Datenbank. Im Ticket Channel auszuführen")]
        public async Task deleteTicket(InteractionContext ctx) {
            if(WhitelistProcedureManager.interactionPrecheck(ctx.Channel)) {
                using(var db = new WhitelistDb()) {
                    var result = await db.whitelist_procedures.Include(w => w.whitelist_procedures_logs).FirstOrDefaultAsync(w => w.channelId == ctx.Channel.Id);

                    WhitelistLoggingController.addWhiteListLog(LogLevel.Bad, result.userId, ctx.Channel, result.currentStep + 1, $"Whitelist-Ticket gelöscht", $"Der Support: {ctx.Member.Mention} mit Id: {ctx.Member.Id} hat das Ticket zu diesem Verfahren gelöscht.");
                }
                await ctx.Channel.DeleteAsync();
            }

            return;
        }


        [SlashCommand("fragen-timeout", "Setze den Whitelist Fragen Timeout des Channels in dem du den Command auführst neu")]
        public Task setQuestionTimeout(InteractionContext ctx, [Option("Zeitspanne", "Zeitspanne AB JETZT die Spieler noch blockiert ist (Zahl in Stunden)")] string timeSpan) {
            int hours = 0;
            try {
                hours = int.Parse(timeSpan);
            } catch(Exception ex) {
                Program.Bot.sendEmbedToUser(ctx.Member.Id, "Fehler aufgetreten!", "Dein Eingabe war vermutlich keine Zahl!");
                return Task.CompletedTask;
            }

            WhitelistProcedureManager.resetQuestionTimeoutSpan(ctx.Member, ctx.Channel, hours);

            ctx.RespondeToSlashCommand(true);

            return Task.CompletedTask;
        }

        [SlashCommand("fragen-erklärung", "Lasse dir die Erklärung einer Whitelist Frage ausgegeben. Nur du kannst diese Nachricht sehen!")]
        public async Task getQuestionExplanation(InteractionContext ctx, [Option("Fragen-ID", "Die ID der Frage. Zu findem im Log")] string questionIdStr) {
            using(var db = new WhitelistDb()) {
                var questionId = int.Parse(questionIdStr);

                var question = db.whitelist_questions.FirstOrDefault(q => q.id == questionId);

                var embed = MessageController.getStandardEmbed($"Erklärung zu Frage: {questionId}", $"Erklärung: {question.explanation}");
                var builder = new DiscordMessageBuilder()
                    .AddEmbed(embed.Build());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(builder).AsEphemeral(true));
            }

            return;
        }

        [SlashCommand("aus-ticket-entfernen", "Entfernt dich aus einem Whitelist-Ticket")]
        public async Task removeFromTicket(InteractionContext ctx) {
            ctx.RespondeToSlashCommand(await SupportController.removeSelfFromTicket(ctx.Member, ctx.Channel));

            return;
        }

        [SlashCommand("status-setzen", "De-/Aktiviere die Whitelist")]
        public Task setWhitelistState(InteractionContext ctx, 
            [Choice("Aktiviert", "ACTIVE")]
            [Choice("Deaktivert", "DEACTIVATED")]
            [Option("Status", "Der Status der Whitelist. Aktiviert oder deaktiviert die Whitelist")] string state) {
            ConfigController.setConfigData("WHITELIST_STATE", state);

            WhitelistProcedureManager.changeWhitelistState(state == "ACTIVE");

            ctx.RespondeToSlashCommand(true);

            return Task.CompletedTask;
        }

        [SlashCommand("fragen-überspringen", "Wenn ein Spieler die Fragen erreicht hat überspringen")]
        public Task skipQuestions(InteractionContext ctx) {
            WhitelistProcedureManager.skipWhitelistQuestion(ctx.Channel);

            ctx.RespondeToSlashCommand(true);

            return Task.CompletedTask;
        }

        [SlashCommand("ticket-fertig-setzen", "Setze das Whitelist Ticket als fertig oder aktiviere es wieder")]
        public Task setFinished(InteractionContext ctx, 
            [Choice("Fertig", "BLOCKED")]
            [Choice("Unfertig", "UNBLOCKED")]
            [Option("Status", "Der Status dieser Whitelist. Setze sie als fertig oder unfertig")] string state) {

            WhitelistProcedureManager.setWhitelistTicketFinished(ctx.Channel, state == "BLOCKED");

            ctx.Channel.sendChannelBlockMessage("Von Supporter als Fertig gesetzt");

            ctx.RespondeToSlashCommand(true);

            return Task.CompletedTask;
        }

        [SlashCommand("ticket-unabbrechbar-setzen", "Setze das Ticket als unabbrechbar oder aktivere das Abbrechen wieder")]
        public Task setNotcanceable(InteractionContext ctx,
            [Choice("Unabbrechbar", "BLOCKED")]
            [Choice("Abbrechbar", "UNBLOCKED")]
            [Option("Status", "Der Status dieser Whitelist. Setze sie als abbrechbar oder unabbrechbar")] string state) {

            WhitelistProcedureManager.setWhitelistTicketCanceable(ctx.Channel, state == "BLOCKED");

            ctx.Channel.sendChannelBlockMessage("Von Supporter blockiert");

            ctx.RespondeToSlashCommand(true);

            return Task.CompletedTask;
        }

        [SlashCommand("server-restarten", "Restarte/Starte den Whitelist Server")]
        public async Task whitelistServerRestart(InteractionContext ctx) {
            var channel = ctx.Guild.GetChannel(Config.ServerCommandInfoChannel);

            Program.Bot.sendEmbedInChannel(channel, "Whitelist-Server restartet", $"Der Whitelist Server wurde von {ctx.Member.Mention} restartet! Er könnte ein paar Minuten zum hochfahren benötigen!");
            ctx.RespondeToSlashCommand(true);

            Process.Start($"{Config.AltVWhitelistServerPath}server-stop.bat");
            await Task.Delay(5000);
            Process.Start($"{Config.AltVWhitelistServerPath}server-start.bat");

            return;
        }
    }
}
