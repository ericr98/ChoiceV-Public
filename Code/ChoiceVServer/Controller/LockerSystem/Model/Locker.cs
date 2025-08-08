using AltV.Net.Elements.Entities;
using ChoiceVServer.Model.Menu;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Base;

namespace ChoiceVServer.Controller.LockerSystem.Model {
    public class Locker {
        public int Id;

        public string Name;
        public decimal DayPrice;

        public long PhoneNumber;

        public int? CompanyId;
        public Company Company { get => CompanyController.findCompanyById(CompanyId ?? -1); }

        public List<LockerDrawer> Drawers;
        public CollisionShape Shape;

        public Locker(int id, string name, long phoneNumber, decimal dayPrice, int? companyId, CollisionShape shape) {
            Id = id;
            Name = name;
            PhoneNumber = phoneNumber;
            DayPrice = dayPrice;

            CompanyId = companyId;

            Drawers = new List<LockerDrawer>();

            Shape = shape;
            Shape.OnCollisionShapeInteraction += onPlayerInteraction;
        }

        private bool onPlayerInteraction(IPlayer player) {
            var menu = new Menu($"{Name}", "Wähle das Schließfach");

            var count = 0;
            var allFinished = false;

            var l = Drawers;

            while(!allFinished) {
                var subMenu = new Menu($"{count}-{count + 20}", "Wähle das Schließfach");
                var lockers = l.Take(20);
                l = l.Skip(20).ToList();

                if(lockers.Count() < 20) {
                    allFinished = true;
                }

                foreach(var locker in lockers) {
                    var lockerMenu = locker.getShowMenu(player);
                    subMenu.addMenuItem(new MenuMenuItem(lockerMenu.Name, lockerMenu));
                }

                if(subMenu.getMenuItemCount() > 0) {
                    menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
                }

                count += 20 + 1;
            }

            if(Drawers.Any(d => d is PublicLockerDrawer)) {
                menu.addMenuItem(new ClickMenuItem("Überzugslager anzeigen", "GPS Koordinaten des Lagers, in welche Gegenstände von überzogenen Fächern gelagert werden", "", "LOCKER_SHOW_LOCKER_MASTER"));
            }

            player.showMenu(menu);

            return true;
        }

        public void onUpdate() {
            foreach(var drawer in Drawers) {
                drawer.onUpdate();
            }
        }

        public void addLockerDrawer(LockerDrawer drawer) {
            Drawers.Add(drawer);
            Drawers = Drawers.OrderBy(c => c.DisplayNumber).ToList();
        }

        public List<ServerAccessedDrawer> getFreeServerAccessedDrawers() {
            return Drawers.Where(d => d is ServerAccessedDrawer drawer && !drawer.IsActive).Select(drawer => drawer as ServerAccessedDrawer).ToList();
        }
    }
}
