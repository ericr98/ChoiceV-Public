using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using ChoiceVServer.Controller.DamageSystem;

namespace ChoiceVServer.InventorySystem {
    public class Stretcher : Item {
        public Stretcher(item item) : base(item) { }

        //Constructor for generic generation
        public Stretcher(configitem configItem, int amount, int quality) : base(configItem, quality, amount) { }
    }
}
