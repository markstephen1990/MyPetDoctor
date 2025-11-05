using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Strength Upgrade", menuName = "Data/Upgrades/Strength Upgrade")]
    public class StrengthUpgrade : Upgrade<StrengthUpgrade.StrengthStage>
    {
        public override void Init()
        {

        }

        [System.Serializable]
        public class StrengthStage : BaseUpgradeStage
        {
            [SerializeField] int playerAnimalCarryingAmount;
            public float PlayerAnimalCarryingAmount => playerAnimalCarryingAmount;

            [SerializeField] int playerItemCarryingAmount;
            public float PlayerItemCarryingAmount => playerItemCarryingAmount;
        }
    }
}