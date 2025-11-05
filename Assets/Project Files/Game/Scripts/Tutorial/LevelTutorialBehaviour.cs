using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class LevelTutorialBehaviour : ExtraLevelBehavior
    {
        private const float ARROW_DISABLE_DISTANCE = 7f;

        private const Animal.Type FIRST_ANIMAL_TYPE = Animal.Type.Cat_01;
        private const SicknessType FIRST_SICKNESS_TYPE = SicknessType.Dirt;
        private const Item.Type FIRST_ITEM_TYPE = Item.Type.Soap;

        private const Animal.Type SECOND_ANIMAL_TYPE = Animal.Type.Cat_02;
        private const SicknessType SECOND_SICKNESS_TYPE = SicknessType.Infection;
        private const Item.Type SECOND_ITEM_TYPE = Item.Type.Injection;

        [SerializeField] Color arrowColor = Color.white;

        [SerializeField] Zone firstZone;

        [SerializeField] AnimalSpawner animalSpawner;
        [SerializeField] Transform animalTableTransform;
        [SerializeField] Transform hireZoneTransform;

        [Space]
        [SerializeField] DispenserBuilding soapDispenser;
        [SerializeField] DispenserBuilding injectionDispenser;

        [Space]
        [SerializeField] MoneyStackBehaviour moneyStackBehaviour;

        [Space]
        [SerializeField] SecretaryInteractionZone secretaryInteractionZone;

        [Space]
        [SerializeField] TableBehaviour secondTableBehaviour;
        [SerializeField] TableBehaviour thirdTableBehaviour;
        [SerializeField] TableBehaviour fourthTableBehaviour;
        [SerializeField] TableZoneBehaviour tableZoneBehaviour;
        [SerializeField] TableZoneBehaviour secondTableZoneBehaviour;

        [Space]
        [SerializeField] Zone secondZone;
        [SerializeField] Transform secondZonePurchaseTransform;

        // Stage 1
        private VisitorBehaviour spawnedVisitor;
        private AnimalBehaviour spawnedAnimal;

        private NavigationController.ArrowCase navigationArrow;

        private TutorialArrowBehaviour tutorialArrowBehaviour;

        private Level level;

        private VirtualCamera tutorialVirtualCamera;

        private bool isItemPicked;
        private bool isAnimalPicked;

        private UIGame uiGame;

        public override void Initialise(Level level)
        {
            this.level = level;

            // Get UI Game Page
            uiGame = UIController.GetPage<UIGame>();

            // Get tutorial virtual camera
            tutorialVirtualCamera = CameraController.GetCamera(CameraType.Preview);

            // Subscribe to tutorial callback
            TutorialController.OnTutorialStageCompleted += OnTutorialStageCompleted;

            TutorialController.OnZoneOpenedEvent += OnZoneOpened;
            TutorialController.OnAnimalPickedEvent += OnAnimalPicked;
            TutorialController.OnAnimalCuredEvent += OnAnimalCured;
            TutorialController.OnAnimalPlacedOnTableEvent += OnAnimalPlacedOnTable;
            TutorialController.OnItemPickedEvent += OnItemPicked;
            TutorialController.OnMoneyPickedEvent += OnMoneyPicked;
            TutorialController.OnTableUnlockedEvent += OnTableUnlocked;
            TutorialController.OnTableZoneUnlockedEvent += OnTableZoneUnlocked;
            TutorialController.OnNurseHiredEvent += OnNurseHired;

            // Activate stage
            ActivateStage(TutorialController.CurrentTutorialStage);
        }

        private void OnDisable()
        {
            TutorialController.OnTutorialStageCompleted -= OnTutorialStageCompleted;

            TutorialController.OnZoneOpenedEvent -= OnZoneOpened;
            TutorialController.OnAnimalPickedEvent -= OnAnimalPicked;
            TutorialController.OnAnimalCuredEvent -= OnAnimalCured;
            TutorialController.OnAnimalPlacedOnTableEvent -= OnAnimalPlacedOnTable;
            TutorialController.OnItemPickedEvent -= OnItemPicked;
            TutorialController.OnMoneyPickedEvent -= OnMoneyPicked;
            TutorialController.OnTableUnlockedEvent -= OnTableUnlocked;
            TutorialController.OnTableZoneUnlockedEvent -= OnTableZoneUnlocked;
            TutorialController.OnNurseHiredEvent -= OnNurseHired;
        }

        private void OnNurseHired()
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.HireNurse)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.UnlockSecondTableZone);
            }
        }

        private void OnTableZoneUnlocked(TableZoneBehaviour tableZoneBehaviour)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockTableZone)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.HireNurse);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockSecondTableZone)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.UnlockNextLocation);

                // Disable joystick
                Control.CurrentControl.DisableMovementControl();

                tutorialVirtualCamera.SetTarget(secondZonePurchaseTransform);

                CameraController.EnableCamera(CameraType.Preview);

                Tween.DelayedCall(2.0f, delegate
                {
                    CameraController.EnableCamera(CameraType.Gameplay);

                    // Enable joystick
                    Control.CurrentControl.EnableMovementControl();
                });
            }
        }

        private void OnMoneyPicked()
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.PickMoney)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.UnlockFirstTable);
            }
        }

        private void OnZoneOpened(Zone zone)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockNextLocation)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.Done);
            }
        }

        private void OnItemPicked(Item item)
        {
            if (isItemPicked)
                return;

            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal && item.ItemType == FIRST_ITEM_TYPE)
            {
                isItemPicked = true;

                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal && item.ItemType == SECOND_ITEM_TYPE)
            {
                isItemPicked = true;

                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
        }

        private void OnAnimalPlacedOnTable(AnimalBehaviour animalBehaviour, TableBehaviour tableBehaviour)
        {
            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal)
            {
                isAnimalPicked = false;

                // Block dispensers
                injectionDispenser.SetUnlockState(false);
                soapDispenser.SetUnlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(soapDispenser.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, soapDispenser.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Disable joystick
                Control.CurrentControl.DisableMovementControl();

                tutorialVirtualCamera.SetTarget(soapDispenser.transform);

                CameraController.EnableCamera(CameraType.Preview);

                Tween.DelayedCall(2.0f, delegate
                {
                    CameraController.EnableCamera(CameraType.Gameplay);

                    // Enable joystick
                    Control.CurrentControl.EnableMovementControl();
                });
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal)
            {
                isAnimalPicked = false;

                // Block dispensers
                injectionDispenser.SetUnlockState(true);
                soapDispenser.SetUnlockState(true);

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(injectionDispenser.transform.position + new Vector3(0, 8, 0));

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, injectionDispenser.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
        }

        private void OnAnimalCured(AnimalBehaviour animalBehaviour)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal)
            {
                isItemPicked = false;

                TutorialController.EnableTutorial(TutorialStage.PickSecondAnimal);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal)
            {
                isItemPicked = false;

                TutorialController.EnableTutorial(TutorialStage.PickMoney);
            }
        }

        private void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {
            if (isAnimalPicked)
                return;

            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal)
            {
                isAnimalPicked = true;

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal)
            {
                isAnimalPicked = true;

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
        }

        private void OnTableUnlocked(TableBehaviour tableBehaviour)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockFirstTable)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                TutorialController.EnableTutorial(TutorialStage.UnlockSecondTable);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockSecondTable)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                TutorialController.EnableTutorial(TutorialStage.UnlockTableZone);
            }
        }

        public override void OnGameLoaded()
        {

        }

        private void ActivateStage(TutorialStage tutorialStage)
        {
            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (tutorialStage == TutorialStage.PickFirstAnimal)
            {
                secondZone.Lock();

                // Block dispensers
                injectionDispenser.SetUnlockState(false);
                soapDispenser.SetUnlockState(false);

                // Disable auto spawn
                animalSpawner.DisableAutoSpawn();

                // Disable tables
                secondTableBehaviour.Lock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                NavMeshController.InvokeOrSubscribe(delegate
                {
                    spawnedVisitor = animalSpawner.SpawnVisitorWithRandomAnimal(FIRST_ANIMAL_TYPE, FIRST_SICKNESS_TYPE);
                    spawnedAnimal = spawnedVisitor.AnimalBehaviour;

                    navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, spawnedAnimal.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                    // Disable joystick
                    Control.CurrentControl.DisableMovementControl();

                    tutorialVirtualCamera.SetTarget(spawnedVisitor.transform);

                    CameraController.EnableCamera(CameraType.Preview);

                    Tween.DelayedCall(2.0f, delegate
                    {
                        CameraController.EnableCamera(CameraType.Gameplay);

                        // Enable joystick
                        Control.CurrentControl.EnableMovementControl();
                    });
                });

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.PickSecondAnimal)
            {
                secondZone.Lock();

                // Block dispensers
                injectionDispenser.SetUnlockState(false);
                soapDispenser.SetUnlockState(true);

                // Disable auto spawn
                animalSpawner.DisableAutoSpawn();

                // Disable tables
                secondTableBehaviour.Lock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                NavMeshController.InvokeOrSubscribe(delegate
                {
                    spawnedVisitor = animalSpawner.SpawnVisitorWithRandomAnimal(SECOND_ANIMAL_TYPE, SECOND_SICKNESS_TYPE);
                    spawnedAnimal = spawnedVisitor.AnimalBehaviour;

                    if (navigationArrow != null)
                        navigationArrow.DisableArrow();

                    navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, spawnedAnimal.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);
                });

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.PickMoney)
            {
                if (moneyStackBehaviour.GetPickedMoney(CurrencyType.Coins) == 0)
                {
                    int totalMoney = 40 - CurrencyController.Get(CurrencyType.Coins);
                    if (totalMoney != 0)
                    {
                        moneyStackBehaviour.AddMoney(new CurrencyAmount(CurrencyType.Coins, 40));
                    }
                }

                secondZone.Lock();

                // Enable auto animal spawner
                animalSpawner.StartAutoSpawn(0.0f);

                // Disable tables
                secondTableBehaviour.Lock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(moneyStackBehaviour.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, moneyStackBehaviour.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockFirstTable)
            {
                secondZone.Lock();

                // Disable tables
                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(secondTableBehaviour.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, secondTableBehaviour.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockSecondTable)
            {
                secondZone.Lock();

                // Disable tables
                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(thirdTableBehaviour.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockTableZone)
            {
                secondZone.Lock();

                // Disable tables
                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Unlock();

                tableZoneBehaviour.Unlock();

                secondTableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(tableZoneBehaviour.transform.position + new Vector3(0, 8, 0));

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.HireNurse)
            {
                secondZone.Lock();

                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Unlock();

                tableZoneBehaviour.Unlock();
                secondTableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(false);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(hireZoneTransform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, hireZoneTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockSecondTableZone)
            {
                secondZone.Lock();

                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Unlock();

                tableZoneBehaviour.Unlock();
                secondTableZoneBehaviour.Unlock();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(secondTableZoneBehaviour.transform.position + new Vector3(0, 8, 0));

                // Enable interstitial
                AdsManager.SetInterstitialDelayTime(0);
            }
            else if (tutorialStage == TutorialStage.UnlockNextLocation)
            {
                secondZone.Unlock();

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(secondZonePurchaseTransform.position + new Vector3(0, 8, 0));
            }
        }

        private void DisableStage(TutorialStage tutorialStage)
        {

        }

        private void OnTutorialStageCompleted(TutorialStage tutorialStage, TutorialStage previousStage)
        {
            if (previousStage != TutorialStage.None)
                DisableStage(previousStage);

            ActivateStage(tutorialStage);
        }
    }
}