using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    public class InputSystem2 : MonoBehaviour {

        public static InputSystem2 instance;

        [Header("Raw Inputs")]

        public List<string> _InputList = new List<string>();

        [SerializeField] bool[] axisInputList; // Is this input an axis (true) or a button (false)

        bool newInputsThisFrame;


        [Space(10)]

        [SerializeField] float _AxisDeadzone;

        [Header("Native Inputs")]

        public int[] _NativeInputTable;
        
        public int _InputBuffer = 9;


        [Header("Misc")]

        [SerializeField] bool _EnableGUI;

        [SerializeField] bool _Debug;


        int delay;

        public delegate void OnInputRegistered();
        public static event OnInputRegistered onInputRegistered;


        // Start is called before the first frame update
        void Awake() {

            // Ensure there is only one Input System instance
            if (instance == null) {
                instance = this;
            } else {
                Debug.LogError("[" + name + "] ERROR -> Too many " + name + "s, destroying game object!");
                Destroy(gameObject);
            }

            ClearNativeInputTable();


        }

        // Update is called once per frame
        void Update() {

            //Retrieve the current inputs and save them as variables
            RetrieveInputs();
            
        }
        


        void RetrieveInputs() {

            string inputName = "";
            newInputsThisFrame = false;

            // Iterate through the input names
            for (int i = 0; i < _InputList.Count; i++) {
                inputName = _InputList[i];

                // If it is an axis, check raw input against deadzone and previous state 
                if (axisInputList[i]) {

                    // If the input is currently held, set to input buffer
                    if (Input.GetAxisRaw(inputName) > _AxisDeadzone) {
                        _NativeInputTable[i] = _InputBuffer;
                        newInputsThisFrame = true;

                    // If the input is not held, decrement by 1
                    } else {
                        _NativeInputTable[i] = Mathf.Clamp(_NativeInputTable[i] - 1, 0, _InputBuffer);
                    }

                // If it is a button, use the in-built get button function
                } else {

                    // If the input is currently held, set to input buffer
                    if (Input.GetButton(inputName)) {
                        _NativeInputTable[i] = _InputBuffer;
                        newInputsThisFrame = true;

                    // If the input is not held, decrement by 1
                    } else {
                        _NativeInputTable[i] = Mathf.Clamp(_NativeInputTable[i] - 1, 0, _InputBuffer);
                    }

                }
            }
            
            // Send event if there were any new inputs this frame
            if (newInputsThisFrame) onInputRegistered?.Invoke();

        }




        public void ClearNativeInputTable() {

            // Clear the native input table
            _NativeInputTable = new int[_InputList.Count];
            for (int i = 0; i < _NativeInputTable.Length; i++) {
                _NativeInputTable[i] = 0;
            }

        }





        void OnGUI() {

            if (_EnableGUI) {

                for (int i = 0; i < _InputList.Count; i++) {
                    GUI.Label(new Rect(10, 10 + (10 * i), 100, 20), _InputList[i] + ": " + _NativeInputTable[i]);
                }

                GUI.Label(new Rect(100, 10, 100, 20), (Mathf.Round(Time.realtimeSinceStartup * 100f) / 100f).ToString());


            }

        }




    }

}