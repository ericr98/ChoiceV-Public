using System.Collections.Generic;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Crime_Stuff;


public abstract class HoboCrimeQuestion {
    public int Id { get; private set; }
    public string Name { get; private set; }
    public List<string> Labels { get; private set; }

    public CrimeNetworkPillar Pillar { get; private set; }
    public int RequiredReputation { get; private set; }

    public HoboCrimeQuestion() { }

    public HoboCrimeQuestion(int id, CrimeNetworkPillar pillar, string name, List<string> labels, int requiredReputation) {
        Id = id;
        Pillar = pillar;
        Name = name;
        Labels = labels;
        RequiredReputation = requiredReputation;
    }

    public abstract Menu getQuestionMenu();


    public abstract List<MenuItem> getSupportMenuInfo();
    public abstract List<MenuItem> onSupportCreateMenuItems();
    public abstract Dictionary<string, string> onSupportCreateSettings(MenuStatsMenuItemEvent evt);
}