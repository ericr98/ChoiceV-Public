using System;
using System.Collections.Generic;
using System.Text;

namespace ChoiceVServer.Model {
    public class Config {
        public static string DatabaseIp { get; set; }
        public static string DatabasePort { get; set; }
        public static string DatabaseDatabase { get; set; }
        public static string DatabaseUser { get; set; }
        public static string DatabasePassword { get; set; }
        public static string FVSDatabaseIp { get; set; }
        public static string FVSDatabasePort { get; set; }
        public static string FVSDatabaseDatabase { get; set; }
        public static string FVSDatabaseUser { get; set; }
        public static string FVSDatabasePassword { get; set; }
    }
}
