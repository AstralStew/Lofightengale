using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    [System.Serializable]
    [CreateAssetMenu(fileName = "NewAnimationPackage", menuName = "Paperticket/Create New Animation Package", order = 1)]
    public class AnimationPackage : ScriptableObject {

        public Vector2 newVelocity;

    }


}