using Services;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace LoadScreen 
{
    public class LogoCanvasImageAnimation : CanvasImageAnimationContainer
    {
        private Image image;
        
        private void Start() 
        {
            image = GetComponent<Image>();
            FrameByFrameAnimation(image);
        }
    }

}