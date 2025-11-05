using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ZonePurchaseCase
    {
        [SerializeField] Transform zonePurchaseTransform;
        public Transform ZonePurchaseTransform => zonePurchaseTransform;

        [SerializeField] Zone linkedZone;
        public Zone LinkedZone => linkedZone;
    }
}