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

        [SerializeField] int concurrentHitsAllowed;

        [Header("Read Only")]

        [SerializeField] bool successfulCheckThisFrame;

        public HitProperties activeProperties;

        ContactFilter2D overlapContactFilter;
        [SerializeField] Collider2D[] overlapContacts;
        Hitbox targetHitbox;


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

        public delegate void OnSuccessfulCheck( HitProperties hitProperties);
        public event OnSuccessfulCheck onSuccessfulCheck;





               

        void OnValidate() {
            if (boxColliders.Length == 0) boxColliders = boxColliders ?? GetComponentsInChildren<BoxCollider2D>();
        }

        void Start() {
            overlapContactFilter.layerMask = hitLayers;
            overlapContactFilter.useTriggers = true;
            overlapContactFilter.useLayerMask = true;
            overlapContacts = new Collider2D[concurrentHitsAllowed];
        }



        void FixedUpdate() {

            successfulCheckThisFrame = false;

            //  Send event to all listeners if any of the hitboxes are touching a valid object
            foreach (BoxCollider2D hitbox in boxColliders) {

                // Skip this collider if it is not active
                if (!hitbox.gameObject.activeInHierarchy) continue;

                //hitbox.IsTouchingLayers(hitLayers) && 
                if (IsTouchingHitbox(hitbox, out targetHitbox)) {

                    // Cancel if my state is the same as the dummy targets state
                    if (hitboxState != targetHitbox.hitboxState) {

                        // Send event to all listeners if any hitbox is successful
                        successfulCheckThisFrame = true;
                        onSuccessfulCheck?.Invoke(targetHitbox.activeProperties);
                        if (_DebugEvents) Debug.Log("[Hitbox] Hitbox ('" + gameObject.name + "') was successfully triggered!");
                        break;   
                    }
                }
            }
        }

       
        bool IsTouchingHitbox(BoxCollider2D hitbox, out Hitbox targetHitbox) {

            // Return if there are no overlap results
            if (hitbox.OverlapCollider(overlapContactFilter, overlapContacts) > 0) {
                // Iterate through each trigger overlapping the hitbox
                for (int i = 0; i < overlapContacts.Length; i++) {
                    // Check against each hit tag
                    for (int j = 0; j < hitTags.Length; j++) {
                        // Make sure the target is not null
                        if (overlapContacts[i]) { 
                            if (_DebugEvents) Debug.Log("myname = " + gameObject.name + ", overlapped name = " + overlapContacts[i].gameObject.name + System.Environment.NewLine +
                                                        "overlap no = " + i + ", overlapped contact's tag =" + overlapContacts[i].tag + ", target tag = " + hitTags[j]);
                            // Return the target if it matches one of the hit tags
                            if (overlapContacts[i].tag == hitTags[j]) {
                                targetHitbox = overlapContacts[i].GetComponentInParent<Hitbox>();
                                return true;
                            }
                        }
                    }
                }
            }
            targetHitbox = null;
            return false;            

        }




        public void SetHitboxActive (bool active) {
            foreach (BoxCollider2D hitbox in boxColliders) {
                hitbox.gameObject.SetActive(active);
            }
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
