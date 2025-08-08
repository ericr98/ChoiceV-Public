using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.InventorySystem {
    public interface InventoryAuraItem {
        public void onEnterInventory(Inventory inventory);
        public void onExitInventory(Inventory inventory, bool becauseOfUnload);
    }

    public class InventoryAuraItemController : ChoiceVScript {
        public InventoryAuraItemController() {
            Inventory.InventoryAddItemDelegate += onItemAddInventory;
            Inventory.InventoryInventoryChangeItemDelegate += onItemChangeInventory;
            Inventory.InventoryRemoveItemDelegate += onItemRemoveInventory;
        }

        private static void onItemAddInventory(Inventory inventory, Item item, int? amount, bool saveToDb) {
            if(item is InventoryAuraItem auraItem) {
                auraItem.onEnterInventory(inventory);
            }
        }

        private static void onItemChangeInventory(Inventory oldInventory, Inventory newInventory, Item item) {
            if(item is InventoryAuraItem auraItem) {
                auraItem.onExitInventory(oldInventory, false);
                auraItem.onEnterInventory(newInventory);
            }
        }

        private static void onItemRemoveInventory(Inventory inventory, Item item, int? amount, bool saveToDb) {
            if(item is InventoryAuraItem auraItem) {
                auraItem.onExitInventory(inventory, false);
            }
        }
    }

    public interface NoEquipSlotItem { }

    public interface NoKeepEquippedItem { }
}
