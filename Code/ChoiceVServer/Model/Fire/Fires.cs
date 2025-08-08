using AltV.Net.Data;

namespace ChoiceVServer.Model.Fire {
    public class Fires {
        public int Id = 0;
        public int Uid = 0;

        public Position colPosition;
        public Rotation colRotation;

        public float Scale = 0f;

        public Fires(int id, int uid, Position position, Rotation rotation, float scale) {
            Id = id;
            Uid = uid;

            colPosition = position;
            colRotation = rotation;

            Scale = scale;
        }
    }
}
