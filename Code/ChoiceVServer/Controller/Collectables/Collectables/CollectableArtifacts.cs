using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Collectables {
    public class CollectableArtefact : Collectable {

        public CollectableArtefact(CollectableAreaTypes areaType, Position position) : base(areaType, position, 1.5f, 1.5f) {

        }

        protected override void loadModels() {
            Models = new List<string> { 
                "ex_mp_h_acc_dec_plate_01",   //Normal Artifact
                "apa_mp_h_acc_dec_sculpt_02", //Normal Artifact
                "ex_mp_h_acc_dec_plate_02",   //Normal Artifact

                "ba_prop_battle_antique_box",  //Rare Artifact
                "apa_mp_h_acc_dec_head_01", //Rare Artifact

                "w_me_stonehatchet", //Legendary Artifact (Weapon)
                "w_ar_musket", //Legendary Artifact (Weapon)
                "prop_w_me_dagger", //Legendary Artifact (Weapon)
            };
        }

        protected override Animation getAnimationForModel(string model) {
            return AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
        }

        protected override float getZOffsetForModel(string model) {
            if(model == "apa_mp_h_acc_dec_head_01") {
                return -0.35f;
            } else if(model == "w_me_stonehatchet") {
                return -0.2f;
            } else if(model == "w_ar_musket") {
                return -0.15f;
            } else if(model == "prop_w_me_dagger") {
                return 0.15f;
            } else if(model == "ex_mp_h_acc_dec_plate_01" ||
                      model == "apa_mp_h_acc_dec_sculpt_02" ||
                      model == "ex_mp_h_acc_dec_plate_02") {
                return -0.2f;
            } else if(model == "ba_prop_battle_antique_box") {
                return -0.05f;
            }

            return 0;
        }

        protected override Rotation getRotationForModel(string model) {
            var random = new Random();
            if(model == "ex_mp_h_acc_dec_plate_01" ||
               model == "apa_mp_h_acc_dec_sculpt_02" ||
               model == "ex_mp_h_acc_dec_plate_02") {
                return new DegreeRotation(0, random.Next(-60, 0), random.Next(0, 60));
            } else if(model == "apa_mp_h_acc_dec_head_01") { //Mask
                return new DegreeRotation(random.Next(-40, 40), random.Next(-40, 0), random.Next(-40, 40));
            } else if(model == "w_me_stonehatchet") {
                return new DegreeRotation(random.Next(-40, 40), 0, 0);
            } else if(model == "w_ar_musket") {
                return new DegreeRotation(random.Next(-20, -20), random.Next(-20, 20), 0);
            } else if(model == "prop_w_me_dagger") {
                return new DegreeRotation(random.Next(120, 180), random.Next(-20, 20), 0);
            } else if(model == "ba_prop_battle_antique_box") {
                return new DegreeRotation(random.Next(-50, 50), random.Next(-50, 50), 0);
            }

            return DegreeRotation.Zero;
        }

        public override void onCollectStep(IPlayer player) {
            configitem cfg = null;
            if(Object.ModelName == "ex_mp_h_acc_dec_plate_01" ||
               Object.ModelName == "apa_mp_h_acc_dec_sculpt_02" ||
               Object.ModelName == "ex_mp_h_acc_dec_plate_02") {
                cfg = InventoryController.getConfigItemByCodeIdentifier("ANTIQUE_PLATE");
            } else if(Object.ModelName == "apa_mp_h_acc_dec_head_01") {
                cfg = InventoryController.getConfigItemByCodeIdentifier("ANTIQUE_MASK");
            } else if(Object.ModelName == "w_me_stonehatchet") {
                cfg = InventoryController.getConfigItemByCodeIdentifier("ANTIQUE_AXE");
            }else if(Object.ModelName == "prop_w_me_dagger") {
                cfg = InventoryController.getConfigItemByCodeIdentifier("ANTIQUE_DAGGER");
            } else if(Object.ModelName == "w_ar_musket") {
                cfg = InventoryController.getConfigItemByCodeIdentifier("ANTIQUE_MUSKET");
            } else if(Object.ModelName == "ba_prop_battle_antique_box") {
                cfg = InventoryController.getConfigItemByCodeIdentifier("ANTIQUE_MUNITION_BOX");
            }

            var item = InventoryController.createItem(cfg, 1);

            if(player.getInventory().addItem(item)) {
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast eine/ein {item.Name} gesammelt!", $"{item.Name} erhalten");
                base.onCollectStep(player);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Danger, "Dein Inventar ist voll!", "Inventar voll");
            }
        }

        protected override string getObjetModel() {
            var random = new Random();
            var next = random.NextDouble();

            if(next > 0.9) {
                next = random.NextDouble();
                //30% change for dagger
                //30% change for axe
                //20% change for munition
                //10% change for musket
                if(next > 0.70) {
                    return "prop_w_me_dagger";
                } else if(next > 0.4) {
                    return "w_me_stonehatchet";
                } else if(next > 0.1) {
                    return "ba_prop_battle_antique_box";
                } else {
                    return "w_ar_musket";
                }
            } else {
                return new List<string> {
                    "ex_mp_h_acc_dec_plate_01",
                    "apa_mp_h_acc_dec_sculpt_02",
                    "ex_mp_h_acc_dec_plate_02",
                    "apa_mp_h_acc_dec_head_01"
                }[random.Next(0, 4)];   
            }
        }

        public override void destroy() {
            base.destroy();
        }
    }
}
