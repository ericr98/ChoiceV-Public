using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.InventorySystem {
    public enum MakeShiftTreatType : int {
        All = 0, //MedKit
        Cooling = 1, //Burning and Dull
        Bandage = 2, //Sting and Shot
    }

    public class MakeShiftTreatingItemsController : ChoiceVScript {
        public MakeShiftTreatingItemsController() {
            EventController.addMenuEvent("MAKESHIFT_TREAT_INJURY", onMakeShiftTreatInjury);

            InteractionController.addPlayerInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Notdürftige Behandlung",
                    makeShiftTreatingMenuGenerator,
                    sender => sender is IPlayer player && player.getInventory().hasItem<MakeshiftTreatingItem>(i => true),
                    target => target is IPlayer interact && interact.getCharacterData().CharacterDamage.AllInjuries.Count > 0
                )
            );

            CharacterController.addSelfMenuElement(
                new ConditionalPlayerSelfMenuElement(
                    "Notdürftige Behandlung",
                    makeShiftTreatingSelfMenuGenerator,
                    p => p.getInventory().hasItem<MakeshiftTreatingItem>(i => true) && p.getCharacterData().CharacterDamage.AllInjuries.Count > 0
               )
            );
        }

        private Menu makeShiftTreatingSelfMenuGenerator(IPlayer player) {
            return makeShiftTreatingMenuGenerator(player, player);
        }

        private Menu makeShiftTreatingMenuGenerator(IEntity sender, IEntity receiver) {
            var player = sender as IPlayer;
            var target = receiver as IPlayer;

            var items = player.getInventory().getItems<MakeshiftTreatingItem>(i => true).Distinct((i, j) => i.MakeShiftTreatType.Equals(j.MakeShiftTreatType));

            var menu = new Menu("Notdürftige Behandlung", "Mit welchem Item behandeln?");

            foreach(var item in items) {
                var damageTypes = item.getUsefulDamageTypes();

                var injs = target.getCharacterData().CharacterDamage.AllInjuries.Where(i => damageTypes.Contains(i.Type) && !i.IsMakeShiftTreated).ToList();

                var itemMenu = new Menu(item.Name, "Welche Verletzung behandeln?");

                foreach(var inj in injs) {
                    var type = "";
                    var strength = "";
                    inj.getMessage(ref type, ref strength);

                    itemMenu.addMenuItem(new ClickMenuItem(type, $"Behandle die {strength} {type} im {CharacterBodyPartToString[inj.BodyPart]}", strength, "MAKESHIFT_TREAT_INJURY").withData(new Dictionary<string, dynamic> { { "Item", item }, { "Injury", inj }, { "Target", target } }).needsConfirmation("Diese Verletzung behandeln?", $"{strength} {type} behandeln?"));
                }

                if(itemMenu.getMenuItemCount() > 0) {
                    menu.addMenuItem(new MenuMenuItem(itemMenu.Name, itemMenu));
                }
            }

            return menu;
        }

        private bool onMakeShiftTreatInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var inj = (Injury)data["Injury"];
            var item = (MakeshiftTreatingItem)data["Item"];

            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                inj.IsMakeShiftTreated = true;

                item.externalUse();

                player.sendNotification(NotifactionTypes.Success, $"Die gewählte Verletzung wurde behandelt!", "Alle Verletzungen behandelt", NotifactionImages.Bone);

                if(data.ContainsKey("Target")) {
                    var type = "";
                    var strength = "";
                    inj.getMessage(ref type, ref strength);

                    ((IPlayer)data["Target"]).sendNotification(NotifactionTypes.Info, $"Eine deiner {strength} {type} wurde notdürftig behandelt!", "Wunde notdürftig behandelt!");
                }
            });
            return true;
        }
    }

    public class MakeshiftTreatingItem : ToolItem {
        public MakeShiftTreatType MakeShiftTreatType;

        public MakeshiftTreatingItem(item item) : base(item) {
            processAdditionalInfo(item.config.additionalInfo);
        }

        //Constructor for generic generation
        public MakeshiftTreatingItem(configitem configItem, int amount, int quality) : base(configItem, -1, amount) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public override void processAdditionalInfo(string info) {
            MakeShiftTreatType = (MakeShiftTreatType)int.Parse(info);
        }

        public List<DamageType> getUsefulDamageTypes() {
            var types = new List<DamageType> { DamageType.Burning, DamageType.Dull };

            switch(MakeShiftTreatType) {
                case MakeShiftTreatType.All:
                    types = Enum.GetValues<DamageType>().ToList();
                    break;

                case MakeShiftTreatType.Bandage:
                    types = new List<DamageType> { DamageType.Sting, DamageType.Shot };
                    break;
            }

            return types;
        }

        public override void use(IPlayer player) {
            var damg = player.getCharacterData().CharacterDamage;

            var types = getUsefulDamageTypes();

            var injs = damg.AllInjuries.Where(i => types.Contains(i.Type) && !i.IsMakeShiftTreated).ToList();

            if(injs.Count > 0) {
                var menu = new Menu("Verletzung behandeln", "Welche Verletzung soll behandelt werden?");

                foreach(var inj in injs) {
                    var type = "";
                    var strength = "";
                    inj.getMessage(ref type, ref strength);

                    menu.addMenuItem(new ClickMenuItem(type, $"Behandle die {strength} {type} im {CharacterBodyPartToString[inj.BodyPart]}", strength, "MAKESHIFT_TREAT_INJURY").withData(new Dictionary<string, dynamic> { { "Item", this }, { "Injury", inj } }).needsConfirmation("Diese Verletzung behandeln?", $"{type} behandeln?"));
                }

                player.showMenu(menu);
            } else {
                player.sendBlockNotification("Du besitzt keine Verletzungen für die das Item etwas nützt!", "Item nutzlos", NotifactionImages.Bone);
            }
        }
    }
}
