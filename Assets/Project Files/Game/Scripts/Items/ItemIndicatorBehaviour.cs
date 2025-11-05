using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ItemIndicatorBehaviour : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image iconImage;

        private TweenCase fadeTweenCase;

        public void Initialise(Item item)
        {
            iconImage.sprite = item.Icon;

            canvasGroup.alpha = 0.0f;
            fadeTweenCase = canvasGroup.DOFade(1.0f, 0.2f);
        }

        public void Disable()
        {
            if (fadeTweenCase != null && !fadeTweenCase.IsCompleted)
                fadeTweenCase.Kill();

            canvasGroup.alpha = 0.0f;

            gameObject.SetActive(false);
        }
    }
}