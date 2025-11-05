using UnityEngine;
using Watermelon.SkinStore;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        [Header("References")]
        [SerializeField] UIController uiController;
        [SerializeField] CameraController cameraController;
        [SerializeField] MusicSource musicSource;

        private static LevelController levelController;
        private static ItemController itemController;
        private static UpgradesController upgradesController;
        private static ParticlesController particlesController;
        private static FloatingTextController floatingTextController;
        private static NavigationController navigationController;
        private static TutorialController tutorialController;
        private static SkinController skinsController;
        private static SkinStoreController skinStoreController;

        private void Awake()
        {
            instance = this;

            // Cache components
            CacheComponent(out levelController);
            CacheComponent(out itemController);
            CacheComponent(out upgradesController);
            CacheComponent(out particlesController);
            CacheComponent(out floatingTextController);
            CacheComponent(out navigationController);
            CacheComponent(out tutorialController);
            CacheComponent(out skinsController);
            CacheComponent(out skinStoreController);

            musicSource.Init();
            musicSource.Activate();

            cameraController.Initialise();
            PreviewCamera.Initialise();

            uiController.Init();

            skinsController.Init();
            skinStoreController.Init(skinsController);

            itemController.Init();

            upgradesController.Init();

            navigationController.Init();

            levelController.Init();

            particlesController.Init();

            floatingTextController.Init();

            tutorialController.Init();

            uiController.InitPages();
        }

        private void Start()
        {
            OnGameLoaded();
        }

        public static void OnGameLoaded()
        {
            UIController.ShowPage<UIGame>();

            // Unzoom camera
            CameraController.EnableCamera(CameraType.Gameplay);

            levelController.OnGameLoaded();

            GameLoading.MarkAsReadyToHide();
        }

        public static void Unload(SimpleCallback onSceneUnloaded)
        {
            Tween.RemoveAll();

            levelController.UnloadLevel(onSceneUnloaded);
            instance.cameraController.Unload();
        }

        private void OnDestroy()
        {
            Tween.RemoveAll();
        }

        #region Extensions
        public bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));

            component = null;

            return false;
        }
        #endregion
    }
}