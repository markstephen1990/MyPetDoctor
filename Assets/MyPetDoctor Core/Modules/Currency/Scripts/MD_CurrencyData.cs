using System;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CurrencyData
    {
        [SerializeField] bool displayAlways = false;
        public bool DisplayAlways => displayAlways;

        [SerializeField] GameObject purchaseParticlePrefab;
        public GameObject PurchaseParticlePrefab => purchaseParticlePrefab;

        [SerializeField] GameObject stackPrefab;
        public GameObject StackPrefab => stackPrefab;

        [SerializeField] Sprite stackIcon;
        public Sprite StackIcon => stackIcon;

        private Pool stackElementsPool;
        public Pool StackElementsPool => stackElementsPool;

        [NonSerialized]
        private Currency currency;

        public void Init(Currency currency)
        {
            this.currency = currency;
        }

        public void CreateStackPool()
        {
            if(stackElementsPool != null)
            {
                stackElementsPool.Destroy();
                stackElementsPool = null;
            }

            stackElementsPool = new Pool(stackPrefab, $"Stack_{currency.CurrencyType}_{stackPrefab.name}");
        }

        public void DestroyStackPool()
        {
            stackElementsPool?.Destroy();
        }
    }
}