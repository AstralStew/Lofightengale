using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using AK.Wwise;

namespace Paperticket
{
    public class AnimationManager : MonoBehaviour {

        //CharacterManager characterManager;
        [SerializeField] protected Animator animator;
        [SerializeField] protected AkGameObj wwiseEmitter;
              

        [Header("Controls")]

        [SerializeField] protected bool _WipeTriggersOnAnimationStarted;
        [SerializeField] protected bool _WipeTriggersOnAnimationFinished;
               
        [SerializeField] protected bool _Debug;

        [Header("Read Only")]

        [SerializeField] protected Hitbox activeboxes;
        [SerializeField] protected Hitbox hurtboxes;


        //AnimationPackage animPackage;


        public delegate void OnAnimationStarted();
        public delegate void OnAnimationFinished();

        public event OnAnimationStarted onAnimationStarted;
        public event OnAnimationFinished onAnimationFinished;

               
        public virtual void Awake() {

            // Save reference to and disable the script if cannot find animator component
            animator = animator ?? GetComponentInChildren<Animator>();
            if (animator == null) {
                Debug.LogError("[AnimationManager] ERROR -> No animator component found! Please add one to Animator variable.");
                enabled = false;
            }
            // Save reference to and disable the script if cannot find wwise/akgameobj component
            wwiseEmitter = wwiseEmitter ?? GetComponentInChildren<AkGameObj>();
            if (wwiseEmitter == null) {
                Debug.LogError("[AnimationManager] ERROR -> No AkGameObj component found! Please add one to WwiseEmitter variable.");
                enabled = false;
            }

            // Save reference to and disable the script if cannot find hitboxes and hurtboxes 
            foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>()) {
                switch (hitbox.hitboxState) {
                    case HitboxStates.Active:
                        //activeboxes.Add(hitbox);
                        activeboxes = hitbox;
                        break;
                    case HitboxStates.Hurtbox:
                        //hurtboxes.Add(hitbox);
                        hurtboxes = hitbox;
                        break;
                    case HitboxStates.Proximity:
                        // Ignore detectbox, as it is handled by CharacterManager
                        // Actually we'll probs do it here but later
                        break;
                    default:
                        Debug.LogError("[AnimationManager] ERROR -> Bad HitboxState when saving references!");
                        break;
                }
            }
            if (activeboxes == null | hurtboxes == null) {
                Debug.LogError("[AnimationManager] ERROR -> Active/hurtboxes seem to be missing! Please add at least one of each as children to this object.");
            }            

        }

        public virtual void OnEnable() {
            // Subscribe to hitboxes registering a hit
            activeboxes.onSuccessfulCheck += TakeHitProperties;
            hurtboxes.onSuccessfulCheck += TakeHitProperties;

        }
        public virtual void OnDisable() {
            // Unsubscribe to hitboxes registering a hit
            activeboxes.onSuccessfulCheck -= TakeHitProperties;
            hurtboxes.onSuccessfulCheck -= TakeHitProperties;
        }




        //void Update() {

        //    SetAnimationBool("isCrouching", characterManager.isCrouching);
        //    SetAnimationBool("isGrounded", characterManager.isGrounded);

        //    SetAnimationInt("isWalking", characterManager.isWalking);

        //    SetAnimationBool("isNearEnemy", characterManager.isInEnemyProximity);
            
        //}

        //void FixedUpdate() {

        //    characterManager.isInEnemyProximity = false;

        //}



        //public void PlayCommandAnimation( Command command ) {

        //    // Check if there are any parameters to change
        //    if (command.animationParametres.Length > 0) {

        //        // For each of the animation parameters listed with the command
        //        for (int i = 0; i < command.animationParametres.Length; i++) {

        //            // Check the type of animation parameter
        //            switch (command.animationParametres[i].parameterType) {

        //                // Set the float value
        //                case AnimatorControllerParameterType.Float:
        //                    SetAnimationFloat(command.animationParametres[i].parameterName, command.animationParametres[i].parameterValue);
        //                    break;

        //                // Set the int value
        //                case AnimatorControllerParameterType.Int:
        //                    SetAnimationInt(command.animationParametres[i].parameterName, (int)Mathf.Round(command.animationParametres[i].parameterValue));
        //                    break;

        //                // Set the bool value
        //                case AnimatorControllerParameterType.Bool:
        //                    SetAnimationBool(command.animationParametres[i].parameterName, command.animationParametres[i].parameterValue > 0);
        //                    break;

        //                // Set the trigger value
        //                case AnimatorControllerParameterType.Trigger:
        //                    SetAnimationTrigger(command.animationParametres[i].parameterName);
        //                    break;
        //            }
        //        }
        //    }
        //}


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
                    //if (_Debug) Debug.Log("[AnimationManager] Resetting parameter = " + animator.parameters[i].name);
                    animator.ResetTrigger(animator.parameters[i].name);
                }
            }

        }



        // Animation Event Functions

        public virtual void SetRecovering (int active) {            
            if (_Debug) Debug.Log("["+gameObject.name+"/Base] SetRecovering triggered.");
        }

        public virtual void SetGravity (int active) {
            if (_Debug) Debug.Log("[" + gameObject.name + "/Base] SetGravity triggered.");
        }

        public virtual void SetGrounded (int active ) {
            if (_Debug) Debug.Log("[" + gameObject.name + "/Base] SetGrounded triggered.");
        }

        public virtual void SetInvulnerable ( int active ) {
            if (_Debug) Debug.Log("[" + gameObject.name + "/Base] SetInvulnerable triggered.");
        }


        public virtual void SetVelocity( AnimationEvent animationEvent ) {
            if (_Debug) Debug.Log("[" + gameObject.name + "/Base] SetVelocity triggered.");
        }

        /// <summary>
        /// Set the hit properties of the active/hurt hitboxes
        /// </summary>
        /// <param name="hitProperties"> The hit properties provided in the animation event</param>
        public virtual void SetHitProperties ( HitProperties hitProperties ) {
            if (_Debug) Debug.Log("[" + gameObject.name + "/Base] SetHitProperties triggered.");

            if (!hitProperties) {
                Debug.LogError("[AnimationManager] ERROR -> No hit properties provided to SetHitProperties. Did you forget to add the object to the animation event?");
                return;
            }

            // Set the active properties of all the active hitboxes 
            activeboxes.activeProperties = hitProperties;

        }

        /// <summary>
        /// Receive the hit properties from the external hitbox 
        /// </summary>
        /// <param name="hitProperties"> The hit properties provided by the external hitbox</param>
        public virtual void TakeHitProperties( HitProperties hitProperties ) {
            if (_Debug) Debug.Log("[" + gameObject.name + "/Base] TakeHitProperties triggered.");

        }


        public void PlaySound (string eventName) {
            if (_Debug) Debug.Log("[" + gameObject.name + "/Base] PlaySound triggered.");

            AkSoundEngine.PostEvent(eventName, wwiseEmitter.gameObject);

        }

        

        public void SetActiveboxActive (int active) {
            activeboxes.SetHitboxActive(active > 0);
        }

        public void SetHurtboxActive (int active) {
            hurtboxes.SetHitboxActive(active > 0);
        }
        


        public virtual void SetPhysicsColliderActive(int active) {
            if (_Debug) Debug.Log("[AnimationManager(" + gameObject.name + ")] SetPhysicsColliderActive triggered.");
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

