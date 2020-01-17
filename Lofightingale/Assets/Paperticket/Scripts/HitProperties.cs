using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace Paperticket
{

    [System.Serializable]
    [CreateAssetMenu(fileName = "NewHitProperties", menuName = "Paperticket/Create New Hit Properties", order = 1)]
    public class HitProperties : ScriptableObject
    {
        [Header("For use with SetHitProperties animation event.")]

        [Tooltip("Set the damage of the next successful hit.")]
        public int hitDamage;

        [Tooltip("Set the stun of the next successful hit.")]
        public float hitStun;

        [Tooltip("Set the proration of the next successful hit.")]
        public float hitProration;
        
        [Tooltip("Set the velocity to transfer to the object hit.")]
        public Vector2 hitVelocity;


    }


}