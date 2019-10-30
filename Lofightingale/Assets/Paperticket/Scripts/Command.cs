using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{
    [System.Serializable]
    public class Command
    {        
        public string commandName;

        [Tooltip("If this is unchecked, the command will be completely disabled.\n\nEnable All Commands / Disable All Commands buttons at the bottom of this component")]
        public bool commandEnabled;



        [Header("Requirements")]
                
        public bool requireGrounded;
        public bool requireAirborne;
        public bool requireCrouching;



        [Header("Command Values")]

        [Tooltip("The inputs of the command.\n\nUse '+' to include mutliple inputs required on the same frame.\n\nUse '^' to designate that the input must be released as part of the command")]
        public string[] commandInputs;

        [Tooltip("The command must be executed within this number of frames. A value of (0) will look through the entire buffer.")]
        public int inputAllowance;

        [Tooltip("The amount of recovery frames in this command.\n\nNOTE: This is in frames, not seconds")]
        public int recoveryLength;

        [Tooltip("The animation parametre that triggers the animation of the command")]
        public string animationTrigger;

                     

        [Header("Movement Options")]

        public bool moveCharacter;
        public Vector2 moveForce;
        public float forceMulltiplier;

        

        [Header("Misc")]

        [Tooltip("Output debug logs in the console.\n\nEnable All Commands / Disable All Commands buttons at the bottom of this component")]
        public bool debug;
        

        // Read Only

        [HideInInspector]
        public NativeInput[] commandSteps;      // Create at compile (start of runtime atm)

    }

}
