using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    public class EnemyAnimationManager : AnimationManager {

        [SerializeField] string hurtAnimationTrigger;
        BaseEnemy baseEnemy;

        public override void Awake() {
            base.Awake();

            // Save reference to and disable the script if cannot find character manager
            baseEnemy = baseEnemy ?? GetComponentInParent<BaseEnemy>();
            if (baseEnemy == null) {
                Debug.LogError("[EnemyAnimationManager] ERROR -> No BaseEnemy found! Child this object to the BaseEnemy or a child class!");
                enabled = false;
            }
                       
            //sprite = GetComponentInChildren<SpriteRenderer>().gameObject;
            //foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>()) {
            //    if (hitbox.hitboxState == HitboxStates.Active) activeBox = hitbox;
            //    if (hitbox.hitboxState == HitboxStates.Proximity) proxbox = hitbox;
            //}

            //if (!hitProperties) {
            //    Debug.LogError("[" + gameObject.name + "] ERROR -> No hit properties provided. Did you forget to add the object to the inspector?");
            //    return;
            //}
                                 
        }

        public override void OnEnable() {
            base.OnEnable();

            // this is where we'll subscribe to the equiv of CommandManager
            
        }

        public override void OnDisable() {
            base.OnDisable();

            // this is where we'll unsubscribe to the equiv of CommandManager
        }

        void Update() {

            SetAnimationBool("isCrouching", baseEnemy.isCrouching);
            SetAnimationBool("isGrounded", baseEnemy.isGrounded);

            SetAnimationInt("isWalking", baseEnemy.isWalking);

            SetAnimationBool("isNearPlayer", baseEnemy.isInPlayerProximity);

        }

        void FixedUpdate() {

            baseEnemy.isInPlayerProximity = false;

        }



        public virtual void OnActiveboxCheck( HitProperties hitProperties ) {
            if (_Debug) Debug.Log("[BaseEnemy] OnActiveboxCheck triggered.");
        }


        public virtual void OnProxboxCheck( HitProperties hitProperties ) {
            if (_Debug) Debug.Log("[BaseEnemy] OnProxboxCheck triggered.");
        }







        /// NOTE -> this is where the equiv of command will be converted into animations
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



        public override void SetRecovering( int active ) {
            base.SetRecovering(active);

            baseEnemy.SetRecovering(active > 0);
        }

        public override void SetGravity( int active ) {
            base.SetGravity(active);

            baseEnemy.SetGravity(active > 0);
        }

        public override void SetGrounded( int active ) {
            base.SetGrounded(active);

            baseEnemy.SetGrounded(active > 0);
        }


        public override void SetVelocity( AnimationEvent animationEvent ) {
            base.SetVelocity(animationEvent);

            if (animationEvent.objectReferenceParameter) {

                AnimationPackage animPackage = (AnimationPackage)animationEvent.objectReferenceParameter;

                baseEnemy.SetVelocity(animPackage.newVelocity, false);

            }

        }


        public override void TakeHitProperties( HitProperties hitProperties ) {
            base.TakeHitProperties(hitProperties);

            switch (hitProperties.HitboxState) {
                case HitboxStates.Active:
                    if (_Debug) Debug.Log("[EnemyAnimationManager] HitProperties received from an activebox. We were hit by something!");

                    if (hurtAnimationTrigger != "") {
                        SetAnimationTrigger(hurtAnimationTrigger);
                    } else {
                        Debug.LogError("[EnemyAnimationManager] ERROR -> No Hurt Animation trigger found! Please enter the name of the animation trigger in the Inspector!");
                    }

                    // Apply the damage of the hit properties
                    if (hitProperties.HitDamage != 0) { }// characterManager.ChangeHealth(-hitProperties.HitDamage);

                    // Apply the hit stun of the hit properties
                    // This will apply to this script, and change the length of the WarriorHurt 

                    // Apply the proration of the hit properties
                    // The damage multiplier, this may not be a thing yet

                    // Apply the velocity of the hit properties
                    if (hitProperties.HitVelocity != Vector2.zero) { }//characterManager.SetVelocity(hitProperties.HitVelocity, false);


                    break;
                case HitboxStates.Hurtbox:
                    if (_Debug) Debug.Log("[EnemyAnimationManager] HitProperties received from a hurtbox. We hit something else!");

                    // Turn off hitboxes (they will reenable as part of the animation track)
                    activeboxes.SetHitboxActive(false);
                    //foreach (Hitbox hitbox in activeboxes) {
                    //    hitbox.SetHitboxActive(false);
                    //}


                    // Set isRecovering off
                    baseEnemy.SetRecovering(false);


                    break;
                case HitboxStates.Proximity:
                    if (_Debug) Debug.Log("[EnemyAnimationManager] HitProperties received from a proximity. An attack is probably imminent! Ignoring tho...");

                    baseEnemy.isInPlayerProximity = true;

                    //characterManager.facingEnemy = true;

                    break;
                default:
                    Debug.LogError("[EnemyAnimationManager] HitProperties received from unknown state!");
                    break;
            }

        }


    }




}

