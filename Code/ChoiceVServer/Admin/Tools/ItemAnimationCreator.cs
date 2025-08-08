using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Numerics;

namespace ChoiceVServer.Admin.Tools {

    public class ItemAnimationCreator : ChoiceVScript {

        public static void CommandAnimationPlace(IPlayer player, string name, string model, int bone = 36029, string animDict = "weapons@first_person@aim_idle@generic@pistol@pistol_50@", string animName = "aim_med_loop") {
            if(AnimationPlacer.isEditMode) {
                ChoiceVAPI.SendChatMessageToPlayer(player, "The ItemAnimationCreator ist currently in use!");
                return;
            }

            AnimationPlacer.isEditMode = true;
            if(!player.hasData("just_scale_POS"))
                player.setData("just_scale_POS", 0.01f);
            if(!player.hasData("just_scale_ROT"))
                player.setData("just_scale_ROT", 1.0f);

            AnimationPlacer.pos = new Vector3(0.08f, -0.15f, 0.03f);
            AnimationPlacer.rot = new Vector3(0, 90, 90);
            AnimationPlacer.Bone = bone;
            AnimationPlacer.AnimDict = animDict;
            AnimationPlacer.AnimName = animName;
            AnimationPlacer.ModelHash = model;
            AnimationPlacer.Name = name;
            AnimationPlacer.AnimationObject = ObjectController.createObject(model, player, AnimationPlacer.pos, (Rotation)AnimationPlacer.rot, AnimationPlacer.Bone);
            player.playAnimation(animDict, animName, -1, 1);

            ChoiceVAPI.SendChatMessageToPlayer(player, "ItemAnimationCreator");
            ChoiceVAPI.SendChatMessageToPlayer(player, "Use Numpad to move/rotate object, /* to change scale");
            ChoiceVAPI.SendChatMessageToPlayer(player, "NumPad0 SAVES the object, NumPad Decimal aborts");
            ChoiceVAPI.SendChatMessageToPlayer(player, "Num8/Num2 for Z+/-, Num1/Num2 für Q+/-, Num4/Num6 X+/-, ");
            ChoiceVAPI.SendChatMessageToPlayer(player, "Press NumPad5 to switch between movement and rotation mode");
            player.emitClientEvent("OP_START", "IAP_");
        }
    }

    public class AnimationPlacer : ChoiceVScript {
        public static Vector3 pos, rot;
        public static Controller.Object AnimationObject;
        public static int Bone = 36029;
        public static string Name = "newname";
        public static string ModelHash = "prop_acc_guitar_01_d1";
        public static string AnimDict;
        public static string AnimName;
        public static string curMode = "POS";
        public static bool isEditMode = false;
        public static bool IsPedMode = false;

        public AnimationPlacer() {
            EventController.addEvent("IAP_SAVE", OnSave);
            EventController.addEvent("IAP_ABORT", OnAbort);
            EventController.addEvent("IAP_CMD", OnCommand);
        }

        public bool OnAbort(IPlayer player, string eventName, params object[] args) {
            ObjectController.deleteObject(AnimationObject);

            isEditMode = false;
            player.stopAnimation();
            return true;
        }

        public bool OnSave(IPlayer player, string eventName, params object[] args) {
            using(var db = new ChoiceVDb()) {
                var newItemAnim = new configitemanimation {
                    identifier = Name,
                    modelHash = ModelHash,
                    position = pos.ToJson(),
                    rotation = rot.ToJson(),
                    dict = AnimDict,
                    name = AnimName,
                    bone = Bone,
                    flag = 49,
                    duration = 5000,
                    group = "",
                    shown = 0,
                    showName = "",
                };

                db.configitemanimations.Add(newItemAnim);
                db.SaveChanges();
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"Saved!", "Admin-Info");
            ObjectController.deleteObject(AnimationObject);

            isEditMode = false;
            player.stopAnimation();
            return true;
        }

        public bool OnCommand(IPlayer player, string eventName, params object[] args) {
            var cmd = Convert.ToString(args[0]);
            float scale = player.getData("just_scale_" + curMode);
            switch(cmd) {
                case "POS":
                    curMode = "POS";
                    ChoiceVAPI.SendChatMessageToPlayer(player, "Position mode");
                    break;
                case "ROT":
                    curMode = "ROT";
                    ChoiceVAPI.SendChatMessageToPlayer(player, "Rotation mode");
                    break;
                case "R+":
                    scale += 0.01f;
                    player.setData("just_scale_" + curMode, scale);
                    ChoiceVAPI.SendChatMessageToPlayer(player, $"SCALE: ~g~{scale}");
                    break;
                case "R-":
                    scale -= 0.01f;
                    player.setData("just_scale_" + curMode, scale);
                    ChoiceVAPI.SendChatMessageToPlayer(player, $"SCALE: ~g~{scale}");
                    break;
                case "X+":
                    pos.X += scale;
                    break;
                case "X-":
                    pos.X -= scale;
                    break;
                case "Y+":
                    pos.Y += scale;
                    break;
                case "Y-":
                    pos.Y -= scale;
                    break;
                case "Z+":
                    pos.Z += scale;
                    break;
                case "Z-":
                    pos.Z -= scale;
                    break;
                case "1X+":
                    rot.X += scale;
                    break;
                case "1X-":
                    rot.X -= scale;
                    break;
                case "1Y+":
                    rot.Y += scale;
                    break;
                case "1Y-":
                    rot.Y -= scale;
                    break;
                case "1Z+":
                    rot.Z += scale;
                    break;
                case "1Z-":
                    rot.Z -= scale;
                    break;
                default:
                    break;
            }

            ObjectController.reattachObject(AnimationObject, pos, new DegreeRotation(rot.X, rot.Y, rot.Z), Bone);
            return true;
        }
    }
}
