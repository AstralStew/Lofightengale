﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    [AddComponentMenu("Paperticket/Command Manager")]
    public class CommandManager : MonoBehaviour {

        BasePlayer basePlayer;
        public CommandList _CommandList;

        
        [Header("Misc")]

        [SerializeField] bool _DebugManager;
        [SerializeField] bool _DebugCommands;

        //[SerializeField] bool _DontClearInputBuffer;
                       

        public delegate void OnCommandRegistered( Command command );
        public static event OnCommandRegistered onCommandRegistered;


        void OnEnable() {

            basePlayer = basePlayer ?? GetComponentInParent<BasePlayer>();
            if(basePlayer == null) {
                Debug.LogError("[CommandManager] ERROR -> No BasePlayer found! Child this object to a BasePlayer script!");
                enabled = false;
            }

            // Disable this component if no commands have been created
            if (_CommandList.commandList.Length == 0) {
                Debug.LogError("[CommandManager] ERROR -> Bad command list! Disabling " + name);
                enabled = false;
            }            

            CompileCommands();

            InputSystem.onInputRegistered += CheckCommands;
            basePlayer.animationManager.onAnimationFinished += CheckCommands;
            basePlayer.animationManager.onAnimationStarted += CheckCommands;
        }

        void OnDisable() {
            InputSystem.onInputRegistered -= CheckCommands;
            basePlayer.animationManager.onAnimationFinished -= CheckCommands;
            basePlayer.animationManager.onAnimationStarted -= CheckCommands;
        }



        void CompileCommands() {

            InputSystem inputSystem = InputSystem.instance;

            // Go through each of the commands in the commands list
            for (int i = 0; i < _CommandList.commandList.Length; i++) {


                Command command = _CommandList.commandList[i];
                string[] commandInputs = command.commandInputs;

                if (_DebugCommands) Debug.Log("[CommandManager] Checking Command (" + command.commandName + ")");

                // Create an array of native inputs, each representing a command step
                command.commandSteps = new NativeInput[commandInputs.Length];

                // Go through each command step one-by-one
                for (int j = 0; j < commandInputs.Length; j++) {

                    if (_DebugManager) Debug.Log("[CommandManager] Checking command step (" + commandInputs[j] + ")");

                    // Create a native input entry for the current command step
                    command.commandSteps[j] = new NativeInput();
                    command.commandSteps[j].rawInputs = new int[inputSystem._InputList.Count];
                    for (int k = 0; k < command.commandSteps[j].rawInputs.Length; k++) {
                        command.commandSteps[j].rawInputs[k] = 0;
                    }
                    command.commandSteps[j].combinedInputs = commandInputs[j];

                    // Save each command step as seperate strings of inputs and iterate through them 
                    string[] seperateInputs = commandInputs[j].Split(char.Parse("+"));

                    // Go through each distinct string input in this particular command step
                    for (int k = 0; k < seperateInputs.Length; k++) {

                        

                        // Check the string input is preceded by ^ (meaning the input must be released)
                        if (inputSystem._InputList.Contains(seperateInputs[k].Substring(1))) {

                            if (_DebugManager) Debug.Log("[CommandManager] Saving command step input (" + seperateInputs[k] + ") in native input table");

                            // Set the appropriate raw input for this command step 
                            if (seperateInputs[k].StartsWith("^")) {
                                _CommandList.commandList[i].commandSteps[j].rawInputs[inputSystem._InputList.IndexOf(seperateInputs[k].Substring(1))] = 3;
                            } else if (seperateInputs[k].StartsWith("v")) {
                                _CommandList.commandList[i].commandSteps[j].rawInputs[inputSystem._InputList.IndexOf(seperateInputs[k].Substring(1))] = 2;
                            } else {
                                Debug.LogError("[CommandManager] ERROR -> Bad input qualifier (" + seperateInputs[k] + ")!");
                            }                            

                        } else if (inputSystem._InputList.Contains(seperateInputs[k])) {

                            if (_DebugManager) Debug.Log("[CommandManager] Saving command step input (" + seperateInputs[k] + ") in native input table");

                            // Set the raw input for this command step 
                            _CommandList.commandList[i].commandSteps[j].rawInputs[inputSystem._InputList.IndexOf(seperateInputs[k])] = 1;

                        } else {
                            Debug.LogError("[CommandManager] ERROR -> Command step input (" + seperateInputs[k] + ") not found in Input List!");
                        }

                    }
                }
            }

        }



        NativeInput[] commandSteps;         // A reference to the command steps in the current command
        int frameCount;                     // Tracks which frame of the Native Input Table the CheckCommands function are up to 
        bool commandSuccess = false;        // Tracks whether all the steps in the command were found 
        int stepIndex;                      // Tracks which command step the CheckCommands function is up to
        bool requiredInputMissing = false;  // Tracks whether any of the inputs were missing for the current command step
        int numberOfFramesToSearch;         // The number of frames to search through the NTI for each command
        int requiredInput;                  // The current input being checked 
        int inputState;                     // The state of the input at the frame we're up to

        void CheckCommands() {
            //if (_Debug) Debug.Log("[CommandManager] Checking for commands!");
            if (basePlayer.isRecovering) return;
            

            // Go through each command in the command list
            Command[] commandList = _CommandList.commandList;
            for (int i = 0; i < commandList.Length; i++) {

                // Make sure this command is enabled and it's other requirements are met
                if ((!commandList[i].commandEnabled) ||
                    (commandList[i].requireGrounded && !basePlayer.isGrounded) ||
                    (commandList[i].requireAirborne && basePlayer.isGrounded) ||
                    (commandList[i].requireCrouching && !basePlayer.isCrouching) ||
                    (commandList[i].requireAirActions && basePlayer.airActions == 0)) continue;

                if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] Checking Command(" + commandList[i].commandName + ")...");
                
                // Grab the steps for this command
                commandSteps = commandList[i].commandSteps;

                // Set the number of frames to search through
                if (commandList[i].inputAllowance > 0) {
                    numberOfFramesToSearch = Mathf.Clamp(commandList[i].inputAllowance, 1, InputSystem.instance._InputBuffer);
                } else {
                    numberOfFramesToSearch = InputSystem.instance._InputBuffer;
                }                

                // Reset the frame count, the step we're checking for, and the command success bool 
                frameCount = 0;
                stepIndex = commandSteps.Length - 1;
                commandSuccess = false;

                if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] Checking for Input(" + commandSteps[stepIndex].combinedInputs + ")");

                // Iterate backwards through each of the frames, as long as there are command steps left
                while (frameCount < numberOfFramesToSearch && stepIndex >= 0) {    

                    // Go through the required input list for this command step
                    requiredInputMissing = false;
                    for (int j = 0; j < commandSteps[stepIndex].rawInputs.Length; j++) {

                        // Check if each of the inputs required for this command step are present                        
                        requiredInput = commandSteps[stepIndex].rawInputs[j];
                        
                        // Only check inputs that are assigned to
                        if (requiredInput != 0) {
                            if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] Required input for (" + InputSystem.instance._InputList[j] + ") = " + requiredInput);

                            // Check the state of the input at the frame we're up to
                            inputState = InputSystem.instance.InputStateInFrame(j, frameCount, _DebugCommands && commandList[i].debug);

                            // Move on if the required input is the same as the input state
                            if (requiredInput == inputState
                                // Also if it's a lazy input, consider pressed and held down inputs the same
                                || (commandList[i].lazyInput && requiredInput == 1 && inputState == 2)) {

                                if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] Getting there! Input found!");

                            // If required input not present, break to goto the next frames
                            } else {
                                if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] Input NOT found, moving to next frame...");
                                requiredInputMissing = true;
                                break;
                            }
                        }
                    }

                    // Register command if the last input was successful                    
                    if (!requiredInputMissing) {
                        stepIndex--;
                        if (stepIndex < 0) {                            
                            commandSuccess = true;                            
                            break;
                        } else if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] There are still more inputs tho...");
                    } else {
                        // Break out of command if the first step is missed
                        if (stepIndex == commandSteps.Length - 1) {
                            if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] First input not found! Cancelling check");
                            break;
                        } else if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] Required input missing! Moving to next input");                        
                    }

                    // Goto the next frame
                    frameCount++;

                }

                // Register a successful command, or move to next one
                if (commandSuccess) {
                    RegisterCommand(i, frameCount);
                    break;
                } else {
                    if (_DebugCommands && commandList[i].debug) Debug.Log("[CommandManager] Ran out of frames, giving up on Command(" + commandList[i].commandName + ")");
                }
            }

            // Ran out of commands, goto default animation trigger
            if (!commandSuccess) {
                if (_DebugManager) Debug.Log("[CommandManager] Ran out of commands! Playing default animation...");
            }
        }


        void RegisterCommand( int commandIndex, int frameCount ) {

            // Do stuff because the command was successful
            if (_DebugManager) Debug.Log("[CommandManager] Command (" + _CommandList.commandList[commandIndex].commandName + ") registered!");

            // Mark off air actions if applicable
            if (_CommandList.commandList[commandIndex].requireAirActions) {
                basePlayer.airActions = Mathf.Max(basePlayer.airActions - 1, 0);
            }

            // Clear the input buffer if applicable
            if (_CommandList.commandList[commandIndex].clearInputBuffer) {
                if (_DebugManager) Debug.Log("[CommandManager] Clearing the input buffer!");
                InputSystem.instance.ActivateInputBuffer(false);
            }

            // Send an event
            onCommandRegistered?.Invoke(_CommandList.commandList[commandIndex]);

            // NOTE -> this is probably not necessary anymore
                // Clear the cache of frames held in the Native Input Table
                //if (_DontClearInputBuffer) {
                //    if (_Debug) Debug.LogWarning("[InputSystem] NOTE -> You aren't clearing the input buffer! This could be bad!");
                //    return;
                //} else {
                //    if (_Debug) Debug.LogWarning("[InputSystem] NOTE -> You aren't clearing the input buffer! This could be bad!");
                //    InputSystem.instance.ClearNativeInputTable();
                //}           

        }


               




        // CONTEXT BUTTONS

        [ContextMenu("Enable All Commands")]
        void EnableAllCommands() {
            // Enable each of the commands in the commands list
            for (int i = 0; i < _CommandList.commandList.Length; i++) {
                Command command = _CommandList.commandList[i];
                command.commandEnabled = true;
            }
        }

        [ContextMenu("Disable All Commands")]
        void DisableAllCommands() {
            // Disable each of the commands in the commands list
            for (int i = 0; i < _CommandList.commandList.Length; i++) {
                Command command = _CommandList.commandList[i];
                command.commandEnabled = false;
            }
        }

        [ContextMenu("Debug All Commands")]
        void EnableDebugOnCommands() {
            // Go through each of the commands in the commands list and turn on debugging
            for (int i = 0; i < _CommandList.commandList.Length; i++) {
                Command command = _CommandList.commandList[i];
                command.debug = true;
            }
        }

        [ContextMenu("Debug No Commands")]
        void DisableDebugOnCommands() {
            // Go through each of the commands in the commands list and turn off debugging
            for (int i = 0; i < _CommandList.commandList.Length; i++) {
                Command command = _CommandList.commandList[i];
                command.debug = false;
            }
        }

    }
}






