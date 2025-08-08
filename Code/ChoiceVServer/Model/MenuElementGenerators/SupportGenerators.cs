using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;


namespace ChoiceVServer.Controller {
    public enum SupportMenuCategories {
        Support_Aktionen,
        Kleidung,
        Fahrzeuge,
        Bankensystem,
        Infos,
        Accounts,
        ItemSystem,
        Fahrstühle,
        Wetter,
        Hotels,
        DamageSystem,
        TürSystem,
        PlayerStyle,
        Firmen,
        Misc,
        Crime,
        Einreise,

        Registrieren,
    }

    public abstract class SupportMenuElement : SelfMenuElement {
        public bool ShowOnBusy { get; set; }
       
        public List<CharacterType> ShowForTypes { get; set; }


        public int AdminLevel { get; private set; }
        public SupportMenuCategories Category { get; private set; }

        public SupportMenuElement(int adminLevel, SupportMenuCategories category) {
            AdminLevel = adminLevel;
            Category = category;
        }

        public bool checkShow(IPlayer player) {
            return player.getAdminLevel() >= AdminLevel;
        }

        public abstract MenuElement getMenuElement(IPlayer player);
    }

    public class StaticSupportMenuElement : SupportMenuElement {
        public GenericMenuItemGenerator MenuElementGenerator { get; private set; }
        public GenericMenuGenerator MenuGenerator { get; private set; }

        private string Name;

        public StaticSupportMenuElement(GenericMenuItemGenerator menuElementGenerator, int adminLevel, SupportMenuCategories category) : base(adminLevel, category) {
            MenuElementGenerator = menuElementGenerator;
        }

        public StaticSupportMenuElement(GenericMenuGenerator menuGenerator, int adminLevel, SupportMenuCategories category, string name) : base(adminLevel, category) {
            MenuGenerator = menuGenerator;

            Name = name;
        }

        public override MenuElement getMenuElement(IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke();
            } else {
                return new VirtualMenu(Name, MenuGenerator);
            }
        }
    }

    public class GeneratedSupportMenuElement : SupportMenuElement {
        private GeneratedSelfMenuItemDelegate MenuElementGenerator = null;
        private GeneratedSelfMenuDelegate MenuGenerator = null;

        private string Name;

        public GeneratedSupportMenuElement(int adminLevel, SupportMenuCategories category, GeneratedSelfMenuItemDelegate menuElementGenerator) : base(adminLevel, category) {
            MenuElementGenerator = menuElementGenerator;
        }

        public GeneratedSupportMenuElement(int adminLevel, SupportMenuCategories category, string name, GeneratedSelfMenuDelegate menuGenerator) : base(adminLevel, category) {
            MenuGenerator = menuGenerator;

            Name = name;
        }

        public override MenuElement getMenuElement(IPlayer player) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke(player);
            } else {
                return new VirtualMenu(Name, () => MenuGenerator.Invoke(player));
            }
        }
    }
}
