using AltV.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Base {

    /// <summary>
    /// This is a real script which binds to RAGEMP Server. Restricted Use
    /// There should be only one RealScript in the Gamemode
    /// </summary>
    public abstract class RealScript : Resource {

        protected static T GetArg<T>(object[] args, int index, T defaultValue = default(T)) {
            if((args == null) || (index >= args.Length))
                return defaultValue;
            try {
                T tmp = (T)Convert.ChangeType(args[index], typeof(T), CultureInfo.InvariantCulture);
                return tmp;
            } catch { return defaultValue; }

        }

        protected static T GetArg<T>(IEnumerable<object> args, int index, T defaultValue = default(T)) {
            var tmpList = args.ToList();
            if((args == null) || (index >= tmpList.Count))
                return defaultValue;
            try {
                T tmp = (T)Convert.ChangeType(tmpList[index], typeof(T), CultureInfo.InvariantCulture);
                return tmp;
            } catch { return defaultValue; }
        }
    }

    /// <summary>
    /// This is a GameMode-Internal script which will be called by the RealScript
    /// </summary>
    public abstract class ChoiceVScript {
        protected static T GetArg<T>(object[] args, int index, T defaultValue = default(T)) {
            if((args == null) || (index >= args.Length))
                return defaultValue;
            try {
                T tmp = (T)Convert.ChangeType(args[index], typeof(T), CultureInfo.InvariantCulture);
                return tmp;
            } catch { return defaultValue; }

        }

        protected static T GetArg<T>(IEnumerable<object> args, int index, T defaultValue = default(T)) {
            var tmpList = args.ToList();
            if((args == null) || (index >= tmpList.Count))
                return defaultValue;
            try {
                T tmp = (T)Convert.ChangeType(tmpList[index], typeof(T), CultureInfo.InvariantCulture);
                return tmp;
            } catch { return defaultValue; }
        }
    }
}
