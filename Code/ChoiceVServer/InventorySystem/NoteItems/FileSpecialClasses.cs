using ChoiceVServer.InventorySystem;

namespace ChoiceVServer.InventorySystem {
    public interface File {
        public int FileId { get; set; }
        public bool IsCopy { get; set; }

        public Item getCopy();
    }

    public interface PoliceFile : File { }
}
