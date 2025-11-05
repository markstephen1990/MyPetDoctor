using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(BoxCollider))]
    public class MoneyStackBehaviour : MonoBehaviour
    {
        [SerializeField] int rows = 2;
        [SerializeField] int columns = 2;
        [SerializeField] int layers = 2;

        [Space]
        [SerializeField] int currencyPerStack = 5;

        private Zone zone;

        private Dictionary<CurrencyType, int> collectedMoney;

        private List<Transform> activeObjets = new List<Transform>();

        private WaitForSeconds spawnDelay;

        private bool isActive = true;
        public bool IsActive => isActive;

        private bool isPlayerInZone = false;
        private bool moneyAnimationIsPlaying;

        private BoxCollider boxCollider;

        private Vector3 size;
        private Vector3 center;

        private float rowSpacing;
        private float columnSpacing;
        private float layerSpacing;

        private int elementsCount;

        private Coroutine animationCoroutine;

        private Queue<CurrencyAmount> elementsQueue;

        private void Awake()
        {
            // Chache collider component
            boxCollider = GetComponent<BoxCollider>();

            // Create delays
            spawnDelay = new WaitForSeconds(0.05f);

            collectedMoney = new Dictionary<CurrencyType, int>();
        }

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            size = boxCollider.size;
            center = boxCollider.center;

            rowSpacing = size.x / (float)(rows + 1);
            columnSpacing = size.z / (float)(columns + 1);
            layerSpacing = size.y / (float)(layers + 1);

            elementsCount = rows * columns * layers;

            elementsQueue = new Queue<CurrencyAmount>();

            PrepareCurrenciesDictionary();
        }

        private void PrepareCurrenciesDictionary()
        {
            collectedMoney = new Dictionary<CurrencyType, int>();

            Currency[] currencies = CurrencyController.Currencies;
            for (int i = 0; i < currencies.Length; i++)
            {
                collectedMoney.Add(currencies[i].CurrencyType, 0);
            }
        }

        private void Update()
        {
            if (isPlayerInZone && !moneyAnimationIsPlaying)
                PickMoney();
        }

        public void AddMoney(CurrencyAmount currencyPrice)
        {
            if (activeObjets.Count < elementsCount)
            {
                moneyAnimationIsPlaying = true;

                elementsQueue.Enqueue(currencyPrice);

                collectedMoney[currencyPrice.CurrencyType] += currencyPrice.Amount;

                animationCoroutine = StartCoroutine(SpawnMoneyElements());
            }
        }

        public int GetPickedMoney(CurrencyType currencyType)
        {
            return collectedMoney[currencyType];
        }

        private IEnumerator SpawnMoneyElements()
        {
            if(elementsQueue.Count != 0)
            {
                CurrencyAmount lastElement = elementsQueue.Dequeue();

                int tempActiveElements = lastElement.Amount / currencyPerStack;

                if (tempActiveElements > 0)
                {
                    for (int i = 0; i < tempActiveElements; i++)
                    {
                        // Check if next stack object exist
                        if (activeObjets.Count < elementsCount)
                        {
                            // Get object from pool and initialise transform
                            GameObject tempMoneyObject = lastElement.Currency.Data.StackElementsPool.GetPooledObject();
                            tempMoneyObject.transform.ResetLocal();
                            tempMoneyObject.transform.position = transform.position + GetElementLocalPosition(activeObjets.Count);
                            tempMoneyObject.transform.rotation = Quaternion.Euler(0, Random.Range(-5.0f, 5.0f), 0);
                            tempMoneyObject.transform.localScale = Vector3.zero;
                            tempMoneyObject.SetActive(true);

                            // Play scale animation
                            tempMoneyObject.transform.DOScale(Vector3.one, 0.3f).SetEasing(Ease.Type.BackOut);

                            // Add element to list
                            activeObjets.Add(tempMoneyObject.transform);

                            yield return spawnDelay;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            moneyAnimationIsPlaying = false;
        }

        private void DisableMoneyElements()
        {
            // Cache variables
            int moneyElementsAmount = activeObjets.Count;
            List<Transform> moneyElementsTransform = activeObjets;

            // Reset global variables
            activeObjets = new List<Transform>();

            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            for (int i = moneyElementsAmount - 1; i >= 0; i--)
            {
                // Cache transform element
                Transform elementTransform = moneyElementsTransform[i];

                float time = Random.Range(0.6f, 1.4f);

                // Play scale animation
                elementTransform.DOScale(0, time).SetEasing(Ease.Type.SineIn);
                elementTransform.DOBezierFollow(playerBehavior.transform, Random.Range(5, 10), Random.Range(-1, 1), 0, Random.Range(0.4f, 1.0f)).SetEasing(Ease.Type.SineIn).OnComplete(delegate
                {
                    // Disable object and return to pool
                    elementTransform.gameObject.SetActive(false);
                });
            }
        }

        private void PickMoney()
        {
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            bool anyCurrencyPicked = false;
            foreach(KeyValuePair<CurrencyType, int> money in collectedMoney)
            {
                if(money.Value > 0)
                {
                    Currency currency = CurrencyController.GetCurrency(money.Key);
                    Sprite currencyIcon = currency.Data.StackIcon;
                    if (currencyIcon == null)
                        currencyIcon = currency.Icon;

                    CurrencyController.Add(money.Key, money.Value);

                    MoneyFloatingTextBehavior moneyFloatingText = (MoneyFloatingTextBehavior)FloatingTextController.SpawnFloatingText("Money", "+" + money.Value, transform.position + new Vector3(0, 6, 2), Quaternion.Euler(45, 0, 0), 1.0f, Color.white);
                    moneyFloatingText.SetIcon(currencyIcon);

                    anyCurrencyPicked = true;
                }
            }

            if(anyCurrencyPicked)
            {
                AudioController.PlaySound(AudioController.AudioClips.moneyPickUpSound);

                PrepareCurrenciesDictionary();

                DisableMoneyElements();

                zone.OnMoneyPicked();

                TutorialController.OnMoneyPicked();

                Tween.DelayedCall(1.0f, delegate
                {
                    AdsManager.ShowInterstitial(delegate
                    {
                        // Interstitial is shown
                    });
                });
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive)
                return;

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                isPlayerInZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                isPlayerInZone = false;
            }
        }


        public void SetActiveState(bool state)
        {
            isActive = state;
        }

        private Vector3 GetElementLocalPosition(int index)
        {
            int k = layers - 1 - index / (rows * columns); // layer index
            int remainder = index % (rows * columns);
            int i = remainder / columns; // row index
            int j = remainder % columns; // column index

            float x = center.x - size.x / 2 + rowSpacing * (i + 1);
            float y = center.y + size.y / 2 - layerSpacing * (k + 1);
            float z = center.z - size.z / 2 + columnSpacing * (j + 1);

            return new Vector3(x, y, z);
        }

        private void OnDrawGizmosSelected()
        {
            boxCollider = GetComponent<BoxCollider>();

            Vector3 size = boxCollider.size;
            Vector3 center = boxCollider.center;

            float rowSpacing = size.x / (float)(rows + 1);
            float columnSpacing = size.z / (float)(columns + 1);
            float layerSpacing = size.y / (float)(layers + 1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    for (int k = 0; k < layers; k++)
                    {
                        float x = center.x - size.x / 2 + rowSpacing * (i + 1);
                        float y = center.y + size.y / 2 - layerSpacing * (k + 1);
                        float z = center.z - size.z / 2 + columnSpacing * (j + 1);

                        Vector3 position = new Vector3(x, y, z);

                        Gizmos.DrawWireCube(transform.position + position, new Vector3(1f, 0.2f, 0.5f));
                    }
                }
            }
        }

        public void Unload()
        {
            moneyAnimationIsPlaying = false;

            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
        }
    }
}