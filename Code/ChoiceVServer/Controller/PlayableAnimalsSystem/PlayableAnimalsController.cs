using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChoiceVServer.Controller.PlayableAnimalsSystem {
    public class PlayableAnimalsController : ChoiceVScript {
        private static Dictionary<string, AnimalTalkCategory> ANIMAL_TALK_CATEGORIES = new();
        private static Dictionary<string, string> WORDS_TO_SOUNDS = new();
        
        private const int NUMBER_OF_GENERIC_SOUNDS = 5;
        
        public PlayableAnimalsController() {
            EventController.addKeyEvent("ANIMAL_TALK_MENU", (ConsoleKey)18, "Tiersprache (de)aktivieren", onToogleAnimalTalk, true, true);
            EventController.addCefEvent("ANIMAL_TALK_SELECT", onAnimalTalkSelect);

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;
            
            using(var db = new ChoiceVDb()) {
                foreach(var word in db.configanimaltalkwords) {
                    if(!ANIMAL_TALK_CATEGORIES.TryGetValue(word.category, out var cat)) {
                        ANIMAL_TALK_CATEGORIES[word.category] = new AnimalTalkCategory(word.category, [word.word]);
                    } else {
                        cat.words.Add(word.word);
                    }
                    
                    if(word.soundIdentifier == null) {
                        WORDS_TO_SOUNDS[word.word] = "TALK_" + ((word.word.ToCharArray().Sum(x => x) % NUMBER_OF_GENERIC_SOUNDS) + 1);
                    } else {
                        WORDS_TO_SOUNDS[word.word] = word.soundIdentifier;
                    }
                }
            }
        }

        public record AnimalTalkCategory(string name, List<string> words);
        public class AnimalTalkCefEvent : IPlayerCefEvent {
            public string Event { get;  }

            public AnimalTalkCategory[] categories;
            
            public AnimalTalkCefEvent(AnimalTalkCategory[] categories) {
                Event = "OPEN_ANIMAL_TALK";
                this.categories = categories;
            }
        }
        
        private void onPlayerConnect(IPlayer player, character character) {
            if(player.getCharacterType() != CharacterType.Player) {
                
                player.emitCefEventNoBlock(new AnimalTalkCefEvent(ANIMAL_TALK_CATEGORIES.Values.ToArray()));
            }
        }
        
        public class AnimalTalkFocusCefEvent : IPlayerCefEvent {
            public string Event { get;  }
            public bool focus;
            
            public AnimalTalkFocusCefEvent(bool focus) {
                Event = "ANIMAL_TALK_FOCUS_TOGGLE";
                this.focus = focus;
            }
        }
        
        private bool onToogleAnimalTalk(IPlayer player, ConsoleKey key, string eventname) {
            if(player.getCharacterType() == CharacterType.Player) {
                return false;
            }
            
            if(player.hasData("ANIMAL_TALK")) {
                player.setData("ANIMAL_TALK", !(bool)player.getData("ANIMAL_TALK"));
            } else {
                player.setData("ANIMAL_TALK", true);
            } 
           
            var focus = (bool)player.getData("ANIMAL_TALK");

            if(focus) {
                player.emitCefEventWithBlock(new AnimalTalkFocusCefEvent(true), "ANIMAL_TALK_FOCUS");
            } else {
                player.emitCefEventNoBlock(new AnimalTalkFocusCefEvent(false));
                WebController.setMovementBlockForCef(player, "ANIMAL_TALK_FOCUS", false);
            }

            return true;
        }

        public class AnimalTalkSelectEvent {
            public string[] selection;
        }
        
        private void onAnimalTalkSelect(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var data = evt.Data.FromJson<AnimalTalkSelectEvent>();
            
            if(data.selection.Length == 0) {
                return;
            } 
            
            var random = new Random();
            foreach(var word in data.selection) {
                if(WORDS_TO_SOUNDS.TryGetValue(word, out var soundId)) {
                    // TODO Filter the correct sound from the set of sounds (Add Gameplay stuff)
                    SoundController.playSoundAtCoords(player.Position, 7.5f, "HUSKY_" + soundId);
                    player.playAnimation("creatures@rottweiler@amb@world_dog_barking@idle_a", "idle_a", 100000, 1);
                }
                Thread.Sleep(random.Next(500, 1500));
            }

            player.stopAnimation();
        }
    }
}