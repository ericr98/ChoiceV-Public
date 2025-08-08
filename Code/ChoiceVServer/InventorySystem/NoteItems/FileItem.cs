using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {

    public class FileItemCefEvent : IPlayerCefEvent {
        public string Event { get; private set; }
        public FileItemElement[] data;

        public bool isCopy;
        public int id;

        public FileItemCefEvent(List<FileItemElement> data, string eventName, bool isCopy, int id) {
            Event = eventName;
            this.data = data.ToArray();

            this.isCopy = isCopy;
            this.id = id;
        }
    }

    public class FileItemElement {

        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "data")]
        public object Data;

        public FileItemElement(string name, object data) {
            Name = name;
            Data = data;
        }
    }

    public abstract class FileItem : Item, File {
        public int FileId { get => ((int)Data["FileId"]); set { Data["FileId"] = value; } }
        public bool IsCopy { get => ((bool)Data["IsCopy"]); set { Data["IsCopy"] = value; } }

        protected string EventName;

        public FileItem(item item) : base(item) { }

        public FileItem(FileItem item) : base(InventoryController.getConfigById(item.ConfigId)) {
            foreach(var el in Data.Items) {
                if(el.Key == "FileId") {
                    FileId = getNextFileId();
                } else if(el.Key == "IsCopy") {
                    IsCopy = true;
                } else {
                    Data[el.Key] = el.Value;
                }
            }
        }

        public FileItem(configitem cfg) : base(cfg) {
            var elements = getInitialElements();
            FileId = getNextFileId();
            IsCopy = false;

            foreach(var element in elements) {
                Data[element.Name] = element.Data;
            }
        }

        public void fillFromCef(FileItemCefEvent cefEvent, IPlayer player) {
            foreach(var el in cefEvent.data) {
                if(el.Data != null && el.Data.ToString() == "TO_FILL") {
                    Data[el.Name] = getSignature(player);
                } else {
                    Data[el.Name] = el.Data;
                }
            }
        }

        public abstract int getNextFileId();
        public abstract List<FileItemElement> getInitialElements();
        public abstract string getSignature(IPlayer player);
        public abstract Item getCopy();

        public void setData(List<FileItemElement> list) {
            foreach(var el in list) {
                Data[el.Name] = el.Data;
            };
        }

        public List<FileItemElement> getData() {
            var list = new List<FileItemElement>();
            list.Add(new FileItemElement("fileId", FileId));
            foreach(var el in Data.Items) {
                list.Add(new FileItemElement(el.Key, el.Value));
            }

            return list;
        }

        public override void use(IPlayer player) {
            base.use(player);

            player.emitCefEventWithBlock(new FileItemCefEvent(getData(), EventName, IsCopy, Id ?? -1), "CEF_FILE");
        }
    }
}
