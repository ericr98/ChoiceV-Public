using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class UniversalMissionsController : ChoiceVScript {
        public UniversalMissionsController() {
            CrimeMissionsController.addJobTaskCreator("Illegale Waren transportieren", onCreateIllegalGoodsTransportTrigger);
        }

        private List<MenuItem> onCreateIllegalGoodsTransportTrigger(IPlayer player, ref CrimeAction action, ref string codeItem) {
            action = CrimeAction.IllegalItemSell;
            codeItem = nameof(IllegalItemDeliveryTrigger);

            var list = new List<MenuItem>();
            for (var i = 0; i < 10; i++) {
                var subMenu = new VirtualMenu($"Teilauftrag {i}", () => {
                    var menu = new Menu("Teilauftrag", "Gib die Daten ein");

                    menu.addMenuItem(new CheckBoxMenuItem("Verkaufen statt kaufen", "Hier müssen die Items gekauft und nicht verkauft werden", true, ""));
                    menu.addMenuItem(InventoryController.getConfigItemSelectMenuItem("Zu stehlendes Item", "", true));
                    menu.addMenuItem(new InputMenuItem("Anzahl", "Anzahl", "Anzahl", InputMenuItemTypes.number, ""));
                    menu.addMenuItem(WorldController.getBigRegionSelectMenuItem("Zone", "", true));

                    return menu;
                });

                list.Add(new MenuMenuItem(subMenu.Name, subMenu));
            }

            return list;
        }
    }
}