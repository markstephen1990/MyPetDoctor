using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Item", menuName = "Data/Items/Item")]
    public class Item : ScriptableObject
    {
        [SerializeField] Type itemType;
        public Type ItemType => itemType;

        [Space]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] GameObject model;
        public GameObject Model => model;

        [SerializeField] float modelHeight;
        public float ModelHeight => modelHeight;

        private Pool pool;
        public Pool Pool => pool;

        public void Init()
        {
            pool = new Pool(model, $"Item_{model.name}");
        }

        public enum Type
        {
            None = -1,
            Soap = 0,
            Injection = 1,
            Pill = 2
        }
    }
}