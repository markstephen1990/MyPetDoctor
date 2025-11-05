using UnityEngine;

namespace Watermelon
{
    public interface IPurchaseObject
    {
        public bool IsOpened { get; }

        public CurrencyAmount Price { get; }

        public int PlacedCurrencyAmount { get; }

        public Transform Transform { get; }

        public void PlaceCurrency(int amount);
        public void OnPurchaseCompleted();

        public void OnPlayerEntered(PlayerBehavior playerBehavior);
        public void OnPlayerExited(PlayerBehavior playerBehavior);
    }
}