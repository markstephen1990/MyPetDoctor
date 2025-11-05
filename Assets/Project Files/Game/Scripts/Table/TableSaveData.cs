using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class TableSaveData
    {
        [SerializeField] int placedCurrencyAmount;
        public int PlacedCurrencyAmount => placedCurrencyAmount;

        [SerializeField] bool isOpened;
        public bool IsOpened => isOpened;

        [SerializeField] bool isUnlocked;
        public bool IsUnlocked => isUnlocked;

        public TableSaveData(int placedCurrencyAmount, bool isOpened, bool isLocked)
        {
            this.placedCurrencyAmount = placedCurrencyAmount;
            this.isOpened = isOpened;
            isUnlocked = isLocked;
        }
    }
}