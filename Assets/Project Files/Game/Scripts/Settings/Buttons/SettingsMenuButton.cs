using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class SettingsMenuButton : SettingsButtonBase
    {
        public override void Init()
        {
            gameObject.SetActive(SceneUtils.DoesSceneExist("Menu"));
        }

        public override void OnClick()
        {
            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            // Show fullscreen black overlay
            Overlay.Show(0.3f, () =>
            {
                // Save the current state of the game
                SaveController.Save(true);

                // Unload the current level and all the dependencies
                GameController.Unload(() =>
                {
                    SceneManager.LoadScene("Menu");

                    // Disable fullscreen black overlay
                    Overlay.Hide(0.3f);
                });
            });
        }

        public override void Select()
        {
            IsSelected = true;

            Button.Select();

            EventSystem.current.SetSelectedGameObject(null); //clear any previous selection (best practice)
            EventSystem.current.SetSelectedGameObject(Button.gameObject, new BaseEventData(EventSystem.current));
        }

        public override void Deselect()
        {
            IsSelected = false;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}