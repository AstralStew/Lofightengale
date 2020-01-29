using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    public class Chakram : BaseEnemy {

        [Header("Chakram Properties")]

        [SerializeField] float reloadTime;

        [SerializeField] string spinAttackName;



        // Start is called before the first frame update
        public override void Awake() {

            base.Awake();

            // Set the active properties of all the active hitboxes 
            //activeBox.activeProperties = hitProperties;

            // Let it rip!
            Reload();

        }

        void OnEnable() {
            animationManager.onAnimationFinished += Reload;
        }

        void OnDisable() {
            animationManager.onAnimationFinished -= Reload;
        }




        //public void OnActiveboxCheck( HitProperties hitProperties ) {

        //    if (_DebugEvents) Debug.Log("[" + gameObject.name + "/Chakram] OnActiveboxCheck triggered.");
        //    Reload(hitProperties);

        //}

        //public void OnProxboxCheck( HitProperties hitProperties ) {
        //    if (_DebugEvents) Debug.Log("[" + gameObject.name + "/Chakram] OnActiveboxCheck triggered.");
        //}



        //void Reload( HitProperties hitProperties ) {

        //    if (hitProperties.HitboxState == HitboxStates.Hurtbox) {
        //        if (_DebugEvents) Debug.Log("[Chakram] Omgosh I hit something! OwO");

        //        StopAllCoroutines();
        //        StartCoroutine(WaitToReload());

        //    }

        //}



        void Reload() {
            StopAllCoroutines();
            StartCoroutine(WaitToReload());
        }
        IEnumerator WaitToReload() {

            yield return new WaitForSeconds(reloadTime);

            animationManager.SetAnimationTrigger(spinAttackName);


        }

    }

}