//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Clothing;
//using ChoiceVServer.Model.Company;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static ChoiceVServer.Base.Constants;
//using static ChoiceVServer.Model.Menu.InputMenuItem;

//namespace ChoiceVServer.Controller {
//    public class MaterialDistributionController : ChoiceVScript {

//        public MaterialDistributionController() {
//            EventController.addCollisionShapeEvent("OPEN_MATERIAL_DISTRIBUTION", onMaterialDistribution);

//            EventController.addMenuEvent("MATERIAL_DISTRIBUTION_GET_AMMUNATION", onMaterialDistributionGetAmmunation);
//            EventController.addMenuEvent("MATERIAL_DISTRIBUTION_GET_WEAPON", onMaterialDistributionGetWeapon);
//            EventController.addMenuEvent("MATERIAL_DISTRIBUTION_GET_ARMOR", onMaterialDistributionGetArmor);
//            EventController.addMenuEvent("MATERIAL_DISTRIBUTION_GET_CLOTHES", onMaterialDistributionGetClothes);
//            EventController.addMenuEvent("MATERIAL_DISTRIBUTION_GET_MEDICATION", onMaterialDistributionGetMedication);
//            EventController.addMenuEvent("MATERIAL_DISTRIBUTION_GET_GENERIC_STACKABLE_ITEM", onMaterialCreateGenericStackableItem);
//            EventController.addMenuEvent("MATERIAL_DISTRIBUTION_GET_GENERIC_ITEM", onMaterialCreateGenericItem);
//            loadMatSpots();
//        }

//        private void loadMatSpots() {
//            using(var db = new ChoiceVDb()) {
//                foreach(var spot in db.configmaterialdistributionspots) {
//                    var data = new Dictionary<string, dynamic> {
//                        {"CompanyId", spot.companyId },
//                        {"ShowName", spot.showName }
//                    };

//                    CollisionShape.Create(spot.position.FromJson(), spot.width, spot.height, spot.rotation, true, false, true, "OPEN_MATERIAL_DISTRIBUTION", data);
//                }
//            }
//        }

//        public static void createMaterialSpot(string name, int companyId, Position position, float width, float height, float rotation) {
//            using(var db = new ChoiceVDb()) {
//                var newMat = new configmaterialdistributionspots {
//                    showName = name,
//                    companyId = companyId,
//                    position = position.ToJson(),
//                    width = width,
//                    height = height,
//                    rotation = rotation,
//                };

//                db.configmaterialdistributionspots.Add(newMat);
//                db.SaveChanges();
//            }
//        }

//        private bool onMaterialDistribution(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            var company = CompanyController.getCompany((int)data["CompanyId"]);
//            if(company != null && company.hasEmployee(player.getCharacterId())) {
//                var menu = new Menu("Materialausgabe", "Was möchtest du haben?");

//                //Ammunation
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_WEAPON_AMMUNATION"])) {
//                    var ammoMenu = new Menu("Munitionsausgabe", "Welche Munition möchtest du?");

//                    var materialAmmunations = company.getSettings("MATERIAL_AMMUNATION").Select(s => (WeaponType)Enum.Parse(typeof(WeaponType), s.settingsValue)).ToList();
//                    foreach(var weaponType in materialAmmunations) {
//                        var ammoData = new Dictionary<string, dynamic> { { "AmmunationType", weaponType }, { "Company", company } };
//                        var item = new InputMenuItem(weaponType.ToString() + " Munition", $"Wirklich X {weaponType.ToString()} Waffenmuniton ausgeben", "Anzahl", "MATERIAL_DISTRIBUTION_GET_AMMUNATION").withData(ammoData);
//                        ammoMenu.addMenuItem(item);
//                    }

//                    menu.addMenuItem(new MenuMenuItem(ammoMenu.Name, ammoMenu));
//                }

//                //Weapons
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_WEAPON_WEAPONS"])) {
//                    var weaponMenu = new Menu("Waffenausgabe", "Welche Waffe möchtest du?");
//                    var cfiWeapons = InventoryController.AllConfigItems.Where(c => (c.codeItem == typeof(Weapon).Name || c.codeItem == typeof(LongWeapon).Name));

