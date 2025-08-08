using System;
using System.Collections.Generic;

namespace DiscordBot.Model.Database;

public partial class whitelist_questions_test
{
    public int id { get; set; }

    public ulong userId { get; set; }

    public ulong channelId { get; set; }

    public int wrongQuestions { get; set; }

    public int rightQuestions { get; set; }

    public bool finished { get; set; }

    public virtual whitelist_procedure whitelist_procedure { get; set; }

    public virtual ICollection<whitelist_question_test_answer> whitelist_question_test_answers { get; } = new List<whitelist_question_test_answer>();
}
