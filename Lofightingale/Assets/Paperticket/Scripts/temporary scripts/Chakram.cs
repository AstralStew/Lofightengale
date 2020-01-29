using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    public class Chakram : MonoBehaviour {
                

        [SerializeField] HitProperties hitProperties;

        [SerializeField] float attackTime;
        [SerializeField] float reloadTime;
        [SerializeField] int spinSpeed;

        [SerializeField] bool _Debug;

        GameObject sprite;
        Hitbox activeBox;
        Hitbox proxbox;


        // Start is called before the first frame update
        void Awake() {

            sprite = GetComponentInChildren<SpriteRenderer>().gameObject;
            foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>()) {
                if (hitbox.hitboxState == HitboxStates.Active) activeBox = hitbox;
                if (hitbox.hitboxState == HitboxStates.Proximity) proxbox = hitbox;
            }

            if (!hitProperties) {
                Debug.LogError("[Chakram] ERROR -> No hit properties provided to SetHitProperties. Did you forget to add the object to the inspector?");
                return;
            }

            // Set the active properties of all the active hitboxes 
            activeBox.activeProperties = hitProperties;

            StartCoroutine(WaitToReload());

        }


        void OnEnable() {
            activeBox.onSuccessfulCheck += Reload;
        }
        void OnDisable() {
            activeBox.onSuccessfulCheck -= Reload;
        }


        void Reload (HitProperties hitProperties) {            

            if (hitProperties.HitboxState == HitboxStates.Hurtbox) {
                if (_Debug) Debug.Log("[Chakram] Omgosh I hit something! OwO");

                StopAllCoroutines();
                StartCoroutine(WaitToReload());          

            }

        }

        
        IEnumerator WaitToReload() {
            
            yield return null;
            activeBox.SetHitboxActive(false);
            proxbox.SetHitboxActive(false);
                        
            yield return new WaitForSeconds(reloadTime);            
            
            // Start attacking instead
            StartCoroutine(WaitForAttack());
                       
        }

        float time;
        IEnumerator WaitForAttack() {

            activeBox.SetHitboxActive(true);
            proxbox.SetHitboxActive(true);

            // Spin the sprite as long as the attack is active
            time = reloadTime;
            while (time > 0) {
                time -= Time.deltaTime;
                sprite.transform.Rotate(0, 0, (180f / reloadTime) * spinSpeed * Time.deltaTime);
                yield return null;
            }
            sprite.transform.rotation = Quaternion.identity;
            
            // Start reloading
            StartCoroutine(WaitToReload());
        }

    }

}