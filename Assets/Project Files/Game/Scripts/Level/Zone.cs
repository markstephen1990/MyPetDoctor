using System;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public partial class Zone : MonoBehaviour, IPurchaseObject, ISceneSavingCallback
    {
        private readonly static int PARTICLE_CONFETTI_HASH = "Table Confetti".GetHashCode();

        [UniqueID]
        [SerializeField] string id;
        public string ID => id;

        [Group("Containers")]
        [SerializeField] Transform floorContainer;
        public Transform FloorContainer => floorContainer;

        [Group("Containers")]
        [SerializeField] Transform wallsContainer;
        public Transform WallsContainer => wallsContainer;

        [SerializeField] CurrencyAmount price;
        public CurrencyAmount Price => price;

        [Group("Purchasing")]
        [SerializeField] ZonePurchaseCase[] zonePurchaseCases;

        [Group("Refs")]
        [SerializeField] AnimalSpawner animalSpawner;
        public AnimalSpawner AnimalSpawner => animalSpawner;

        [Group("Refs")]
        [SerializeField] PhysicsDoorBehaviour physicsDoorBehaviour;

        [Group("Refs")]
        [SerializeField] NurseSpawner nurseSpawner;
        public NurseSpawner NurseSpawner => nurseSpawner;

        [Group("Refs")]
        [SerializeField] SecretaryBehaviour secretaryBehaviours;

        [Group("Refs")]
        [SerializeField] MoneyStackBehaviour moneyStackBehaviour;

        [Group("Refs")]
        [SerializeField] DispenserBuilding[] dispensers;

        [Group("Refs")]
        [SerializeField] TableZoneBehaviour[] tableZones;

        private bool isOpened;
        public bool IsOpened => isOpened || price.Amount == 0;

        private int placedCurrencyAmount;
        public int PlacedCurrencyAmount => placedCurrencyAmount;

        public Transform Transform => transform;

        private bool isActivated;

        private PurchaseAreaBehaviour zonePurchaseBehaviour;

        // Waiting Animals
        private List<AnimalBehaviour> waitingAnimalsList = new List<AnimalBehaviour>();

        private int waitingAnimalsAmount;
        public int WaitingAnimalsAmount => waitingAnimalsAmount;

        // Table Animals
        private List<AnimalBehaviour> tableAnimalsList = new List<AnimalBehaviour>();

        private int tableAnimalsAmount;
        public int TableAnimalsAmount => tableAnimalsAmount;

        // Nurse
        private List<NurseBehaviour> activeNurseBehaviours = new List<NurseBehaviour>();
        private int activeNursesAmount;

        // Opening animation
        private BaseZoneOpenAnimation zoneOpenAnimation;

        public void Initialise(PlayerBehavior playerBehavior)
        {
            // Try to get zone open animation component
            zoneOpenAnimation = GetComponent<BaseZoneOpenAnimation>();
            if (zoneOpenAnimation != null)
            {
                zoneOpenAnimation.OnZoneInitialised(this);
            }

            // Initialise physics door
            if (IsOpened)
            {
                if (dispensers != null)
                {
                    for (int i = 0; i < dispensers.Length; i++)
                    {
                        dispensers[i].Init();
                    }
                }

                if (animalSpawner != null)
                {
                    animalSpawner.Initialise(this);
                }

                if (tableZones != null)
                {
                    for (int i = 0; i < tableZones.Length; i++)
                    {
                        tableZones[i].Initialise(this);
                    }
                }

                if (secretaryBehaviours != null)
                {
                    secretaryBehaviours.Initialise(this);
                }

                if (moneyStackBehaviour != null)
                {
                    moneyStackBehaviour.Initialise(this);
                }

                // Initialise zone settings
                nurseSpawner.Initialise(this);

                if (physicsDoorBehaviour != null)
                {
                    Tween.NextFrame(delegate
                    {
                        physicsDoorBehaviour.SetState(true);
                    });
                }

                // Activate purchase zones
                for (int i = 0; i < zonePurchaseCases.Length; i++)
                {
                    if(zonePurchaseCases[i].LinkedZone != null)
                    {
                        if (!zonePurchaseCases[i].LinkedZone.IsOpened)
                        {
                            zonePurchaseCases[i].LinkedZone.CreatePurchaseZone(zonePurchaseCases[i].ZonePurchaseTransform.position);
                        }
                    }
                }
            }
        }

        public void Lock()
        {
            if (zonePurchaseBehaviour != null)
                zonePurchaseBehaviour.SetBlockState(true);
        }

        public void Unlock()
        {
            if (zonePurchaseBehaviour != null)
                zonePurchaseBehaviour.SetBlockState(false);
        }

        public void CreatePurchaseZone(Vector3 zoneTransform)
        {
            zonePurchaseBehaviour = LevelController.CreatePurchaseZone();
            zonePurchaseBehaviour.transform.position = zoneTransform;
            zonePurchaseBehaviour.Initialise(this, false);
        }

        public void Activate()
        {
            if (isActivated)
                return;

            isActivated = true;
            isOpened = true;

            // Initialise components
            if (dispensers != null)
            {
                for (int i = 0; i < dispensers.Length; i++)
                {
                    dispensers[i].Init();
                }
            }

            if (animalSpawner != null)
            {
                animalSpawner.Initialise(this);
            }

            if (tableZones != null)
            {
                for (int i = 0; i < tableZones.Length; i++)
                {
                    tableZones[i].Initialise(this);
                }
            }

            if (secretaryBehaviours != null)
            {
                secretaryBehaviours.Initialise(this);
            }

            if (moneyStackBehaviour != null)
            {
                moneyStackBehaviour.Initialise(this);
            }

            // Initialise zone settings
            nurseSpawner.Initialise(this);

            if (physicsDoorBehaviour != null)
            {
                Tween.NextFrame(delegate
                {
                    physicsDoorBehaviour.SetState(true);
                });
            }

            if (zoneOpenAnimation != null)
            {
                zoneOpenAnimation.OnZoneOpened();
            }

            // Activate purchase zones
            for (int i = 0; i < zonePurchaseCases.Length; i++)
            {
                if(zonePurchaseCases[i].LinkedZone != null)
                {
                    if (!zonePurchaseCases[i].LinkedZone.IsOpened)
                    {
                        zonePurchaseCases[i].LinkedZone.CreatePurchaseZone(zonePurchaseCases[i].ZonePurchaseTransform.position);
                    }
                }
            }
        }

        public void PlaceCurrency(int amount)
        {
            if (!IsOpened)
            {
                SaveController.MarkAsSaveIsRequired();

                placedCurrencyAmount += amount;

                zonePurchaseBehaviour.SetAmount(price.Amount - placedCurrencyAmount);
            }
        }

        public void OnMoneyAdded(CurrencyAmount currencyPrice)
        {
            moneyStackBehaviour.AddMoney(currencyPrice);
        }

        public void OnMoneyPicked()
        {

        }

        public void OnPurchaseCompleted()
        {
            isOpened = true;

            ActivateWithAnimation();

            AudioController.PlaySound(AudioController.AudioClips.locationOpenSound);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            ParticlesController.PlayParticle(PARTICLE_CONFETTI_HASH).SetPosition(PlayerBehavior.Position);

            SaveController.MarkAsSaveIsRequired();

            // Disable already existing zone
            if (zonePurchaseBehaviour != null)
            {
                // Disable purchase area
                zonePurchaseBehaviour.Disable();
                zonePurchaseBehaviour = null;
            }

            TutorialController.OnZoneOpened(this);
        }

        public void ActivateWithAnimation()
        {
            if (isActivated)
                return;

            isActivated = true;
            isOpened = true;

            // Initialise components
            if (dispensers != null)
            {
                for (int i = 0; i < dispensers.Length; i++)
                {
                    dispensers[i].Init();
                }
            }

            if (animalSpawner != null)
            {
                animalSpawner.Initialise(this);
            }

            if (tableZones != null)
            {
                for (int i = 0; i < tableZones.Length; i++)
                {
                    tableZones[i].Initialise(this);
                }
            }

            if (secretaryBehaviours != null)
            {
                secretaryBehaviours.Initialise(this);
            }

            if (moneyStackBehaviour != null)
            {
                moneyStackBehaviour.Initialise(this);
            }

            // Initialise zone settings
            nurseSpawner.Initialise(this);

            if (physicsDoorBehaviour != null)
            {
                Tween.NextFrame(delegate
                {
                    physicsDoorBehaviour.SetState(true);
                });
            }

            if (zoneOpenAnimation != null)
            {
                zoneOpenAnimation.OnZoneOpened();
            }

            // Activate purchase zones
            for (int i = 0; i < zonePurchaseCases.Length; i++)
            {
                if (zonePurchaseCases[i].LinkedZone != null)
                {
                    if (!zonePurchaseCases[i].LinkedZone.IsOpened)
                    {
                        zonePurchaseCases[i].LinkedZone.CreatePurchaseZone(zonePurchaseCases[i].ZonePurchaseTransform.position);
                    }
                }
            }

            NavMeshController.RecalculateNavMesh(delegate { });
        }

        public void OnPlayerEntered(PlayerBehavior playerBehavior)
        {

        }

        public void OnPlayerExited(PlayerBehavior playerBehavior)
        {

        }

        #region Nurse
        public void SpawnNurse()
        {
            // Spawn nurse
            NurseBehaviour nurseBehaviour = LevelController.SpawnNurse(GetNurseSpawnPosition());
            nurseBehaviour.Initialise(this);

            // Add created nurse to list
            activeNurseBehaviours.Add(nurseBehaviour);

            // Adjust nurses amount
            activeNursesAmount++;
        }

        public void UnlockNurse()
        {
            // Unlock nurse
            nurseSpawner.UnlockNurse();

            // Spawn nurse
            SpawnNurse();

            TutorialController.OnNurseHired();

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
        }

        public NurseSettings GetNextNurseSettings()
        {
            return nurseSpawner.GetNextNurseSettings();
        }

        public Vector3 GetNurseSpawnPosition()
        {
            return nurseSpawner.GetIdlePosition();
        }

        public void RecalculateNurses()
        {
            for (int i = 0; i < activeNursesAmount; i++)
            {
                activeNurseBehaviours[i].RecalculateMoveSpeed();
            }
        }
        #endregion

        #region Table
        public void OnAnimalCured(AnimalBehaviour animalBehaviour)
        {
            VisitorBehaviour visitorBehaviour = animalSpawner.SpawnVisitor();
            visitorBehaviour.TakeAnimal(animalBehaviour);

            animalBehaviour.FollowVisitor(visitorBehaviour);
        }

        public bool HasFreeTable()
        {
            for (int i = 0; i < tableZones.Length; i++)
            {
                for (int t = 0; t < tableZones[i].TableBehaviours.Length; t++)
                {
                    if (tableZones[i].TableBehaviours[t].IsOpened && !tableZones[i].TableBehaviours[t].IsAnimalPlaced && !tableZones[i].TableBehaviours[t].IsBusy)
                        return true;
                }
            }

            return false;
        }

        public TableBehaviour GetFreeTable()
        {
            for (int i = 0; i < tableZones.Length; i++)
            {
                for (int t = 0; t < tableZones[i].TableBehaviours.Length; t++)
                {
                    if (tableZones[i].TableBehaviours[t].IsOpened && !tableZones[i].TableBehaviours[t].IsAnimalPlaced && !tableZones[i].TableBehaviours[t].IsBusy)
                        return tableZones[i].TableBehaviours[t];
                }
            }

            return null;
        }

        public TableBehaviour GetRandomFreeTable()
        {
            List<TableBehaviour> availableTableBehaviours = new List<TableBehaviour>();

            for (int i = 0; i < tableZones.Length; i++)
            {
                for (int t = 0; t < tableZones[i].TableBehaviours.Length; t++)
                {
                    if (tableZones[i].TableBehaviours[t].IsOpened && !tableZones[i].TableBehaviours[t].IsAnimalPlaced && !tableZones[i].TableBehaviours[t].IsBusy)
                    {
                        availableTableBehaviours.Add(tableZones[i].TableBehaviours[t]);
                    }
                }
            }

            if (availableTableBehaviours.Count > 0)
                return availableTableBehaviours.GetRandomItem();

            return null;
        }

        public int GetFreeTablesAmount()
        {
            int freeTablesAmount = 0;

            for (int i = 0; i < tableZones.Length; i++)
            {
                for (int t = 0; t < tableZones[i].TableBehaviours.Length; t++)
                {
                    if (tableZones[i].TableBehaviours[t].IsOpened && !tableZones[i].TableBehaviours[t].IsAnimalPlaced && !tableZones[i].TableBehaviours[t].IsBusy)
                    {
                        freeTablesAmount++;
                    }
                }
            }

            return freeTablesAmount;
        }

        public void AddTableAnimal(AnimalBehaviour animalBehaviour)
        {
            if (tableAnimalsList.FindIndex(x => x == animalBehaviour) != -1)
                return;

            tableAnimalsAmount++;
            tableAnimalsList.Add(animalBehaviour);
        }

        public void RemoveTableAnimal(AnimalBehaviour animalBehaviour)
        {
            int animalIndex = tableAnimalsList.FindIndex(x => x == animalBehaviour);
            if (animalIndex != -1)
            {
                tableAnimalsAmount--;
                tableAnimalsList.RemoveAt(animalIndex);
            }
        }

        public AnimalBehaviour GetTableAnimalByItem(Item.Type itemType)
        {
            if (tableAnimalsAmount > 0)
            {
                for (int i = 0; i < tableAnimalsAmount; i++)
                {
                    if (!tableAnimalsList[i].IsBusy && tableAnimalsList[i].ActiveSickness.RequiredItem == itemType)
                    {
                        return tableAnimalsList[i];
                    }
                }
            }

            return null;
        }

        public bool HasRequiredItem()
        {
            if (tableAnimalsAmount > 0)
            {
                for (int i = 0; i < tableAnimalsAmount; i++)
                {
                    if (!tableAnimalsList[i].IsBusy)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Item.Type GetRequiredItem()
        {
            if (tableAnimalsAmount > 0)
            {
                for (int i = 0; i < tableAnimalsAmount; i++)
                {
                    if (!tableAnimalsList[i].IsBusy)
                    {
                        return tableAnimalsList[i].ActiveSickness.RequiredItem;
                    }
                }
            }

            return Item.Type.None; // CALL HasRequiredItem METHOD BEFORE CALLING GetRequiredItem
        }

        public AnimalBehaviour GetSickAnimal()
        {
            List<AnimalBehaviour> sickAnimals = new List<AnimalBehaviour>();

            if (tableAnimalsAmount > 0)
            {
                for (int i = 0; i < tableAnimalsAmount; i++)
                {
                    if (!tableAnimalsList[i].IsBusy)
                    {
                        sickAnimals.Add(tableAnimalsList[i]);
                    }
                }
            }

            if (sickAnimals.Count > 0)
                return sickAnimals.GetRandomItem();

            return null;
        }
        #endregion

        #region Waiting Animal
        public void AddWaitingAnimal(AnimalBehaviour animalBehaviour)
        {
            if (waitingAnimalsList.FindIndex(x => x == animalBehaviour) != -1)
                return;

            waitingAnimalsAmount++;
            waitingAnimalsList.Add(animalBehaviour);
        }

        public void RemoveWaitingAnimal(AnimalBehaviour animalBehaviour)
        {
            int animalIndex = waitingAnimalsList.FindIndex(x => x == animalBehaviour);
            if (animalIndex != -1)
            {
                waitingAnimalsAmount--;
                waitingAnimalsList.RemoveAt(animalIndex);
            }
        }

        public AnimalBehaviour GetFreeWaitingAnimal()
        {
            for (int i = 0; i < waitingAnimalsAmount; i++)
            {
                if (!waitingAnimalsList[i].IsBusy)
                {
                    return waitingAnimalsList[i];
                }
            }

            return null;
        }
        #endregion

        #region Dispensers
        public bool IsDispenserAllowed(Item.Type itemType)
        {
            for (int i = 0; i < dispensers.Length; i++)
            {
                if (dispensers[i].ItemType == itemType)
                {
                    if (dispensers[i].IsUnlocked)
                        return true;
                }
            }

            return false;
        }

        public DispenserBuilding GetDispenser(Item.Type itemType)
        {
            for (int i = 0; i < dispensers.Length; i++)
            {
                if (dispensers[i].ItemType == itemType)
                {
                    if (dispensers[i].IsUnlocked)
                        return dispensers[i];
                }
            }

            return null;
        }
        #endregion

        #region Load/Save
        public void Load(ZoneSave zoneSave)
        {
            placedCurrencyAmount = zoneSave.PlacedCurrencyAmount;
            isOpened = zoneSave.IsOpened;

            // Load tables
            if (zoneSave.TableZones != null)
            {
                for (int i = 0; i < zoneSave.TableZones.Length; i++)
                {
                    for (int z = 0; z < tableZones.Length; z++)
                    {
                        if (zoneSave.TableZones[i].ID == tableZones[z].ID)
                        {
                            tableZones[z].Load(zoneSave.TableZones[i]);
                        }
                    }
                }
            }

            // Load nurse
            if (zoneSave.NurseSettings != null)
            {
                nurseSpawner.Load(zoneSave.NurseSettings);
            }
        }

        public ZoneSave Save()
        {
            TableZoneBehaviour.SaveData[] tableZonesSave = new TableZoneBehaviour.SaveData[tableZones.Length];
            for (int i = 0; i < tableZonesSave.Length; i++)
            {
                tableZonesSave[i] = tableZones[i].Save();
            }

            return new ZoneSave(id, placedCurrencyAmount, isOpened, tableZonesSave, nurseSpawner.Save());
        }
        #endregion

        private void OnDrawGizmos()
        {
            if(!zonePurchaseCases.IsNullOrEmpty())
            {
                for(int i = 0; i < zonePurchaseCases.Length; i++)
                {
                    Transform purchaseTranform = zonePurchaseCases[i].ZonePurchaseTransform;
                    if(purchaseTranform != null)
                    {
                        Gizmos.DrawWireCube(purchaseTranform.position, new Vector3(4, 1, 4));
                    }
                }
            }
        }

        public void OnSceneSaving()
        {
            if(animalSpawner == null)
                animalSpawner = transform.GetComponentInChildren<AnimalSpawner>(false);

            if (physicsDoorBehaviour == null)
                physicsDoorBehaviour = transform.GetComponentInChildren<PhysicsDoorBehaviour>(false);

            if (nurseSpawner == null)
                nurseSpawner = transform.GetComponentInChildren<NurseSpawner>(false);

            if (moneyStackBehaviour == null)
                moneyStackBehaviour = transform.GetComponentInChildren<MoneyStackBehaviour>(false);

            if(secretaryBehaviours == null)
                secretaryBehaviours = transform.GetComponentInChildren<SecretaryBehaviour>(false);

            dispensers = transform.GetComponentsInChildren<DispenserBuilding>(false);
            tableZones = transform.GetComponentsInChildren<TableZoneBehaviour>(false);

            RuntimeEditorUtils.SetDirty(this);
        }

        public void Unload()
        {
            animalSpawner.DisableAutoSpawn();

            if (moneyStackBehaviour != null)
                moneyStackBehaviour.Unload();
        }
    }
}