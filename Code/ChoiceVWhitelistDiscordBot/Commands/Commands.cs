using DiscordBot.Controller.Whitelist;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot {
   // public class Commands : BaseCommandModule {
       // public int Id = 0;

        //[Command("ticket")]
        //public Task ticket(CommandContext ctx) {
        //    WhitelistController.startWhitelistProcedure(ctx.Guild, ctx.Member);
        //    return Task.CompletedTask;
        //}

        //[Command("test")]
        //public Task test(CommandContext ctx) {
        //    var options = new List<DiscordSelectComponentOption>() {
        //            new DiscordSelectComponentOption(
        //                "Label, no description",
        //                "label_no_desc"),

        //            new DiscordSelectComponentOption(
        //                "Label, Description",
        //                "label_with_desc",
        //                "This is a description!"),

        //            new DiscordSelectComponentOption(
        //                "Label, Description, Emoji",
        //                "label_with_desc_emoji",
        //                "This is a description!",
        //                emoji: new DiscordComponentEmoji(854260064906117121)),

        //            new DiscordSelectComponentOption(
        //                "Label, Description, Emoji (Default)",
        //                "label_with_desc_emoji_default",
        //                "This is a description!",
        //                isDefault: true,
        //                new DiscordComponentEmoji(854260064906117121))
        //        };

        //    // Make the dropdown
        //    var dropdown = new DiscordSelectComponent("dropdown", null, options, false, 1, 2);
        //    return Task.CompletedTask;
        //}

        //[Command("test2")]
        //public async Task test2(CommandContext ctx) {
        //    var reallyLongString = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.   \r\n\r\nDuis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.   \r\n\r\nUt wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi.   \r\n\r\nNam liber tempor cum soluta nobis eleifend option congue nihil imperdiet doming id quod mazim placerat facer possim assum. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat.   \r\n\r\nDuis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis.   \r\n\r\nAt vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, At accusam aliquyam diam diam dolore dolores duo eirmod eos erat, et nonumy sed tempor et et invidunt justo labore Stet clita ea et gubergren, kasd magna no rebum. sanctus sea sed takimata ut vero voluptua. est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur";

        //    var interactivity = ctx.Client.GetInteractivity();
        //    var pages = interactivity.GeneratePagesInEmbed(reallyLongString, DSharpPlus.Interactivity.Enums.SplitType.Character);

        //    await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages, DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround, DSharpPlus.Interactivity.Enums.ButtonPaginationBehavior.DeleteMessage);
        //    return;
        //}
    //}
}
