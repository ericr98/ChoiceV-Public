using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.Model;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.CraftingSystem {
    public class PlacedCraftingTimedSpot {
        public int Id;
        public CollisionShape CollisionShape;
        public List<CraftingTimedSpot> CraftingTimedSpots;

        
        public PlacedCraftingTimedSpot(int id, CollisionShape collisionShape, List<CraftingTimedSpot> craftingTimedSpots) {
            Id = id;
            CollisionShape = collisionShape;
            CraftingTimedSpots = craftingTimedSpots;

            CollisionShape.OnCollisionShapeInteraction += onInteract;
        }

        private bool onInteract(IPlayer player) {
            CraftingController.openCraftingSpotsMenu(player, CraftingTimedSpots.Select(c => c.Id).ToList());

            return true;
        }
    }
}