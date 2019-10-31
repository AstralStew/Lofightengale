using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    [AddComponentMenu("Paperticket/Input System")]
    public class InputSystem : MonoBehaviour {

        public static InputSystem instance;

        [Header("Raw Inputs")]

        public List<string> _InputList = new List<string>();

        [SerializeField] bool[] axisInputList; // Is this input an axis (true) or a button (false)
        int[] rawInputList; // A list populated by the raw (0/1) input registered in the current frame 
        int frameIterator; // A number representing which frame is the current one to write to
        string combinedInputs; // A single string that notes all the inputs registered in the current frame
        

        [Space(10)]
        
        [SerializeField] float _AxisDeadzone;

        [Header("Native Inputs")]

        public NativeInput[] _NativeInputTable;

        public int _InputBuffer = 9;
        [SerializeField] int _InputDelay = 5;


        [Header("Misc")]

        [SerializeField] bool _EnableGUI;

        [SerializeField] bool _Debug;
        string debugLog;


        int delay;

        public delegate void OnInputRegistered();
        public static event OnInputRegistered onInputRegistered;


        // Start is called before the first frame update
        void Awake() {

            // Ensure there is only one Input System instance
            if (instance == null) {
                instance = this;
            } else {
                Debug.LogError("["+name+"] ERROR -> Too many "+name+"s, destroying game object!");
                Destroy(gameObject);
            }

            // Setup the initial raw input list and native input table
            rawInputList = new int[_InputList.Count];
            combinedInputs = " ";
            ClearNativeInputTable();


        }

        // Update is called once per frame
        void Update() {

            //Retrieve the current inputs and save them as variables
            RetrieveInputs();

            //Save the inputs into the Native Input Table
            SaveInputs();

            //Increment the current frame of the NTI(wrap - around if necesary)
            frameIterator++;
            if (frameIterator >= _NativeInputTable.Length) {
                frameIterator = 0;
            }


        }

        void OnGUI() {

            if (_EnableGUI) {

                // Show the time since startup
                GUI.Label(new Rect(10, 10, 100, 20), (Mathf.Round(Time.realtimeSinceStartup * 100f) / 100f).ToString());

                //Show the inputs as they happen
                for (int i = 0; i < _InputList.Count; i++) {
                    GUI.Label(new Rect(10, 50 + (10*i), 100, 20), _InputList[i] + ": " + rawInputList[i]);
                }

                // Show the rolling Native Input Table
                for (int i = 0; i < _NativeInputTable.Length; i++) {
                    GUI.Label(new Rect(400, 50 + (10 * i), 800, 20), 
                        (i == (frameIterator-1) ? "*" : " ") + "f" + (i<10 ? "0"+i : ""+i) + ": " + 
                        _NativeInputTable[i].combinedInputs); 
                }

                

            }

        }





        void RetrieveInputs() {

            string inputName = "";
            combinedInputs = " ";
            
            // Iterate through the input names
            for (int i = 0; i < _InputList.Count; i++) {
                inputName = _InputList[i];

                // If it is an axis, check raw input against deadzone and previous state 
                if (axisInputList[i]) {                    

                    // If the input is currently held 
                    if (Input.GetAxisRaw(inputName) > _AxisDeadzone) {

                        // If the input was not previously held (down)
                        if (rawInputList[i] == 0 || rawInputList[i] == 3) {
                            rawInputList[i] = 2;
                            combinedInputs += "v" + inputName + ",";

                        // If the input was previously held (on)
                        } else if (rawInputList[i] == 1 || rawInputList[i] == 2) {
                            rawInputList[i] = 1;
                            combinedInputs += " " + inputName + ",";
                        }
                        
                    // If the input is not currently held 
                    } else {

                        // If the input was not previously held (off)
                        if (rawInputList[i] == 0 || rawInputList[i] == 3) {
                            rawInputList[i] = 0;
                            // If the input was previously held (up)
                        } else if (rawInputList[i] == 1 || rawInputList[i] == 2) {
                            rawInputList[i] = 3;
                            combinedInputs += "^" + inputName + ",";
                        }
                    }                    

                // If it is a button, use the in-built up/down functions
                } else {

                    if (Input.GetButtonUp(inputName)) {

                        rawInputList[i] = 3;
                        combinedInputs += "^" + inputName + ",";

                    } else if (Input.GetButtonDown(inputName)) {

                        rawInputList[i] = 2;
                        combinedInputs += "v" + _InputList[i] + ",";

                    } else if (Input.GetButton(inputName)) {

                        rawInputList[i] = 1;
                        combinedInputs += " " + inputName + ",";

                    } else {

                        rawInputList[i] = 0;

                    }
                }                                
            }
        }


        void SaveInputs() {

            // Do a delay if there is one       <- (this is shit)
            if (_InputDelay > 0) {
                if (delay > 0) {
                    delay -= 1;
                    return;
                }
                delay = _InputDelay;
            }
            
            // Save the raw input data
            System.Array.Copy(rawInputList, _NativeInputTable[frameIterator].rawInputs, rawInputList.Length);
            _NativeInputTable[frameIterator].combinedInputs = combinedInputs;

            // Send event if inputs have been registered this frame
            if (System.Array.IndexOf(rawInputList, 2) != -1) {
                onInputRegistered?.Invoke();
            }
            
            

        }



        public int InputIndexFromName( string input ) {

            // Check this is a valid input
            if (_InputList.Contains(input)) {

                return _InputList.IndexOf(input);
            
            } else {
                Debug.LogError("[" + name + "] ERROR -> Bad input!");
                return -1;
            }

        }

        public bool InputPresentInFrame( int inputIndex, int frameNo ) {

            if (_Debug) Debug.Log("[InputManager] Checking InputIndex(" + inputIndex +") at frameNo(" + frameNo + ")");

            // Check this is a valid input
            if (_InputList.Count > inputIndex) {

                // Match the provided index to the current frame is in the NTI             
                int frame = (frameIterator - frameNo + _InputBuffer) % _InputBuffer;

                //if (_Debug) Debug.Log("[InputManager] Adjusted frame(" + frame + ") = [iterator(" + frameIterator + ") - unadjusted frame(" + frameNo + ")] % input buffer(" + _InputBuffer + ")");

                // Check state of input during frame
                if (_NativeInputTable[frame].rawInputs[inputIndex] == 1 || _NativeInputTable[frame].rawInputs[inputIndex] == 2) {
                    if (_Debug) Debug.Log("[InputManager] InputIndex(" + inputIndex + ") found at frame(" + frame + ")");
                    return true;
                } else {
                    if (_Debug) Debug.Log("[InputManager] InputIndex(" + inputIndex + ") NOT found at frame(" + frame + ")");
                    return false;
                }

            } else {
                Debug.LogError("[" + name + "] ERROR -> Bad input!");
                return false;
            }


        }
        public bool InputPresentInFrame(string input, int frameNo ) {

            // Input is a combination of multiple distinct inputs 
            if (input.Contains("+")) {

                if (_Debug) Debug.Log("[InputManager] Multiple inputs parsed: " + input);

                // Save the inputs as seperate strings and iterate through them 
                string[] seperateInputs = input.Split(char.Parse("+"));
                for (int j = 0; j < seperateInputs.Length; j++) {

                    if (_Debug) Debug.Log("[InputManager] Checking for " + seperateInputs[j]);

                    // Check this input a valid input
                    if (_InputList.Contains(seperateInputs[j])) {

                        // Return false if any of the inputs are missing
                        if (!InputPresentInFrame(_InputList.IndexOf(seperateInputs[j]), frameNo)) {
                            return false;
                        }

                    } else {
                        Debug.LogError("[" + name + "] ERROR -> Bad input!");
                        return false;
                    }
                }

                return true;

            // Input is a standard single input
            } else {

                return InputPresentInFrame(InputIndexFromName(input), frameNo);

            }
        }


        public int InputStateInFrame( int inputIndex, int frameNo ) {

            if (_Debug) debugLog = "[InputManager] Checking InputIndex(" + inputIndex + ") at frameNo(" + frameNo + ")";
            

            // Match the provided index to the current frame in the NTI    
            int frame = (frameIterator - frameNo + _InputBuffer) % _InputBuffer;       // maybe +1 to brackets

            //if (_Debug) {
            //    Debug.Log("[InputManager] Adjusted frame(" + frame + ") = [iterator(" + frameIterator + ") - unadjusted frame(" + frameNo + ")] % input buffer(" + _InputBuffer + ")");
            //    Debug.Log("[InputManager] InputIndex(" + inputIndex + ") = " + _NativeInputTable[frame].rawInputs[inputIndex] + " at frameNo(" + frame + ")");
            //}

            if (_Debug) {
                debugLog += "\n [InputManager] Adjusted frame(" + frame + ") = [iterator(" + frameIterator + ") - unadjusted frame(" + frameNo + ")] % input buffer(" + _InputBuffer + ")";
                debugLog += "\n [InputManager] InputIndex(" + inputIndex + ") = " + _NativeInputTable[frame].rawInputs[inputIndex] + " at frameNo(" + frame + ")";
                Debug.Log(debugLog);
            }

            // Check state of input during frame
            return _NativeInputTable[frame].rawInputs[inputIndex];

        }
        public int InputStateInFrame( string input, int frameNo ) {

            return InputStateInFrame(InputIndexFromName(input), frameNo);

        }
        public int InputStateInFrame( int inputIndex, int frameNo, bool debug ) {
            if (debug) debugLog = "[InputManager] Checking InputIndex(" + inputIndex + ") at frameNo(" + frameNo + ")";
            
            // Match the provided index to the current frame in the NTI    
            int frame = (frameIterator - frameNo + _InputBuffer) % _InputBuffer;       // maybe +1 to brackets
            
            if (debug) {
                debugLog += "\n [InputManager] Adjusted frame(" + frame + ") = [iterator(" + frameIterator + ") - unadjusted frame(" + frameNo + ")] % input buffer(" + _InputBuffer + ")";
                debugLog += "\n [InputManager] InputIndex(" + inputIndex + ") = " + _NativeInputTable[frame].rawInputs[inputIndex] + " at frameNo(" + frame + ")";
                Debug.Log(debugLog);
            }

            // Check state of input during frame
            return _NativeInputTable[frame].rawInputs[inputIndex];
        }

        




        public void ClearNativeInputTable() {
                        
            // Clear the native input table
            _NativeInputTable = new NativeInput[_InputBuffer];
            for (int i = 0; i < _NativeInputTable.Length; i++) {
                _NativeInputTable[i] = new NativeInput();
                _NativeInputTable[i].rawInputs = new int[_InputList.Count];
                for (int j = 0; j < _InputList.Count; j++) {
                    _NativeInputTable[i].rawInputs[j] = 0;

                }
                _NativeInputTable[i].combinedInputs = " ";
            }

            frameIterator = 0;

        }


    }

}



