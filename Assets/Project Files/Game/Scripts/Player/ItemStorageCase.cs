using UnityEngine;

namespace Watermelon
{
    public class ItemStorageCase
    {
        private int index;
        public int Index => index;

        private GameObject itemObject;
        public GameObject ItemObject => itemObject;

        private Item.Type itemType;
        public Item.Type ItemType => itemType;

        private Item item;
        public Item Item => item;

        private bool isPicked;
        public bool IsPicked => isPicked;

        private GameObject storageObject;
        public GameObject StorageObject => storageObject;

        public ItemStorageCase(GameObject itemObject, Item.Type itemType, Item item, GameObject storageObject)
        {
            this.itemObject = itemObject;
            this.itemType = itemType;
            this.item = item;
            this.storageObject = storageObject;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void MarkAsPicked()
        {
            isPicked = true;
        }

        public void Reset()
        {
            storageObject.transform.SetParent(null);
            storageObject.SetActive(false);

            itemObject.transform.SetParent(null);
            itemObject.SetActive(false);
        }
    }
}