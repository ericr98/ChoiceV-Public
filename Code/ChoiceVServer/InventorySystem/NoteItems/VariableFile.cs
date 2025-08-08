using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public record VariableFileData(string type, string identifier, string x, string y, string fontSize, string width);
    public record VariableDataElement(string type, string identifier, string x, string y, string fontSize, string width, string height, string text, string placeholder) : VariableFileData(type, identifier, x, y, fontSize, width);
    public record VariableSignatureElement(string type, string identifier, string x, string y, string fontSize, string width, string signatureInfo, string signatureText) : VariableFileData(type, identifier, x, y, fontSize, width);

    public class VariableFileEvent : IPlayerCefEvent {
        public string Event { get; }
        public int id;
        public string backgroundImage;
        public string width;
        public string height;
        public List<VariableFileData> data;
        public bool debugMode;
        public bool readOnly;
        public bool isCopy;

        public VariableFileEvent(string backgroundImage, string width, string height, List<VariableFileData> data, bool debugMode, bool readOnly, bool isCopy, int id = -1) {
            Event = "OPEN_VARIABLE_FILE";
            this.backgroundImage = backgroundImage;
            this.width = width;
            this.height = height;
            this.data = data;
            this.debugMode = debugMode;
            this.readOnly = readOnly;
            this.isCopy = isCopy;

            this.id = id;
        }
    }

    public class VariableFile : Item, File {
        private string VariableFileIdenfier;
        public bool Finalized { get => ((bool)Data["Finalized"]); set { Data["Finalized"] = value; } }
        public int FileId { get => (int)Data["FileId"]; set { Data["FileId"] = value; } }
        public bool IsCopy { get => (bool)Data["IsCopy"]; set { Data["IsCopy"] = value; } }

        public VariableFile(item item) : base(item) {
            updateDescription();
        }

        public VariableFile(configitem configItem, int amount, int quality) : base(configItem) {
            Finalized = false;
            IsCopy = false;
            FileId = -1;
        }

        public VariableFile(configitem configItem, VariableFile otherFile) : base(configItem) {
            foreach(var el in otherFile.Data.Items) {
                if(el.Key != "IsCopy") {
                    Data[el.Key] = el.Value;
                }
            }

            if(otherFile.Finalized) {
                IsCopy = true;
            } else {
                IsCopy = false;
                FileId = -1;
            }
        }

        public override void processAdditionalInfo(string info) {
            var split = info.Split('#');
            if(split.Length == 1) {
                split = new string[] { split[0], new Random().Next(1000, 9999).ToString() };
            }

            VariableFileIdenfier = split[0];

            if(FileId == -1) {
                FileId = int.Parse(split[1]);

                var newFileId = int.Parse(split[1]) + 1;
                var cfg = InventoryController.getConfigById(ConfigId);
                cfg.additionalInfo = $"{split[0]}#{newFileId}";
                using(var db = new ChoiceVDb()) {
                    var dbCfg = db.configitems.Find(ConfigId);

                    if(dbCfg != null) {
                        dbCfg.additionalInfo = $"{split[0]}#{newFileId}";

                        db.SaveChanges();
                    }
                }
            }

            updateDescription();
        }

        public override void use(IPlayer player) {
            base.use(player);

            var dataList = new List<VariableFileData>();
            using(var db = new ChoiceVDb()) {
                var variableFile = db.configvariablefiles.Include(f => f.configvariablefilesfields).FirstOrDefault(f => f.identifer == VariableFileIdenfier);

                var atLeastOneSignature = false;
                foreach(var data in variableFile.configvariablefilesfields) {
                    var text = "";
                    if(Data.hasKey(data.identifier)) {
                        text = Data[data.identifier];
                    }

                    if(data.type == "VARIABLE" || data.type == "SAVE_BUTTON") {
                        dataList.Add(new VariableDataElement(
                            data.type,
                            data.identifier,
                            data.x + "%",
                            data.y + "%",
                            data.fontSize + "vh",
                            data.width + "%",
                            data.height + "%",
                            text,
                            data.placeholder
                        ));
                    } else if(data.type == "SIGNATURE") {
                        if(text != "" && text != null) {
                            atLeastOneSignature = true;
                        }

                        dataList.Add(new VariableSignatureElement(
                            data.type,
                            data.identifier,
                            data.x + "%",
                            data.y + "%",
                            data.fontSize + "vh",
                            data.width + "%",
                            data.placeholder,
                            text
                        ));
                    }
                }

                player.emitCefEventWithBlock(new VariableFileEvent(variableFile.backgroundImage, variableFile.width + "vh", variableFile.height + "vh", dataList, false, atLeastOneSignature, IsCopy, Id ?? -1), "CEF_FILE");
            }
        }

        public static void showVariableFileEvent(IPlayer player, configvariablefile file, bool showDebugMode) {
            var dataList = new List<VariableFileData>();

            foreach(var data in file.configvariablefilesfields) {
                if(data.type == "VARIABLE" || data.type == "SAVE_BUTTON") {
                    var text = "";
                    if(data.placeholder != "") {
                        text = data.placeholder;
                    }

                    dataList.Add(new VariableDataElement(
                        data.type,
                        data.identifier,
                        data.x + "%",
                        data.y + "%",
                        data.fontSize + "vh",
                        data.width + "%",
                        data.height + "%",
                        text,
                        data.placeholder
                    ));
                } else if(data.type == "SIGNATURE") {
                    var text = $"{DateTime.Now:d MMM}, {player.getCharacterShortName()}";
                    if(data.placeholder != "") {
                        text = "";
                    }

                    dataList.Add(new VariableSignatureElement(
                        data.type,
                        data.identifier,
                        data.x + "%",
                        data.y + "%",
                        data.fontSize + "vh",
                        data.width + "%",
                        data.placeholder,
                        text
                    ));
                }
            }

            player.emitCefEventWithBlock(new VariableFileEvent(file.backgroundImage, file.width + "vh", file.height + "vh", dataList, showDebugMode, false, false, 0), "CEF_FILE");
        }

        public override void updateDescription() {
            var copyStr = "";
            if(IsCopy) copyStr = ", KOPIE";

            Description = $"DokumentenId: {FileId}{copyStr}";
        }

        public Item getCopy() {
            var cfg = InventoryController.getConfigById(ConfigId);

            return new VariableFile(cfg, this);
        }
    }
}
