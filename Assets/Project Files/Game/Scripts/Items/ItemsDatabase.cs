using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Items Database", menuName = "Data/Items/Items Database")]
    public class ItemsDatabase : ScriptableObject
    {
        [SerializeField] Item[] items;
        public Item[] Items => items;

        public void Init()
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i].Init();
            }
        }
    }
}