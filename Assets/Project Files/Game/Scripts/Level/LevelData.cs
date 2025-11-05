using UnityEngine;

namespace Watermelon
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