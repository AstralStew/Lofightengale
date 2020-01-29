using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{
    public class BaseEnemy : MonoBehaviour
    {

        [Header("References")]
        
        [Tooltip("The AnimationManager script that controls the animations for this enemy")]
        public EnemyAnimationManager animationManager;

        [SerializeField] Rigidbody2D rigidbody2D;
        [SerializeField] BoxCollider2D physicsCollider;
        [SerializeField] TriggerBoxChecker groundChecker;


        [Header("Move Settings")]

        [SerializeField] [Min(0.1f)] float moveSpeed;
        [SerializeField] [Min(0.1f)] float maxSpeed;
        [SerializeField] [Min(0.1f)] float minSpeed;
        public float gravityScale;
        public int maxAirActions;
        private Vector2 adjustedVelocity;


        [Header("Read Only")]

        [Tooltip("How many hit points the enemy has")]
        public int HitPoints;

        [Tooltip("The multiplier of damage recieved by the enemy")]
        public float DefenseMultiplier;

        [Tooltip("Whether the enemy is recovering or not")]
        public bool isRecovering;

        [Tooltip("Whether the enemy is grounded or not")]
        public bool isGrounded;

        [Tooltip("Whether the enemy is crouching or not")]
        public bool isCrouching;

        [Tooltip("Whether the enemy is idle (0) or walking left (-1) or right (1)")]
        public int isWalking;

        [Tooltip("Whether the enemy is currently able to be damaged")]
        public int isInvulnerable;

        [Tooltip("Whether the enemy is in proximity of an player activebox")]
        public bool isInPlayerProximity;

        [Tooltip("Whether the enemy has flipped and is currently facing left")]
        public bool facingLeft;

        [Tooltip("The number of commands marked as Air Actions that the enemy can perform before having to return to the ground")]
        public int airActions;

        [SerializeField] Vector2 currentVelocity;
        [SerializeField] Vector2 oldVelocity;
               

        [Header("Enemy Properties")]
        
        public HitProperties hitProperties;


        [Header("Debugging Options")]

        [SerializeField] protected bool _DebugUpdates;
        [SerializeField] protected bool _DebugEvents;
        [SerializeField] protected bool _DebugGizmos;
        
        [Space(10)]

        protected GameObject sprite;
        protected Hitbox activeBox;
        protected Hitbox proxbox;




        public virtual void Awake() {

            CheckRequiredComponents();

            // Set the initial gravity setting
            SetGravity(true);
                       
        }
        void CheckRequiredComponents() {
            rigidbody2D = rigidbody2D ?? GetComponent<Rigidbody2D>();
            physicsCollider = physicsCollider ?? GetComponentInChildren<BoxCollider2D>();

            // Save reference to and disable the script if cannot find animation manager
            animationManager = animationManager ?? GetComponentInChildren<EnemyAnimationManager>();
            if (animationManager == null) {
                Debug.LogError("[BaseEnemy] ERROR -> No animation manager script found! Please add one to Animation Manager variable.");
                enabled = false;
            }
            // Save reference to and disable the script if cannot find ground checker
            groundChecker = groundChecker ?? GetComponentInChildren<TriggerBoxChecker>();
            if (groundChecker == null) {
                Debug.LogError("[BaseEnemy] ERROR -> No Trigger Box Checker found! Please add one to Ground Checker variable.");
                enabled = false;
            }

        }

        void FixedUpdate() {

            // Update whether the enemy is grounded or not
            isGrounded = groundChecker.IsTouchingLayers;

            // Update the velocity if necessary
            if (oldVelocity != currentVelocity) {
                adjustedVelocity = new Vector2(transform.right.x * currentVelocity.x, currentVelocity.y);
                rigidbody2D.velocity = adjustedVelocity;
                oldVelocity = currentVelocity;
            }
        }
        void Update() {
            //isCrouching = isGrounded && inputSystem.InputPresentInFrame("Down", 1);
            // If grounded, check the whether the enemy is walking
            if (isGrounded) {
                airActions = maxAirActions;
                //if (inputSystem.InputPresentInFrame("Forward", 1)) { isWalking = 1; } else if (inputSystem.InputPresentInFrame("Back", 1)) { isWalking = -1; } else { isWalking = 0; }
            }
            //if (Input.GetButtonDown("RightStickButton")) {
            //    SetFacing(!facingLeft);
            //}
        }


        public virtual void SetRecovering( bool active ) {

            // Turn input buffer back on if we are coming out of recovery
            //if (isRecovering && !active) {
            //    inputSystem.ActivateInputBuffer(true);
            //}

            isRecovering = active;
        }

        public virtual void SetGravity( bool active ) {
            rigidbody2D.gravityScale = active ? gravityScale : 0;
        }

        public virtual void SetGrounded( bool active ) {
            isGrounded = active;
        }


        public virtual void SetVelocity( Vector2 velocity, bool additive ) {

            if (_DebugEvents) Debug.Log("[BaseEnemy] Setting new velocity! Vector2 = " + velocity + ", additive = " + additive);

            // If not additive, reset the current velocity before setting new one
            if (!additive) {
                currentVelocity = Vector2.zero;
            }

            // Save the new velocity
            currentVelocity += velocity;

            if (_DebugEvents) Debug.Log("[BaseEnemy] New velocity = " + currentVelocity);

        }
        public virtual void SetVelocity( Vector2 direction, float magnitude, bool additive ) {
            SetVelocity(direction * magnitude, additive);
        }

        public virtual void SetInPlayerProximity( bool inPlayerProximity ) {
            isInPlayerProximity = inPlayerProximity;
        }

        public virtual void SetFacing( bool faceLeft ) {

            transform.rotation *= Quaternion.Euler(Vector2.down * 180);
            facingLeft = faceLeft;

            Debug.Log("[BaseEnemy] Now facing " + (facingLeft ? "left" : "right"));

        }



        public virtual void ChangeHealth( int modifier ) {
            HitPoints = Mathf.Max(0, HitPoints + modifier);
        }

        public virtual void SetDefenseMultiplier( int value ) {
            DefenseMultiplier = value;
        }




        public virtual void AddForce( Vector2 direction, float magnitude, bool wipeVelocity ) {

            // Wipe the existing velocity of the enemy if applicable
            if (wipeVelocity) rigidbody2D.velocity = Vector2.zero;

            // Add the specified force
            rigidbody2D.AddForce(direction.normalized * magnitude);

        }




    }

}