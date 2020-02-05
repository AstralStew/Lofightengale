using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{
    public class BaseEnemy : BaseCharacter {

        [Space(20)]
        [Header("BaseEnemy Properties")]
        
        [Tooltip("The AnimationManager script that controls the animations for this enemy")]
        public EnemyAnimationManager animationManager;
                
                       
       
        public override void CheckRequiredComponents() {
            base.CheckRequiredComponents();

            // Save reference to and disable the script if cannot find animation manager
            animationManager = animationManager ?? GetComponentInChildren<EnemyAnimationManager>();
            if (animationManager == null) {
                Debug.LogError("[BaseEnemy] ERROR -> No animation manager script found! Please add one to Animation Manager variable.");
                enabled = false;
            }

        }

        public override void CrouchingCheck() {
            base.CrouchingCheck();
        }
        public override void MovementCheck() {
            base.MovementCheck();
        }

        

        

        public override void ChangeHealth( int modifier ) {
            base.ChangeHealth(modifier);

            // Check to see if the enemy should be killed
            CheckForDeath();
        }
        void CheckForDeath() {

            if (HitPoints == 0) {
                if (_DebugEvents) Debug.Log("[BaseEnemy] HP has run out, we should prepare to die");
                animationManager.SetAnimationTrigger(animationManager.deathAnimationTrigger);
                animationManager.onAnimationFinished += DestroySelf;
            }
        }
        void DestroySelf() {
            animationManager.onAnimationFinished -= DestroySelf;
            if (_DebugEvents) Debug.Log("[BaseEnemy] Destroying self, goodnight (T w T)'");
            Destroy(gameObject);
        }

        public override void SetDefenseMultiplier( int value ) {
            base.ChangeHealth(value);
        }

        public override void SetInvulnerable( bool active ) {
            base.SetInvulnerable(active);
        }
               


        public override void SetRecovering( bool active ) {
            base.SetRecovering(active);
        }

        public override void SetGravity( bool active ) {
            base.SetGravity(active);
        }

        public override void SetGrounded( bool active ) {
            base.SetGrounded(active);
        }



        public override void SetVelocity( Vector2 velocity, bool additive ) {
            base.SetVelocity(velocity, additive);
        }

        public override void SetVelocity( Vector2 direction, float magnitude, bool additive ) {
            base.SetVelocity(direction, magnitude, additive);
        }

        public override void AddForce( Vector2 direction, float magnitude, bool wipeVelocity ) {
            base.AddForce(direction, magnitude, wipeVelocity);
        }


        public override void SetInOpponentProximity( bool inProximity ) {
            base.SetInOpponentProximity(inProximity);
        }

        public override void SetFacing( bool faceLeft ) {
            base.SetFacing(faceLeft);
        }
        
    }
}