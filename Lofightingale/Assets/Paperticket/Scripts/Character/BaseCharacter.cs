using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    [RequireComponent(typeof(Rigidbody2D))]
    public class BaseCharacter : MonoBehaviour {

        [Header("References")]

        [SerializeField] protected Rigidbody2D rigidbody2D;
        [SerializeField] protected BoxCollider2D physicsCollider;
        [SerializeField] protected TriggerBoxChecker groundChecker;


        [Header("Movement Settings")]

        [SerializeField] protected bool enableGravity;
        public float gravityScale;

        [SerializeField] protected bool enableMovement;
        [SerializeField] [Min(0.1f)] protected float moveSpeed;
        [SerializeField] [Min(0.1f)] protected float maxSpeed;
        [SerializeField] [Min(0.1f)] protected float minSpeed;

        [SerializeField] protected bool enableCrouch;

        public int maxAirActions;
        


        [Header("Debugging Options")]

        [SerializeField] protected bool _DebugUpdates;
        [SerializeField] protected bool _DebugEvents;
        [SerializeField] protected bool _DebugGizmos;
        

        [Space(10)]
        [Header("Read Only")]

        [Tooltip("How many hit points the character has")]
        public int HitPoints;

        [Tooltip("The multiplier of damage recieved by the character")]
        public float DefenseMultiplier;

        [Tooltip("Whether the character is currently able to be damaged")]
        public bool isInvulnerable;

        //[Tooltip("The number of consecutive moves that have been performed withj")]
        //public int comboCounter;

        [Space(5)]
        [Tooltip("Whether the character is recovering or not")]
        public bool isRecovering;

        [Tooltip("Whether the character is grounded or not  /n NOTE: Make sure EnableGravity is true")]
        public bool isGrounded;

        [Tooltip("Whether the character is idle (0) or walking left (-1) or right (1)  /n NOTE: Make sure EnableMovement is true")]
        public int isWalking;

        [Tooltip("Whether the character is crouching or not  /n NOTE: Make sure EnableCrouch is true")]
        public bool isCrouching;

        [Tooltip("The number of commands marked as Air Actions that the character can perform before having to return to the ground")]
        public int airActions;

        [Space(5)]
        [Tooltip("Whether the character has flipped and is currently facing left")]
        public bool facingLeft;

        [Tooltip("Whether the character is in proximity of an opponent activebox")]
        public bool isInOpponentProximity;

        [SerializeField] protected Vector2 currentVelocity;
        [SerializeField] protected Vector2 oldVelocity;





        void Awake() {

            CheckRequiredComponents();

            // Set the initial gravity setting
            if (enableGravity) {
                SetGravity(true);
            }

        }
        public virtual void CheckRequiredComponents() {
            rigidbody2D = rigidbody2D ?? GetComponent<Rigidbody2D>();
            physicsCollider = physicsCollider ?? GetComponentInChildren<BoxCollider2D>();
                        
            // Save reference to and disable the script if cannot find ground checker
            groundChecker = groundChecker ?? GetComponentInChildren<TriggerBoxChecker>();
            if (groundChecker == null) {
                Debug.LogError("[BaseCharacter] ERROR -> No TriggerBoxChecker found! Please add one to Ground Checker variable.");
                enabled = false;
            }

        }

        void FixedUpdate() {
            
            if (enableGravity) {
                // Update whether the player is grounded or not
                isGrounded = groundChecker.IsTouchingLayers;
            }
            
            // Update the velocity if necessary
            if (oldVelocity != currentVelocity) {
                rigidbody2D.velocity = new Vector2(transform.right.x * currentVelocity.x, currentVelocity.y);
                oldVelocity = currentVelocity;
            }

        }

        public virtual void Update() {
             
            if (enableGravity) {
                // Reset the ability to use air actions
                if (isGrounded) airActions = maxAirActions;
            }
                        
            if (enableMovement) {
                MovementCheck();
                // Check the whether the player is walking         
                //if (isGrounded) isWalking = MovementCheck();
            }

            if (enableCrouch && isGrounded) {
                CrouchingCheck();
                //isCrouching = isGrounded && CrouchingCheck();
            }

            
        }
        public virtual void CrouchingCheck () {
            isCrouching = false;
        }
        public virtual void MovementCheck() {
            isWalking = 0;
        }







        public virtual void ChangeHealth( int modifier ) {
            HitPoints = Mathf.Max(0, HitPoints + modifier);
        }

        public virtual void SetDefenseMultiplier( int value ) {
            DefenseMultiplier = value;
        }

        public virtual void SetInvulnerable( bool active ) {
            isInvulnerable = active;
        }


                     
        public virtual void SetRecovering( bool active ) {                        
            isRecovering = active;
        }
        
        public virtual void SetGravity( bool active ) {
            rigidbody2D.gravityScale = active ? gravityScale : 0;
        }

        public virtual void SetGrounded( bool active ) {
            isGrounded = active;
        }
               


        public virtual void SetVelocity( Vector2 velocity, bool additive ) {
            if (_DebugEvents) Debug.Log("[BaseCharacter] Setting new velocity! Vector2 = " + velocity + ", additive = " + additive);

            // If not additive, reset the current velocity before saving new one
            if (!additive) currentVelocity = Vector2.zero;
            currentVelocity += velocity;

            if (_DebugEvents) Debug.Log("[BaseCharacter] New velocity = " + currentVelocity);

        }
        public virtual void SetVelocity( Vector2 direction, float magnitude, bool additive ) {
            SetVelocity(direction * magnitude, additive);
        }

        public virtual void AddForce( Vector2 direction, float magnitude, bool wipeVelocity ) {

            // Wipe the existing velocity of the character if applicable
            if (wipeVelocity) rigidbody2D.velocity = Vector2.zero;

            // Add the specified force
            rigidbody2D.AddForce(direction.normalized * magnitude);

        }



        public virtual void SetInOpponentProximity( bool inProximity ) {
            isInOpponentProximity = inProximity;
        }

        public virtual void SetFacing( bool faceLeft ) {

            transform.rotation *= Quaternion.Euler(Vector2.down * 180);
            facingLeft = faceLeft;

            Debug.Log("[BaseCharacter] Now facing " + (facingLeft ? "left" : "right"));

        }


        void OnDrawGizmosSelected() {

            // Could put on screen indicators here

        }

    }

}
