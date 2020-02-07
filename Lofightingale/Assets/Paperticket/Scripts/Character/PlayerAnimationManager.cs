using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    public class PlayerAnimationManager : AnimationManager {

        [SerializeField] string hurtAnimationTrigger = "WHurt";
        BasePlayer characterManager;
        
        public override void Awake() {
            base.Awake();

            // Save reference to and disable the script if cannot find character manager
            characterManager = characterManager ?? GetComponentInParent<BasePlayer>();
            if (characterManager == null) {
                Debug.LogError("[PlayerAnimationManager] ERROR -> No character manager found! Child this object to the character manager!");
                enabled = false;
            }
        }

        public override void OnEnable() {
            base.OnEnable();

            // Subscribe to commands being registered
            CommandManager.onCommandRegistered += PlayCommandAnimation;
        }

        public override void OnDisable() {
            base.OnDisable();

            // Unsubscribe to commands being registered
            CommandManager.onCommandRegistered -= PlayCommandAnimation;
        }
               
        void Update() {

            SetAnimationBool("isCrouching", characterManager.isCrouching);
            SetAnimationBool("isGrounded", characterManager.isGrounded);

            SetAnimationInt("isWalking", characterManager.isWalking);

            SetAnimationBool("isNearEnemy", characterManager.isInOpponentProximity);

            if (resetActiveHitboxes) {
                Debug.Log("PLAYER DEBUGGING ACTIVE HITBOXES");
                activeboxes.SetHitboxActive(false);
                resetActiveHitboxes = false;
            }
        }

        void FixedUpdate() {

            characterManager.isInOpponentProximity = false;

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



        public override void SetRecovering( int active ) {
            base.SetRecovering(active);

            characterManager.SetRecovering(active > 0);
        }

        public override void SetGravity( int active ) {
            base.SetGravity(active);

            characterManager.SetGravity(active > 0);
        }

        public override void SetGrounded( int active ) {
            base.SetGrounded(active);

            characterManager.SetGrounded(active > 0);
        }

        public override void SetInvulnerable( int active ) {
            base.SetInvulnerable(active);

            characterManager.SetInvulnerable(active > 0);
        }


        public override void SetVelocity( AnimationEvent animationEvent ) {
            base.SetVelocity(animationEvent);

            if (animationEvent.objectReferenceParameter) {

                AnimationPackage animPackage = (AnimationPackage)animationEvent.objectReferenceParameter;

                characterManager.SetVelocity(animPackage.newVelocity, false);

            }

        }

        bool resetActiveHitboxes;
        public override void TakeHitProperties( HitProperties hitProperties ) {
            base.TakeHitProperties(hitProperties);

            switch (hitProperties.HitboxState) {
                case HitboxStates.Active:
                    if (_Debug) Debug.Log("[PlayerAnimationManager] HitProperties received from an activebox. We were hit by something!");

                    // As long as the character is not currently invulnerable
                    if (!characterManager.isInvulnerable) {

                        if (hurtAnimationTrigger != "") {
                            SetAnimationTrigger(hurtAnimationTrigger);
                        } else {
                            Debug.LogError("[PlayerAnimationManager] ERROR -> No Hurt Animation trigger found! Please enter the name of the animation trigger in the Inspector!");
                        }

                        // Apply the damage of the hit properties
                        if (hitProperties.HitDamage != 0) characterManager.ChangeHealth(-hitProperties.HitDamage);

                        // Apply the hit stun of the hit properties
                        // This will apply to this script, and change the length of the WarriorHurt 

                        // Apply the proration of the hit properties
                        // The damage multiplier, this may not be a thing yet

                        // Apply the velocity of the hit properties
                        if (hitProperties.HitVelocity != Vector2.zero) characterManager.SetVelocity(hitProperties.HitVelocity, false);

                    }

                    break;
                case HitboxStates.Hurtbox:
                    if (_Debug) Debug.Log("[PlayerAnimationManager] HitProperties received from a hurtbox. We hit something else!");

                    //  turn this back on
                    //activeboxes.SetHitboxActive(false);
                    resetActiveHitboxes = true;

                    //foreach (Hitbox hitbox in activeboxes) {
                    //    hitbox.SetHitboxActive(false);
                    //}


                    // Set isRecovering off
                    characterManager.SetRecovering(false);


                    break;
                case HitboxStates.Proximity:
                    if (_Debug) Debug.Log("[PlayerAnimationManager] HitProperties received from a proximity. An attack is probably imminent! Ignoring tho...");

                    characterManager.isInOpponentProximity = true;

                    //characterManager.facingEnemy = true;

                    break;
                default:
                    Debug.LogError("[PlayerAnimationManager] HitProperties received from unknown state!");
                    break;



            }

        }


    }
}