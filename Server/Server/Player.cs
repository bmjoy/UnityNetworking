using System.Numerics;

namespace Server
{
    public class Player
    {

        public int id;
        public string nickname;

        public Vector3 position;
        public Quaternion rotation;

        public Player(int id, string nickname, Vector3 spawnPosition)
        {
            this.id = id;
            this.nickname = nickname;
            position = spawnPosition;
            rotation = Quaternion.Identity;
        }

    }
}