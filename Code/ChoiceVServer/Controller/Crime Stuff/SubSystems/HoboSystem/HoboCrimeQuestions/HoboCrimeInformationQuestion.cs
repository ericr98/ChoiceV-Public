using System.Collections.Generic;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.Crime_Stuff;

public class HoboCrimeInformationQuestion : HoboCrimeQuestion {
    private string Information;

    public HoboCrimeInformationQuestion() { }

    public HoboCrimeInformationQuestion(int id, CrimeNetworkPillar pillar, string name, List<string> labels, int requiredReputation, Dictionary<string, string> settings) : base(id, pillar, name, labels, requiredReputation) {
        Information = settings["Information"];
    }

    public override Menu getQuestionMenu() {
        var menu = new Menu(Name, "Siehe dir die Information an");

        menu.addMenuItem(new StaticMenuItem("Information", Information, ""));

        return menu;
    }

    public override List<MenuItem> getSupportMenuInfo() {
        return [
            new StaticMenuItem("Information", "", Information),
        ];
    }

    public override List<MenuItem> onSupportCreateMenuItems() {
       return [
            new InputMenuItem("Information", "Die Information die angezeigt wird", "", "")
       ]; 
    }

    public override Dictionary<string, string> onSupportCreateSettings(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
        var information = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();

        return new Dictionary<string, string> {
            { "Information", information.input }
        };
    }
}