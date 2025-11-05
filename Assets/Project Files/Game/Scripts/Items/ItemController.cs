using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ItemController : MonoBehaviour
    {
        private static ItemController itemController;

        [SerializeField] ItemsDatabase itemsDatabase;
        [SerializeField] GameObject indicatorObject;

        private Item[] registeredItems;
        private Dictionary<Item.Type, Item> itemsTypeLink;

        private Pool indicatorPool;

        public void Init()
        {
            itemController = this;

            // Initilise items database
            itemsDatabase.Init();
            registeredItems = itemsDatabase.Items;

            itemsTypeLink = new Dictionary<Item.Type, Item>();
            for (int i = 0; i < registeredItems.Length; i++)
            {
                if (!itemsTypeLink.ContainsKey(registeredItems[i].ItemType))
                {
                    itemsTypeLink.Add(registeredItems[i].ItemType, registeredItems[i]);
                }
                else
                {
                    Debug.LogError(string.Format("[Item System]: Item {0} already exists in database!", registeredItems[i].ItemType));
                }
            }

            indicatorPool = new Pool(indicatorObject, indicatorObject.name);
        }

        private void OnDestroy()
        {
            if(!registeredItems.IsNullOrEmpty())
            {
                for (int i = 0; i < registeredItems.Length; i++)
                {
                    if (registeredItems[i].Pool != null)
                        PoolManager.DestroyPool(registeredItems[i].Pool);
                }
            }

            indicatorPool?.Destroy();
        }

        public static Item GetItem(Item.Type itemType)
        {
            if (itemController.itemsTypeLink.ContainsKey(itemType))
            {
                return itemController.itemsTypeLink[itemType];
            }

            Debug.LogError(string.Format("[Item System]: Item {0} can't be found in the database!", itemType));

            return null;
        }

        public static Item[] GetItems()
        {
            return itemController.registeredItems;
        }

        public static ItemIndicatorBehaviour SpawnIndicator(Vector3 position, Item.Type itemType)
        {
            GameObject indicatorObject = itemController.indicatorPool.GetPooledObject();
            indicatorObject.transform.position = position;
            indicatorObject.transform.localScale = Vector3.zero;
            indicatorObject.SetActive(true);
            indicatorObject.transform.DOScale(1.0f, 0.3f).SetEasing(Ease.Type.CubicOut);

            ItemIndicatorBehaviour itemIndicatorBehaviour = indicatorObject.GetComponent<ItemIndicatorBehaviour>();
            itemIndicatorBehaviour.Initialise(GetItem(itemType));

            return itemIndicatorBehaviour;
        }
    }
}