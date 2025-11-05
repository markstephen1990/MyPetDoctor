using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class AnimalSicknessSettings
    {
        [SerializeField] SicknessType sicknessType;
        public SicknessType SicknessType => sicknessType;

        [SerializeField] Material sicknessMaterial;
        public Material SicknessMaterial => sicknessMaterial;
    }
}