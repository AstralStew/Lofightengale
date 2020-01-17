using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    [System.Serializable]
    [CreateAssetMenu(fileName = "NewAnimationPackage", menuName = "Paperticket/Create New Animation Package", order = 1)]
    public class AnimationPackage : ScriptableObject {

        [Header("SetVelocity values")]
        [Tooltip("For use with SetVelocity animation event. Set the velocity of the controlling character.")]
        public Vector2 newVelocity;


        [Header("SetHitProperties values")]
        [Tooltip("For use with SetHitProperties animation event. Set the damage of the next successful hit.")]
        public float hitDamage;

        [Tooltip("For use with SetHitProperties animation event. Set the stun of the next successful hit.")]
        public float hitStun;

        [Tooltip("For use with SetHitProperties animation event. Set the proration of the next successful hit.")]
        public float hitProration;

        [Tooltip("For use with SetHitProperties animation event. Set the velocity to transfer to the object hit.")]
        public Vector2 hitVelocity;


    }


}