//                    var materialWeapons = company.getSettings("MATERIAL_WEAPONS").Select(s => WeaponController.getConfigWeapon(s.settingsValue)).ToList();
//                    foreach(var cfWeapon in materialWeapons) {
//                        var cfi = cfiWeapons.FirstOrDefault(cfi => cfi.additionalInfo == cfWeapon.weaponName);
//                        if(cfi != null) {
//                            var weaponData = new Dictionary<string, dynamic> { { "ConfigWeapon", cfWeapon }, { "Company", company } };
//                            var item = new ClickMenuItem(cfi.name + "", $"Lass ein/eine {cfi.name} ausgeben", "", "MATERIAL_DISTRIBUTION_GET_WEAPON").withData(weaponData);
//                            item.needsConfirmation($"{cfi.name} wirklich ausgeben?", $"Wirklich ein/eine {cfi.name} ausgeben lassen?");
//                            weaponMenu.addMenuItem(item);
//                        } else {
//                            player.sendBlockNotification("Etwas ist schiefgelaufen, kontaktiere das Entwicklerteam. Code: BlaubärWaffeln!", "Dev-Team", Constants.NotifactionImages.System);
//                        }
//                    }

//                    menu.addMenuItem(new MenuMenuItem(weaponMenu.Name, weaponMenu));
//                }

//                //Armor
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_WEAPON_ARMOR"])) {
//                    var armorData = new Dictionary<string, dynamic> { { "Company", company } };
//                    var armorItem = new ClickMenuItem("Schutzweste ausgeben", "Lasse dir eine Schutzweste ausgeben", "", "MATERIAL_DISTRIBUTION_GET_ARMOR").withData(armorData);
//                    menu.addMenuItem(armorItem);
//                }

//                //Clothes
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_CLOTHES"])) {
//                    var clothesMenu = new Menu("Kleidungsausgabe", "Welche Kleidung möchtest du?");

//                    var materialClothing = ClothingOutfitController.getOutfits(company.getSettings("MATERIAL_CLOTHING").Select(s => s.settingsValue).ToList());
//                    foreach(var cfo in materialClothing.Where(o => o.gender == player.getCharacterData().Gender.ToString())) {
//                        if(cfo != null) {
//                            var clothingData = new Dictionary<string, dynamic> { { "ClothingOutfit", cfo }, { "Company", company } };
//                            var item = new ClickMenuItem(cfo.info, cfo.description, "", "MATERIAL_DISTRIBUTION_GET_CLOTHES").withData(clothingData);
//                            item.needsConfirmation($"{cfo.info} ausgeben?", $"Wirklich ausgeben lassen?");
//                            clothesMenu.addMenuItem(item);
//                        } else {
//                            player.sendBlockNotification("Etwas ist schiefgelaufen, kontaktiere das Entwicklerteam. Code: BlaubärWaffeln!", "Dev-Team", Constants.NotifactionImages.System);
//                        }
//                    }

//                    menu.addMenuItem(new MenuMenuItem(clothesMenu.Name, clothesMenu));
//                }

//                //Accessoires
//                //[ConfigId] | [Texture] | [Drawable] | [Description]
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_ACCESSOIRES"])) {
//                    var materialAccessoire = company.getSettings("MATERIAL_ACCESSOIRE").Select(s => InventoryController.AllConfigItems[int.Parse(s.settingsValue)]).ToList();

//                    var accessoireMenu = new Menu("Accessoireausgabe", "Was möchtest du haben?");
//                    var slotDic = new Dictionary<int, Menu>();
//                    foreach(var accessoire in materialAccessoire) {
//                        var slotId = int.Parse(accessoire.additionalInfo.Split('#')[0]);
//                        var accData = new Dictionary<string, dynamic> { { "ConfigItem", accessoire }, { "Company", company } };
//                        if(slotDic.ContainsKey(slotId)) {
//                            var slotMenu = slotDic[slotId];
//                            slotMenu.addMenuItem(new ClickMenuItem(accessoire.name, $"Lass dir ein/eine {accessoire.name} ausgeben", "", "MATERIAL_DISTRIBUTION_GET_GENERIC_ITEM").withData(accData));
//                        } else {
//                            var slotMenu = new Menu($"{AccessoireSlotToName[slotId]}", "Was möchtest du dir ausgeben lassen?");
//                            slotMenu.addMenuItem(new ClickMenuItem(accessoire.name, $"Lass dir ein/eine {accessoire.name} ausgeben", "", "MATERIAL_DISTRIBUTION_GET_GENERIC_ITEM").withData(accData));
//                            slotDic[slotId] = slotMenu;
//                        }
//                    }

