using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller.DamageSystem {
    public class DamageDummyController : ChoiceVScript {
        private static int RunningId = 1;

        public DamageDummyController() {
            EventController.addMenuEvent("DUMMY_HEAL_INJURY", onDummyHealInjury);
            EventController.addMenuEvent("DUMMY_ADD_INJURY", onDummyAddInjury);
            EventController.addMenuEvent("DUMMY_DELETE_INJURIES", onDummyDeleteInjuries);

            //Placeable
            EventController.addMenuEvent("PICK_DUMMY_TO_SHOULDER", obPickUpDummyOnShoulder);
            EventController.addMenuEvent("PICK_DUMMY_TO_STRETCHER", onPickUpDummyOnStretcher);

            //MedicDummyItem
            EventController.addMenuEvent("USE_MEDIC_DUMMY", onUseMedicDummy);
        }

        public static DamageDummy createDamageDummy() {
            return new DamageDummy(RunningId++);
        }

        public static DamageDummy createDamageDummy(string shortSave) {
            var dummy = new DamageDummy(RunningId++);

            if(shortSave != null) {
                using(var db = new ChoiceVDb()) {
                    foreach(var split in shortSave.Split('#')) {
                        if(string.IsNullOrWhiteSpace(split)) continue;

                        var subSplit = split.Split(';');

                        var cfgId = int.Parse(subSplit[0]);
                        var seed = int.Parse(subSplit[1]);
                        var bodyPart = Enum.Parse<CharacterBodyPart>(subSplit[2]);
                        var operatedOrder = subSplit[3].FromJson<List<int>>();

                        var cfgInjury = db.configinjuries
                            .Include(i => i.treatmentCategoryNavigation)
                            .ThenInclude(t => t.configinjurytreatmentssteps)
                            .FirstOrDefault(i => i.id == cfgId);

                        if(cfgInjury != null) {
                            var (injury, parsedBodyPart) = getInjuryFromCfg(dummy.CountingInjuryId++, cfgInjury, seed, bodyPart);
                            injury.OperatedOrder = operatedOrder;
                            injury.checkForRightTreatment();

                            dummy.addInjury(injury);
                        }
                    }
                }
            }

            return dummy;
        }

        private bool onDummyAddInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var injury = data["Injury"] as configinjury;
            var dummy = data["Dummy"] as DamageDummy;

            if(injury != null) {
                var (newInjury, bodyPart) = getInjuryFromCfg(dummy.CountingInjuryId++, injury);
                if(newInjury == null) {
                    player.sendNotification(NotifactionTypes.Danger, "Verletzung konnte nicht hinzugefügt werden", "Verletzung konnte nicht hinzugefügt werden");
                    return true;
                }

                dummy.addInjury(newInjury);
                dummy.OnDummyUpdateDelegate?.Invoke(dummy);

                player.sendNotification(NotifactionTypes.Success, $"Du hast eine {injury.name} zum Köperteil {CharacterBodyPartToString[bodyPart]} hinzugefügt", "");
            }

            player.showMenu(dummy.getInteractionMenu(player));

            return true;
        }

        private static (Injury, CharacterBodyPart) getInjuryFromCfg(int countingId, configinjury injury, int seed = 0, CharacterBodyPart bodyPart = CharacterBodyPart.None) {
            if(seed == 0) {
                seed = new Random().Next(100000, 999999);
            }

            CharacterBodyPart parsedBodyPart = bodyPart;
            if(parsedBodyPart == CharacterBodyPart.None && !Enum.TryParse(injury.bodyPart, out parsedBodyPart)) {
                var category = Enum.Parse<CharacterBodyPartCategories>(injury.bodyPart);

                var parts = getBodyPartsForCategory(category);
                parsedBodyPart = parts[new Random().Next(0, parts.Count)];
            }

            if(parsedBodyPart != CharacterBodyPart.None) {
                return (new Injury(countingId, parsedBodyPart, Enum.Parse<DamageType>(injury.damageType), 0, 0, seed, DateTime.Now, false, false, injury), parsedBodyPart);
            } else {
                return (null, CharacterBodyPart.None);
            }
        }

        private bool onDummyHealInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = data["Item"] as MedicItem;
            var injury = data["Injury"] as Injury;
            var dummy = data["Dummy"] as DamageDummy;

            var anim = AnimationController.getAnimationByName("INSPECT_MEDIC");
            AnimationController.animationTask(player, anim, () => {
                if(item.treatInjury(injury)) {
                    injury.IsHealing = true;
                    injury.IsTreated = false;

                    player.sendNotification(Constants.NotifactionTypes.Success, "Verletzung erfolgreich behandelt!", "Verletzung behandelt");
                }

                dummy.OnDummyUpdateDelegate?.Invoke(dummy);
                player.showMenu(dummy.getInteractionMenu(player));
            });

            return true;
        }

        private bool onDummyDeleteInjuries(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var dummy = data["Dummy"] as DamageDummy;

            dummy.InjuryList.Clear();

            dummy.OnDummyUpdateDelegate?.Invoke(dummy);
            player.sendNotification(NotifactionTypes.Warning, "Alle Verletzungen wurden gelöscht", "Alle Verletzungen gelöscht");
            player.showMenu(dummy.getInteractionMenu(player));

            return true;
        }


        //Placeable
        private bool obPickUpDummyOnShoulder(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var placeable = data["placeable"] as DamageDummyPlaceable;
            var (mode, model, offsetPos, offsetRot, bone, animationName) = MedicDummyItem.getInfoForIdentifier("SHOULDER_CARRY");           
            var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
       
            AnimationController.animationTask(player, anim, () => {
                var img = NotifactionImages.Garage;
                if(placeable.onPickUp(player, ref img)) {
                    placeable.onRemove();
                    WebController.closePlayerCef(player);

                    var anim = AnimationController.getAnimationByName(animationName);
                    CarryController.putObjectOnCarry(player, anim, model, offsetPos, offsetRot, bone, (act) => {
                        placeDummy(player, player.getInventory().getItem<MedicDummyItem>(i => true), MedicDummyItem.getDefaultPlaceModel(), act);
                    });
                } else {
                    player.sendBlockNotification("Puppe konnte nicht aufgenommen werden", "Puppe konnte nicht aufgenommen werden");
                }
            }, null, true, null, TimeSpan.FromSeconds(1.5f));

            return true;
        }

        private bool onPickUpDummyOnStretcher(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var placeable = data["placeable"] as DamageDummyPlaceable;
            var (mode, model, offsetPos, offsetRot, bone, animationName) = MedicDummyItem.getInfoForIdentifier("STRETCHER_CARRY");
            var anim = AnimationController.getAnimationByName("KNEEL_DOWN");

            AnimationController.animationTask(player, anim, () => {
                var img = NotifactionImages.Garage;
                if(placeable.onPickUp(player, ref img)) {
                    placeable.onRemove();
                    WebController.closePlayerCef(player);

                    var anim = AnimationController.getAnimationByName(animationName);
                    CarryController.putObjectOnCarry(player, anim, model, offsetPos, offsetRot, bone, (act) => {
                        placeDummy(player, player.getInventory().getItem<MedicDummyItem>(i => true), MedicDummyItem.getDefaultPlaceModel(), act);
                    });
                } else {
                    player.sendBlockNotification("Puppe konnte nicht aufgenommen werden", "Puppe konnte nicht aufgenommen werden");
                }
            }, null, true, null, TimeSpan.FromSeconds(1.5f));

            return true;
        }


        //MedicDummyItem
        private bool onUseMedicDummy(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var dummy = (MedicDummyItem)data["Dummy"];
            var model = (string)data["Model"];
            var mode = (string)data["Mode"];
            var offsetPos = (Position)data["OffsetPos"];
            var offsetRot = (DegreeRotation)data["OffsetRot"];
            var bone = (int)data["Bone"];
            var animationName = (string)data["AnimationName"];

            var evt = menuItemCefEvent as ListMenuItemEvent;

            if(mode == null) {
                return true;
            } else if(mode == "OBJECT") {
                placeDummy(player, dummy, model, null);
            } else if(mode == "CARRY") {
                dummy.externalUse();
                CarryController.putObjectOnCarry(player,AnimationController.getAnimationByName(animationName), model, offsetPos, offsetRot, bone, (act) => {
                    placeDummy(player, dummy, MedicDummyItem.getDefaultPlaceModel(), act);
                });
            }

            return true;
        }

        private void placeDummy(IPlayer player, MedicDummyItem dummy, string model, Action workedCallback) {
            ObjectController.startObjectPlacerMode(player, model, 0, (pl, pos, heading) => {
                var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
                AnimationController.animationTask(player, anim, () => {
                    var dummyPlaceable = new DamageDummyPlaceable(player, pos, new DegreeRotation(0, 0, heading), model);
                    dummyPlaceable.initialize();
                    dummy.externalUse();

                    workedCallback?.Invoke();
                }, null, true, null, TimeSpan.FromSeconds(1.5f));
            });
        }
    }
}
