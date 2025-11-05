using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class WaitingIndicatorBehaviour : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image fillbarImage;

        private bool isActive;
        public bool IsActive => isActive;

        private float state;

        private float duration;
        private SimpleCallback completeCallback;

        private TweenCase fadeTweenCase;

        public void Initialise(float duration, SimpleCallback completeCallback)
        {
            this.duration = duration;
            this.completeCallback = completeCallback;

            state = 0;

            if (fadeTweenCase != null && !fadeTweenCase.IsCompleted)
                fadeTweenCase.Kill();

            fillbarImage.fillAmount = 1.0f;

            canvasGroup.alpha = 0.0f;
            fadeTweenCase = canvasGroup.DOFade(1.0f, 0.4f);

            isActive = true;
        }

        private void Update()
        {
            if (isActive)
            {
                state += Time.deltaTime / duration;

                fillbarImage.fillAmount = Mathf.Clamp01(1.0f - state);

                if (state >= 1.0f)
                {
                    completeCallback?.Invoke();

                    Disable();
                }
            }
        }

        public void Disable()
        {
            isActive = false;

            completeCallback = null;

            if (fadeTweenCase != null && !fadeTweenCase.IsCompleted)
                fadeTweenCase.Kill();

            fadeTweenCase = canvasGroup.DOFade(0.0f, 0.4f).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }
    }
}