//                    foreach(var slotMenu in slotDic.Values) {
//                        accessoireMenu.addMenuItem(new MenuMenuItem(slotMenu.Name, slotMenu));
//                    }

//                    menu.addMenuItem(new MenuMenuItem(accessoireMenu.Name, accessoireMenu));
//                }

//                //Medication
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_MEDICATION"])) {
//                    var medicationMenu = new Menu("Medikamentenausgabe", "Welche Medikamente möchtest du?");

//                    var materialMedic = company.getSettings("MATERIAL_MEDICATION").Select(s => InventoryController.AllConfigItems[int.Parse(s.settingsValue)]).ToList();
//                    foreach(var cfm in materialMedic) {
//                        if(cfm != null) {
//                            var medicData = new Dictionary<string, dynamic> { { "ConfigItem", cfm }, { "Company", company } };
//                            var item = new ClickMenuItem(cfm.name, $"Lasse dir {cfm.name} ausgeben", "", "MATERIAL_DISTRIBUTION_GET_MEDICATION").withData(medicData);
//                            item.needsConfirmation($"{cfm.name} ausgeben?", $"Wirklich ausgeben lassen?");
//                            medicationMenu.addMenuItem(item);
//                        } else {
//                            player.sendBlockNotification("Etwas ist schiefgelaufen, kontaktiere das Entwicklerteam. Code: BlaubärWaffeln!", "Dev-Team", Constants.NotifactionImages.System);
//                        }
//                    }

//                    menu.addMenuItem(new MenuMenuItem(medicationMenu.Name, medicationMenu));
//                }


//                //Normal Items
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_GENERIC"])) {
//                    var materialgeneric = company.getSettings("MATERIAL_GENERIC").Select(s => InventoryController.AllConfigItems[int.Parse(s.settingsValue)]).ToList();

//                    var subMenu = new Menu("Generische Items", "Was möchtest du haben?");
//                    foreach(var generic in materialgeneric) {
//                        var genericData = new Dictionary<string, dynamic> { { "ConfigItem", generic }, { "Company", company } };
//                        menu.addMenuItem(new ClickMenuItem(generic.name, $"Lass dir ein/eine {generic.name} ausgeben", "", "MATERIAL_DISTRIBUTION_GET_GENERIC_ITEM").withData(genericData));
//                    }

//                    menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
//                }

//                //Normal Items Stackable
//                if(CompanyController.hasPlayerPermission(player, company, CompanyController.AllCompanyPermissions["MATERIAL_DISTRIBUTION_GENERIC_STACKABLE"])) {
//                    var materialgenericStack = company.getSettings("MATERIAL_GENERIC_STACKABLE").Select(s => InventoryController.AllConfigItems[int.Parse(s.settingsValue)]).ToList();

//                    var subMenu = new Menu("Generische Items 2", "Was möchtest du haben?");
//                    foreach(var genericStack in materialgenericStack) {
//                        var genericStackData = new Dictionary<string, dynamic> { { "ConfigItem", genericStack }, { "Company", company } };
//                        subMenu.addMenuItem(new InputMenuItem(genericStack.name, $"Lasse dir {genericStack} ausgeben", "Anzahl", "MATERIAL_DISTRIBUTION_GET_GENERIC_STACKABLE_ITEM").withData(genericStackData));
//                    }

//                    menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
//                }


//                player.showMenu(menu);
//                return true;
//            } else {
//                player.sendBlockNotification("Was machst du hier, du bist kein Mitarbeiter?", "Kein Mitarbeiter", Constants.NotifactionImages.Package);
//            }

