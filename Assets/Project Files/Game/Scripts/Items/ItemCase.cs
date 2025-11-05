using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ItemCase
    {
        [SerializeField] Item.Type itemType;
        public Item.Type ItemType => itemType;

        [SerializeField] int amount;

        public int Amount { get { return amount; } set { amount = value; } }
        public string AmountFormatted => CurrencyHelper.Format(amount);

        private Item item;
        public Item Item => item;

        public ItemCase(Item item, int amount)
        {
            itemType = item.ItemType;

            this.item = item;
            this.amount = amount;
        }
    }
}