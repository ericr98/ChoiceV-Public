using System;
using System.Collections.Generic;

namespace DiscordBot.Model.Database;

public partial class whitelist_procedures_log
{
    public int id { get; set; }

    public ulong userId { get; set; }

    public ulong channelId { get; set; }

    public int step { get; set; }

    public string title { get; set; }

    public string message { get; set; }

    public DateTime date { get; set; }

    public int level { get; set; }

    public virtual whitelist_procedure whitelist_procedure { get; set; }
}
