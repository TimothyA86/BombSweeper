using UnityEngine;
using System;
using System.Collections;

namespace Assets.GameLogic.Core
{
    public class Bomb : MonoBehaviour
    {
        public static event Action<Bomb> Exploded = _ => { };

        [SerializeField] private float explosiveForce;
        [SerializeField] private float upwardForce;
        [SerializeField] [Range(1, uint.MaxValue)] private uint damage = 1;
        [SerializeField] private LayerMask hitMask;
        private IEnumerator routine;

        public void Explode(float fuseTime)
        {
            if (routine == null)
            {
                routine = ExplodeRoutine(fuseTime);
                StartCoroutine(routine);
            }
        }

        private IEnumerator ExplodeRoutine(float fuseTime)
        {
            yield return new WaitForSeconds(fuseTime);
            Exploded(this);
            // TODO: particle effect       
            DamageNearByHealth();
            gameObject.SetActive(false);
        }

        private void DamageNearByHealth()
        {
            var position = transform.position;
            var hits = Physics.OverlapBox(position, Vector3.one * 1.5f, Quaternion.identity, hitMask);

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<Health>();

                if (health != null)
                {
                    health.Damage(damage);
                }

                var rigidbody = hit.GetComponent<Rigidbody>();

                if (rigidbody != null)
                {
                    rigidbody.AddExplosionForce(explosiveForce, position, 2.5f, upwardForce, ForceMode.Impulse);
                }
            }
        }
    }
}