/// NUMBERED INPUTS

//void CheckCommands() {

//    // Cancel the command check if we are still recovering
//    if (_Recovering) return;

//    // Save references to the current input values, the input buffer value, and the command list
//    int[] currentInputs = InputSystem2.instance._NativeInputTable;
//    int inputBuffer = InputSystem2.instance._InputBuffer;
//    Command[] commandList = _CommandList.commandList;

//    // Setup some temp variables
//    int stepCount = 0;
//    int bufferCount = 0;
//    bool requiredInputsPresent = true;

//    // Go through the list of commands
//    for (int i = 0; i < commandList.Length; i++) {

//        // Skip this command if it is disabled
//        if (!commandList[i].commandEnabled) continue;

//        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Checking for [" + commandList[i].commandName + "]");

//        // Reset the buffer count and the presence of required inputs
//        bufferCount = inputBuffer;
//        requiredInputsPresent = true;

//        // Starting from the last command step in the list
//        for (int j = commandList[i].commandSteps.Length - 1; j >= 0; j--) {

//            if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Checking for " + commandList[i].commandSteps[j].combinedInputs);

//            // Go through the list of inputs for this command step
//            for (int k = 0; k < currentInputs.Length; k++) {

//                //Debug.Log("i = "+i+", j = "+j+", k = "+k);
//                //Debug.Log("command list = " + commandList[i] + ", command step = " + commandList[i].commandSteps[j] + ", raw input = " + commandList[i].commandSteps[j].rawInputs[k]);

