using UnityEngine;

namespace FernGames
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField] int id;
        public int ID => id;

        [SerializeField] string levelName;
        public string LevelName => levelName;
    }
}