using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] UIController uiController;
        [SerializeField] MusicSource musicSource;

        [Space]
        [SerializeField] Button world1Button;
        [SerializeField] Button world2Button;

        private void Awake()
        {
            uiController.Init();
            uiController.InitPages();

            // Activate menu music
            musicSource.Init();
            musicSource.Activate();

            // Add click events
            world1Button.onClick.AddListener(() => { LoadWorld(0); });
            world2Button.onClick.AddListener(() => { LoadWorld(1); });
        }

        public void LoadWorld(int worldID)
        {
            GlobalLevelSave globalLevelSaveData = SaveController.GetSaveObject<GlobalLevelSave>("globalLevelSave");
            globalLevelSaveData.CurrentLevelID = worldID;

            Overlay.Show(0.3f, () =>
            {
                SceneManager.LoadScene("Game");

                Overlay.Hide(0.3f);
            });
        }
    }
}