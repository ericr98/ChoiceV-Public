using System;
using System.Collections.Generic;

namespace DiscordBot.Model.Database;

public partial class whitelist_question
{
    public uint id { get; set; }

    public string question { get; set; }

    public string answer1 { get; set; }

    public string answer2 { get; set; }

    public string answer3 { get; set; }

    public string answer4 { get; set; }

    public string answer5 { get; set; }

    public bool answer1Right { get; set; }

    public bool answer2Right { get; set; }

    public bool answer3Right { get; set; }

    public bool answer4Right { get; set; }

    public bool answer5Right { get; set; }

    public int wronglyAnsweredCounter { get; set; }

    public string explanation { get; set; }

    public virtual ICollection<whitelist_question_test_answer> whitelist_question_test_answers { get; } = new List<whitelist_question_test_answer>();
}
