using UnityEngine;

namespace Watermelon
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] GameObject arrowPrefab;
        [SerializeField] TutorialStage defaultTutorialStage = TutorialStage.PickFirstAnimal;

        private static TutorialStage currentTutorialStage = TutorialStage.None;
        public static TutorialStage CurrentTutorialStage => currentTutorialStage;

        // Arrow
        private static Pool arrowPool;

        // Events
        public static OnTutorialStageCompletedCallback OnTutorialStageCompleted;

        public static OnZoneOpenedCallback OnZoneOpenedEvent;
        public static OnAnimalPickedCallback OnAnimalPickedEvent;
        public static OnAnimalPlacedOnTableCallback OnAnimalPlacedOnTableEvent;
        public static OnItemPickedCallback OnItemPickedEvent;
        public static OnAnimalCuredCallback OnAnimalCuredEvent;
        public static OnMoneyPickedCallback OnMoneyPickedEvent;
        public static OnTableUnlockedCallback OnTableUnlockedEvent;
        public static OnTableZoneUnlockedCallback OnTableZoneUnlockedEvent;
        public static OnNurseHiredCallback OnNurseHiredEvent;

        private static TutorialSave tutorialSave;

        public void Init()
        {
            // Create arrow pool
            arrowPool = new Pool(arrowPrefab, arrowPrefab.name);

            // Get tutorial save data
            tutorialSave = SaveController.GetSaveObject<TutorialSave>("tutorial");

            // Load tutorial stage if it's exists
            if (tutorialSave != null && tutorialSave.TutorialStage != TutorialStage.None)
            {
                EnableTutorial(tutorialSave.TutorialStage);
            }
            else
            {
                EnableTutorial(defaultTutorialStage);
            }
        }

        private void OnDestroy()
        {
            arrowPool?.Destroy();
        }

        [Button("Finish Tutorial")]
        public void FinishTutorial()
        {
            EnableTutorial(TutorialStage.Done);
        }

        public static void EnableTutorial(TutorialStage tutorialStage)
        {
            // Store previous stage
            TutorialStage previousStage = currentTutorialStage;

            // Invoke complete stage method
            OnStageCompleted(currentTutorialStage);

            // Rewrite stage
            currentTutorialStage = tutorialStage;

            // Save stage
            tutorialSave.TutorialStage = currentTutorialStage;

            // Invoke stage event
            OnTutorialStageCompleted?.Invoke(tutorialStage, previousStage);

            SaveController.MarkAsSaveIsRequired();
        }

        private static void OnStageCompleted(TutorialStage tutorialStage)
        {

        }

        public static TutorialArrowBehaviour CreateTutorialArrow(Vector3 position)
        {
            GameObject arrowObject = arrowPool.GetPooledObject();
            arrowObject.transform.position = position;
            arrowObject.transform.rotation = Quaternion.identity;

            TutorialArrowBehaviour tutorialArrowBehaviour = arrowObject.GetComponent<TutorialArrowBehaviour>();
            tutorialArrowBehaviour.Activate();

            return tutorialArrowBehaviour;
        }

        #region Callback
        public static void OnZoneOpened(Zone zone)
        {
            OnZoneOpenedEvent?.Invoke(zone);
        }

        public static void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {
            OnAnimalPickedEvent?.Invoke(animalBehaviour);
        }

        public static void OnAnimalPlacedOnTable(AnimalBehaviour animalBehaviour, TableBehaviour tableBehaviour)
        {
            OnAnimalPlacedOnTableEvent?.Invoke(animalBehaviour, tableBehaviour);
        }

        public static void OnAnimalCured(AnimalBehaviour animalBehaviour)
        {
            OnAnimalCuredEvent?.Invoke(animalBehaviour);
        }

        public static void OnItemPicked(Item item)
        {
            OnItemPickedEvent?.Invoke(item);
        }

        public static void OnMoneyPicked()
        {
            OnMoneyPickedEvent?.Invoke();
        }

        public static void OnTableUnlocked(TableBehaviour tableBehaviour)
        {
            OnTableUnlockedEvent?.Invoke(tableBehaviour);
        }

        public static void OnTableZoneUnlocked(TableZoneBehaviour tableZoneBehaviour)
        {
            OnTableZoneUnlockedEvent?.Invoke(tableZoneBehaviour);
        }

        public static void OnNurseHired()
        {
            OnNurseHiredEvent?.Invoke();
        }
        #endregion

        public delegate void OnTutorialStageCompletedCallback(TutorialStage tutorialStage, TutorialStage previousStage);
        public delegate void OnZoneOpenedCallback(Zone zone);
        public delegate void OnAnimalPickedCallback(AnimalBehaviour animalBehaviour);
        public delegate void OnAnimalPlacedOnTableCallback(AnimalBehaviour animalBehaviour, TableBehaviour tableBehaviour);
        public delegate void OnItemPickedCallback(Item item);
        public delegate void OnAnimalCuredCallback(AnimalBehaviour animalBehaviour);
        public delegate void OnMoneyPickedCallback();
        public delegate void OnTableUnlockedCallback(TableBehaviour tableBehaviour);
        public delegate void OnTableZoneUnlockedCallback(TableZoneBehaviour tableZoneBehaviour);
        public delegate void OnNurseHiredCallback();
    }
}