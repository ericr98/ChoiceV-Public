using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiscordBot {
    public abstract class BotScript {
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