//                // For any required input in this step
//                if (commandList[i].commandSteps[j].rawInputs[k] != 0) {

//                    // Cancel command if any required inputs are missing 
//                    if (currentInputs[k] == 0) {
//                        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Input missing! Moving to next command...");
//                        requiredInputsPresent = false;
//                        break;

//                        // Otherwise set buffer count to value of this input
//                    } else {
//                        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Input found! Moving to next step...");
//                        bufferCount = (bufferCount < currentInputs[k]) ? bufferCount : currentInputs[k];
//                    }
//                }
//            }

//            // If the required inputs are missing, cancel and goto next command
//            if (!requiredInputsPresent) {
//                break;
//            }

//        }

//        // Register the command if the required inputs were present across all command steps
//        if (requiredInputsPresent) {
//            if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] All steps completed! Registering event...");
//            RegisterCommand(i, frameCount);
//            break;
//        }


//    }
//}


//void RegisterCommand( int commandIndex, int frameCount ) {

//    // Do stuff because the command was successful
//    if (_Debug) Debug.Log("[CommandManager] Command (" + _CommandList.commandList[commandIndex].commandName + ") registered!");

//    // Send an event
//    onCommandRegistered?.Invoke(_CommandList.commandList[commandIndex]);

//    // Start recovery time for command
//    StopAllCoroutines();
//    StartCoroutine(WaitForRecovery(_CommandList.commandList[commandIndex].recoveryLength));


