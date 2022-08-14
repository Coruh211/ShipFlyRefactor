using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Services
{
    public abstract class CanvasImageAnimationContainer : MonoBehaviour
    {
        [SerializeField] private Sprite[] logoAnimSprites;
        [SerializeField] private float speedSwitchSprite;

        private int actualSprite;

        protected void FrameByFrameAnimation(Image animObject)
        {
            if (actualSprite == logoAnimSprites.Length)
            {
                actualSprite = 0;
            }

            Observable.Timer(speedSwitchSprite.sec()).TakeUntilDisable(gameObject)
                .Subscribe(x =>
                {
                    
                    animObject.sprite = logoAnimSprites[actualSprite];
                    actualSprite++;
                    FrameByFrameAnimation(animObject);
                });
        }
    }
}
