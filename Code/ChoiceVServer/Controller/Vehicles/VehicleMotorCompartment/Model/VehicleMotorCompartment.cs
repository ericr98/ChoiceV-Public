using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Vehicles {
    public class VehicleMotorCompartmentMapping {
        public static int VehicleCompartmentIdCounter = 0;

        public int Id { get; private set; }
        public string ImageOrientation { get; private set; }
        public VehicleMotorCompartmentPart Part { get; private set; }
        public List<GameComponentVector> Positions { get; private set; }
        public int Depth { get; private set; }

        public VehicleMotorCompartmentMapping(VehicleMotorCompartmentPart part, int depth, string imageOrientation = "") {
            Id = VehicleCompartmentIdCounter++;
            Part = part;
            Depth = depth;
            ImageOrientation = imageOrientation;

            Positions = new List<GameComponentVector>();
        }

        public void addPosition(GameComponentVector position) {
            Positions.Add(position);
        }
    }

    public class VehicleMotorCompartment {
        public static float DEVELOPMENT_SPEEDUP_MULTIPLIER = Config.IsDevServer ? 1 : 1;

        public string Identifier { get; private set; }
        private List<VehicleMotorCompartmentMapping> Mappings;
        private List<VehicleMotorCompartmentPart> DamageAbleParts;
        private int RowCount;
        private int ColCount;

        //Ordered by Depth
        public VehicleMotorCompartment(string compartmentIdentifier, List<string> blueprints) {
            Identifier = compartmentIdentifier;

            Mappings = new List<VehicleMotorCompartmentMapping>();
            DamageAbleParts = new List<VehicleMotorCompartmentPart>();

            var depthCounter = 0;
            foreach(var depth in blueprints) {
                var rowCounter = 0;
                foreach(var row in depth.Split('\n')) {
                    var colCounter = 0;
                    foreach(var element in row.Split(' ')) {
                        if(element == "") continue;

                        var split = element.Split('_');

                        var partIdentifier = split[0];
                        if(partIdentifier != "NON") {
                            //There already is a entry for this part
                            var already = Mappings.FirstOrDefault(m => m.Part.Identifier == partIdentifier && m.Depth == depthCounter);
                            if(already != null) {
                                //Yes x is up/down and y is left/right, i fucked it up xD
                                already.addPosition(new GameComponentVector(rowCounter, colCounter));
                            } else {
                                var part = VehicleMotorCompartmentController.getCompartmentPart(partIdentifier);
                                if(part != null) {
                                    if(part.isDamageAble()) {
                                        DamageAbleParts.Add(part);
                                    }

                                    var imageOrientation = "";
                                    if(split.Length >= 2) {
                                        imageOrientation = "_" + split[1];
                                    }
                                    var newMapping = new VehicleMotorCompartmentMapping(part, depthCounter, imageOrientation);
                                    newMapping.addPosition(new GameComponentVector(rowCounter, colCounter));
                                    Mappings.Add(newMapping);
                                }
                            }
                        }

                        colCounter++;
                        ColCount = colCounter;
                    }
                    rowCounter++;
                    RowCount = rowCounter;
                }
                depthCounter++;
            }
        }

        public MechanicalGame createAsMinigame(ChoiceVVehicle vehicle, MechanicalGameComponentActionCallback actionCallback) {
            var list = new List<MechanicalGameComponent>();
            foreach(var mapping in Mappings) {
                list.Add(mapping.Part.getComponent(vehicle, mapping));
            }

            return new MechanicalGame(ColCount, RowCount, list, actionCallback);
        }

        public void onVehicleDamage(ChoiceVVehicle vehicle, uint bodyDamage, uint engineDamage) {
            var random = new Random();
            foreach(var part in DamageAbleParts.GetRandomElements(getRandomElementsFromDamage(DamageAbleParts.Count, bodyDamage, engineDamage))) {
                var hullChanged = part.onUpdate(vehicle, VehicleMotorCompartmentUpdateType.Hulldamage, random.Next((int)(bodyDamage / 3), (int)bodyDamage));
                var motorChanged = part.onUpdate(vehicle, VehicleMotorCompartmentUpdateType.Motordamage, random.Next((int)(engineDamage / 3), (int)engineDamage));

                if(hullChanged || motorChanged) {
                    part.onApplyToVehicle(vehicle);
                }
            }
        }

        private int getRandomElementsFromDamage(int initialCount, uint bodyDamage, uint engineDamage) {
            var meanDamage = (float)(bodyDamage + engineDamage) / 2;

            if(meanDamage <= 500) {
                return (int)Math.Floor(meanDamage.Map(0, 500, initialCount / 4, initialCount));
            } else {
                return initialCount;
            }
        }

        public void onVehicleDrive(ChoiceVVehicle vehicle, float kilometers) {
            foreach(var mapping in Mappings) {
                if(mapping.Part.onUpdate(vehicle, VehicleMotorCompartmentUpdateType.KilometerTick, kilometers)) {
                    mapping.Part.onApplyToVehicle(vehicle);
                }
            }
        }

        public void applyMotorDamage(ChoiceVVehicle vehicle, float damage) {
            var motorMapping = Mappings.FirstOrDefault(m => m.Part.Identifier == "MTR");

            if(motorMapping != null) {
                motorMapping.Part.onUpdate(vehicle, VehicleMotorCompartmentUpdateType.Motordamage, damage * 10);
            }
        }

        public void setPartDamageLevel(ChoiceVVehicle vehicle, string identifier, float damageLevel) {
            var partMapping = Mappings.FirstOrDefault(m => m.Part.Identifier == identifier);

            if(partMapping != null) {
                partMapping.Part.setDamageLevel(vehicle, damageLevel);
                partMapping.Part.onApplyToVehicle(vehicle);
            }
        }

        public void applyCompartment(ChoiceVVehicle vehicle) {
            foreach(var mapping in Mappings) {
                mapping.Part.onApplyToVehicle(vehicle);
            }
        }

        public void destroyEverything(ChoiceVVehicle vehicle) {
            foreach(var mapping in Mappings) {
                mapping.Part.setDamageLevel(vehicle, 1);
                mapping.Part.onApplyToVehicle(vehicle);
            }
        }

        public void repairEverything(ChoiceVVehicle vehicle) {
            foreach(var mapping in Mappings) {
                mapping.Part.setDamageLevel(vehicle, 0);
                mapping.Part.onApplyToVehicle(vehicle);
            }
        }

        public List<VehicleMotorCompartmentPart> getCompartmentParts(Predicate<VehicleMotorCompartmentPart> predicate) {
            return DamageAbleParts.Where(p => predicate(p)).ToList();
        }
    }
}
