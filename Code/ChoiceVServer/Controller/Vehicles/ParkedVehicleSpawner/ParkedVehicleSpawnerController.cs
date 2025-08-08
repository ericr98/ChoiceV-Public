using AltV.Net.Data;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ChoiceVServer.Controller {

    //    Billigkarren:
    //	Variante 1:
    //		Tür wird mit Brecheisen aufgebrochen(Ist in Schadenssystem dann weg) (Wenn Tür ab ist, dann kann Fahrzeug nicht abgeschlossen werden)
    //		Autoalarm wird abgespielt(von der Position des Autos, hört auf nachdem Motor an ist)

    //        Mit gewisser Wahrscheinlichkeit werden Cops informiert(Modell und Farbe, partial Nummernkennzeichen)




    //    Nach max. 30min wird das Fahrzeug als gestohlen gemeldetet (wenn nicht Status "Zur Abfährtigung übergeben") und Status als "Gestohlen" gestellt.
    //    Dann ist Diebstahl für Cops aktuell

    //Unfall:
    //	Spieler crasht in Fahrzeug:
    //		1. Variante: Spieler hält an und ruft Polizei

    //            Polizei kommt und rpt einen Unfall aus(Unfallbericht, Alkoholtest wenn angebracht, Daten aufnehmen, durchfunken nach Infos für Auto)

    //            A.Variante: Auto gehört Spieler: Spieler 1 und 2 machen unter sich Schaden aus, und Unfallverursacher zahlt ggf.Strafe

    //            B.Variante
    //                Auto gehört NPC: Polizei funkt Leitstelle an:

    //                Leistelle merkt, dass Auto NPC gehört (Protokoll überlegen z.B. "NPC möchte, dass wir das übernehmen")

    //                Cops rufen ACLS, der schleppt ab
    //                Cops bringen Fahrzeug in spezielle Garage wo es dann nach Zeit x despawnt

    //		2. Variante:
    //			Wie erste nur dass 5min nach Unfall ein Dispatch an die Cops geht, dern nach dem VehicleDamageEvent ausgelöst wird

    //        Cops kriegen Schlüssel indem sie zum Impound NPC gehen und die Chassisnummer angeben.Steht das Auto im FS dann auf "Zur Abfährtigung übergeben" (stateDate 20min nach setzen des states), so kriegt der Cop den Schlüssel vom Impound Typ
    //        Ist das Auto in der Impound Garage, und hat den State "Zur Abfährtigung übergeben", wird es nach ein paar Tagen gelöscht

    //        Fahrzeuge die den State "Zur Abfährtigung übergeben" haben lösen keine Dispatches aus




    public class ParkedVehicleSpawnerController : ChoiceVScript {
        //All Parking Spots
        private List<CarGeneratorItem> carGeneratorItems = new List<CarGeneratorItem>();

        //Vehicle Models in the PopGroups
        private Dictionary<String, String[]> popGroup = new Dictionary<string, string[]>();

        private List<ChoiceVVehicle> RandomlySpawnedVehicle;
        
        //Load all JSONs
        public ParkedVehicleSpawnerController() {
            var basePath = "";
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                basePath = @"resources\ChoiceVServer\data\";
            } else {
                basePath = "resources/ChoiceVServer/data/";
            }
            //Pfade machen wie WordController und Heightmap
            this.carGeneratorItems = FileLoader.LoadDataFromJsonFile<List<CarGeneratorItem>>(basePath + "CarGenerators.json");
            this.popGroup = FileLoader.LoadDataFromJsonFile<Dictionary<string, string[]>>(basePath + "PopGroup.json");

            RandomlySpawnedVehicle = new List<ChoiceVVehicle>();
            if(Config.RandomlySpawnedVehicles > 0) {
                EventController.MainAfterReadyDelegate += onMainReady;
                InvokeController.AddTimedInvoke("Randomly-Spawned-Car-Changer", updateCars, TimeSpan.FromMinutes(30), true);
            }
        }

        private void onMainReady() {
            RandomlySpawnedVehicle = ChoiceVAPI.GetAllVehicles().Reverse<ChoiceVVehicle>().Where(v => v.RandomlySpawnedDate != null).ToList();
            updateCars(null);
        }

        private void updateCars(IInvoke obj) {
            var random = new Random();
            var newList = new List<ChoiceVVehicle>();
            foreach(var vehicle in RandomlySpawnedVehicle) {

                if(vehicle.RandomlySpawnedDate + TimeSpan.FromDays(2) + TimeSpan.FromHours(random.Next(0, 3)) < DateTime.Now) {
                    VehicleController.removeVehicle(vehicle);
                } else {
                    newList.Add(vehicle);
                }
            }

            RandomlySpawnedVehicle = newList;
            createRandomlySpawnedVehicles();
        }

        public ChoiceVVehicle findParkedCar(Predicate<ChoiceVVehicle> predicate) {
            return RandomlySpawnedVehicle.FirstOrDefault(v => predicate(v));
        }

        private void createRandomVehicle() {
            var random = new Random();

            var counter = 0;
            var carGeneratorItem = carGeneratorItems[random.Next(0, carGeneratorItems.Count)];
            while(counter < 100) {
                var foundVeh = ChoiceVAPI.GetAllVehicles().FirstOrDefault(v => v.Position.Distance(carGeneratorItem.Position) < 10);
                if(foundVeh == null) {
                    var foundPlayer = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.Position.Distance(carGeneratorItem.Position) < 30);

                    if(foundPlayer == null) {
                        break;
                    }
                }

                counter++;
                carGeneratorItem = carGeneratorItems[random.Next(0, carGeneratorItems.Count)];
            }

            //Load the vehicle model to spawn
            var model = "";
            if(carGeneratorItem.CarModel != "") {
                model = carGeneratorItem.CarModel;
            } else if(carGeneratorItem.PopGroup != "") {
                model = popGroup[carGeneratorItem.PopGroup][random.Next(0, popGroup[carGeneratorItem.PopGroup].Length - 1)];
            } else {
                model = popGroup["none"][random.Next(0, popGroup["none"].Length - 1)];
            }

            spawnRandomlyParkedVehicle(carGeneratorItem, model, (byte)random.Next(0, 159));
        }

        public ChoiceVVehicle createRandomParkedVehicle(string model, byte color) {
            var random = new Random();

            var counter = 0;
            var carGeneratorItem = carGeneratorItems[random.Next(0, carGeneratorItems.Count)];
            while(counter < 1000) {
                counter++;
                if(carGeneratorItem.PopGroup != "" && carGeneratorItem.CarModel != "") {
                    var foundVeh = ChoiceVAPI.GetAllVehicles().FirstOrDefault(v => v.Position.Distance(carGeneratorItem.Position) < 10);
                    if(foundVeh == null) {
                        var foundPlayer = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.Position.Distance(carGeneratorItem.Position) < 30);

                        if(foundPlayer == null) {
                            break;
                        }
                    }
                }

                carGeneratorItem = carGeneratorItems[random.Next(0, carGeneratorItems.Count)];
            }

            return spawnRandomlyParkedVehicle(carGeneratorItem, model, color);
        }

        private ChoiceVVehicle spawnRandomlyParkedVehicle(CarGeneratorItem carGeneratorItem, string model, byte color) {
            ChoiceVVehicle vehicle = null;
            if(model != "") {
                vehicle = VehicleController.createVehicle(
                        ChoiceVAPI.Hash(model),
                        new Position(carGeneratorItem.Position.X, carGeneratorItem.Position.Y, carGeneratorItem.Position.Z + 1),
                        new Rotation(0, 0, 0 -
                            (float)Math.Atan2(
                             Convert.ToDouble(carGeneratorItem.OrientX),
                             Convert.ToDouble(carGeneratorItem.OrientY)
                            )
                        ),
                        Constants.GlobalDimension,
                        color,
                        true
                    );

                if(vehicle != null) {
                    var rand = new Random();
                    if(rand.NextDouble() < 1) {
                        using(var db = new ChoiceVDb()) {
                            var viableModTypes = new List<int> { 0, 1, 2, 3, 8 };
                            var randomSelect = viableModTypes.GetRandomElements(rand.Next(viableModTypes.Count / 2, viableModTypes.Count));

                            var types = db.configvehiclemodtypes.Include(m => m.configvehiclemods).Where(mt => randomSelect.Contains(mt.ModTypeIndex)).ToList();

                            foreach(var type in types) {
                                var dbMods = type.configvehiclemods.Where(m => m.configvehiclemodels_id == vehicle.DbModel.id).ToList();
                                if(dbMods.Count != 0) {
                                    var dbMod = dbMods[rand.Next(1, dbMods.Count)];
                                    vehicle.VehicleTuning.setMod((VehicleModType)type.ModTypeIndex, dbMod.ModIndex);
                                }
                            }

                            if(vehicle.VehicleTuning.anyModsInstalled()) {
                                vehicle.VehicleTuning.ModKit = 1;
                                vehicle.applyVehicleTuning(vehicle.VehicleTuning);
                                VehicleController.saveVehicleTuning(vehicle);
                            }
                        }
                    }
                }
            }

            if(vehicle != null) {
                RandomlySpawnedVehicle.Add(vehicle);
                Logger.logDebug(LogCategory.Vehicle, LogActionType.Created, vehicle, $"Created Parked car with Numberplate: {vehicle.NumberplateText}");

                return vehicle;
            } else {
                return createRandomParkedVehicle(model, color);
            }
        }

        public void createRandomlySpawnedVehicles() {
            var count = RandomlySpawnedVehicle.Count;
            for(var i = 0; i < Config.RandomlySpawnedVehicles - count; i++) {
                createRandomVehicle();
            }
        }
    }
}
