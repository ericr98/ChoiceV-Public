namespace ChoiceVServer.Controller.Model {
    public class TrashZone {
        public string ZoneCollectionIdentifier;
        public string Name;
        public float MinGain;
        public float MaxGain;
        public float FillLevel;
        
        public TrashZone(string zoneCollectionIdentifier, string name, float minGain, float maxGain, float fillLevel) {
            ZoneCollectionIdentifier = zoneCollectionIdentifier; 
            Name = name;
            MinGain = minGain;
            MaxGain = maxGain;
            FillLevel = fillLevel;
        }
    }
}