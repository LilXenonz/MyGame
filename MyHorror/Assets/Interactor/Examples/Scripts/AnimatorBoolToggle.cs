using UnityEngine;

namespace razz
{
    public class AnimatorBoolToggle : MonoBehaviour
    {
        private Animator _animator;
        private bool _used;
        private string _usedString;

        void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetAnimatorParamFalse()
        {
            if (!_animator) return;
            if (string.IsNullOrEmpty(_usedString)) return;

            _used = false;
            _animator.SetBool(_usedString, _used);
        }

        public void ToggleAnimation(GameObject objectName)
        {
            if (!_animator) return;

            _usedString = objectName.name;

            if (!_used)
            {
                _animator.SetBool(objectName.name, true);
                _used = true;
            }
            else
            {
                _animator.SetBool(objectName.name, false);
                _used = false;
            }
        }
    }
}
