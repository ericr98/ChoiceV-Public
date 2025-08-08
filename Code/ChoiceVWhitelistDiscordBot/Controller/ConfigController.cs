using DiscordBot.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Controller {
    public class ConfigController : BotScript {
        public ConfigController() {

        }

        public async static void setConfigData(string name, string value) {
            using(var db = new WhitelistDb()) {
                var datum = db.whitelist_data.Find(name);

                if(datum != null) {
                    datum.data = value;
                } else {
                    db.whitelist_data.Add(new whitelist_datum {
                        name = name,
                        data = value,
                    });
                }

                await db.SaveChangesAsync();
            }
        }

        public static string getConfigData(string name) {
            using(var db = new WhitelistDb()) {
                var datum = db.whitelist_data.Find(name);

                if(datum != null) {
                    return datum.data;
                } else {
                    return null;
                }
            }
        }

        //public static Dictionary<string, string> getWhitelistData(ulong userId, ulong channelId, List<string> names) {
        //    using(var db = new WhitelistDb()) {
        //        var procedure = db.whitelist_procedures.Include(wp => wp.whitelist_procedure_data).FirstOrDefault(wp => wp.userId == userId && wp.channelId == channelId);

        //        if(procedure != null) {
        //            var 
        //            return datum.data;
        //        } else {
        //            return null;
        //        }
        //    }
        //}
    }
}
