using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace Paperticket
{

    [System.Serializable]
    [CreateAssetMenu(fileName = "NewHitProperties", menuName = "Paperticket/Create New Hit Properties", order = 1)]
    public class HitProperties : ScriptableObject {
                
        [Header("Hit Properties")]
               
        [Tooltip("Set the damage of the next successful hit.")]
        public int HitDamage;

        [Tooltip("Set the stun of the next successful hit.")]
        public float HitStun;

        [Tooltip("Set the proration of the next successful hit.")]
        public float HitProration;
        
        [Tooltip("Set the velocity to transfer to the object hit.")]
        public Vector2 HitVelocity;


        [Header("Automatically Set")]

        [Tooltip("The state of the hitbox this HitProperties belongs to")]
        public HitboxStates HitboxState;


    }


}