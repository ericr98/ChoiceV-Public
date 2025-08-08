using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.FsDatabase;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Terminal.TerminalShops {
    public enum LicenseDataTypes {
        VehicleLicense,
        Certificate
    }

    public class LicenseDataClass {
        public LicenseDataTypes Type;
        public string Identifier;
        public string Data;
    }

    public class TerminalLicenseShop : TerminalShop {
        private const int TERMINAL_CAR_LICENSE = 10;
        private const int TERMINAL_MOTORCYCLE_LICENSE = 8;
        private const int TERMINAL_LKW_LICENSE = 6;

        private const int TERMINAL_HELICOPTER_LICENSE = 12;
        private const int TERMINAL_PLANE_LICENSE = 12;

        private const int TERMINAL_BOAT_LICENSE = 7;

        private const int TERMINAL_CERTIFICATE_PRICE = 3;

        public TerminalLicenseShop() : base("LICENSE_SHOP", "Lizenzanmeldestelle") {

        }

        public override Menu getBuyableOptions(IPlayer player) {
            var menu = new Menu("Lizenz kaufen", "Was möchtest du kaufen?");
            menu.addMenuItem(new StaticMenuItem("Information", "Alle gekauften Lizenzen werden erst NACH dem Landen des Flugzeuges dem Inventar hinzugefügt!", "siehe Beschreibung"));

            var tokens = TerminalShopController.getPlayerTokens(player);

            List<LicenseDataClass> list = new();
            if(player.hasData("TERMINAL_LICENSE_LIST")) {
                list = ((string)player.getData("TERMINAL_LICENSE_LIST")).FromJson<List<LicenseDataClass>>();
            }

            menu.addMenuItem(getVehicleLicenseBuyItem("DRIVER", list, tokens));
            menu.addMenuItem(getVehicleLicenseBuyItem("MOTORCYCLE", list, tokens));
            menu.addMenuItem(getVehicleLicenseBuyItem("LKW", list, tokens));
            menu.addMenuItem(getVehicleLicenseBuyItem("HELICOPTER", list, tokens));
            menu.addMenuItem(getVehicleLicenseBuyItem("PLANE", list, tokens));
            menu.addMenuItem(getVehicleLicenseBuyItem("BOAT", list, tokens));

            if(tokens >= TERMINAL_CERTIFICATE_PRICE) {
                var certificateMenu = new Menu("Zertifikat anfertigen", "Fertige ein Zertifikat an.");
                certificateMenu.addMenuItem(new StaticMenuItem("BEACHTE! Beschreibung lesen!", "Diese Funktion sollte nur in Absprache mit dem Support genutzt werden. Sie ermöglicht sich versch. Sachen zertifizieren zu lassen. Studiumabschlüsse, Ausbildungen, Auszeichnungen, etc. nach der Erstellung kann sich das Zertifikat probeweise angezeigt werden!", "", MenuItemStyle.yellow));

                certificateMenu.addMenuItem(new InputMenuItem("Titel", "Der zertifitzierte Titel bzw. der Name der Auszeichnung. Maximal 55 Zeichen!", "z.B. max. 55 Zeichen", ""));
                certificateMenu.addMenuItem(new InputMenuItem("Name", "Der Name auf den das Zertifikat ausgestellt ist. Max. 40 Zeichen!", "max. 40 Zeichen", ""));
                certificateMenu.addMenuItem(new InputMenuItem("Untertext", "Ein Text unterhalb des Namens steht und das Zertifikat erklärt. Kann unter anderem Noten, Details, Erklärungen enthalten. ER WIRD AM BESTEN VORGESCHRIEBEN UND IN DAS FELD KOPIERT. Max. 500 Zeichen!", "max. 500 Zeichen", ""));
                certificateMenu.addMenuItem(new InputMenuItem("Unterschriftsdatum", "Das Datum der Unterzeichnung des Zertifikats", "z.B. 19.09.2010", ""));
                certificateMenu.addMenuItem(new InputMenuItem("Unterschriftname", "Der Name der Person welche Das Zertifikat unterschrieben hat. z.B. der Dekan der Universität oder der General der Armee", "max. 55 Zeichen", ""));
                certificateMenu.addMenuItem(new MenuStatsMenuItem($"Erwerben für {TERMINAL_CERTIFICATE_PRICE} Marken", "Kaufe das Zertifikat. Das genau Aussehen wird nach dem Kauf angezeigt", "TERMINAL_BUY_CERTIFICATE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Price", TERMINAL_CERTIFICATE_PRICE } }).needsConfirmation("Zertifikat kaufen?", $"Wirklich für {TERMINAL_CERTIFICATE_PRICE}M kaufen?"));

                menu.addMenuItem(new MenuMenuItem(certificateMenu.Name, certificateMenu));
            } else {
                menu.addMenuItem(new StaticMenuItem("Zeritifkat nicht anfertigbar", "Du kannst dir hier ein Zertifikat erstellen lassen. (In Absprache mit dem Support)", $"zu teuer ({TERMINAL_CERTIFICATE_PRICE}M)", MenuItemStyle.yellow));
            }
            return menu;
        }

        private MenuItem getVehicleLicenseBuyItem(string identifier, List<LicenseDataClass> licenses, int playerTokens) {
            var name = vehicleLicenseIdentifierToName(identifier);
            var price = vehicleLicenseIdentifierToPrice(identifier);

            if(!licenses.Any(l => l.Identifier == identifier)) {
                if(playerTokens >= price) {
                    return new ClickMenuItem(name, $"Melde einen {name} an. Du erhälst ihn bei der Landung", $"{price} Marken", "TERMINAL_BUY_LICENSE").withData(new Dictionary<string, dynamic> { { "Type", identifier }, { "Price", price } }).needsConfirmation($"{name} kaufen?", "Führerschein wirklich kaufen?");
                } else {
                    return new StaticMenuItem(name, $"Melde einen {name} an. Du erhälst ihn bei der Landung", $"zu teuer! ({price}M)", MenuItemStyle.yellow);
                }
            } else {
                return new StaticMenuItem(name, $"Melde einen {name} an. Du erhälst ihn bei der Landung", $"bereits erworben");
            }
        }

        public override Menu getAlreadyBoughtListing(IPlayer player) {
            var menu = new Menu("Bereits Gekauftes zurückgeben", "Was möchtest du kaufen?");

            var list = new List<LicenseDataClass>();
            if(player.hasData("TERMINAL_LICENSE_LIST")) {
                list = ((string)player.getData("TERMINAL_LICENSE_LIST")).FromJson<List<LicenseDataClass>>();
            }

            foreach(var license in list) {
                if(license.Type == LicenseDataTypes.VehicleLicense) {
                    var name = vehicleLicenseIdentifierToName(license.Identifier);
                    var price = vehicleLicenseIdentifierToPrice(license.Identifier);

                    menu.addMenuItem(new ClickMenuItem(name, $"Klicke um den Fahrzeug-Führerschein zurückzugeben. Du erhältst die Marken zurück.", $"{price} Marken", "TERMINAL_UNBUY_LICENSE").withData(new Dictionary<string, dynamic> { { "License", license }, { "Price", price } }).needsConfirmation($"Lizenz zurückgeben?", $"{name} für {price}M zurückgeben?"));
                } else if(license.Type == LicenseDataTypes.Certificate) {
                    var data = license.Data.FromJson<List<string>>();
                    menu.addMenuItem(new ClickMenuItem($"Zertifikat: {data[0]}", $"Ein Zertifikat über {data[0]} für {data[1]}. Du erhältst die Marken zurück.", $"{TERMINAL_CERTIFICATE_PRICE} Marken", "TERMINAL_UNBUY_LICENSE").withData(new Dictionary<string, dynamic> { { "License", license }, { "Price", TERMINAL_CERTIFICATE_PRICE } }).needsConfirmation($"Zertifikat zurückgeben?", $"Zertifkat für {TERMINAL_CERTIFICATE_PRICE}M zurückgeben?"));
                }
            }
            return menu;
        }

        private string vehicleLicenseIdentifierToName(string identifier) {
            switch(identifier) {
                case "DRIVER":
                    return "PKW-Führerschein";
                case "MOTORCYCLE":
                    return "Motorradschein";
                case "LKW":
                    return "LKW-Führerschein";
                case "HELICOPTER":
                    return "Helikopterschein";
                case "PLANE":
                    return "Flugschein";
                case "BOAT":
                    return "Bootsschein";
                default:
                    return "";
            }
        }

        private int vehicleLicenseIdentifierToPrice(string identifier) {
            switch(identifier) {
                case "DRIVER":
                    return TERMINAL_CAR_LICENSE;
                case "MOTORCYCLE":
                    return TERMINAL_MOTORCYCLE_LICENSE;
                case "LKW":
                    return TERMINAL_LKW_LICENSE;
                case "HELICOPTER":
                    return TERMINAL_HELICOPTER_LICENSE;
                case "PLANE":
                    return TERMINAL_PLANE_LICENSE;
                case "BOAT":
                    return TERMINAL_BOAT_LICENSE;
                default:
                    return int.MaxValue;
            }
        }

        public override bool hasPlayerBoughtSomething(IPlayer player) {
            return player.hasData("TERMINAL_LICENSE_LIST");
        }

        public override void onPlayerLand(IPlayer player, ref executive_person_file file) {
            if(player.hasData("TERMINAL_LICENSE_LIST")) {

                var list = ((string)player.getData("TERMINAL_LICENSE_LIST")).FromJson<List<LicenseDataClass>>();

                foreach(var license in list) {
                    if(license.Type == LicenseDataTypes.VehicleLicense) {
                        var item = TerminalLicenseShopEventController.generateTerminalDriverLicense(player, license.Identifier, false);
                        if(license.Identifier == "DRIVER") {
                            file.driversLicense = long.Parse(item.getData("dlNumber").Remove(0, 2));
                        }

                        player.getInventory().addItem(item, true);
                    } else {
                        var cfgCertificate = InventoryController.getConfigItemForType<CertificateFile>();
                        var data = license.Data.FromJson<List<string>>();
                        var certificateItem = new CertificateFile(cfgCertificate, data[0], data[1], data[2], data[3], data[4]);

                        player.getInventory().addItem(certificateItem, true);
                    }
                }

                player.resetPermantData("TERMINAL_LICENSE_LIST");
            }
        }
    }

    public class TerminalLicenseShopEventController : ChoiceVScript {
        public TerminalLicenseShopEventController() {
            EventController.addMenuEvent("TERMINAL_BUY_LICENSE", onTerminalBuyLicense);
            EventController.addMenuEvent("TERMINAL_UNBUY_LICENSE", onTerminalUnbuyLicense);

            EventController.addMenuEvent("TERMINAL_BUY_CERTIFICATE", onTerminalBuyCertificate);

            TerminalShopController.addTerminalShop(new TerminalLicenseShop());
        }

        private bool onTerminalBuyLicense(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var identifier = (string)data["Type"];
            var price = (int)data["Price"];

            if(TerminalShopController.addOrRemovePlayerTokens(player, -price)) {
                var license = generateTerminalDriverLicense(player, identifier, true);

                var list = new List<LicenseDataClass>();
                if(player.hasData("TERMINAL_LICENSE_LIST")) {
                    list = ((string)player.getData("TERMINAL_LICENSE_LIST")).FromJson<List<LicenseDataClass>>();
                }

                list.Add(new LicenseDataClass { Type = LicenseDataTypes.VehicleLicense, Identifier = identifier, Data = "" });
                player.setPermanentData("TERMINAL_LICENSE_LIST", list.ToJson());

                license.use(player);
                player.sendNotification(Constants.NotifactionTypes.Success, "Lizenz erfolgreich erworben. Eine Vorschau wird dir angezeigt! Die Lizenz wird dir NACH dem Landeanflug ausgehändigt!", "Lizenz erworben", Constants.NotifactionImages.Plane);
            } else {
                player.sendBlockNotification("Etwas ist schiefgelaufen?", "");
            }

            return true;
        }

        private bool onTerminalUnbuyLicense(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var license = (LicenseDataClass)data["License"];
            var price = (int)data["Price"];

            if(TerminalShopController.addOrRemovePlayerTokens(player, price)) {
                var list = ((string)player.getData("TERMINAL_LICENSE_LIST")).FromJson<List<LicenseDataClass>>();
                list.RemoveAll(l => l.Identifier == license.Identifier && l.Data == license.Data);
                player.setPermanentData("TERMINAL_LICENSE_LIST", list.ToJson());
                player.sendNotification(Constants.NotifactionTypes.Info, $"Lizenz wurde zurückgegeben. Es wurden dir {price} Marken erstattet!", "Lizenz zurückgegeben", Constants.NotifactionImages.Plane);
            }

            return true;
        }

        public static DriversLicense generateTerminalDriverLicense(IPlayer player, string identifier, bool noId) {
            var type = DriverLicenseClasses.PKW;

            switch(identifier) {
                case "MOTORCYCLE":
                    type = DriverLicenseClasses.Motorrad;
                    break;
                case "LKW":
                    type = DriverLicenseClasses.LKW;
                    break;
                case "HELICOPTER":
                    type = DriverLicenseClasses.Helikopter;
                    break;
                case "PLANE":
                    type = DriverLicenseClasses.Flugzeug;
                    break;
                case "BOAT":
                    type = DriverLicenseClasses.Boot;
                    break;
            }

            var cfg = InventoryController.getConfigItemForType<DriversLicense>();
            return new DriversLicense(cfg, player, type, null, noId);
        }

        private bool onTerminalBuyCertificate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var price = (int)data["Price"];
            if(TerminalShopController.addOrRemovePlayerTokens(player, -price)) {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

                var titleEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
                var nameEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
                var textEvt = evt.elements[3].FromJson<InputMenuItemEvent>();
                var signDateEvt = evt.elements[4].FromJson<InputMenuItemEvent>();
                var signNameEvt = evt.elements[5].FromJson<InputMenuItemEvent>();

                var list = new List<LicenseDataClass>();
                if(player.hasData("TERMINAL_LICENSE_LIST")) {
                    list = ((string)player.getData("TERMINAL_LICENSE_LIST")).FromJson<List<LicenseDataClass>>();
                }

                var cfgCertificate = InventoryController.getConfigItemForType<CertificateFile>();
                var certificateItem = new CertificateFile(cfgCertificate, titleEvt.input, nameEvt.input, textEvt.input, signDateEvt.input, signNameEvt.input);

                var stringList = new List<string> { titleEvt.input, nameEvt.input, textEvt.input, signDateEvt.input, signNameEvt.input };
                list.Add(new LicenseDataClass { Type = LicenseDataTypes.Certificate, Identifier = "CERTIFICATE", Data = stringList.ToJson() });

                player.setPermanentData("TERMINAL_LICENSE_LIST", list.ToJson());

                certificateItem.use(player);
                player.sendNotification(Constants.NotifactionTypes.Success, "Zertifikat erfolgreich erworben. Eine Vorschau wird dir angezeigt! Das Zertifikat wird dir NACH dem Landeanflug ausgehändigt!", "Zertifikat erworben", Constants.NotifactionImages.Plane);
            }

            return true;
        }
    }
}
