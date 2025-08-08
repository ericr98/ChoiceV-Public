#nullable enable
using System.Linq;
using System.Threading.Tasks;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.Inventory;

public static class InventoryHelper {
    public static async Task<InventoryApiModel?> getByCharacterIdAsync(int characterId) {
        await using var db = new ChoiceVDb();

        var inv = await db.inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ownerId == characterId && (InventoryTypes)i.inventoryType == InventoryTypes.Player)
            .ConfigureAwait(false);
        
        if (inv == null) return null;

        var playerInventory = InventoryController.loadInventory(inv);

        return new InventoryApiModel {
            CharacterId = characterId,
            Items = playerInventory.Items.Select(x => x.convertToApiModel()).ToList()
        };
    }
}