//// Work out the Native Input Table 
//            for (int i = 0; i<_NativeInputTable.Length; i++) {

//                // Save the raw input data to the last frame of the Native Input Table
//                if (i == _NativeInputTable.Length - 1) {

//                    // Save the raw input data
//                    _NativeInputTable[i].rawInputs = rawInputList;
//                    _NativeInputTable[i].combinedInputs = combinedInputs;


//                // Increment the previous frame data
//                } else {
//                    _NativeInputTable[i].rawInputs = _NativeInputTable[i + 1].rawInputs;
//                    _NativeInputTable[i].combinedInputs = _NativeInputTable[i + 1].combinedInputs;
//                }

//            }

//            // Send event if inputs have been registered this frame
//            if (System.Array.IndexOf(rawInputList, 2) != -1 ) {
//                onInputRegistered?.Invoke();
//            }






//// Check the last frame for its input
//int inputState = InputStateInFrame(_InputList[i], 1);

//                    // If the input is currently held 
//                    if (Input.GetAxisRaw(_InputList[i]) > _AxisDeadzone) {
                        
//                        // If the input was not previously held (down)
//                        if (inputState == 0 || inputState == 3) {
//                            rawInputList[i] = 2;
//                            combinedInputs += "v" + _InputList[i] + ",";

//                        // If the input was previously held (on)
//                        } else if (inputState == 1 || inputState == 2 ) {
//                            rawInputList[i] = 1;
//                            combinedInputs += " " + _InputList[i] + ",";
//                        } 

//                    // If the input is not currently held 
//                    } else {

//                        // If the input was not previously held (off)
//                        if (inputState == 0 || inputState == 3) {
//                            rawInputList[i] = 0;
//                        // If the input was previously held (up)
//                        } else if (inputState == 1 || inputState == 2) {
//                            rawInputList[i] = 3;
//                            combinedInputs += "^" + _InputList[i] + ",";
//                        }
//                    }