using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class LongWeapon : Weapon, InventoryAuraItem {

        public LongWeapon(item item) : base(item) { }

        public LongWeapon(configitem configItem, int amount, int quality) : base(configItem, amount, quality) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public LongWeapon(configitem configItem, List<WeaponPart> weaponParts) : base(configItem, weaponParts) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public void onEnterInventory(Inventory inventory) {
            var player = ChoiceVAPI.GetAllPlayers().Find(p => p.getCharacterId() == inventory.OwnerId);
            if(player != null) {
                inventory.BlockStatements.Add(new InventoryAddBlockStatement(this, i => i is LongWeapon));

                WeaponController.showLongWeaponOnPlayer(player, this);
            }
        }

        public void onExitInventory(Inventory inventory, bool becauseOfUnload) {
            var player = ChoiceVAPI.GetAllPlayers().Find(p => p.getInventory() == inventory);
            if(player != null) {
                var statement = inventory.BlockStatements.FirstOrDefault(s => s.Owner == this);
                if(statement != null) {
                    if(!inventory.removeBlockStatement(statement)) {
                        Logger.logError($"onExitInventory: Block Statement not found! inventoryId: {inventory.Id}, itemId: {Id}",
                                    $"Fehler im Langwaffen-System: Es ist ein Fehler im Entfernen aufgetreten! Item-Id: {Id}, Inventar-Id: {inventory.Id}", player);
                    }
                } else {
                    Logger.logError($"onExitInventory: Error occured! inventoryId: {inventory.Id}, itemId: {Id}",
                                    $"Fehler im Langwaffen-System: Das Item befand sich illegalerweise im Inventar! Item-Id: {Id}, Inventar-Id: {inventory.Id}", player);
                }

                WeaponController.noLongerShowLongWeaponOnPlayer(player, this);
            }
        }
    }
}
