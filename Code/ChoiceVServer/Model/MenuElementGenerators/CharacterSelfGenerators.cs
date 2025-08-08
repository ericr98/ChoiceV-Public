using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public delegate MenuItem GeneratedSelfMenuItemDelegate(IPlayer player);
    public delegate Menu GeneratedSelfMenuDelegate(IPlayer player);

    public interface SelfMenuElement {
        public bool ShowOnBusy { get; set; }
        public List<CharacterType> ShowForTypes { get; set; }

        public bool checkShow(IPlayer player);

        public MenuElement getMenuElement(IPlayer player);
    }

    public class UnconditionalPlayerSelfMenuElement : SelfMenuElement {
        public bool ShowOnBusy { get; set; }
        public List<CharacterType> ShowForTypes { get; set; }

        private GeneratedSelfMenuItemDelegate MenuElementGenerator;
        private GeneratedSelfMenuDelegate MenuGenerator;

        private string Name;

        public UnconditionalPlayerSelfMenuElement(GeneratedSelfMenuItemDelegate menuElementGenerator) {
            MenuElementGenerator = menuElementGenerator;
        }

        public UnconditionalPlayerSelfMenuElement(string name, GeneratedSelfMenuDelegate menuGenerator) {
            MenuGenerator = menuGenerator;
            Name = name;
        }

        public bool checkShow(IPlayer player) {
            return true;
        }

        public MenuElement getMenuElement(IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke(player);
            } else {
                return new VirtualMenu(Name, () => MenuGenerator.Invoke(player));
            }
        }
    }

    public class ConditionalPlayerSelfMenuElement : SelfMenuElement {
        public bool ShowOnBusy { get; set; }
       
        public List<CharacterType> ShowForTypes { get; set; }


        private GeneratedSelfMenuItemDelegate MenuElementGenerator;
        private GeneratedSelfMenuDelegate MenuGenerator;

        private string Name;
        private MenuItemStyle Style;

        private Predicate<IPlayer> SenderPredicate { get; set; }

        public ConditionalPlayerSelfMenuElement(GeneratedSelfMenuItemDelegate menuElementGenerator, Predicate<IPlayer> senderPredicate) {
            MenuElementGenerator = menuElementGenerator;
            SenderPredicate = senderPredicate;
        }

        public ConditionalPlayerSelfMenuElement(string name, GeneratedSelfMenuDelegate menuGenerator, Predicate<IPlayer> senderPredicate, MenuItemStyle style = MenuItemStyle.normal) {
            MenuGenerator = menuGenerator;
            SenderPredicate = senderPredicate;
            Name = name;
            Style = style;
        }

        public bool checkShow(IPlayer player) {
            return SenderPredicate(player);
        }

        public MenuElement getMenuElement(IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke(player);
            } else {
                return new VirtualMenu(Name, () => MenuGenerator.Invoke(player), Style);
            }
        }
    }

}
