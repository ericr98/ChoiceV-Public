using System;

namespace ChoiceVServer.Controller.Clothing {

    public class ClothingComponent {
        public static readonly ClothingComponent Empty = new ClothingComponent(0, 0);

        public int Drawable { get; set; }
        public int Texture { get; set; }

        public string Dlc { get; set; }

        public ClothingComponent() { }

        public ClothingComponent(int drawable, int texture, string dlc = null) {
            Drawable = drawable;
            Texture = texture;
            Dlc = dlc;
        }

        public ClothingComponent(string shortStr) {
            var split = shortStr.Split(',');
            Drawable = int.Parse(split[0]);
            Texture = int.Parse(split[1]);
            Dlc = split[2];
        }

        public static implicit operator ClothingComponent(long value) {
            return new ClothingComponent(0, 0);
        }

        public override bool Equals(object obj) {
            var cO = obj as ClothingComponent;
            if (cO == null)
                return base.Equals(obj);
            return Drawable == cO.Drawable && Texture == cO.Texture && cO.Dlc == Dlc;
        }

        public override int GetHashCode() => base.GetHashCode();

        public string toShortStr() {
            return $"{Drawable},{Texture}";
        }
    }
}
