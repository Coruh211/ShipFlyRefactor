using System.Collections;
using UnityEngine;

namespace FSG.MeshAnimator
{
    public class AnimatorStateMachine : MonoBehaviour
    {
        public MeshAnimatorBase meshAnimator;
        public bool crossfade = false;
        public float crossfadeDuration = 0.25f;

        void OnEnable()
        {
            meshAnimator.defaultAnimation = meshAnimator.animations[1];
            meshAnimator.Play();
            meshAnimator.OnAnimationFinished += OnAnimationFinished;
        }
        void OnAnimationFinished(string anim)
        {
            int newAnim = 0;
            switch (anim)
            {
                case "BreathingIdle":
                    newAnim = 4;
                    break;
                case "Running":
                    newAnim = 8;
                    break;
                case "RunningBackward":
                    newAnim = 9;
                    break;
                case "LeftStrafe":
                    newAnim = 7;
                    break;
                case "RightStrafe":
                    newAnim = 1;
                    break;
            }
            if (crossfade)
                meshAnimator.Crossfade(newAnim, crossfadeDuration);
            else
                meshAnimator.Play(newAnim);
        }
    }
}