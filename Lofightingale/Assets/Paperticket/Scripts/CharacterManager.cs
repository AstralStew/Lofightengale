using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Paperticket
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class CharacterManager : MonoBehaviour
    {
        [Header("References")]

        [Tooltip("The InputSystem script that handles input for this character")]
        public InputSystem inputSystem;

        [Tooltip("The CommandManager script that sends commands for this character")]
        public CommandManager commandManager;

        [Tooltip("The AnimationManager script that controls the animations for this character")]
        public AnimationManager animationManager;

        [SerializeField] Rigidbody2D rigidbody2D;
        [SerializeField] BoxCollider2D boxCollider;


        [Header("Move Settings")]

        [SerializeField] [Min(0.1f)] float moveSpeed;
        [SerializeField] [Min(0.1f)] float maxSpeed;
        [SerializeField] [Min(0.1f)] float minSpeed;
        Vector2 currentVelocity;

        [Header("Ground Settings")]

        public float gravityScale;
        [SerializeField] float groundedAllowance;
        [SerializeField] LayerMask groundLayers;


        [Header("Misc")]

        [SerializeField] bool _Debug;


        [Header("Read Only")]

        [Tooltip("Whether the player is recovering or not")]
        public bool isRecovering;

        [Tooltip("Whether the player is grounded or not")]
        public bool isGrounded;

        [Tooltip("Whether the player is crouching or not")]
        public bool isCrouching;

        [Tooltip("Whether the player is idle (0) or walking left (-1) or right (1)")]
        public int isWalking;


        [Tooltip("The last saved move frame")]
        [SerializeField] Vector2 nextMoveFrame;


        void Awake() {

            rigidbody2D = rigidbody2D ?? GetComponent<Rigidbody2D>();
            boxCollider = boxCollider ?? GetComponent<BoxCollider2D>();

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
            animationManager = animationManager ?? GetComponentInChildren<AnimationManager>();
            if (animationManager == null) {
                Debug.LogError("[CharacterController] ERROR -> No animation manager script found! Please add one to Animation Manager variable.");
                enabled = false;
            }

            // Set the initial gravity setting
            SetGravity(true);

        }

        public void AddForce( Vector2 direction, float magnitude, bool wipeVelocity ) {

            // Wipe the existing velocity of the character if applicable
            if (wipeVelocity) rigidbody2D.velocity = Vector2.zero;

            // Add the specified force
            rigidbody2D.AddForce(direction.normalized * magnitude);
            
        }

        [ContextMenu("TranslateCharacter")]
        public void TestTranslate() {
            TranslateCharacter(nextMoveFrame.normalized, nextMoveFrame.magnitude);
        }


        public void TranslateCharacter(Vector2 direction, float speed) {

            nextMoveFrame = direction * speed * Time.deltaTime;

        }




        Vector2 direction;
        float moveSpeedAdjusted;
        void FixedUpdate() {
        
            // Update whether the player is grounded or not
            isGrounded = rigidbody2D.IsTouchingLayers(groundLayers);

            rigidbody2D.velocity = currentVelocity;                      

        }


        Vector2 move;
        void Update() {

            isCrouching = isGrounded && inputSystem.InputPresentInFrame("LeftStickDown", 1);

            // If grounded, check the whether the player is walking
            if (isGrounded) {
                if (inputSystem.InputPresentInFrame("LeftStickRight", 1)) { isWalking = 1; }
                else if (inputSystem.InputPresentInFrame("LeftStickLeft", 1)) { isWalking = -1; }
                else { isWalking = 0; }
            }

        }



        public void SetRecovering( bool active ) {
            isRecovering = active;
        }

        public void SetGravity( bool active) {
            rigidbody2D.gravityScale = active ? gravityScale : 0;
        }




        Vector2 newVelocity;
        public void SetVelocity (Vector2 direction, float magnitude, bool additive ) {

            if (_Debug) Debug.Log("[CharacterManager] Setting new velocity! Vector2 = "+direction*magnitude+", additive = "+additive);

            // If not additive, reset the current velocity before setting new one
            if (!additive) {
                currentVelocity = Vector2.zero;
            }
            
            // Save the new velocity
            currentVelocity += (direction * magnitude);

            if (_Debug) Debug.Log("[CharacterManager] New velocity = "+ currentVelocity);            

        }











        void OnDrawGizmosSelected() {

            //boxCollider.bounds.min.x - (groundedAllowance / 2)

            if (groundedAllowance > 0) {
                Gizmos.DrawCube(transform.position, Vector3.one * groundedAllowance);
            }

        }

    }

}