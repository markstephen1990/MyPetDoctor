using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if MODULE_IAP
using UnityEngine.Purchasing;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
#endif

namespace FernGames
{
    /// <summary>
    /// Wrapper class for Unity IAP v5 functionality.
    /// Uses the new event-based StoreController API.
    /// </summary>
    public class UnityIAPWrapper : IAPWrapper
    {
#if MODULE_IAP
        private static StoreController storeController;
        private static IAPSettings cachedSettings;
        private static bool isConnected;

        public static StoreController Controller => storeController;
#endif

        /// <summary>
        /// Initializes the IAP system with the provided settings.
        /// </summary>
        /// <param name="settings">The IAP settings to use for initialization.</param>
        public override async Task Init(IAPSettings settings)
        {
#if MODULE_IAP
            try
            {
                cachedSettings = settings;

                var options = new InitializationOptions().SetEnvironmentName("production");
                await UnityServices.InitializeAsync(options);

                storeController = UnityIAPServices.StoreController();

                // Subscribe to events
                storeController.OnPurchasePending += OnPurchasePending;
                storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
                storeController.OnPurchaseFailed += OnPurchaseFailed;
                storeController.OnProductsFetched += OnProductsFetched;
                storeController.OnProductsFetchFailed += OnProductsFetchFailed;
                storeController.OnPurchasesFetched += OnPurchasesFetched;
                storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

                // Connect to the store
                await storeController.Connect();
                isConnected = true;

                if (Monetization.VerboseLogging)
                    Debug.Log("[IAP Manager]: Connected to store successfully.");

                // Fetch products
                List<ProductDefinition> productDefinitions = new List<ProductDefinition>();
                IAPItem[] items = settings.StoreItems;

                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.ID))
                    {
                        productDefinitions.Add(new ProductDefinition(item.ID, (UnityEngine.Purchasing.ProductType)item.ProductType));
                    }
                    else
                    {
                        Debug.LogWarning($"[IAP Manager]: Product {item.ProductKeyType} does not have configured IDs.");
                    }
                }

                if (productDefinitions.Count > 0)
                {
                    storeController.FetchProducts(productDefinitions);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[IAP Manager]: Initialization failed with exception: {exception}");
            }
#else
            await Task.Run(() => Debug.Log("[IAP Manager]: Define MODULE_IAP is disabled!"));
#endif
        }

#if MODULE_IAP
        /// <summary>
        /// Called when products are successfully fetched from the store.
        /// </summary>
        private void OnProductsFetched(List<Product> products)
        {
            if (Monetization.VerboseLogging)
                Debug.Log($"[IAP Manager]: Fetched {products.Count} products.");

            // Fetch existing purchases to restore any pending transactions
            storeController.FetchPurchases();
        }

        /// <summary>
        /// Called when product fetching fails.
        /// </summary>
        private void OnProductsFetchFailed(ProductFetchFailed failureInfo)
        {
            if (Monetization.VerboseLogging)
                Debug.LogError($"[IAP Manager]: Failed to fetch products: {failureInfo.FailureReason}");
        }

        /// <summary>
        /// Called when existing purchases are fetched (used for restoration).
        /// </summary>
        private void OnPurchasesFetched(Orders orders)
        {
            if (Monetization.VerboseLogging)
                Debug.Log($"[IAP Manager]: Fetched {orders.ConfirmedOrders.Count} confirmed orders, {orders.PendingOrders.Count} pending orders.");

            // Process any pending orders
            foreach (var pendingOrder in orders.PendingOrders)
            {
                ProcessPendingOrder(pendingOrder);
            }

            IAPManager.OnModuleInitialized();
        }

        /// <summary>
        /// Called when fetching purchases fails.
        /// </summary>
        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failureDescription)
        {
            if (Monetization.VerboseLogging)
                Debug.LogError($"[IAP Manager]: Failed to fetch purchases: {failureDescription.Message}");

            // Still mark as initialized even if fetch fails
            IAPManager.OnModuleInitialized();
        }

        /// <summary>
        /// Called when a new purchase is pending confirmation.
        /// </summary>
        private void OnPurchasePending(PendingOrder pendingOrder)
        {
            if (Monetization.VerboseLogging)
                Debug.Log($"[IAP Manager]: Purchase pending for order.");

            ProcessPendingOrder(pendingOrder);
        }

        /// <summary>
        /// Processes a pending order by confirming it and granting rewards.
        /// </summary>
        private void ProcessPendingOrder(PendingOrder pendingOrder)
        {
            var cartItem = pendingOrder.CartOrdered.Items().FirstOrDefault();
            if (cartItem != null)
            {
                var productId = cartItem.Product.definition.id;
                IAPItem item = IAPManager.GetIAPItem(productId);

                if (item != null)
                {
                    IAPManager.OnPurchaseCompleted(item.ProductKeyType);
                }
                else
                {
                    if (Monetization.VerboseLogging)
                        Debug.Log($"[IAP Manager]: Product - {productId} can't be found!");
                }

                // Confirm the purchase to complete the transaction
                storeController.ConfirmPurchase(pendingOrder);
            }

            SystemMessage.ChangeLoadingMessage("Payment complete!");
            SystemMessage.HideLoadingPanel();
        }

        /// <summary>
        /// Called when a purchase is confirmed (successfully completed).
        /// </summary>
        private void OnPurchaseConfirmed(Order confirmedOrder)
        {
            if (Monetization.VerboseLogging)
            {
                Debug.Log($"[IAP Manager]: Purchase confirmed successfully.");
            }
        }

        /// <summary>
        /// Called when a purchase fails.
        /// </summary>
        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            if (Monetization.VerboseLogging)
            {
                Debug.Log($"[IAP Manager]: Purchase failed with reason: {failedOrder.FailureReason}");
            }

            // Try to find the product that failed
            var cartItem = failedOrder.CartOrdered?.Items()?.FirstOrDefault();
            if (cartItem != null)
            {
                var productId = cartItem.Product.definition.id;
                IAPItem item = IAPManager.GetIAPItem(productId);

                if (item != null)
                {
                    IAPManager.OnPurchaseFailed(item.ProductKeyType, ConvertFailureReason(failedOrder.FailureReason));
                }
            }

            SystemMessage.ChangeLoadingMessage("Payment failed!");
            SystemMessage.HideLoadingPanel();
        }

        /// <summary>
        /// Converts Unity IAP v5 failure reason to internal failure reason.
        /// </summary>
        private FernGames.PurchaseFailureReason ConvertFailureReason(UnityEngine.Purchasing.PurchaseFailureReason reason)
        {
            return reason switch
            {
                UnityEngine.Purchasing.PurchaseFailureReason.PurchasingUnavailable => FernGames.PurchaseFailureReason.PurchasingUnavailable,
                UnityEngine.Purchasing.PurchaseFailureReason.ExistingPurchasePending => FernGames.PurchaseFailureReason.ExistingPurchasePending,
                UnityEngine.Purchasing.PurchaseFailureReason.ProductUnavailable => FernGames.PurchaseFailureReason.ProductUnavailable,
                UnityEngine.Purchasing.PurchaseFailureReason.SignatureInvalid => FernGames.PurchaseFailureReason.SignatureInvalid,
                UnityEngine.Purchasing.PurchaseFailureReason.UserCancelled => FernGames.PurchaseFailureReason.UserCancelled,
                UnityEngine.Purchasing.PurchaseFailureReason.PaymentDeclined => FernGames.PurchaseFailureReason.PaymentDeclined,
                UnityEngine.Purchasing.PurchaseFailureReason.DuplicateTransaction => FernGames.PurchaseFailureReason.DuplicateTransaction,
                _ => FernGames.PurchaseFailureReason.Unknown
            };
        }
