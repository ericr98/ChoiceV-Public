using AltV.Net.Elements.Entities;
using AltV.Net.Shared.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class DealerNPCModule : NPCModule {
        public bool IsHired => HireCharacterId != -1;

        public int HireCharacterId {
            get => Ped.Data.hasKey("DealerNPCModuleHireCharacterId") ? (int)Ped.Data["DealerNPCModuleHireCharacterId"] : -1; 
            private set => Ped.Data["DealerNPCModuleHireCharacterId"] = value; 
        }
        
        private DateTime HireTime {
            get => Ped.Data.hasKey("DealerNPCModuleHireTime") ? (DateTime)Ped.Data["DealerNPCModuleHireTime"] : DateTime.MinValue;
            set => Ped.Data["DealerNPCModuleHireTime"] = value;
        }

        public Inventory Inventory {
            get => InventoryController.loadInventory((int)Ped.Data["DealerNPCModuleInventory"]);
            private set => Ped.Data["DealerNPCModuleInventory"] = value.Id;
        }
        
        private string ZoneGroup {
            get => Ped.Data.hasKey("DealerNPCModuleZoneGroup") ? (string)Ped.Data["DealerNPCModuleZoneGroup"] : null;
            set => Ped.Data["DealerNPCModuleZoneGroup"] = value;
        }
        
        private decimal Cash {
            get => Ped.Data.hasKey("DealerNPCModuleCash") ? (decimal)Ped.Data["DealerNPCModuleCash"] : 0;
            set => Ped.Data["DealerNPCModuleCash"] = value;
        }

        public DealerNPCModule(ChoiceVPed ped, configcrimedealerzone zone, configcrimedealerposition position) : base(ped) { 
            Inventory = InventoryController.createInventory(ped.Id, 10, InventoryTypes.DealerNPC);
            ZoneGroup = zone.zoneGroup;

            fillInventoryWithRandomDrugs(); 
        }

        private void fillInventoryWithRandomDrugs() {
            var demands = DealerNPCController.getDrugDemandsForZone(ZoneGroup);
            var list = demands.Where(d => d.demand > 0).ToList();
            var rand = new Random();
            using(var db = new ChoiceVDb()) {
                foreach(var demand in list) {
                    var viableOptions = db.configcrimedealerdrugitems.Include(c => c.configItem).Where(d => d.canSpawnInDealer == 1 && d.drugType == (int)demand.type).ToList();
            
                    if (viableOptions.Count == 0) {
                        continue;
                    }
            
                    var item = viableOptions[rand.Next(0, viableOptions.Count)];
            
                    var items = InventoryController.createItems(item.configItem, rand.Next(2, 7));
                    Inventory.addItems(items);
                }
            }
        }

        public DealerNPCModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) { }

        public override List<MenuItem> getMenuItems(IPlayer player) {
            var list = new List<MenuItem>();
            if (player.isCopOrRobber() && Ped.IsBeingThreatend) {
                list.Add(new ClickMenuItem("Wegschicken", "Vertreibe den Dealer von seiner aktuellen Position", "", "COP_REMOVE_DEALER")
                    .withData(new Dictionary<string, dynamic> {
                        { "Dealer", Ped },
                    })
                    .needsConfirmation("Person wegschicken?", "Die Person wirklich wegschicken?"));
            }

            if(HireCharacterId == player.getCharacterId()) {
                list.Add(new ClickMenuItem("Cash einsammeln", $"Sammele das Cash ein welches der Dealer verdient hat.", $"$ {Cash}", "DEALER_COLLECT_CASH"));
                list.Add(new ClickMenuItem("Inventar öffnen", "Öffne das Inventar des Dealers", "", "OPEN_DEALER_INVENTORY")
                    .withData(new Dictionary<string, dynamic> {
                        { "Inventory", Inventory },
                    }));
                
                list.Add(new ClickMenuItem("Verschieben", "Verschiebe den Dealer an eine andere Position", "", "MOVE_DEALER")
                    .withData(new Dictionary<string, dynamic> {
                        { "Dealer", Ped },
                    }));

                list.Add(new ClickMenuItem("Entlassen", "Entlasse den Dealer aus deinem Dienst", "", "FIRE_DEALER"));
            }

            if (player.hasCrimeFlag()) {
                if (!IsHired) {
                    list.Add(new ClickMenuItem("Anheuern", "Heuere den Dealer für deine Dienste an. Über ihn kannst du Drogen in Gebieten verkaufen.", $"${DealerNPCController.DEALER_HIRE_AMOUNT}", "HIRE_DEALER")
                        .withData(new Dictionary<string, dynamic> {
                            { "Module", this }
                        })
                        .needsConfirmation("Dealer anheuern?", $"Möchtest du den Dealer für ${DealerNPCController.DEALER_HIRE_AMOUNT} anheuern?"));
                }
            }

            var shopMenu = new Menu("Waren kaufen", "Welche Waren möchtest du kaufen?"); 
            shopMenu.addMenuItem(new StaticMenuItem("Bargeld", $"Du hast ${player.getCash()}", $"${player.getCash()}"));
            foreach(var item in Inventory.Items) {
                var price = DealerNPCController.getPriceForItem(item, true); 
                if(price != null) {
                    shopMenu.addMenuItem(new InputMenuItem($"{item.StackAmount}x {item.Name}", $"Kaufe {item.Name} für {price} $", $"${price}", "ON_DEALER_BUY_ITEM").withData(new Dictionary<string, dynamic> {
                        { "Item", item }
                    }));
                }
            } 
            list.Add(new MenuMenuItem(shopMenu.Name, shopMenu));

            return list;
        }
        
        public void setHireCharacter(IPlayer player) {
            HireCharacterId = player.getCharacterId();
            HireTime = DateTime.Now;
        }

        public void clearBelongings() {
            Cash = 0;
            Inventory.clearAllItems();
        }
        
        internal bool canBeAutomaticallyRemoved() {
            return !IsHired;
        }

        internal void update() {
            if(HireTime.AddHours(DealerNPCController.DEALER_HIRE_HOURS) < DateTime.Now) {
                DealerNPCController.onHiredDealerTimeUp(this);
            }
            
            if(HireCharacterId == -1 && Inventory?.Items.Sum(i => i.StackAmount ?? 1) < 2) {
                fillInventoryWithRandomDrugs();
            } 
        }

        internal List<(DrugType, int)> getAvailableDrugs() {
            var list = new List<(DrugType, int)>();
            foreach(var item in Inventory?.Items ?? new List<Item>()) {
                if(item is not DrugItem drugItem) {
                    continue;
                }
                
                var drugType = DealerNPCController.getDrugTypeForItem(drugItem);
                if(drugType != null) {
                   if(list.Any(d => d.Item1 == (DrugType)drugType)) {
                       var value = list.First(d => d.Item1 == (DrugType)drugType);
                       value.Item2 += item.StackAmount ?? 1;
                   } else {
                       list.Add(((DrugType)drugType, item.StackAmount ?? 1));
                   } 
                }
            }
            
            return list;
        }

        internal List<(DrugType, int)> sellProduct(double optimalDist, double actualDist, int zoneMaxSupportedDealer, List<(DrugType, int)> drugDemands, Dictionary<DrugType, int> zoneSellingDealers) {
           var priceMultiplier = Convert.ToDecimal(Math.Max(0.5, Math.Min(1, actualDist / optimalDist))); 
         
           var drugSellList = new List<(DrugType, int)>();
           // Assumes that the items are stackable
           foreach(var item in Inventory?.Items.Reverse<Item>() ?? []) {
               if(item is not DrugItem drugItem) {
                   continue;
               }
               
               var price = DealerNPCController.getPriceForItem(item, false);
               var drugType = DealerNPCController.getDrugTypeForItem(drugItem);
               if(price != null && drugType != null) {
                   var finalPrice = (int)(price * priceMultiplier);
                   
                   if(drugDemands.All(d => d.Item1 != drugType)) {
                       continue;
                   }
                   
                   var demand = drugDemands.FirstOrDefault(d => d.Item1 == drugType).Item2;
                   if(demand != 0) {
                      var sellingDealers = zoneSellingDealers[(DrugType)drugType]; 
                      var maxAmountPerDealer = Math.Max(1, demand / zoneMaxSupportedDealer);
                      var amount = Math.Min(maxAmountPerDealer, Math.Min(item.StackAmount ?? 1, Math.Max(1, demand / sellingDealers)));

                      if(Inventory.removeItem(drugItem, amount)) {
                          //TODO Also sometimes generate Jewelry 
                          var money = finalPrice * amount;
                          Cash += money;
                          
                          drugSellList.Add(((DrugType)drugType, amount));
                          Logger.logDebug(LogCategory.System, LogActionType.Event, $"Dealer {Ped.Id} sold {amount}x {drugItem.Name} for {money} $");
                      }
                   }
               }
           }

           return drugSellList;
        }

        public override void onRemove() {
            InventoryController.destroyInventory(Inventory);
        }

        public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
            return new StaticMenuItem("Dealer NPC Modul", "Ein Ped mit diesem Modul ist ein Dealer", "");
        }
    }
}