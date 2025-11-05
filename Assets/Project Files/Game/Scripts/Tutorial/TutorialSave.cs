using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class TutorialSave : ISaveObject
    {
        public TutorialStage TutorialStage;

        public TutorialSave()
        {
            TutorialStage = TutorialStage.None;
        }

        public void Flush()
        {

        }
    }
}