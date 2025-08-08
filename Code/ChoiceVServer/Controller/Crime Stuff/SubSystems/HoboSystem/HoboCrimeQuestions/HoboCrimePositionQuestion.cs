using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.Crime_Stuff;

public class HoboCrimeModuleInShiftSearchQuestion : HoboCrimeQuestion {
    private readonly Type ModuleType;

    public HoboCrimeModuleInShiftSearchQuestion() { }

    public HoboCrimeModuleInShiftSearchQuestion(int id, CrimeNetworkPillar pillar, string name, List<string> labels, int requiredReputation, Dictionary<string, string> settings) : base(id, pillar, name, labels, requiredReputation) {
        var typeName = settings["ModuleName"];

        ModuleType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == typeName);
    }

    public override Menu getQuestionMenu() {
        var menu = new Menu(Name, "Siehe dir die Position an");
        
        var peds = PedController.findPeds(p => p.hasModule(ModuleType, m => (Pillar == null || (m as CrimeNetworkModule).Pillar == Pillar) && (m as CrimeNetworkModule).Active)); 
        var randomPed = peds.ElementAt(new Random().Next(0, peds.Count));

        menu.addMenuItem(new ClickMenuItem("Position anzeigen", $"Zeige die eine/einen {Name}, welcher aktuell aktiv ist, an", "", "ON_PLAYER_SHOW_MODULE_POSITION").withData(new Dictionary<string, dynamic> {
            { "Position", randomPed.Position },
        }));

        return menu;
    }

    public override List<MenuItem> getSupportMenuInfo() {
        return [
            new StaticMenuItem("Modul", "", ModuleType.Name),
        ];
    }

    public override List<MenuItem> onSupportCreateMenuItems() {
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(i => i.IsSubclassOf(typeof(CrimeNetworkModule))).ToList();

        return [new ListMenuItem("Modul auswählen", "Wähle ein Modul aus, um die Position zu sehen", types.Select(t => t.Name).ToArray(), "")];
    }

    public override Dictionary<string, string> onSupportCreateSettings(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
        var typeName = evt.elements[0].FromJson<ListMenuItem.ListMenuItemEvent>();

        return new Dictionary<string, string> {
            { "ModuleName", typeName.currentElement }
        };
    }
}