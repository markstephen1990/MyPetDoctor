using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class NurseSettings
    {
        [SerializeField] int price;
        public int Price => price;

        [SerializeField] CurrencyType currencyType;
        public CurrencyType CurrencyType => currencyType;

        [SerializeField] string title;
        public string Title => title;

        [SerializeField] Sprite preview;
        public Sprite Preview => preview;

        public bool EnoughMoney()
        {
            return CurrencyController.HasAmount(currencyType, price);
        }
    }
}