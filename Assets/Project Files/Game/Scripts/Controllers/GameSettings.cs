using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class GameSettings
    {
        [Header("Rate Us")]
        [SerializeField] string appleGameID = "";
        public string AppleGameID => appleGameID;
    }
}