using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class Animal
    {
        [SerializeField] Type animalType;
        public Type AnimalType => animalType;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] float carryingHeight;
        public float CarryingHeight => carryingHeight;

        [Space]
        [SerializeField] CurrencyAmount reward;
        public CurrencyAmount Reward => reward;

        private Pool pool;
        public Pool Pool => pool;

        public void Init()
        {
            pool = new Pool(prefab, $"Animal_{prefab.name}");
        }

        public void Unload()
        {
            PoolManager.DestroyPool(pool);
        }

        public enum Type
        {
            Cat_01 = 0,
            Cat_02 = 1,
            Cat_03 = 2,
            Cat_04 = 3,
            Cat_05 = 4,
        }
    }
}