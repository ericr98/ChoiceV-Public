using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Controller.DamageSystem {
    public class AcidTownController : ChoiceVScript {
        private static List<CollisionShape> TownShapes;
        private static List<IPlayer> AllPlayersInAcidTown { get => TownShapes.SelectMany(c => c.AllEntities).Distinct().Cast<IPlayer>().ToList(); }

        public AcidTownController() {
            //Entzündungen bei aufhalten ohne Schutzkleidung
            //Sludge abbaubar (ymaps werden entfernt) und dann durch Sanitation gereinigt werden, oder durch Crimler in irgendwelche Drogen verarbeitet
            
            TownShapes = new List<CollisionShape> {
                CollisionShape.Create("{\"X\":935.01685,\"Y\":2993.614,\"Z\":40.46814}#130.7112#74.39706#0.0#true#false#false"),
                CollisionShape.Create("{\"X\":930.9616,\"Y\":3046.3315,\"Z\":40.46814}#106.38#34.276#0.0#true#false#false"),
                CollisionShape.Create("{\"X\":926.90643,\"Y\":3066.6074,\"Z\":40.46814}#90.1592#14.1104#0.0#true#false#false")
            };
        
            TownShapes.ForEach(c => c.HasNoHeight = true);

            InvokeController.AddTimedInvoke("AcidTownCounter", onUpdateAcidTown, TimeSpan.FromSeconds(30), true);
        }

        private static bool isPlayerProtected(IPlayer player) {
            return false;
        }

        private void onUpdateAcidTown(IInvoke invoke) {
            var random = new Random();
            foreach(var player in AllPlayersInAcidTown) {
                // If no full protective suit is equipped, player can get Inflammations
                if(!player.getInventory().hasItem<ProtectiveSuitFullClothingItem>(i => i.IsEquipped)) {
                    if(random.NextDouble() > 0.2) {
                        var bodyPart = (CharacterBodyPart)random.Next(1, 6);
                        DamageController.addPlayerInjury(player, DamageType.Inflammation, bodyPart, random.Next(3, 10));
                        player.sendNotification(NotifactionTypes.Warning, "Deine Haut fängt an zu jucken!", "Haut juckt");
                    }
                }
            }
        }
    }
}
