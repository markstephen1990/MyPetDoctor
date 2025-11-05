using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Movement Speed Upgrade", menuName = "Data/Upgrades/Movement Speed Upgrade")]
    public class MovementSpeedUpgrade : Upgrade<MovementSpeedUpgrade.MovementSpeedStage>
    {
        public override void Init()
        {

        }

        [System.Serializable]
        public class MovementSpeedStage : BaseUpgradeStage
        {
            [SerializeField] float playerMovementSpeed;
            public float PlayerMovementSpeed => playerMovementSpeed;

            [SerializeField] float playerAcceleration;
            public float PlayerAcceleration => playerAcceleration;
        }
    }
}