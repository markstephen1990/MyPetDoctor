using UnityEngine;

namespace Watermelon
{
    public class LevelChangeZoneBehaviour : MonoBehaviour
    {
        [SerializeField] int levelIndex;

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                // Show fullscreen black overlay
                Overlay.Show(0.3f, () =>
                {
                    // Save the current state of the game
                    SaveController.Save(true);

                    // Unload the current level and all the dependencies
                    GameController.Unload(() =>
                    {
                        // Load next level
                        LevelController.ActivateLevel(levelIndex);

                        // Disable fullscreen black overlay
                        Overlay.Hide(0.3f, () =>
                        {
                            GameController.OnGameLoaded();
                        });
                    });
                });
            }
        }
    }
}