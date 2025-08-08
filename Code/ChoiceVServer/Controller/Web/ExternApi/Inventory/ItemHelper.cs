using ChoiceVServer.InventorySystem;
using ChoiceVSharedApiModels.Inventory;

namespace ChoiceVServer.Controller.Web.ExternApi.Inventory;

public static class ItemHelper {

    public static InventoryItemApiModel convertToApiModel(this Item item) {
        var amount = item.CanBeStacked ? item.StackAmount.Value : 1;

        var response = new InventoryItemApiModel(
            item.Id.Value,
            item.Name,
            item.Description,
            amount
        );
        return response;
    }
}
