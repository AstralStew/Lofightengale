using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket
{

    public enum HitboxStates { Active, Hurtbox }
       
    public class Hitbox : MonoBehaviour

    {
        public HitboxStates hitboxState;
        [SerializeField] BoxCollider2D[] boxColliders;                             
        [SerializeField] LayerMask hitLayers;
        [SerializeField] string[] hitTags;

        public HitProperties activeProperties;

        [Header("Read Only")]

        [SerializeField] bool successfulCheckThisFrame;

        ContactFilter2D overlapContactFilter;
        Collider2D[] overlapContacts;
        Collider2D dummyTarget;


        [Header("Debugging")]

        [SerializeField] Color activeColour;
        [SerializeField] Color inactiveColour;
        [SerializeField] [Range(0, 1)] float gizmoOpacity;

        [SerializeField] bool _DebugUpdates;
        [SerializeField] bool _DebugEvents;
        [SerializeField] bool _DebugGizmos;




        // Public properties/events

        /// <summary>
        /// Returns true if the hitbox is currently touching a valid object
        /// </summary>
        public bool IsTouchingLayers {
            get { return successfulCheckThisFrame; }
        }

        public delegate void OnSuccessfulCheck();
        public event OnSuccessfulCheck onSuccessfulCheck;





               

        void OnValidate() {
            if (boxColliders.Length == 0) boxColliders = boxColliders ?? GetComponentsInChildren<BoxCollider2D>();
        }

        void Start() {
            overlapContactFilter.layerMask = hitLayers;
            overlapContactFilter.useTriggers = true;
            overlapContactFilter.useLayerMask = true;
        }



        void FixedUpdate() {

            successfulCheckThisFrame = false;

            //  Send event to all listeners if any of the hitboxes are touching a valid object
            foreach (BoxCollider2D hitbox in boxColliders) {

                // Skip this collider if it is not active
                if (!hitbox.gameObject.activeInHierarchy) continue;

                //hitbox.IsTouchingLayers(hitLayers) && 
                if (IsTouchingHitbox(hitbox, out dummyTarget)) {

                    // Cancel if my state is the same as the dummy targets state
                    if (hitboxState != dummyTarget.GetComponentInParent<Hitbox>().hitboxState) {

                        // If I am the active hitbox
                        if (hitboxState == HitboxStates.Active) {

                            // Send event to all listeners if any hitbox is successful
                            successfulCheckThisFrame = true;
                            onSuccessfulCheck?.Invoke();
                            break;

                        }
                        // If I am the hurtbox
                        else if (dummyTarget.GetComponentInParent<Hitbox>().activeProperties) {

                            // Set the damage, stun, proration, velocity


                        } else Debug.LogError("[Hitbox] ERROR -> No active properties set in hitbox target!");    
                    }
                }
            }
        }

       
        bool IsTouchingHitbox(BoxCollider2D hitbox, out Collider2D targetHitbox) {

            // Return if there are no overlap results
            if (hitbox.OverlapCollider(overlapContactFilter, overlapContacts) > 0) {
                // Iterate through each trigger overlapping the hitbox
                for (int i = 0; i < overlapContacts.Length; i++) {
                    // Check against each hit tag
                    for (int j = 0; j < hitTags.Length; j++) {
                        // Return the target if it matches one of the hit tags
                        if (overlapContacts[i].tag == hitTags[j]) {
                            targetHitbox = overlapContacts[i];
                            return true;
                        }
                    }
                }
            }
            targetHitbox = null;
            return false;            

        }



        void OnDrawGizmosSelected() {

            if (boxColliders.Length != 0) {

                foreach (BoxCollider2D hitbox in boxColliders) {
                    // Draw the wireframe
                    Gizmos.color = successfulCheckThisFrame ? activeColour : inactiveColour;
                    Gizmos.DrawWireCube(hitbox.bounds.center, hitbox.bounds.size);

                    // Draw the centre
                    Gizmos.color = successfulCheckThisFrame ? new Color(activeColour.r, activeColour.g, activeColour.b, gizmoOpacity)
                                                            : new Color(inactiveColour.r, inactiveColour.g, inactiveColour.b, gizmoOpacity);
                    Gizmos.DrawCube(hitbox.bounds.center, hitbox.bounds.size);
                }                

            } else boxColliders = boxColliders ?? GetComponentsInChildren<BoxCollider2D>();  

        }

    }
}