//            return false;
//        }

//        private bool onMaterialDistributionGetAmmunation(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var inputEvt = menuItemCefEvent as InputMenuItemEvent;
//            var weaponType = (WeaponType)data["AmmunationType"];
//            var company = (Company)data["Company"];

//            if(int.TryParse(inputEvt.input, out var ammoCount)) {
//                //Just to assure, that if the WeaponAmmunation ClassName changes this notifies!
//                var cf = InventoryController.AllConfigItems.FirstOrDefault(c => c.codeItem == typeof(WeaponAmmunation).Name && c.additionalInfo == weaponType.ToString());
//                if(cf != null) {
//                    var ammoItem = new WeaponAmmunation(cf, ammoCount, -1);
//                    if(player.getInventory().addItem(ammoItem)) {
//                        company.logMessage("MATERIAL_DISTRIBUTION_AMMUNATION", $"{player.getCharacterName()} {ammoCount} {weaponType.ToString()}");
//                        player.sendNotification(NotifactionTypes.Success, $"Es wurden dir {ammoCount} Schuss {weaponType.ToString()} Munition ausgegeben!", "Munition ausgegeben", NotifactionImages.Package);
//                        return true;
//                    } else {
//                        player.sendBlockNotification("Es war nicht genug Platz in deinem Inventar!", "Kein Platz", NotifactionImages.Package);
//                    }
//                } else {
//                    Logger.logError($"onMaterialDistributionGetAmmunation: configItem not found!, charId: {player.getCharacterId()} ");
//                }
//            } else {
//                player.sendBlockNotification("Die Eingabe war keine gültige Zahl!", "Ungültige Eingabe", Constants.NotifactionImages.Package);
//            }

//            return false;
//        }

//        private bool onMaterialDistributionGetWeapon(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var configWeapon = (configweapons)data["ConfigWeapon"];
//            var company = (Company)data["Company"];

//            var cf = InventoryController.AllConfigItems.FirstOrDefault(c => (c.codeItem == typeof(Weapon).Name || c.codeItem == typeof(LongWeapon).Name) && c.additionalInfo == configWeapon.weaponName);
//            if(cf != null) {
//                Item weaponItem;
//                if(cf.codeItem == typeof(Weapon).Name) {
//                    weaponItem = new Weapon(cf);
//                } else {
//                    weaponItem = new LongWeapon(cf);
//                }

//                if(player.getInventory().addItem(weaponItem)) {
//                    company.logMessage("MATERIAL_DISTRIBUTION_WEAPON", $"{player.getCharacterName()} {weaponItem.Name}");
//                    player.sendNotification(NotifactionTypes.Success, $"Es wurde dir ein/eine {weaponItem.Name} ausgegeben!", "Waffe ausgegeben", NotifactionImages.Package);
//                    return true;
//                } else {
//                    player.sendBlockNotification("Es war nicht genug Platz in deinem Inventar!", "Kein Platz", NotifactionImages.Package);
//                }
//            } else {
//                Logger.logError($"onMaterialDistributionGetWeapon: configItem not found!, charId: {player.getCharacterId()} ");
//            }

//            return false;
//        }

//        private bool onMaterialDistributionGetArmor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (Company)data["Company"];

//            var cf = InventoryController.getConfigItemForType(typeof(ArmorWest));
//            if(cf != null) {
//                var armorItem = new ArmorWest(cf, 100);

//                if(player.getInventory().addItem(armorItem)) {
//                    company.logMessage("MATERIAL_DISTRIBUTION_ARMOR", $"{player.getCharacterName()} Schutzweste");
//                    player.sendNotification(NotifactionTypes.Success, $"Es wurde dir eine Schutzweste ausgegeben!", "Weste ausgegeben", NotifactionImages.Package);
//                    return true;
//                } else {
//                    player.sendBlockNotification("Es war nicht genug Platz in deinem Inventar!", "Kein Platz", NotifactionImages.Package);
//                }
//            } else {
//                Logger.logError($"onMaterialDistributionGetArmor: configItem not found!, charId: {player.getCharacterId()} ");
//            }

//            return false;
//        }

