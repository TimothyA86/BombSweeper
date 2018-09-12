using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace Assets.GameLogic.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [Serializable] private class BombExplodedHandler : UnityEvent<Bomb>, IUnityEvent<Bomb> { }
        [SerializeField] private BombExplodedHandler BombExplodedEvent;
        private IEnumerator routine;

        private void OnEnable()
        {
            Bomb.Exploded += BombExplodedEvent.Invoke;
        }

        private void OnDisable()
        {
            Bomb.Exploded -= BombExplodedEvent.Invoke;
        }

        // TEMP Methods
        public void RestartGame(float delay)
        {
            if (routine == null)
            {
                routine = RestartGameRoutine(delay);
                StartCoroutine(routine);
            }
        }

        private IEnumerator RestartGameRoutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            routine = null;
        }
    }
}