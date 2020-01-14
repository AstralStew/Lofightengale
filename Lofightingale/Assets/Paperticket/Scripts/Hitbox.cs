using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket
{
       
    public class Hitbox : MonoBehaviour

    {
        [SerializeField] BoxCollider2D[] boxColliders;                             
        [SerializeField] LayerMask hitLayers;
        [SerializeField] string[] hitTags;


        [Header("Debugging Options")]

        [SerializeField] Color activeColour;
        [SerializeField] Color inactiveColour;
        [SerializeField] [Range(0, 1)] float gizmoOpacity;

        [SerializeField] bool _DebugUpdates;
        [SerializeField] bool _DebugEvents;
        [SerializeField] bool _DebugGizmos;


        [Header("Read Only")]

        [SerializeField] bool successfulCheckThisFrame;

        ContactFilter2D overlapContactFilter;
        Collider2D[] overlapContacts;


        // Public properties/events

        /// <summary>
        /// Returns true if the hitbox is currently touching a valid object
        /// </summary>
        public bool IsTouchingLayers {
            get { return successfulCheckThisFrame; }
        }

        public delegate void OnSuccessfulCheck();
        public event OnSuccessfulCheck onSuccessfulCheck;
               

        public void ReceiveHitInformation() {

        }



               

        void OnValidate() {
            if (boxColliders.Length == 0) boxColliders = boxColliders ?? GetComponentsInChildren<BoxCollider2D>();
        }

        void Start() {
            overlapContactFilter.layerMask = hitLayers;
            overlapContactFilter.useTriggers = true;
            overlapContactFilter.useLayerMask = true;
        }



        void FixedUpdate() {

            //  Send event to all listeners if any of the hitboxes are touching a valid object
            foreach (BoxCollider2D hitbox in boxColliders) {
                if (hitbox.IsTouchingLayers(hitLayers) && IsTouchingHitbox()) {
                    successfulCheckThisFrame = true;
                    onSuccessfulCheck?.Invoke();
                    break;
                } else {
                    successfulCheckThisFrame = false;
                }
            }            

        }

       
        bool IsTouchingHitbox() {

            foreach (BoxCollider2D hitbox in boxColliders) {

                // Iterate through each trigger overlapping the collider
                hitbox.OverlapCollider(overlapContactFilter, overlapContacts);
                for (int i = 0; i < overlapContacts.Length; i++) {
                    // Check against each hit tag
                    for (int j = 0; j < hitTags.Length; j++) {
                        if (overlapContacts[i].tag == hitTags[j]) {
                            return true;
                        }
                    }
                }
            }

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
