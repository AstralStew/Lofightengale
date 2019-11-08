using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    [RequireComponent(typeof(BoxCollider2D))]
    public class TriggerBoxChecker : MonoBehaviour {

               
        

        [SerializeField] LayerMask groundLayers;
        BoxCollider2D boxCollider;

        [Header("Debugging Options")]

        [SerializeField] bool _DebugUpdates;
        [SerializeField] bool _DebugEvents;
        [SerializeField] bool _DebugGizmos;
        [SerializeField] Color gizmoColour;
        [SerializeField] [Range(0,1)] float gizmoOpacity;

        [Header("Read Only")]

        [SerializeField] bool successfulCheckThisFrame;

        public bool IsTouchingLayers {
            get { return successfulCheckThisFrame; }
        }

        public delegate void OnSuccessfulCheck();
        public event OnSuccessfulCheck onSuccessfulCheck;

        

        void OnValidate() {

            if (!boxCollider) boxCollider = boxCollider ?? GetComponent<BoxCollider2D>(); 

        }

        void FixedUpdate() {

            if (boxCollider.IsTouchingLayers(groundLayers)) {
                successfulCheckThisFrame = true;
                onSuccessfulCheck?.Invoke();
            } else {
                successfulCheckThisFrame = false;
            }

        }

        //void OnTriggerStay( Collider other ) {
            
        //    // If we haven't been successfully grounded this frame
        //    if (!successfulCheckThisFrame) {

        //        // Check if the collider is the appropriate layer
        //        if (groundLayers == (groundLayers | (1 << other.gameObject.layer))) {

        //            onSuccessfulCheck?.Invoke();
        //            successfulCheckThisFrame = true;

        //        }

        //    }

        //}
                              

        void OnDrawGizmosSelected() {
            
            if (_DebugGizmos) {

                // Draw the wireframe
                Gizmos.color = gizmoColour;
                Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);

                // Draw the centre
                Gizmos.color = new Color(gizmoColour.r, gizmoColour.g, gizmoColour.b, gizmoOpacity);
                Gizmos.DrawCube(boxCollider.bounds.center, boxCollider.bounds.size);

            }

        }


    }

}