//    // Clear the cache of frames held in the Native Input Table
//    if (_DontClearInputBuffer) {
//        Debug.LogWarning("[InputSystem] NOTE -> You aren't clearing the input buffer! This could be bad!");
//        return;
//    }
//    InputSystem2.instance.ClearNativeInputTable();

//}



//IEnumerator WaitForRecovery( int recoveryLength ) {
//    _Recovering = true;

//    // Wait for the recovery length
//    int frame = recoveryLength;
//    while (frame > 0) {
//        yield return null;
//        frame--;
//    }

//    _Recovering = false;
//}






//void CompileCommands() {

//    InputSystem2 inputSystem = InputSystem2.instance;

//    // Go through each of the commands in the commands list
//    for (int i = 0; i < _CommandList.commandList.Length; i++) {

//        Command command = _CommandList.commandList[i];
//        string[] commandInputs = command.commandInputs;

//        if (_Debug) Debug.Log("[CommandManager] Checking Command (" + command.commandName + ")");

//        // Create an array of native inputs, each representing a command step
//        command.commandSteps = new NativeInput[commandInputs.Length];

//        // Go through each command step one-by-one
//        for (int j = 0; j < commandInputs.Length; j++) {

//            if (_Debug) Debug.Log("[CommandManager] Checking command step (" + commandInputs[j] + ")");

//            // Create a native input entry for the current command step
//            command.commandSteps[j] = new NativeInput();
//            command.commandSteps[j].rawInputs = new int[inputSystem._InputList.Count];
//            for (int k = 0; k < command.commandSteps[j].rawInputs.Length; k++) {
//                command.commandSteps[j].rawInputs[k] = 0;
//            }
//            command.commandSteps[j].combinedInputs = commandInputs[j];

//            // Save each command step as seperate strings of inputs and iterate through them 
//            string[] seperateInputs = commandInputs[j].Split(char.Parse("+"));

//            // Go through each distinct string input in this particular command step
//            for (int k = 0; k < seperateInputs.Length; k++) {

//                // Check the string input is valid
//                if (inputSystem._InputList.Contains(seperateInputs[k])) {

