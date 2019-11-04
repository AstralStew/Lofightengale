using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Paperticket
{
    public class AnimationManager : MonoBehaviour {

        CharacterManager characterManager;
        [SerializeField] Animator animator;

        [Header("Controls")]

        [SerializeField] bool _WipeTriggersOnAnimationStarted;
        [SerializeField] bool _WipeTriggersOnAnimationFinished;
               
        [SerializeField] bool _Debug;


        public delegate void OnAnimationStarted();
        public event OnAnimationStarted onAnimationStarted;
        public delegate void OnAnimationFinished();
        public event OnAnimationFinished onAnimationFinished;

               
        void Awake() {

            // Save reference to and disable the script if cannot find character manager
            characterManager = characterManager ?? GetComponentInParent<CharacterManager>();
            if (characterManager == null) {
                Debug.LogError("[CommandManager] ERROR -> No character manager found! Child this object to the character manager!");
                enabled = false;
            }
            // Save reference to and disable the script if cannot find animator component
            animator = animator ?? GetComponentInChildren<Animator>();
            if (animator == null) {
                Debug.LogError("[CharacterController] ERROR -> No animator component found! Please add one to Animator variable.");
                enabled = false;
            }

        }

        void OnEnable() {
            CommandManager.onCommandRegistered += PlayCommandAnimation;
        }
        void OnDisable() {
            CommandManager.onCommandRegistered -= PlayCommandAnimation;
        }




        void Update() {

            SetAnimationBool("isCrouching", characterManager.isCrouching);
            SetAnimationBool("isGrounded", characterManager.isGrounded);

            SetAnimationInt("isWalking", characterManager.isWalking);

        }





        public void PlayCommandAnimation( Command command ) {

            // Check if there are any parameters to change
            if (command.animationParametres.Length > 0) {

                // For each of the animation parameters listed with the command
                for (int i = 0; i < command.animationParametres.Length; i++) {

                    // Check the type of animation parameter
                    switch (command.animationParametres[i].parameterType) {

                        // Set the float value
                        case AnimatorControllerParameterType.Float:
                            SetAnimationFloat(command.animationParametres[i].parameterName, command.animationParametres[i].parameterValue);
                            break;

                        // Set the int value
                        case AnimatorControllerParameterType.Int:
                            SetAnimationInt(command.animationParametres[i].parameterName, (int)Mathf.Round(command.animationParametres[i].parameterValue));
                            break;

                        // Set the bool value
                        case AnimatorControllerParameterType.Bool:
                            SetAnimationBool(command.animationParametres[i].parameterName, command.animationParametres[i].parameterValue > 0);
                            break;

                        // Set the trigger value
                        case AnimatorControllerParameterType.Trigger:
                            SetAnimationTrigger(command.animationParametres[i].parameterName);
                            break;
                    }
                }
            }
        }


        // Animator Parameter Functions

        public void SetAnimationBool (string boolName, bool state ) {
            animator.SetBool(boolName, state);
        }

        public void SetAnimationFloat( string floatName, float state ) {
            animator.SetFloat(floatName, state);
        }

        public void SetAnimationInt( string intName, int state ) {
            animator.SetInteger(intName, state);
        }

        public void SetAnimationTrigger( string triggerName ) {
            // Reset all trigger parametres
            ResetAnimationTriggerParameters();
            // Fire off the given trigger parametre
            animator.SetTrigger(triggerName);
        }
               
        void ResetAnimationTriggerParameters() {

            // Reset all trigger parametres
            for (int i = 0; i < animator.parameterCount; i++) {
                if (animator.parameters[i].type == AnimatorControllerParameterType.Trigger) {
                    if (_Debug) Debug.Log("[AnimationManager] Resetting parameter = " + animator.parameters[i].name);
                    animator.ResetTrigger(animator.parameters[i].name);
                }
            }

        }



        // Animation Event Functions

        public void SetRecovering (int active) {            
            characterManager.SetRecovering(active>0);
        }

        public void SetGravity (int active) {
            characterManager.SetGravity(active>0);
        }

        AnimationPackage animPackage;
        public void SetVelocity( AnimationEvent animationEvent ) {

            if (animationEvent.objectReferenceParameter) {

                animPackage = (AnimationPackage)animationEvent.objectReferenceParameter;

                characterManager.SetVelocity(animPackage.newVelocity.normalized, animPackage.newVelocity.magnitude, false);

            }

        }



        // Animation State Event Functions

        public void RegisterAnimationStateEnter() {
            onAnimationStarted?.Invoke();

            if (_WipeTriggersOnAnimationStarted) {
                ResetAnimationTriggerParameters();
            }

        }
        public void RegisterAnimationStateExit() {
            onAnimationFinished?.Invoke();

            if (_WipeTriggersOnAnimationFinished) {
                ResetAnimationTriggerParameters();
            }

        }
               
        


    }
}


//public void PlayCommandAnimation (string animationTrigger ) {


//    // Cancel if there's no animation trigger set
//    if (animationTrigger != "") {

//        // Reset all trigger parametres
//        for (int i = 0; i < animator.parameters.Length; i++) {                    
//            if (animator.parameters[i].type == AnimatorControllerParameterType.Trigger) {
//                if (_Debug) Debug.Log("[AnimationManager] Resetting parameter = " + animator.parameters[i].name);
//                animator.ResetTrigger(animator.parameters[i].name);
//            }
//        }

//        // Fire off the given trigger parametre
//        animator.SetTrigger(animationTrigger);


//    }
//}

