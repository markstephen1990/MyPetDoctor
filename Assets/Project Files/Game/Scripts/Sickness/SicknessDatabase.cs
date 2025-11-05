using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Sicknesses Database", menuName = "Data/Sicknesses/Sicknesses Database")]
    public class SicknessDatabase : ScriptableObject
    {
        [SerializeField] Sickness[] sicknesses;
        public Sickness[] Sicknesses => sicknesses;

        public void Init()
        {
            for (int i = 0; i < sicknesses.Length; i++)
            {
                sicknesses[i].Init();
            }
        }
    }
}