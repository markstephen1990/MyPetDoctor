using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class SecretaryBehaviour : MonoBehaviour
    {
        [SerializeField] SecretaryInteractionZone secretaryInteractionZone;

        private Zone zone;

        private UISecretaryWindow secretaryWindow;

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            secretaryInteractionZone.Initialise(this);

            // Get secretary window
            secretaryWindow = UIController.GetPage<UISecretaryWindow>();
        }

        public void OnSecretaryZoneActivated()
        {
            // Apply zone settings
            secretaryWindow.ApplyNurseSettings(zone);

            // Open page
            UIController.ShowPage<UISecretaryWindow>();
        }
    }
}