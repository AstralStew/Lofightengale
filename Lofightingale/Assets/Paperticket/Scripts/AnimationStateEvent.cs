using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Paperticket {

    public class AnimationStateEvent : StateMachineBehaviour {

        public bool SendEventOnStateEnter;
        public bool SendEventOnStateExit;

        override public void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex ) {

            if (SendEventOnStateEnter) animator.GetComponent<AnimationManager>().RegisterAnimationStateEnter();

        }

        override public void OnStateExit( Animator animator, AnimatorStateInfo stateInfo, int layerIndex ) {

            if (SendEventOnStateExit) animator.GetComponent<AnimationManager>().RegisterAnimationStateExit();
                       
        }

        




    }

}