#endif

        /// <summary>
        /// Restores previously purchased products.
        /// </summary>
        public override void RestorePurchases()
        {
#if MODULE_IAP
            if (!IAPManager.IsInitialized || !isConnected)
            {
                SystemMessage.ShowMessage("Network error. Please try again later");
                return;
            }

            SystemMessage.ShowLoadingPanel();
            SystemMessage.ChangeLoadingMessage("Restoring purchased products..");

            storeController.RestoreTransactions((result, error) =>
            {
                if (result)
                {
                    SystemMessage.ChangeLoadingMessage("Restoration completed!");
                }
                else
                {
                    SystemMessage.ChangeLoadingMessage($"Restoration failed: {error}");
                }

                SystemMessage.HideLoadingPanel();
            });
#endif
        }

        /// <summary>
        /// Initiates the purchase of a product.
        /// </summary>
        /// <param name="productKeyType">The key type of the product to purchase.</param>
        public override void BuyProduct(ProductKeyType productKeyType)
        {
#if MODULE_IAP
            if (!IAPManager.IsInitialized || !isConnected)
            {
                SystemMessage.ShowMessage("Network error. Please try again later");
                return;
            }

            SystemMessage.ShowLoadingPanel();
            SystemMessage.ChangeLoadingMessage("Payment in progress..");

            IAPItem item = IAPManager.GetIAPItem(productKeyType);
            if (item != null)
            {
                var product = storeController.GetProductById(item.ID);
                if (product != null)
                {
                    storeController.PurchaseProduct(product);
                }
                else
                {
                    if (Monetization.VerboseLogging)
                        Debug.LogError($"[IAP Manager]: Product {item.ID} not found in store.");

                    SystemMessage.ChangeLoadingMessage("Product not available!");
                    SystemMessage.HideLoadingPanel();
                }
            }
#else
            SystemMessage.ShowMessage("Network error.");
#endif
        }

        /// <summary>
        /// Gets the product data for a specified product key type.
        /// </summary>
        /// <param name="productKeyType">The key type of the product.</param>
        /// <returns>The product data.</returns>
        public override ProductData GetProductData(ProductKeyType productKeyType)
        {
            if (!IAPManager.IsInitialized)
                return null;

#if MODULE_IAP
            IAPItem item = IAPManager.GetIAPItem(productKeyType);
            if (item != null)
            {
                var product = storeController.GetProductById(item.ID);
                if (product != null)
                {
                    return new ProductData(product);
                }
            }
#endif

            return null;
        }

        /// <summary>
        /// Checks if a product is subscribed.
        /// </summary>
        /// <param name="productKeyType">The key type of the product.</param>
        /// <returns>True if the product is subscribed, otherwise false.</returns>
        public override bool IsSubscribed(ProductKeyType productKeyType)
        {
#if MODULE_IAP
            IAPItem item = IAPManager.GetIAPItem(productKeyType);
            if (item != null)
            {
                var product = storeController.GetProductById(item.ID);
                if (product != null && product.definition.type == UnityEngine.Purchasing.ProductType.Subscription)
                {
                    // Check if the product has a valid receipt (subscription is active)
                    return product.hasReceipt;
                }
            }
#endif

            return false;
        }
    }
}
