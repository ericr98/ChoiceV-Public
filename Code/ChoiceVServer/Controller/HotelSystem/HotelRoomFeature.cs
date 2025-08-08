using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.HotelSystem {
    public class HotelRoomFeature : IDisposable {
        public int Id { get; private set; }
        private CollisionShape CollisionShape;

        public string Name { get; private set; }

        private List<HotelRoomFeatureInstance> Instances;

        public string CodeInstance { get; private set; }

        public Dictionary<string, string> Settings;

        public HotelRoomFeature(confighotelroomfeature dbFeature) {
            Id = dbFeature.id;
            Name = dbFeature.name;
            CodeInstance = dbFeature.codeInstance;

            Instances = new List<HotelRoomFeatureInstance>();

            Settings = dbFeature.settings.FromJson<Dictionary<string, string>>();

            CollisionShape = CollisionShape.Create(dbFeature.colPos.FromJson(), dbFeature.colWidth, dbFeature.colHeight, dbFeature.colRotation, true, false, true);
            CollisionShape.OnCollisionShapeInteraction += onShapeInteraction;
        }

        public HotelRoomFeature(int id, string name, CollisionShape collisionShape, string codeInstance) {
            Id = id;
            Name = name;
            CollisionShape = collisionShape;


            CodeInstance = codeInstance;
            Instances = new List<HotelRoomFeatureInstance>();
            CollisionShape.OnCollisionShapeInteraction += onShapeInteraction;
        }

        public void addInstance(HotelRoomFeatureInstance instance) {
            Instances.Add(instance);
        }

        public bool removeInstance(HotelRoomFeatureInstance instance) {
            return Instances.Remove(instance);
        }

        private bool onShapeInteraction(IPlayer player) {
            var toInvoke = Instances.FirstOrDefault(f => f.Owner.Dimension == player.Dimension);
            if(toInvoke != null) {
                toInvoke.onInteraction(player);

                return true;
            } else {
                return false;
            }
        }

        public static Menu getCreateMenu(string displayName, string displayInfo, Type type, HotelRoom room) {
            var menu = new Menu(displayName, displayInfo);

            menu.addMenuItem(new InputMenuItem("Name", "Gib den Namen des Features ein", "z.B. Regal", ""));

            if(type == typeof(HotelRoomStorageFeatureInstance)) {
                menu.addMenuItem(new InputMenuItem("Lager-Kapazität (kg)", "Gib an, wieviel kg in dem Spot gelagert werden kann", "z.B. 30.5", ""));
            }

            menu.addMenuItem(new MenuStatsMenuItem("Bestätigen", "Bestätige das Feature", "SUPPORT_HOTEL_CREATE_FEATURE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Type", type }, { "Room", room } }).needsConfirmation("Feature erstellen?", "Feature wirklich erstellen?"));

            return menu;
        }

        public static Dictionary<string, string> getCreateSettings(Type type, MenuStatsMenuItemEvent evt) {
            if(type == typeof(HotelRoomStorageFeatureInstance)) {
                var sizeEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var size = 0f;
                try {
                    size = float.Parse(sizeEvt.input);
                } catch(Exception) { }

                return new Dictionary<string, string> { { "InventorySize", size.ToString() } };
            } else {
                return null;
            }
        }

        public void Dispose() {
            CollisionShape.Dispose();
            Instances.Clear();
        }
    }

    public delegate void HotelRoomFeatureInstanceDataChange(HotelRoomFeatureInstance instance, string parameter, dynamic value);
    public abstract class HotelRoomFeatureInstance : IDisposable {
        public static HotelRoomFeatureInstanceDataChange HotelRoomFeatureInstanceDataChange;

        public int Id { get; private set; }

        public HotelRoomBooking Owner { get; private set; }
        protected HotelRoomFeature Main;
        protected ExtendedDictionary<string, dynamic> Data;

        //For Reflection
        public HotelRoomFeatureInstance() { }

        public HotelRoomFeatureInstance(hotelroomfeatureinstance dbInstance, HotelRoomFeature main, HotelRoomBooking owner) {
            Id = dbInstance.id;
            Main = main;
            Owner = owner;

            if(dbInstance.hotelroomfeatureinstancesdata != null) {
                var dic = new Dictionary<string, dynamic>();
                foreach(var row in dbInstance.hotelroomfeatureinstancesdata) {
                    dic[row.parameter] = row.value.FromJson<dynamic>();
                }

                Data = new ExtendedDictionary<string, dynamic>(dic);
                Data.OnValueChanged += (key, value) => {
                    if(HotelRoomFeatureInstanceDataChange != null) {
                        HotelRoomFeatureInstanceDataChange.Invoke(this, key, value);
                    }
                };
            }
        }

        public HotelRoomFeatureInstance(int id, HotelRoomFeature main, HotelRoomBooking owner) {
            Id = id;
            Main = main;
            Owner = owner;

            Data = new ExtendedDictionary<string, dynamic>(new Dictionary<string, dynamic>());

            Data.OnValueChanged += (key, value) => {
                if(HotelRoomFeatureInstanceDataChange != null) {
                    HotelRoomFeatureInstanceDataChange.Invoke(this, key, value);
                }
            };
        }

        public abstract void onInteraction(IPlayer player);

        public abstract bool needsToBeSaved();

        public abstract string getDisplayName();
        public abstract string getDisplayInfo();

        public abstract void deleteLate();

        public abstract void onLateInteract(IPlayer player);
        public abstract UseableMenuItem createLateMenuItem();

        public virtual void Dispose() {
            Owner = null;
            Main = null;
            Data = null;
        }
    }

    public class HotelRoomStorageFeatureInstance : HotelRoomFeatureInstance {
        private Inventory Inventory { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }

        public HotelRoomStorageFeatureInstance() : base() { }

        public HotelRoomStorageFeatureInstance(hotelroomfeatureinstance dbInstance, HotelRoomFeature main, HotelRoomBooking owner) : base(dbInstance, main, owner) { }

        public HotelRoomStorageFeatureInstance(int id, HotelRoomFeature main, HotelRoomBooking owner) : base(id, main, owner) {
            Inventory = InventoryController.createInventory(Id, float.Parse(main.Settings["InventorySize"]), InventoryTypes.Storage);
        }

        public override void onInteraction(IPlayer player) {
            InventoryController.showMoveInventory(player, player.getInventory(), Inventory, null, null, Main.Name, true);
        }

        public override bool needsToBeSaved() {
            try {
                if(Inventory == null) {
                    return false;
                }
                return Inventory.getCount() > 0;
            } catch(Exception) {
                return false;
            }
        }

        public override string getDisplayName() {
            return "Lager-Feature";
        }

        public override string getDisplayInfo() {
            return "Lagere Items im Raum";
        }

        public override void onLateInteract(IPlayer player) {
            var already = Inventory.BlockStatements.FirstOrDefault(b => b.Owner.Equals(Id));
            if(already == null) {
                Inventory.BlockStatements.Add(new InventoryAddBlockStatement(Id, i => true));
            }
            InventoryController.showMoveInventory(player, player.getInventory(), Inventory, null, null, "Lager-Feature", true);
        }
        public override UseableMenuItem createLateMenuItem() {
            return new ClickMenuItem("Lager-Feature", "Gelagerte Items im Raum", "", "HOTEL_OPEN_LATE_FEATURE");
        }

        public override void Dispose() {
            if(!needsToBeSaved()) {
                InventoryController.destroyInventory(Inventory);
            }

            base.Dispose();
        }

        public override void deleteLate() {
            if(Data.Items.ContainsKey("InventoryId")) {
                InventoryController.destroyInventory(Inventory);
            }
        }
    }

    public class HotelRoomBathFeatureInstance : HotelRoomFeatureInstance {
        public HotelRoomBathFeatureInstance() : base() { }

        public HotelRoomBathFeatureInstance(hotelroomfeatureinstance dbInstance, HotelRoomFeature main, HotelRoomBooking owner) : base(dbInstance, main, owner) { }

        public HotelRoomBathFeatureInstance(int id, HotelRoomFeature main, HotelRoomBooking owner) : base(id, main, owner) {

        }

        public override void onInteraction(IPlayer player) {
            FaceFeatureController.openHygineMenu(player);
        }

        public override bool needsToBeSaved() {
            return false;
        }

        public override string getDisplayName() {
            return "sauberes Bad-Feature";
        }

        public override string getDisplayInfo() {
            return "Nutze ein voll ausgestattetes Bad";
        }

        public override void onLateInteract(IPlayer player) {
            //No late save
        }

        public override UseableMenuItem createLateMenuItem() {
            return null;
        }

        public override void Dispose() {
            base.Dispose();
        }

        public override void deleteLate() {
            //No late save
        }
    }
}
