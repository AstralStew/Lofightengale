using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Paperticket
{
    public class AnimationManager : MonoBehaviour {

        CharacterManager characterManager;
        [SerializeField] Animator animator;

        
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
            // Fire off the given animation trigger parametre
            animator.SetTrigger(animationTrigger);

        }


        public void SetRecovery (int active) {
            
            characterManager.commandManager.SetRecovering(active>0);
        }



    }
}