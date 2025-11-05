using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Pick Up Speed Upgrade", menuName = "Data/Upgrades/Pick Up Speed Upgrade")]
    public class PickUpSpeedUpgrade : Upgrade<PickUpSpeedUpgrade.PickUpSpeedStage>
    {
        public override void Init()
        {

        }

        [System.Serializable]
        public class PickUpSpeedStage : BaseUpgradeStage
        {
            [SerializeField] float itemPickUpDuration = 0.3f;
            public float ItemPickUpDuration => itemPickUpDuration;

            [SerializeField] float animalPickUpDuration = 0.3f;
            public float AnimalPickUpDuration => animalPickUpDuration;
        }
    }
}