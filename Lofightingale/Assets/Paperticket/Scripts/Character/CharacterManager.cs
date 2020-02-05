using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{
    public class CharacterManager : BaseCharacter
    {
        [Space(20)]
        [Header("BasePlayerCharacter Properties")]

        [Tooltip("The InputSystem script that handles input for this character")]
        public InputSystem inputSystem;

        [Tooltip("The CommandManager script that sends commands for this character")]
        public CommandManager commandManager;

        [Tooltip("The AnimationManager script that controls the animations for this character")]
        public PlayerAnimationManager animationManager;


        public override void CheckRequiredComponents() {
            base.CheckRequiredComponents();
                        
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
            animationManager = animationManager ?? GetComponentInChildren<PlayerAnimationManager>();
            if (animationManager == null) {
                Debug.LogError("[BaseEnemy] ERROR -> No animation manager script found! Please add one to Animation Manager variable.");
                enabled = false;
            }
        }
               
        

        void Update() {            
            
            if (Input.GetButtonDown("RightStickButton")) {
                SetFacing(!facingLeft);
            }

        }

        public override void CrouchingCheck() {

            // Crouch only successful if the player is grounded
            isCrouching = inputSystem.InputPresentInFrame("Down", 1);

        }
        public override void MovementCheck() {

            // Walk forward
            if (inputSystem.InputPresentInFrame("Forward", 1)) { isWalking = 1; }

            // Walk back
            else if (inputSystem.InputPresentInFrame("Back", 1)) { isWalking = -1; }

            // Not walking
            else isWalking = 0;
            
        }
                              

        public override void SetRecovering( bool active ) {
            
            // Turn input buffer back on if we are coming out of recovery
            if (isRecovering && !active) {
                inputSystem.ActivateInputBuffer(true);
            }

            base.SetRecovering(active);
        }        
                               
                

        void OnDrawGizmosSelected() {

           // Could put on screen indicators here

        }

    }

}