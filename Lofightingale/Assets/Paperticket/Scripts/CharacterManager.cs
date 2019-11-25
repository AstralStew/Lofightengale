using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
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
        [SerializeField] TriggerBoxChecker groundChecker;


        [Header("Move Settings")]

        [SerializeField] [Min(0.1f)] float moveSpeed;
        [SerializeField] [Min(0.1f)] float maxSpeed;
        [SerializeField] [Min(0.1f)] float minSpeed;
        public float gravityScale;

        //[Header("Ground Settings")]        
        //[SerializeField] float groundedCornerThreshold;
        //[SerializeField] LayerMask groundLayers;


        [Header("Debugging Options")]

        [SerializeField] bool _DebugUpdates;
        [SerializeField] bool _DebugEvents;
        [SerializeField] bool _DebugGizmos;
        //[SerializeField] float groundGizmoThickness;
        //[SerializeField] Color groundGizmoColour;

        ContactPoint2D groundedPoint;

        [Header("Read Only")]

        [Tooltip("Whether the player is recovering or not")]
        public bool isRecovering;

        [Tooltip("Whether the player is grounded or not")]
        public bool isGrounded;

        [Tooltip("Whether the player is crouching or not")]
        public bool isCrouching;

        [Tooltip("Whether the player is idle (0) or walking left (-1) or right (1)")]
        public int isWalking;

        [SerializeField] Vector2 currentVelocity;
        [SerializeField] Vector2 oldVelocity;



        void OnValidate() {

            //groundedCornerThreshold = Mathf.Clamp(groundedCornerThreshold, 0, boxCollider.bounds.extents.x - 0.01f);
            //groundGizmoThickness = Mathf.Max(groundGizmoThickness, 0);
        }

        

        void Awake() {

            CheckRequiredComponents();            

            // Set the initial gravity setting
            SetGravity(true);

        }
        void CheckRequiredComponents() {
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
            // Save reference to and disable the script if cannot find animation manager
            groundChecker = groundChecker ?? GetComponentInChildren<TriggerBoxChecker>();
            if (groundChecker == null) {
                Debug.LogError("[CharacterController] ERROR -> No Trigger Box Checker found! Please add one to Ground Checker variable.");
                enabled = false;
            }
        }

        private Vector2 adjustedVelocity;
        void FixedUpdate()
        {

            // Update whether the player is grounded or not
            isGrounded = groundChecker.IsTouchingLayers;


            if (oldVelocity != currentVelocity)
            {
                adjustedVelocity = new Vector2(transform.right.x * currentVelocity.x, currentVelocity.y);
                rigidbody2D.velocity = adjustedVelocity;
                oldVelocity = currentVelocity;
            }

            if(Input.GetKeyDown(KeyCode.Tab))
            {
                transform.rotation *= Quaternion.Euler(Vector2.down*180);
                Debug.Log("Forward = " + transform.right);
            }
            
        }

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

        public void SetGrounded (bool active) {
            isGrounded = active;
        }


        Vector2 newVelocity;
        public void SetVelocity (Vector2 direction, float magnitude, bool additive ) {

            if (_DebugEvents) Debug.Log("[CharacterManager] Setting new velocity! Vector2 = "+direction*magnitude+", additive = "+additive);

            // If not additive, reset the current velocity before setting new one
            if (!additive) {
                currentVelocity = Vector2.zero;
            }
            
            // Save the new velocity
            //currentVelocity += ((Vector2)transform.InverseTransformDirection(direction) * magnitude);
            currentVelocity += (direction * magnitude);

            if (_DebugEvents) Debug.Log("[CharacterManager] New velocity = "+ currentVelocity);            
        }

        public void AddForce( Vector2 direction, float magnitude, bool wipeVelocity ) {

            // Wipe the existing velocity of the character if applicable
            if (wipeVelocity) rigidbody2D.velocity = Vector2.zero;

            // Add the specified force
            rigidbody2D.AddForce(direction.normalized * magnitude);

        }




        void OnDrawGizmosSelected() {

            //if (_DebugGizmos) {

            //    if (groundGizmoThickness > 0) {
                    
            //        // Grounded box outline
            //        Gizmos.color = new Color(groundGizmoColour.r, groundGizmoColour.g, groundGizmoColour.b, 1);
            //        Gizmos.DrawWireCube(new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y),
            //                        new Vector2((boxCollider.bounds.extents.x - groundedCornerThreshold) * 2, groundGizmoThickness));

            //        // Grounded box fill
            //        Gizmos.color = groundGizmoColour;
            //        Gizmos.DrawCube(new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y),
            //                        new Vector2((boxCollider.bounds.extents.x - groundedCornerThreshold) * 2, groundGizmoThickness));
            //    }

            //}

        }

    }

}







// Update whether the player is grounded or not

//Vector2 direction;
//float moveSpeedAdjusted;
//ContactPoint2D[] contactPoints = new ContactPoint2D[5];
//ContactPoint2D currentPoint;
//isGrounded = rigidbody2D.IsTouchingLayers(groundLayers);


//// Check if the player is intersecting any ground layers
//if (rigidbody2D.IsTouchingLayers(groundLayers)) {
//    if (_DebugUpdates) Debug.Log("[CharacterManager] Touching ground layers...");
//    isGrounded = false;
//    groundedCornerThreshold = Mathf.Clamp(groundedCornerThreshold, 0, boxCollider.bounds.extents.x);

//    // Iterate through the ground contacts
//    for (int i = 0; i < rigidbody2D.GetContacts(contactPoints); i++) {

//        currentPoint = contactPoints[i];

//        if (_DebugUpdates) Debug.Log("[CharacterManager] Checking collider: " + currentPoint.collider.name +"\n" +
//                                "Contact point X: ("+ currentPoint.point.x + "), Y: (" + currentPoint.point.y + ")\n" +
//                                "Min bound X: (" + boxCollider.bounds.min.x + "), Y: (" + boxCollider.bounds.min.y + ")\n" +
//                                "Max bound X: (" + boxCollider.bounds.max.x + "), Y: ("+ boxCollider.bounds.max.y + ")\n" +
//                                "Contact point >= min: (" + (currentPoint.point.x >= boxCollider.bounds.min.x + groundedCornerThreshold) + ")\n" +
//                                "Contact point <= max: (" + (currentPoint.point.x <= boxCollider.bounds.max.x - groundedCornerThreshold) + ")\n" +
//                                "Contact point near ground: (" + (Mathf.Abs(currentPoint.point.y - boxCollider.bounds.min.y) < 0.05f) + ")\n");


//        // Make sure the contacts are with the bottom collider bounds, and aren't too close to the side corners
//        if (/*currentPoint.point.x >= boxCollider.bounds.min.x + groundedCornerThreshold &&
//            currentPoint.point.x <= boxCollider.bounds.max.x - groundedCornerThreshold &&
//            */ Mathf.Abs(currentPoint.point.y - boxCollider.bounds.min.y) <= 0.01f) {

//            // Set the player to grounded
//            isGrounded = true;
//            groundedPoint = currentPoint;
//            break;
//        }
//    }              
//} else {
//    isGrounded = false;
//}
