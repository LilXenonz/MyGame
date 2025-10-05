using UnityEngine;

namespace razz
{
    /*Handles all vehicle part animations when used by Interactor. 
    This class is a little messy. But this is written for all-in-one example.
    I've used ids instead of strings for Animation SetBools. Its 2x faster.
    Also since ProtoTruck has only one AnimationController, if any animation remains active, others wont work. 
    Because animation state needs to go back to Default state for other states to work.*/
    [HelpURL("https://negengames.com/interactor/components.html#vehiclepartcontrolscs")]
    [DisallowMultipleComponent]
    public class VehiclePartControls : MonoBehaviour
    {
        private Animator _vehicleAnimator;
        private InteractorObject[] _vehicleInteractorObjects;
        private bool[] _toggleStates;

        private bool _windshieldState;

        private void Start()
        {
            _vehicleAnimator = GetComponent<Animator>();
            _vehicleInteractorObjects = GetComponentsInChildren<InteractorObject>();
            _toggleStates = new bool[_vehicleInteractorObjects.Length];

            //These loops take all vehicle InteractorObjects, compare with Vehicle Animator parameters,
            //sets their hash values to their Ids if has same name.
            for (int i = 0; i < _vehicleInteractorObjects.Length; i++)
            {
                for (int a = 0; a < _vehicleAnimator.parameterCount; a++)
                {
                    if (_vehicleAnimator.parameters[a].name == _vehicleInteractorObjects[i].name)
                    {
                        _vehicleInteractorObjects[i].animationId = Animator.StringToHash(_vehicleAnimator.parameters[a].name);
                    }
                }
            }
        }

        private void Update()
        {
            if (InteractorInput.GetKeyDown(KeyCode.U)) ResetAnim();
        }

        private void ResetAnim()
        {
            for (int i = 0; i < _toggleStates.Length; i++)
            {
                if (_toggleStates[i])
                {
                    _toggleStates[i] = false;
                    _vehicleAnimator.SetBool(_vehicleAnimator.parameters[i].nameHash, false);
                }
            }
        }

        //If animation state is Default, set id part. If not, reset first.
        public void Animate(InteractorObject intObj)
        {
            if (intObj.animationId == 0) return;

            for (int i = 0; i < _vehicleAnimator.parameterCount; i++)
            {
                if (_vehicleAnimator.parameters[i].nameHash == intObj.animationId)
                {
                    if (!_toggleStates[i])
                    {
                        if (!_vehicleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Default"))
                        {
                            ResetAnim();
                        }
                        _vehicleAnimator.SetBool(intObj.animationId, true);
                        _toggleStates[i] = true;
                    }
                    else
                    {
                        _vehicleAnimator.SetBool(intObj.animationId, false);
                        _toggleStates[i] = false;
                    }
                }
            }
        }

        //Called by Interactor when used MultipleMoveBody
        public void ToggleWindshield()
        {
            if (!_windshieldState)
            {
                if (!_vehicleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Default"))
                {
                    ResetAnim();
                }
                _vehicleAnimator.SetBool("Windshield", true);
                _windshieldState = true;
            }
            else
            {
                _vehicleAnimator.SetBool("Windshield", false);
                _windshieldState = false;
            }
        }
    }
}
