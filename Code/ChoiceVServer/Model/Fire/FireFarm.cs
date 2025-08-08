using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;

namespace ChoiceVServer.Model.Fire {
    public class FireFarm : FireObject {
        // particleName = "ent_ray_meth_fires";                 // Forest
        // particleName = "ent_ray_ch2_farm_fire_u_l";          // Oil
        // particleName = "ent_ray_heli_aprtmnt_h_fire";        // Farm
        // particleName = "ent_amb_fire_gaswork";               // Gas
        // particleName = "fire_wrecked_plane_cockpit";         // Apartment
        // particleName = "ent_dst_elec_fire_sp";               // Electric
        // particleName = "ent_amb_fbi_fire_beam";              // Smoke
        // particleName = "fire_petrol_one";                    // Small
        // particleName = "fire_wrecked_tank";                  // Petrol
        // particleName = "fire_wrecked_heli_cockpit";          // Trash
        // particleName = "ent_amb_beach_campfire";             // Campfire
        // particleName = "ent_amb_barrel_fire";                // Barrel
        // particleName = "ent_amb_fbi_fire_wall_lg";           // Firewall

        // smokeName = "ent_amb_smoke_gaswork";                 // Light white smoke
        // smokeName = "exp_grd_bzgas_smoke";                   // Medium white smoke
        // smokeName = "exp_grd_grenade_smoke";                 // Thick gray smoke
        // smokeName = "ent_amb_smoke_general";                 // Heavy gray smoke
        // smokeName = "ent_amb_fbi_smoke_linger_hvy";          // Heavy black smoke
        // smokeName = "ent_amb_smoke_scrap";                   // Medium black smoke
        // smokeName = "ent_amb_smoke_foundry";                 // Heavy black smoke (higher ground)
        // smokeName = "ent_amb_smoke_foundry_white";           // Heavy gray smoke (higher ground)
        // smokeName = "ent_amb_fbi_smoke_fogball";             // Foggy white smoke

        public FireFarm(float firesize, int firecount, int childdelay, int childspread, string eventrundate, string eventrundays, string eventruntime, int eventrunplayers, int eventrundepartment, DateTime eventdate, int eventtimespan, int eventrandomtime, int eventmaxruntime, int eventactive, int explosionpossible, int explosionprobability, Position position, Rotation rotation, float width, float height) : base(firesize, firecount, childdelay, childspread, eventrundate, eventrundays, eventruntime, eventrunplayers, eventrundepartment, eventdate, eventtimespan, eventrandomtime, eventmaxruntime, eventactive, explosionpossible, explosionprobability, position, rotation, width, height) {
            fireModel = "Farm";
            particleName = "ent_ray_heli_aprtmnt_h_fire";
            smokeName = "ent_amb_smoke_scrap";
            explosionType = 0;
        }

        public override void initialize(bool register = true) {
            base.initialize(register);
        }

        public override void onFireEnterShape(CollisionShape shape, IEntity entity) {
            base.onFireEnterShape(shape, entity);
        }

        public override void onFireExitShape(CollisionShape shape, IEntity entity) {
            base.onFireExitShape(shape, entity);
        }

        public override void onInterval() {
            base.onInterval();
        }

        public override bool onExtinguishFire(IPlayer player, string eventName, object[] args) {
            return base.onExtinguishFire(player, eventName, args);
        }

        public override bool onExtinguishedFire(IPlayer player, string eventName, object[] args) {
            return base.onExtinguishedFire(player, eventName, args);
        }
    }
}
