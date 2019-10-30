using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Paperticket
{
    public class AnimationManager : MonoBehaviour {

        CharacterManager characterManager;
        [SerializeField] Animator animator;

        [SerializeField] bool _Debug;
        
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


        public void PlayCommandAnimation( Command command ) {
            // Fire off the given animation trigger parametre
            PlayCommandAnimation(command.animationTrigger);
        }


        public void PlayCommandAnimation (string animationTrigger ) {

            // Cancel if there's no animation trigger set
            if (animationTrigger != "") {

                // Reset all trigger parametres
                for (int i = 0; i < animator.parameters.Length; i++) {                    
                    if (animator.parameters[i].type == AnimatorControllerParameterType.Trigger) {
                        if (_Debug) Debug.Log("[AnimationManager] Resetting parameter = " + animator.parameters[i].name);
                        animator.ResetTrigger(animator.parameters[i].name);
                    }
                }

                // Fire off the given trigger parametre
                animator.SetTrigger(animationTrigger);
            }
        }

        public void SetAnimationState (string stateName, bool state ) {
            animator.SetBool(stateName, state);
        }
                        
        public void SetRecovering (int active) {            
            characterManager.SetRecovering(active>0);
        }

        public void SetGravity (int active) {
            characterManager.SetGravity(active>0);
        }

        void Update() {

            SetAnimationState("isCrouching", characterManager.isCrouching);
            SetAnimationState("isGrounded", characterManager.isGrounded);

        }

    }
}