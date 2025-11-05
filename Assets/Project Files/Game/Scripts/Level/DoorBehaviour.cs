using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class DoorBehaviour : MonoBehaviour
    {
        [SerializeField] Transform leftDoorTransform;
        [SerializeField] Transform rightDoorTransform;

        [Space]
        [SerializeField] float leftDoorAngle = 98;
        [SerializeField] float rightDoorAngle = -98;

        [Space]
        [SerializeField] float openTime;
        [SerializeField] Ease.Type openEasing;
        [SerializeField] float closeTime;
        [SerializeField] Ease.Type closeEasing;

        private bool isOpened;
        private bool isMoving;

        private List<IDoorOpener> activeVisitors = new List<IDoorOpener>();

        private void Start()
        {

        }

        public void Open()
        {
            if (isMoving || isOpened)
                return;

            isMoving = true;

            leftDoorTransform.DOLocalRotate(Quaternion.Euler(0, leftDoorAngle, 0), openTime).SetEasing(openEasing);
            rightDoorTransform.DOLocalRotate(Quaternion.Euler(0, rightDoorAngle, 0), openTime).SetEasing(openEasing).OnComplete(delegate
            {
                isMoving = false;

                isOpened = true;
            });
        }

        public void Close()
        {
            if (isMoving || !isOpened)
                return;

            isMoving = true;

            leftDoorTransform.DOLocalRotate(Quaternion.Euler(0, 0, 0), closeTime).SetEasing(closeEasing);
            rightDoorTransform.DOLocalRotate(Quaternion.Euler(0, 0, 0), closeTime).SetEasing(closeEasing).OnComplete(delegate
            {
                isMoving = false;

                isOpened = false;
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            IDoorOpener visitor = other.GetComponent<IDoorOpener>();
            if (visitor != null)
            {
                // Check if visitor is already in list
                if (activeVisitors.Contains(visitor)) return;

                activeVisitors.Add(visitor);

                Open();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IDoorOpener visitor = other.GetComponent<IDoorOpener>();
            if (visitor != null)
            {
                int visitorIndex = activeVisitors.FindIndex(x => x == visitor);
                if (visitorIndex != -1)
                {
                    activeVisitors.RemoveAt(visitorIndex);

                    if (activeVisitors.Count == 0)
                        Close();
                }
            }
        }
    }
}