using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.Model.Farming {
    public class Farm {
        public int Id;
        public string Name;
        public int Item;
        //Saves FarmingSpots by the NativePointer of the Checkpoint
        public Dictionary<IntPtr, FarmingSpot> Farmspots = new Dictionary<IntPtr, FarmingSpot>();

        public float NitrogenLevel;
        public float PhosphorusLevel;
        public float PotashLevel;
        public float WaterLevel;

        public bool FertilizingActivated;

        public int ItemMaxRegrow;
        public TimeSpan BestRegrowTime;

        public void farmFertilization(FertilizerTypes fertilizer, float amount) {
            //TODO add specifications to Fertilizertypes
            switch(fertilizer) {
                case FertilizerTypes.Nitrogenous:
                    NitrogenLevel = NitrogenLevel + 0.45f;
                    break;
                case FertilizerTypes.OrganicNitrogenous:
                    NitrogenLevel = NitrogenLevel + 0.35f;
                    break;
                case FertilizerTypes.Phosphate:
                    PhosphorusLevel = PhosphorusLevel + 0.35f;
                    break;
                case FertilizerTypes.Potassic:
                    PotashLevel = PotashLevel + 0.35f;
                    break;
                case FertilizerTypes.Compound:
                    NitrogenLevel = NitrogenLevel + 0.25f;
                    PhosphorusLevel = PhosphorusLevel + 0.25f;
                    break;
                case FertilizerTypes.Complete:
                    NitrogenLevel = NitrogenLevel + 0.3f;
                    PhosphorusLevel = PhosphorusLevel + 0.3f;
                    PotashLevel = PotashLevel + 0.3f;
                    break;
            }
        }

        public void farmWatering(float amount) {
            WaterLevel = WaterLevel + amount;
        }
    }
}