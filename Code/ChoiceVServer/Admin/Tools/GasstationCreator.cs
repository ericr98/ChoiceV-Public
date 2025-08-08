using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.GasRefuel;

namespace ChoiceVServer.Admin.Tools {
    public class GasstationCreator {

        public static void createGasstation(IPlayer player, string name, GasStationType type, float petrolPrice, float dieselPrice, float electricityPrice, float kerosinPrice) {
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (pos, width, height, rotation) => {
                var dbGas = new configgasstation {
                    name = name,
                    type = (int)type,
                    pricePetrol = petrolPrice,
                    priceDiesel = dieselPrice,
                    priceElecricity = electricityPrice,
                    priceKerosin = kerosinPrice,
                    bankAccount = 1,
                    remainPosition = pos.ToJson(),
                    width = width,
                    height = height,
                    rotation = rotation,
                };

                using(var db = new ChoiceVDb()) {
                    db.configgasstations.Add(dbGas);
                    db.SaveChanges();

                    player.sendNotification(NotifactionTypes.Success, $"Gasstation mit Id {dbGas.id} erstellt", "", NotifactionImages.System);
                    GasstationController.loadGasstations();
                }
            });
        }

        public static void createGasstationSpot(IPlayer player, int gasstationId, List<GasstationSpotType> list) {
            CollisionShapeCreator.startCollisionShapeCreationWithCallback(player, (pos, width, height, rotation) => {
                var spot = new configgasstationspot {
                    position = pos.ToJson(),
                    width = width,
                    height = height,
                    rotation = rotation,
                    gasstationId = gasstationId,
                    fuelTypeList = list.ToJson()
                };

                using(var db = new ChoiceVDb()) {
                    db.configgasstationspots.Add(spot);
                    db.SaveChanges();
                    GasstationController.loadGasstations();
                    player.sendNotification(NotifactionTypes.Success, $"Zapfsäule mit Id {spot.id} erstellt", "", NotifactionImages.System);
                }
            });
        }
    }
}
