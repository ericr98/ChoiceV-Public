using System;
using System.Collections.Generic;

namespace DiscordBot.Model.Database;

public partial class whitelist_question_test_answer
{
    public int whitelistTestId { get; set; }

    public uint questionId { get; set; }

    public string messageId { get; set; }

    public bool? answer_1 { get; set; }

    public bool? answer_2 { get; set; }

    public bool? answer_3 { get; set; }

    public bool? answer_4 { get; set; }

    public bool? answer_5 { get; set; }

    public bool answered { get; set; }

    public virtual whitelist_question question { get; set; }

    public virtual whitelist_questions_test whitelistTest { get; set; }
}
