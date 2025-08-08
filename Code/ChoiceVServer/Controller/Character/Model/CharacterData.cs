using AltV.Net.Data;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.CallbackController;

namespace ChoiceVServer.Controller.Character {

    public enum CharacterType {
        Player = 0,
        Dog = 1,
        Cat = 2,
    }
    public enum CharacterDrugType : short {
        None = 0,
        Alcohol = 1,
        Mariuahna = 2,
        Cocaine = 3,
    }

    public enum DeathTypes {
        None = 0,
        Concussion = 1,
        Injured = 2
    }

    [Flags]
    public enum CharacterFlag {
        None = 0,
        CrimeFlag = 1,
    }

    public class CharacterData {
        public CharacterType CharacterType;
        public bool AdminMode = false;

        public bool PermadeathActivated;

        public string Title;
        public string FirstName;
        public string MiddleNames;
        public string LastName;
        public char Gender;

        public string SocialSecurityNumber;

        public DateOnly DateOfBirth;

        public int Health;
        public double Hunger;
        public double Thirst;
        public double Energy;

        public DeathTypes State;
        public DateTime StatusUpdateDate = DateTime.MinValue;

        public CharacterDrugType CurrentDrugType;
        public int Age;

        public CharacterFlag CharacterFlag;

        public int VoiceVolume;

        public decimal Cash;

        public DateTime CurrentPlayTimeDate;
        public int CurrentPlayTime;

        [JsonIgnore]
        public Position LastPosition;

        [JsonIgnore]
        public WorldGrid LastGrid;

        public characterstyle Style;

        public CharacterDamage CharacterDamage;
        public List<PainEffect> AllPainEffects = new List<PainEffect>();

        public Dictionary<ConsoleKey, List<string>> ChangeKeyMappings = new Dictionary<ConsoleKey, List<string>>();
        public Dictionary<string, ConsoleKey> ChangeMappingsByIdentifier = new Dictionary<string, ConsoleKey>();

        public Dictionary<ConsoleKey, List<charactersetanimation>> KeysToAnimSets = new Dictionary<ConsoleKey, List<charactersetanimation>>();
        public Dictionary<string, List<ConsoleKey>> AnimationSets = new();

        public List<configtattoo> AllTattoos = new List<configtattoo>();

        public Islands Island;

        [JsonIgnore]
        public Dictionary<string, string> Settings;

        [JsonIgnore]
        public Dictionary<string, PlayerDrugLevel> DrugLevels = new();

        public Materials Material;

        public bool IsCrouching;
    }
}
