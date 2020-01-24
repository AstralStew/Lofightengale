using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket
{

    public enum HitboxStates { Active, Hurtbox, Proximity }
       
    public class Hitbox : MonoBehaviour

    {
        public HitboxStates hitboxState;
        [SerializeField] BoxCollider2D[] boxColliders;                             
        [SerializeField] LayerMask targetLayers;
        [SerializeField] string[] targetTags;
        [SerializeField] HitboxStates[] targetStates;

        [SerializeField] int concurrentHitsAllowed;

        [Header("Read Only")]

        [SerializeField] bool successfulCheckThisFrame;


        public HitProperties activeProperties;

        ContactFilter2D overlapContactFilter;
        [SerializeField] List<Collider2D> overlapContacts;
        [SerializeField] List<Hitbox> ValidHits;
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
            // Setup the overlap contact settings
            overlapContactFilter.layerMask = targetLayers;
            overlapContactFilter.useTriggers = true;
            overlapContactFilter.useLayerMask = true;
            //overlapContacts = new Collider2D[concurrentHitsAllowed];

            // Match the HitProperties state to this hitbox
            if (!activeProperties) {
                activeProperties = ScriptableObject.CreateInstance<HitProperties>();
                activeProperties.HitboxState = hitboxState;
            }
        }

        

        void FixedUpdate() {

            successfulCheckThisFrame = false;
            ValidHits = new List<Hitbox>();

            //  Send event to all listeners if any of the hitboxes are touching a valid object
            foreach (BoxCollider2D hitbox in boxColliders) {

                // Skip this collider if it is not active
                if (!hitbox.gameObject.activeInHierarchy) continue;

                // Add any valid hits to the list 
                GenerateValidHits(hitbox);

            }


            if (ValidHits.Count > 0) {
                successfulCheckThisFrame = true;

                foreach (Hitbox hitbox in ValidHits) {

                    onSuccessfulCheck?.Invoke(hitbox.activeProperties);
                    if (_DebugEvents) Debug.Log("[Hitbox] Hitbox ('" + gameObject.name + "') was successfully triggered by (" + hitbox.name + ")");

                }
            }

        }


            //    //hitbox.IsTouchingLayers(hitLayers) && 
            //    if (IsTouchingHitbox(hitbox, out targetHitbox)) {

            //        //// Cancel if my state is the same as the dummy targets state
            //        //switch (hitboxState) {
            //        //    case HitboxStates.Active:                            
            //        //        if (targetHitbox.hitboxState == HitboxStates.Hurtbox) {                                
            //        //            successfulCheckThisFrame = true;
            //        //        }
            //        //        break;
            //        //    case HitboxStates.Hurtbox:
            //        //        if (targetHitbox.hitboxState == HitboxStates.Active) {
            //        //            successfulCheckThisFrame = true;
            //        //        }
            //        //        break;
            //        //    case HitboxStates.Proximity:
            //        //        if (targetHitbox.hitboxState == HitboxStates.Active) {
            //        //            successfulCheckThisFrame = true;
            //        //        }
            //        //        break;
            //        //    default:
            //        //        Debug.LogError("[Hitbox] ERROR -> Bad HitboxState received by hitbox!");
            //        //        break;
            //        //}

            //        //if (successfulCheckThisFrame) {
            //            // Send event to all listeners if any hitbox is successful
            //            successfulCheckThisFrame = true;
            //            onSuccessfulCheck?.Invoke(targetHitbox.activeProperties);
            //            if (_DebugEvents) Debug.Log("[Hitbox] Hitbox ('" + gameObject.name + "') was successfully triggered!");
            //            break;   
            //        //}
            //    }
            ////}
        //}

       
        //bool IsTouchingHitbox(BoxCollider2D hitbox, out Hitbox targetHitbox) {

        //    // Return if there are no overlap results
        //    if (hitbox.OverlapCollider(overlapContactFilter, overlapContacts) > 0) {
        //        if (_DebugUpdates) Debug.Log("[Hitbox] " + hitbox.OverlapCollider(overlapContactFilter, overlapContacts) + " hit results:");
        //        // Iterate through each trigger overlapping the hitbox
        //        for (int i = 0; i < overlapContacts.Count; i++) {
        //            // Check against each hit tag
        //            for (int j = 0; j < targetTags.Length; j++) {
        //                // Make sure the target is not null
        //                if (overlapContacts[i]) { 
        //                    // Make sure the target has the right tag
        //                    if (overlapContacts[i].tag == targetTags[j]) {
        //                        if (_DebugEvents) Debug.Log("myname = " + gameObject.name + ", target tag = " + targetTags[j] + ", overlap no = " + i + System.Environment.NewLine +
        //                                                            "overlapped name = " + overlapContacts[i].gameObject.name + ", overlapped contact's tag =" + overlapContacts[i].tag);
        //                        targetHitbox = overlapContacts[i].GetComponentInParent<Hitbox>();
        //                        for (int k = 0; k < targetStates.Length; k++) {
        //                            // Return the target if it matches one of the hit tags
        //                            if (targetHitbox.hitboxState == targetStates[k]) {
        //                                if (_DebugEvents) Debug.Log("[Hitbox] Target state (" + targetStates[k] + ") matches!");
        //                                return true;
        //                            }
        //                        }                                
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    targetHitbox = null;
        //    return false;            

        //}

       
        
        void GenerateValidHits ( BoxCollider2D hitbox) {

            //validHits = new List<Hitbox>();

            // Return if there are no overlap results
            if (hitbox.OverlapCollider(overlapContactFilter, overlapContacts) > 0) {
                if (_DebugUpdates) Debug.Log("[Hitbox] " + hitbox.OverlapCollider(overlapContactFilter, overlapContacts) + " hit results:");
                // Iterate through each trigger overlapping the hitbox
                for (int i = 0; i < overlapContacts.Count; i++) {
                        // Make sure the target is not null
                        if (overlapContacts[i]) {
                        // Check against each hit tag
                        for (int j = 0; j < targetTags.Length; j++) {

                            // Make sure the target has the right tag
                            if (overlapContacts[i].tag == targetTags[j]) {
                                if (_DebugEvents) Debug.Log("myname = " + gameObject.name + ", target tag = " + targetTags[j] + ", overlap no = " + i + System.Environment.NewLine +
                                                                    "overlapped name = " + overlapContacts[i].gameObject.name + ", overlapped contact's tag =" + overlapContacts[i].tag);

                                // Grab the hitbox component above the overlapping collider
                                targetHitbox = overlapContacts[i].GetComponentInParent<Hitbox>();

                                // Make sure target matches one of the hit states
                                for (int k = 0; k < targetStates.Length; k++) {                                    
                                    if (targetHitbox.hitboxState == targetStates[k]) {
                                        if (_DebugEvents) Debug.Log("[Hitbox] Target (" + targetHitbox.name + ") state (" + targetStates[k] + ") matches what I'm looking for!");

                                        // Add the target hitbox if it isn't a duplicate
                                        if (!ValidHits.Contains(targetHitbox)) {
                                            ValidHits.Add(targetHitbox);

                                            // Goto the next overlap contact
                                            j = targetTags.Length;
                                            break;
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }      
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
