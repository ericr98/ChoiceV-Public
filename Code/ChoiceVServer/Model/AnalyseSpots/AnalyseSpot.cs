using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Model.AnalyseSpots {
    public class AnalyseSpot {
        public int Id;
        public CollisionShape CollisionShape;
        public Dictionary<string, dynamic> Data;
        public Inventory AnalyseInventory;
        public DateTime AnalyseEnd;

        public AnalyseSpot(CollisionShape shape, int inventoryId, Dictionary<string, dynamic> data = null) {
            CollisionShape = shape;

            if(data == null) {
                Data = new Dictionary<string, dynamic>();
            } else {
                Data = data;
            }

            var colData = new Dictionary<string, dynamic>();
            colData.Add("spot", this);


            var inv = InventoryController.loadInventory(inventoryId);
            if(inv == null) {
                AnalyseInventory = InventoryController.createInventory(Id, 1000, InventoryTypes.AnalyseSpot);
            } else {
                AnalyseInventory = inv;
            }

            CollisionShape.InteractData = colData;
        }

        public virtual void onInteraction(IPlayer player) { }

        public virtual void onAlignWithEvidence(IPlayer player, Evidence evidence) { }

        public Menu.Menu generateEvidenceSpotMenu(IPlayer player, EvidenceType type, string itemName) {
            var menu = new Menu.Menu($"{EvidenceTypeToString[type]}-Analyse", $"Analysiere {EvidenceTypeToString[type]} und gleiche diese ab!");

            if(AnalyseInventory.getCount() != 0) {
                if(AnalyseEnd > DateTime.Now) {
                    menu.addMenuItem(new StaticMenuItem("Analyse im Gange!", "Die Analyse dauert noch: " + (AnalyseEnd - DateTime.Now).Minutes + "min.", ""));
                } else {
                    menu.addMenuItem(new ClickMenuItem("Analyse beenden", "Die Analyse ist fertig.", "", "END_ANALYSE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Spot", this } }));
                }
            } else {
                menu.addMenuItem(new StaticMenuItem("Keine Analyse im Gange", "Du kannst einen Beweis analysieren.", ""));

                foreach(Evidence item in player.getInventory().getItems<Evidence>(i => true)) {
                    if(item.EvidenceType == type) {
                        var subMenu = new Menu.Menu(Constants.EvidenceTypeToString[type] + "-Beweis " + item.EvidenceId, "Wähle einene Beweis zum analysieren aus.");

                        if(!item.Analyzed) {
                            subMenu.addMenuItem(new ClickMenuItem("Beweis analysieren", "Beweis analyiseren. Dauert ca. " + Constants.EVIDENCE_ANALYSE_TIME.Minutes + "min.", "", "ANALYSE_EVIDENCE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Evidence", item }, { "Spot", this } }));
                        } else if(itemName != null) {
                            subMenu.addMenuItem(new ClickMenuItem("Beweis mit " + itemName + " abgleichen", "Beweis mit " + itemName + " aus/in dem Inventar/Spot abgleichen.", "", "ALIGN_EVIDENCE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Evidence", item }, { "Spot", this } }));
                        }

                        menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));

                    }
                }
            }

            return menu;
        }

        public bool analyseInProgress() {
            return AnalyseInventory.getCount() != 0;
        }
    }
}
