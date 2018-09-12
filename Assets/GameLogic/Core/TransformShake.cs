using UnityEngine;
using System.Collections;

namespace Assets.GameLogic.Core
{
    public class TransformShake : MonoBehaviour
    {
        [SerializeField] [Range(0, 5)] private float duration;
        [SerializeField] [Range(0, 5)] private float intensitiy;

        public void Shake()
        {
            StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            var transform = this.transform;
            var offset = Vector3.zero;
            float time = duration;

            while (time > 0f)
            {
                float ratio = time / duration;
                offset.x = Random.Range(-intensitiy, intensitiy) * ratio;
                offset.y = Random.Range(-intensitiy, intensitiy) * ratio;
                offset.z = Random.Range(-intensitiy, intensitiy) * ratio;

                transform.position += offset;

                time -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
