using UnityEngine;

namespace razz
{
    public class HitAnimSwitch : MonoBehaviour
    {
        [Tooltip("The Animator component to change animation")]
        public Animator animator;
        [Tooltip("Name of the animation state to play when hit")]
        public string animationState = "";
        [Tooltip("Duration in seconds for crossfading between animations")]
        public float transitionDuration = 0.5f;

        private int _hitStateHash;
        private int _previousStateHash;
        private bool _inHitAnimation = false;

        private void Start()
        {
            if (!string.IsNullOrEmpty(animationState))
            {
                _hitStateHash = Animator.StringToHash(animationState);
            }
        }

        public void StartHitAnimation()
        {
            if (animator == null || _hitStateHash == 0 || _inHitAnimation) return;

            _inHitAnimation = true;
            _previousStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            animator.CrossFadeInFixedTime(_hitStateHash, transitionDuration, 0, 0f);
            //animator.Play(_hitStateHash, 0, 0f);
        }

        public void StopHitAnimation()
        {
            if (animator == null || !_inHitAnimation) return;

            _inHitAnimation = false;
            if (_previousStateHash != 0)
            {
                animator.Play(_previousStateHash, 0, 0f);
            }
        }
    }
}
