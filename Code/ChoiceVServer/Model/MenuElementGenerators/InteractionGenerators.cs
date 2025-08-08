using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public delegate MenuItem GeneratedInteractionMenuItemDelegate(IEntity sender, IEntity target);
    public delegate Menu GeneratedInteractionMenuDelegate(IEntity sender, IEntity target);

    public interface InteractionMenuElement {
        public bool OnBusy { get; set; }
        public List<CharacterType> ShowForTypes { get; set; }


        public bool checkShow(IEntity entity, IEntity target, bool playerBusy);

        public MenuElement getMenuElement(IEntity sender, IEntity target);
    }

    public class UnconditionalPlayerInteractionMenuElement : InteractionMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }
        
        private GeneratedInteractionMenuItemDelegate MenuElementGenerator;
        private GeneratedInteractionMenuDelegate MenuGenerator;

        private string Name;

        public bool OnBusy { get; set; }

        public UnconditionalPlayerInteractionMenuElement(GeneratedInteractionMenuItemDelegate menuElementGenerator, bool onBusy = false) {
            MenuElementGenerator = menuElementGenerator;
            OnBusy = onBusy;
        }

        public UnconditionalPlayerInteractionMenuElement(string name, GeneratedInteractionMenuDelegate menuGenerator, bool onBusy = false) {
            MenuGenerator = menuGenerator;
            OnBusy = onBusy;

            Name = name;
        }

        public bool checkShow(IEntity entity, IEntity target, bool playerBusy) {
            return OnBusy || !playerBusy;
        }

        public MenuElement getMenuElement(IEntity entity, IEntity target) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke(entity, target);
            } else {
                return new VirtualMenu(Name, () => MenuGenerator.Invoke(entity, target));
            }
        }
    }

    public class ConditionalPlayerInteractionMenuElement : InteractionMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }
        
        private GenericMenuItemGenerator MenuElementGenerator;
        private GenericMenuGenerator MenuGenerator;
        private Predicate<IEntity> SenderPredicate { get; set; }
        private Predicate<IEntity> TargetPredicate { get; set; }

        private Predicate<Tuple<IEntity, IEntity>> CombinedPredicate { get; set; }

        public bool OnBusy { get; set; }

        public bool OrPredicate { get; set; }

        private string Name;
        private MenuItemStyle Style;

        /// <summary>
        /// A InteractionElement which can be specified to show, only when a conditon is met
        /// If MenuElement in MenuItem, then a custom Data Tag with the target will be added to Data!
        /// BaseType = data["InteractionTargetBaseType"], TargetId = data["InteractionTargetId"]
        /// </summary>
        /// <param name="senderPredicate">The Predicate for the sender. If only target predicate is required set to p => true</param>
        /// <param name="targetPredicate">The Predicate for the target. If only sender predicate is required set to p => true</param>
        /// <param name="onBusy">Item is also shown when the player is busy</param>
        public ConditionalPlayerInteractionMenuElement(GenericMenuItemGenerator menuElementGenerator, Predicate<IEntity> senderPredicate, Predicate<IEntity> targetPredicate, bool onBusy = false, bool orPredicate = false) {
            MenuElementGenerator = menuElementGenerator;
            SenderPredicate = senderPredicate;
            TargetPredicate = targetPredicate;

            OnBusy = onBusy;
            OrPredicate = orPredicate;
        }

        public ConditionalPlayerInteractionMenuElement(string name, GenericMenuGenerator menuGenerator, Predicate<IEntity> senderPredicate, Predicate<IEntity> targetPredicate, bool onBusy = false, bool orPredicate = false, MenuItemStyle style = MenuItemStyle.normal) {
            MenuGenerator = menuGenerator;
            SenderPredicate = senderPredicate;
            TargetPredicate = targetPredicate;

            OnBusy = onBusy;
            OrPredicate = orPredicate;

            Name = name;
            Style = style;
        }


        /// <summary>
        /// A InteractionElement which can be specified to show, only when a conditon is met
        /// If MenuElement in MenuItem, then a custom Data Tag with the target will be added to Data!
        /// BaseType = data["InteractionTargetBaseType"], TargetId = data["InteractionTargetId"]
        /// </summary>
        /// <param name="name"></param>
        /// <param name="menuGenerator"></param>
        /// <param name="combinedPredicate">The order of the Entities is: Sender, Target</param>
        /// <param name="onBusy"></param>
        /// <param name="orPredicate"></param>
        /// <param name="style"></param>
        public ConditionalPlayerInteractionMenuElement(GenericMenuItemGenerator menuElementGenerator, Predicate<Tuple<IEntity, IEntity>> combinedPredicate, bool onBusy = false) {
            MenuElementGenerator = menuElementGenerator;
            CombinedPredicate = combinedPredicate;

            OnBusy = onBusy;
        }

        public ConditionalPlayerInteractionMenuElement(string name, GenericMenuGenerator menuGenerator, Predicate<Tuple<IEntity, IEntity>> combinedPredicate, bool onBusy = false, MenuItemStyle style = MenuItemStyle.normal) {
            MenuGenerator = menuGenerator;
            CombinedPredicate = combinedPredicate;

            OnBusy = onBusy;

            Name = name;
            Style = style;
        }


        public bool checkShow(IEntity entity, IEntity target, bool playerBusy) {
            if(OrPredicate) {
                return (OnBusy || !playerBusy) && (SenderPredicate(entity) || TargetPredicate(target));
            } else {
                if(CombinedPredicate != null) {
                    return (OnBusy || !playerBusy) && CombinedPredicate(new Tuple<IEntity, IEntity>(entity, target));
                } else {
                    return (OnBusy || !playerBusy) && SenderPredicate(entity) && TargetPredicate(target);
                }
            }
        }

        public MenuElement getMenuElement(IEntity entity, IEntity target) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke();
            } else {
                return new VirtualMenu(Name, MenuGenerator);
            }
        }
    }

    public class ConditionalGeneratedPlayerInteractionMenuElement : InteractionMenuElement {
        public List<CharacterType> ShowForTypes { get; set; }
        
        public GeneratedInteractionMenuItemDelegate MenuElementGenerator { get; private set; }
        public GeneratedInteractionMenuDelegate MenuGenerator { get; private set; }
        private Predicate<IEntity> SenderPredicate { get; set; }
        private Predicate<IEntity> TargetPredicate { get; set; }
        public bool OnBusy { get; set; }

        private string Name;
        private MenuItemStyle Style;

        /// <summary>
        /// A InteractionElement which can be specified to show, only when a conditon is met
        /// If MenuElement in MenuItem, then a custom Data Tag with the target will be added to Data!
        /// BaseType = data["InteractionTargetBaseType"], TargetId = data["InteractionTargetId"], Target = data["InteractionTarget"]
        /// </summary>
        /// <param name="senderPredicate">The Predicate for the sender. If only target predicate is required set to p => true</param>
        /// <param name="targetPredicate">The Predicate for the target. If only sender predicate is required set to p => true</param>
        /// <param name="onBusy">Item is also shown when the player is busy</param>
        public ConditionalGeneratedPlayerInteractionMenuElement(GeneratedInteractionMenuItemDelegate generator, Predicate<IEntity> senderPredicate, Predicate<IEntity> targetPredicate, bool onBusy = false) {
            MenuElementGenerator = generator;
            SenderPredicate = senderPredicate;
            TargetPredicate = targetPredicate;

            OnBusy = onBusy;
        }

        public ConditionalGeneratedPlayerInteractionMenuElement(string name, GeneratedInteractionMenuDelegate generator, Predicate<IEntity> senderPredicate, Predicate<IEntity> targetPredicate, bool onBusy = false, MenuItemStyle style = MenuItemStyle.normal) {
            MenuGenerator = generator;
            SenderPredicate = senderPredicate;
            TargetPredicate = targetPredicate;

            OnBusy = onBusy;
            Name = name;
            Style = style;
        }

        public bool checkShow(IEntity entity, IEntity target, bool playerBusy) {
            return SenderPredicate(entity) && TargetPredicate(target) && (OnBusy || !playerBusy);
        }

        public MenuElement getMenuElement(IEntity entity, IEntity target) {
            if(MenuElementGenerator != null) {
                return MenuElementGenerator.Invoke(entity, target);
            } else {
                return new VirtualMenu(Name, () => MenuGenerator.Invoke(entity, target), Style);
            }
        }
    }

    public class ConditionalKeyInteractionCallback {
        private Predicate<IPlayer> Predicate { get; set; }

        public KeyInteractionCallDelegate Callback;

        public ConditionalKeyInteractionCallback(KeyInteractionCallDelegate callback, Predicate<IPlayer> predicate) {
            Callback = callback;
            Predicate = predicate;
        }

        public bool check(IPlayer player) {
            return Predicate(player);
        }
    }
}
