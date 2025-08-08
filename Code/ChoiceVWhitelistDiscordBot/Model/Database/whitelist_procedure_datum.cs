using System;
using System.Collections.Generic;

namespace DiscordBot.Model.Database;

public partial class whitelist_procedure_datum
{
    public ulong userId { get; set; }

    public ulong channelId { get; set; }

    public string name { get; set; }

    public ulong messageId { get; set; }

    public string data { get; set; }

    public bool? isInEdit { get; set; }

    public bool finished { get; set; }

    public virtual whitelist_procedure whitelist_procedure { get; set; }
}