//                    if (_Debug) Debug.Log("[CommandManager] Saving command step input (" + seperateInputs[k] + ") in native input table");

//                    // Set the raw input for this command step 
//                    _CommandList.commandList[i].commandSteps[j].rawInputs[inputSystem._InputList.IndexOf(seperateInputs[k])] = 1;

//                } else {
//                    Debug.LogError("[CommandManager] ERROR -> Command step input (" + seperateInputs[k] + ") not found in Input List!");
//                }

//            }
//        }
//    }

//}
//    }







/// OLDER STUFF


// Check the state of the input for this frame
//                    if (InputSystem.instance.InputPresentInFrame(commandInputs[stepIndex], frameCount)) {

//                        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Getting there! Input(" + commandList[i].commandInputs[stepIndex] + ") found");

//                        //inputIndex--;
//                        stepIndex++;

//                        // Register command if this was the last input
//                        if (stepIndex< 0) {
//                            commandSuccess = true;
//                            break;
//                        } else {
//                            if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Moving to next input...");
//                        }

//                    }







//        // Check the state of this input for this frame
//        if () {
//            if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Input(" + requiredInputs[stepIndex].combinedInputs + ") NOT found, moving to next frame");

//            // Cancel looking for 
//            break;

//        }

//        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Getting there! Input(" + requiredInputs[stepIndex].combinedInputs + ") found!");

//    }


//}


//// Iterate backwards through each of the frames
//while (frameCount < nativeInputTableLength) {

//    if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Checking frame(" + frameCount + ")...");

//    // Go through list of required inputs for this command step
//    for (int j = requiredInputs[stepIndex].rawInputs.Length - 1; j >= 0 ; j--) {                       

//        // Check if this input is required for this command step
//        if (requiredInputs[stepIndex].rawInputs[j] != 0) {
//            if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Checking for Input(" + requiredInputs[stepIndex].combinedInputs + ")");

//            // Check the state of this input for this frame
//            if (!InputSystem.instance.InputPresentInFrame(j, frameCount)) {
//                if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Input(" + requiredInputs[stepIndex].combinedInputs + ") NOT found, moving to next frame");

//                // Cancel looking for 
//                break;

//            } 

//            if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Getting there! Input(" + requiredInputs[stepIndex].combinedInputs + ") found!");

//        }

//    }

//    // Register command if this was the last input
//    stepIndex--;
//    if (stepIndex < 0) {
//        commandSuccess = true;
//        goto SkipWhileLoop;      //break;

//    // Move to the next step if there are more to come
//    } else {
//        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] There are still more inputs tho...");
//    }

//    // Goto the next frame
//    frameCount++;
//}

//SkipWhileLoop:

//// Register a successful command, or move to next one
//if (commandSuccess) {
//    RegisterCommand(i, frameCount);
//    break;
//} else {
//    if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Ran out of frames, giving up on Command(" + commandList[i].commandName + ")");
//}













//void CheckCommands() {

//    if (_Recovering) return;

//    bool commandSuccess = false;
//    int inputIndex;
//    int stepIndex;

//    Command[] commandList = _CommandList.commandList;
//    NativeInput[] requiredInputs;

//    //if (_Debug) Debug.Log("[CommandManager] Checking for commands!");

//    // Search every command found in the command list
//    for (int i = 0; i < commandList.Length; i++) {

//        // Make sure this command is enabled
//        if (!commandList[i].commandEnabled) continue;
//        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Checking Command(" + commandList[i].commandName + ")...");

//        // Grab the steps for this command
//        //string[] commandInputs = commandList[i].commandInputs;
//        requiredInputs = commandList[i].nativeInputs;

//        // Reset the frame count and which step we're checking for 
//        frameCount = 0;
//        stepIndex = requiredInputs.Length - 1;

//        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Checking for Input(" + requiredInputs[stepIndex].combinedInputs + ")");

//        // Iterate backwards through each of the frames, as long as there are command steps left
//        while (frameCount < nativeInputTableLength && stepIndex >= 0) {

//            // Go through the required input list for this command step
//            for (int j = 0; j < requiredInputs[stepIndex].rawInputs.Length; j++) {

