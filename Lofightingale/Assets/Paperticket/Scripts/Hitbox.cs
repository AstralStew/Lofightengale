using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket
{

    [RequireComponent(typeof(BoxCollider2D))]
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] BoxCollider2D boxCollider;

        [SerializeField] Color gizmoColour;
        [SerializeField] [Range(0, 1)] float gizmoOpacity;



        void OnDrawGizmosSelected() {


            if (boxCollider) {

                // Draw the wireframe
                Gizmos.color = gizmoColour;
                Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);

                // Draw the centre
                Gizmos.color = new Color(gizmoColour.r, gizmoColour.g, gizmoColour.b, gizmoOpacity);
                Gizmos.DrawCube(boxCollider.bounds.center, boxCollider.bounds.size);

            } else {
                boxCollider = GetComponent<BoxCollider2D>();
            }


        }

    }

}
