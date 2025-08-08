using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.Base {
    public static class VehicleExtensions {

        #region API Extension

        public static void burstTyre(this ChoiceVVehicle vehicle, int wheelId) {
            ChoiceVAPI.emitClientEventToAll("BURST_TYRE", vehicle, wheelId);
        }

        #endregion

        #region Custom Extensions

        public static bool hasPlayerAccess(this vehicle dbVeh, IPlayer player) {
            return player.getInventory().hasItem<VehicleKey>(vk => vk.worksForCar(dbVeh) 
                || CompanyController.hasPlayerPermission(player, CompanyController.getCompanyById(dbVeh.registeredCompanyId ?? -1), "COMPANY_VEHICLE_ACCESS"));
        }

        #endregion
    }
}