using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class PlayerSkinData : AbstractSkinData
    {
        [SkinPreview]
        [SerializeField] Sprite preview;
        public Sprite Preview => preview;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;
    }
}