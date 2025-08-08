using System.Collections.Generic;
using Bogus.DataSets;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public enum CrimeMissionProgressStatus {
        NotStarted,
        InProgress,
        Completed,
        CompletedLate,
        Failed
    }
    
    public  class CrimeMissionProgress {
        public CrimeMissionProgressStatus Status { get; set; }
        public Dictionary<string, string> Data { get; set; }
        
        public CrimeMissionProgress() {
            Status = CrimeMissionProgressStatus.NotStarted;
            Data = [];
        }
        
        public bool has(string key) {
            return Data.ContainsKey(key);
        }
        
        public string get(string key) {
            return Data.GetValueOrDefault(key);
        }
        
        public void set(string key, string value) {
            Data[key] = value;
        }
    }
}