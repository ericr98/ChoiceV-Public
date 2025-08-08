using System;
using System.Collections.Generic;

namespace DiscordBot.Model.Database;

public partial class whitelist_procedure
{
    public int id { get; set; }

    public ulong userId { get; set; }

    public ulong channelId { get; set; }

    public int currentStep { get; set; }

    public bool blocked { get; set; }

    public bool notCanceable { get; set; }

    public DateTime startTime { get; set; }

    public DateTime cancelStartTime { get; set; }

    public virtual ICollection<whitelist_procedure_datum> whitelist_procedure_data { get; } = new List<whitelist_procedure_datum>();

    public virtual ICollection<whitelist_procedures_log> whitelist_procedures_logs { get; } = new List<whitelist_procedures_log>();

    public virtual ICollection<whitelist_questions_test> whitelist_questions_tests { get; } = new List<whitelist_questions_test>();
}
