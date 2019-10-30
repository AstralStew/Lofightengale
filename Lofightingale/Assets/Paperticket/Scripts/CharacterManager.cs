using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{
    public class CharacterManager : MonoBehaviour
    {
        [Header("References")]

        [Tooltip("The InputSystem script that handles input for this character")]
        public InputSystem inputSystem;

        [Tooltip("The CommandManager script that sends commands for this character")]
        public CommandManager commandManager;

        [Tooltip("The AnimationManager script that controls the animations for this character")]
        public AnimationManager animationManager;


        [Header("Read Only")]

        [Tooltip("The inputs of the command.\n\nUse '+' to include mutliple inputs required on the same frame.\n\nUse '^' to designate that the input must be released as part of the command")]
        public bool asd;


        void Awake () {

            // Save reference to and disable the script if cannot find input system
            inputSystem = inputSystem ?? GetComponentInChildren<InputSystem>();
            if (inputSystem == null) {
                Debug.LogError("[CharacterController] ERROR -> No input system script found! Please add one to Input System variable.");
                enabled = false;
            }
            // Save reference to and disable the script if cannot find command manager
            commandManager = commandManager ?? GetComponentInChildren<CommandManager>();
            if (commandManager == null) {
                Debug.LogError("[CharacterController] ERROR -> No command manager script found! Please add one to Command Manager variable.");
                enabled = false;
            }
            // Save reference to and disable the script if cannot find animation manager
            animationManager = animationManager ?? GetComponentInChildren<AnimationManager>();
            if (animationManager == null) {
                Debug.LogError("[CharacterController] ERROR -> No animation manager script found! Please add one to Animation Manager variable.");
                enabled = false;
            }


        }



        void Update() {

        }
    }

}