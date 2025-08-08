using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.PrisonSystem.Model {
    public class PrisonGetOutPoint {
        public Prison Prison;

        public CollisionShape FromShape;
        public Position ToPos;
        public string Message;
        public bool IsFinalPoint;

        public PrisonGetOutPoint(Prison prison, CollisionShape fromShape, Position toPos, string message, bool isFinalPoint) {
            Prison = prison;
            FromShape = fromShape;
            ToPos = toPos;
            Message = message;

            FromShape.OnEntityEnterShape += onPlayerEnterShape;
            IsFinalPoint = isFinalPoint;
        }

        private void onPlayerEnterShape(CollisionShape shape, IEntity entity) {
            var player = entity as IPlayer;
           
            if(Prison.isPlayerInPrison(player)) {
                var inmate = Prison.getInmateForPlayer(player);

                if(inmate.IsReleased) {
                    player.sendNotification(Constants.NotifactionTypes.Info, "Du bist kurz davor durch die Tür gelassen zu werden. Bleibe 5sek stehen", "Gefängnisdurchlass angefragt", Constants.NotifactionImages.Prison);
                    InvokeController.AddTimedInvoke($"PrisonGetOutPoint-{player}", (i) => {
                        if(FromShape.IsInShape(player.Position)) {
                            player.Position = ToPos;
                            if(Message != null && Message.Length > 0) {
                                player.sendNotification(Constants.NotifactionTypes.Info, Message, "Durch Ausgang gelassen", Constants.NotifactionImages.Prison);
                            }

                            if(IsFinalPoint) {
                                var (worked, message) = Prison.tryDeleteInmate(inmate);
                                if(worked) {
                                    player.sendNotification(Constants.NotifactionTypes.Success, "Du hast den letzten Ausgangspunkt erreicht und bist jetzt aus dem Gefängnis raus. Du brauchst nie wieder zurückzusehen!", "Freiheit", Constants.NotifactionImages.Prison);
                                } else {
                                    player.sendNotification(Constants.NotifactionTypes.Warning, $"Du bist zwar aus dem Gefängnis raus, aber keine Insassen-akte ist noch nicht gelöscht. Melde dich bei den Wärtern. {message}", "Freiheit?", Constants.NotifactionImages.Prison);
                                }
                            }
                        }
                    }, TimeSpan.FromSeconds(5), false);
                }
            }
        }

        public string toShortSave() {
            return $"{FromShape.toShortSave()}|{ToPos.X}|{ToPos.Y}|{ToPos.Z}|{Message}|{IsFinalPoint}";
        }
    }
}