//                // Check if each of the inputs required for this command step are present
//                if (requiredInputs[stepIndex].rawInputs[j] != 0) {
//                    if (InputSystem.instance.InputPresentInFrame(j, frameCount)) {
//                        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Getting there! Input found!");

//                        // Register command if this was the last input, otherwise goto next command step
//                        stepIndex--;
//                        if (stepIndex < 0) {
//                            commandSuccess = true;
//                            break;
//                        } else if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] There are still more inputs tho...");

//                        // If required input not present, break to goto the next frames
//                    } else {
//                        if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Input NOT found, moving to next frame...");
//                        break;
//                    }
//                }

//            }

//            // Goto the next frame
//            frameCount++;

//        }

//        // Register a successful command, or move to next one
//        if (commandSuccess) {
//            RegisterCommand(i, frameCount);
//            break;
//        } else {
//            if (_Debug && commandList[i].debug) Debug.Log("[CommandManager] Ran out of frames, giving up on Command(" + commandList[i].commandName + ")");
//        }
//    }
//}


//void RegisterCommand( int commandIndex, int frameCount ) {

//    // Do stuff because the command was successful
//    if (_Debug) Debug.Log("[CommandManager] Command (" + _CommandList.commandList[commandIndex].commandName + ") registered!");

//    // Send an event
//    onCommandRegistered?.Invoke(_CommandList.commandList[commandIndex]);

//    // Start recovery time for command
//    StopAllCoroutines();
//    StartCoroutine(WaitForRecovery(_CommandList.commandList[commandIndex].recoveryLength));


//    // Clear the cache of frames held in the Native Input Table
//    if (_DontClearInputBuffer) {
//        Debug.LogWarning("[InputSystem] NOTE -> You aren't clearing the input buffer! This could be bad!");
//        return;
//    }
//    InputSystem.instance.ClearNativeInputTable();

//}



//IEnumerator WaitForRecovery( int recoveryLength ) {
//    _Recovering = true;

//    // Wait for the recovery length
//    int frame = recoveryLength;
//    while (frame > 0) {
//        yield return null;
//        frame--;
//    }

//    _Recovering = false;
//}






//void CompileCommands() {

//    InputSystem inputSystem = InputSystem.instance;

//    // Go through each of the commands in the commands list
//    for (int i = 0; i < _CommandList.commandList.Length; i++) {


//        Command command = _CommandList.commandList[i];
//        string[] commandInputs = command.commandInputs;

//        if (_Debug) Debug.Log("[CommandManager] Checking Command (" + command.commandName + ")");

//        // Create an array of native inputs, each representing a command step
//        command.nativeInputs = new NativeInput[commandInputs.Length];

//        // Go through each command step one-by-one
//        for (int j = 0; j < commandInputs.Length; j++) {

//            if (_Debug) Debug.Log("[CommandManager] Checking command step (" + commandInputs[j] + ")");

//            // Create a native input entry for the current command step
//            command.nativeInputs[j] = new NativeInput();
//            command.nativeInputs[j].rawInputs = new int[inputSystem._InputList.Count];
//            for (int k = 0; k < command.nativeInputs[j].rawInputs.Length; k++) {
//                command.nativeInputs[j].rawInputs[k] = 0;
//            }
//            command.nativeInputs[j].combinedInputs = commandInputs[j];

//            // Save each command step as seperate strings of inputs and iterate through them 
//            string[] seperateInputs = commandInputs[j].Split(char.Parse("+"));

//            // Go through each distinct string input in this particular command step
//            for (int k = 0; k < seperateInputs.Length; k++) {

//                // Check the string input is valid
//                if (inputSystem._InputList.Contains(seperateInputs[k])) {

//                    if (_Debug) Debug.Log("[CommandManager] Saving command step input (" + seperateInputs[k] + ") in native input table");

//                    // Set the raw input for this command step 
//                    _CommandList.commandList[i].nativeInputs[j].rawInputs[inputSystem._InputList.IndexOf(seperateInputs[k])] = 1;

//                } else {
//                    Debug.LogError("[CommandManager] ERROR -> Command step input (" + seperateInputs[k] + ") not found in Input List!");
//                }

//            }
//        }
//    }

//}