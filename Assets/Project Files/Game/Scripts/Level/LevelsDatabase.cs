using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Data/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField] LevelData[] levels;
        public LevelData[] Levels => levels;

        public LevelData GetLevelByIndex(int index)
        {
            if (levels.IsInRange(index))
            {
                return levels[index];
            }

            return levels.GetRandomItem();
        }

        public int GetLevelIndexByName(string name)
        {
            foreach(LevelData level in levels)
            {
                if (level.LevelName == name)
                    return level.ID;
            }

            return -1;
        }
    }
}