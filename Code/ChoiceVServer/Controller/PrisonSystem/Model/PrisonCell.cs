using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.PrisonSystem.Model {
    public class PrisonCell {
        public int Id;
        public readonly string Name;
        public Prison Prison;
        public CollisionShape StorageSpot;
        public Inventory Inventory;
        public int[] Combination;
        
        public PrisonInmate Inmate;
        
        public PrisonCell(int id, string name, CollisionShape storageSpot, Inventory inventory, int[] combination) {
            Id = id;
            Name = name;
            StorageSpot = storageSpot;
            Inventory = inventory;
            Combination = combination;
            
            StorageSpot.OnCollisionShapeInteraction += onStorageInteraction;
        }
        
        public void addPrisonInmate(PrisonInmate inmate) {
            Inmate = inmate;
        }
        
        private bool onStorageInteraction(IPlayer player) {
            var hasAccess = PrisonController.hasPlayerAccessToPrison(player, Prison);
            if(!hasAccess && Inmate?.CharId != player.getCharacterId()) {
                CombinationLockController.requestPlayerCombination(player, Combination, (_, _) => {
                    InventoryController.showMoveInventory(player, player.getInventory(), Inventory, null, null, Name, true);
                });
            } else {
                var menu = new Menu("Zellenlagerung", "Was möchtest du tun?");
                menu.addMenuItem(new ClickMenuItem("Zellenlager öffnen", "Öffne das Zellenlager", "", "PRISON_OPEN_CELL_STORAGE", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Cell", this } }));
                menu.addMenuItem(new InputMenuItem("Kombination ändern", "Ändere die Kombination des Zellenlagers", "Kombination", InputMenuItemTypes.number, "PRISON_CHANGE_CELL_COMBINATION")
                    .withStartValue(string.Join("", Combination))
                    .withData(new Dictionary<string, dynamic> { { "Cell", this } }));

                if(hasAccess) {
                    var nameStr = Inmate != null ? Inmate.Name : "Kein Insasse";
                    menu.addMenuItem(new ClickMenuItem("Aktuell zugewiesen", $"Die Zelle ist aktuell {nameStr} zugewiesen. Klicken um Zelle niemandem zuzuweisen.", nameStr, "PRISON_CELL_SET_INMATE")
                        .withData(new Dictionary<string, dynamic>{ { "Cell", this }, { "Inmate", null } })
                        .needsConfirmation("Zelle zurückweisen?", "Zelle wirklich zurücknehmen?"));
                    
                    var inmateMenu = new Menu("Insassen zuweisen", "Wähle einen Insassen aus");

                    foreach(var inmate in Prison.getInmates()) {
                        inmateMenu.addMenuItem(new ClickMenuItem(inmate.Name, $"Weise {inmate.Name} der Zelle zu", "", "PRISON_CELL_SET_INMATE")
                            .withData(new Dictionary<string, dynamic>{ { "Cell", this }, { "Inmate", inmate } })
                            .needsConfirmation("Insasse zuweisen?", $"Willst du {inmate.Name} wirklich der Zelle zuweisen?"));
                    }
                    
                    menu.addMenuItem(new MenuMenuItem(inmateMenu.Name, inmateMenu));
                }
                
                player.showMenu(menu);
            }

            return true;
        }

        public void onDelete() {
            StorageSpot.Dispose();
            StorageSpot = null;
            InventoryController.destroyInventory(Inventory);
        }

        public string toShortSave() {
            return $"{Name}|{StorageSpot.toShortSave()}|{Inventory.Id}|{Combination.ToJson()}";
        }
    }
}
