using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Data/Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;

        [BoxGroup("Gameplay", "Gameplay")]
        public AudioClip animalPickUpSound;
        [BoxGroup("Gameplay")]
        public AudioClip itemPickUpSound;

        [Space]
        [BoxGroup("Gameplay")]
        public AudioClip animalPlaceSound;
        [BoxGroup("Gameplay")]
        public AudioClip animalCureSound;

        [Space]
        [BoxGroup("Gameplay")]
        public AudioClip tableOpenSound;

        [Space]
        [BoxGroup("Gameplay")]
        public AudioClip locationOpenSound;

        [Space]
        [BoxGroup("Gameplay")]
        public AudioClip moneyPickUpSound;
        [BoxGroup("Gameplay")]
        public AudioClip moneyPlaceSound;
    }
}

// -----------------
// Audio Controller v 0.4
// -----------------