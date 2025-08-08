using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public enum DriverLicenseClasses {
        PKW,
        Motorrad,
        LKW,
        Helikopter,
        Flugzeug,
        Boot,
    }

    public class DriversLicense : IdCardItem {
        public static readonly List<string> FIRST_NAME_LIST = [
            "James",
            "Kevin",
            "Keith",
            "Kayle",
            "Abraham",
            "George",
            "Christian",
        ];

        public static readonly List<string> LAST_NAME_LIST = [
            "Shepherd",
            "Jackson",
            "Baker",
            "Reid",
            "Zane",
            "Nolan",
            "Jones",
        ];

        public DriversLicense(item item) : base(item) {
            EventName = "OPEN_DRIVERS_LICENSE";
        }

        public DriversLicense(configitem configItem, IPlayer player, DriverLicenseClasses vehicleClass, IPlayer issuer = null, bool noId = false) : base(configItem) {
            EventName = "OPEN_DRIVERS_LICENSE";

            var issuerName = "";
            if(issuer == null) {
                var rand = new Random();
                issuerName = FIRST_NAME_LIST[rand.Next(0, FIRST_NAME_LIST.Count - 1)] + " " + LAST_NAME_LIST[rand.Next(0, LAST_NAME_LIST.Count - 1)];
            } else {
                issuerName = issuer.getCharacterName();
            }

            var fullName = player.getCharacterName();
            var split = fullName.Split(' ');
            var firstName = "";
            for(var i = 0; i < split.Length - 1; i++) {
                firstName += " " + split[i];
            }

            var hairColorName = "Unbekannt";
            using(var db = new ChoiceVDb()) {
                var hairColor = player.getCharacterData().Style.hairColor;
                var color = db.confighaircolors.FirstOrDefault(h => h.id == hairColor);
                if(color != null) {
                    hairColorName = color.name;
                } else {
                    Logger.logError($"DriversLicense: hairColor not found: id: {player.getCharacterData().Style.hairColor}",
                                        $"Fehler im Führerschein-System: Spieler Haarfarbe konnte keinem Namen zugewiesen werden: {player.getCharacterData().Style.hairColor}.", player);
                }
            }

            var eyeColorName = "Unbekannt";
            using(var db = new ChoiceVDb()) {
                var eyeColor = player.getCharacterData().Style.faceEyes;
                var color = db.configeyecolors.FirstOrDefault(h => h.id == eyeColor);
                if(color != null) {
                    eyeColorName = color.name;
                } else {
                    Logger.logError($"DriversLicense: eyeColor not found: id: {player.getCharacterData().Style.hairColor}",
                                        $"Fehler im Führerschein-System: Spieler Haarfarbe konnte keinem Namen zugewiesen werden: {player.getCharacterData().Style.hairColor}.", player);
                }
            }

            var idNumber = "[WIRD GENERIERT]";
            if(!noId) {
                idNumber = getNextIdCardId().ToString();
            }

            setData([
                new IdCardItemElement("dlNumber", $"{getLicenseTypeString(vehicleClass)}{idNumber}"),
                new IdCardItemElement("expDate", (DateTime.Now + TimeSpan.FromDays(90)).ToString("d")),
                new IdCardItemElement("vehicleClass", vehicleClass.ToString()),
                new IdCardItemElement("lastName", split[split.Length - 1]),
                new IdCardItemElement("firstName", firstName),
                new IdCardItemElement("dateOfBirth", player.getCharacterData().DateOfBirth.ToString("d")),
                new IdCardItemElement("gender", player.getCharacterData().Gender + ""),
                new IdCardItemElement("hairColor", hairColorName),
                new IdCardItemElement("eyeColor", eyeColorName),
                new IdCardItemElement("issueDate", (DateTime.Now - TimeSpan.FromDays(365)).ToString("d")),
                new IdCardItemElement("issuer", issuerName),
                new IdCardItemElement("signature", player.getCharacterName()),
            ]);



            Description = $"Name: {fullName}, Nummer: DL{idNumber}";
            updateDescription();
        }

        private static string getLicenseTypeString(DriverLicenseClasses type) {
            switch(type) {
                case DriverLicenseClasses.PKW:
                    return "DL";
                case DriverLicenseClasses.Motorrad:
                    return "ML";
                case DriverLicenseClasses.LKW:
                    return "TL";
                case DriverLicenseClasses.Boot:
                    return "BL";
                case DriverLicenseClasses.Helikopter:
                    return "HL";
                case DriverLicenseClasses.Flugzeug:
                    return "PL";
                default:
                    return "ERROR";
            }
        }

        public override void use(IPlayer player) {
            base.use(player);

            player.emitCefEventWithBlock(getCefEvent(), "DRIVERS_LICENSE");
        }

        private static long getNextIdCardId() {
            var rnd = new Random();
            var step = rnd.Next(0, 50);

            using(var db = new ChoiceVDb()) {
                var dItem = db.configitems.FirstOrDefault(i => i.codeItem == nameof(DriversLicense));
                var cItem = InventoryController.getConfigItemForType<DriversLicense>();

                var value = long.Parse(cItem.additionalInfo);

                cItem.additionalInfo = (value + step).ToString();
                dItem.additionalInfo = (value + step).ToString();

                db.SaveChanges();

                return value;
            }
        }
    }
}