//        private bool onMaterialDistributionGetClothes(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (Company)data["Company"];
//            var outfit = (configoutfits)data["ClothingOutfit"];

//            var cf = InventoryController.getConfigItemForType(typeof(ClothingOutfit));
//            if(cf != null) {
//                var clothesItem = ClothingOutfit.getConfigOutfit(outfit.name, player);

//                if(player.getInventory().addItem(clothesItem)) {
//                    company.logMessage("MATERIAL_DISTRIBUTION_CLOTHES", $"{player.getCharacterName()} {outfit.info}");
//                    player.sendNotification(NotifactionTypes.Success, $"Es wurde dir ein Kleidungsset ausgegeben!", "Kleidung ausgegeben", NotifactionImages.Package);
//                    return true;
//                } else {
//                    player.sendBlockNotification("Es war nicht genug Platz in deinem Inventar!", "Kein Platz", NotifactionImages.Package);
//                }
//            } else {
//                Logger.logError($"onMaterialDistributionGetClothes: configItem not found!, charId: {player.getCharacterId()}");
//            }

//            return false;
//        }

//        private bool onMaterialDistributionGetMedication(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (Company)data["Company"];
//            var cfItem = (configitems)data["ConfigItem"];
//            if(cfItem != null) {
//                var item = new MedicItem(cfItem);
//                if(player.getInventory().addItem(item)) {
//                    company.logMessage("MATERIAL_DISTRIBUTION_MEDICATION", $"{player.getCharacterName()} {item.Name}");
//                    player.sendNotification(NotifactionTypes.Success, $"Es wurde das Medikament ausgegeben!", "Medikament ausgegeben", NotifactionImages.Package);
//                    return true;
//                } else {
//                    player.sendBlockNotification("Es war nicht genug Platz in deinem Inventar!", "Kein Platz", NotifactionImages.Package);
//                }
//            } else {
//                Logger.logError($"onMaterialDistributionGetMedication: configItem not found!, charId: {player.getCharacterId()}");
//            }

//            return false;
//        }

//        private bool onMaterialCreateGenericItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var company = (Company)data["Company"];
//            var cfItem = (configitems)data["ConfigItem"];
//            if(cfItem != null) {
//                var item = InventoryController.createGenericItem(cfItem);
//                if(player.getInventory().addItem(item)) {
//                    company.logMessage("MATERIAL_DISTRIBUTION_GENERIC", $"{player.getCharacterName()} {item.Name}");
//                    player.sendNotification(NotifactionTypes.Success, $"Es wurde ein/eine {item.Name} ausgegeben!", $"{item.Name} ausgegeben", NotifactionImages.Package);
//                    return true;
//                } else {
//                    player.sendBlockNotification("Es war nicht genug Platz in deinem Inventar!", "Kein Platz", NotifactionImages.Package);
//                }
//            } else {
//                Logger.logError($"onMaterialCreateGenericItem: configItem not found!, charId: {player.getCharacterId()}");
//            }

//            return true;
//        }

//        private bool onMaterialCreateGenericStackableItem(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var inputEvt = menuItemCefEvent as InputMenuItemEvent;
//            var company = (Company)data["Company"];
//            var cf = (configitems)data["ConfigItem"];
//            if(int.TryParse(inputEvt.input, out var count)) {
//                var item = InventoryController.createGenericStackableItem(cf, count, -1);
//                if(player.getInventory().addItem(item)) {
//                    company.logMessage("MATERIAL_DISTRIBUTION_GENERIC_2", $"{player.getCharacterName()} {count} {item.Name}");
//                    player.sendNotification(NotifactionTypes.Success, $"Es wurden {count} {item.Name} ausgegeben!", $"{item.Name} ausgegeben", NotifactionImages.Package);
//                    return true;
//                } else {
//                    player.sendBlockNotification("Es war nicht genug Platz in deinem Inventar!", "Kein Platz", NotifactionImages.Package);
//                }
//            } else {
//                player.sendBlockNotification("Die Eingabe war keine gültige Zahl!", "Ungültige Eingabe", Constants.NotifactionImages.Package);
//            }

//            return false;
//        }

//    }
//}
