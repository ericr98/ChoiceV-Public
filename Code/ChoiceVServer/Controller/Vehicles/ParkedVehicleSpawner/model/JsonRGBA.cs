using AltV.Net.Data;

namespace ChoiceVServer.Controller {
    class JsonRGBA {
        public int R = 0;
        public int G = 0;
        public int B = 0;
        public int A = 1;

        public Rgba parse() {
            return new Rgba((byte)R, (byte)G, (byte)B, (byte)A);
        }
    }
}
