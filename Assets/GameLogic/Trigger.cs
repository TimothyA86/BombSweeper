using UnityEngine;
using UnityEngine.Events;
using System;

namespace Assets.GameLogic.Core
{
    public class Trigger : MonoBehaviour
    {
        [Serializable] private class TriggerEventHandler : UnityEvent<Collider>, IUnityEvent<Collider> { }
        [SerializeField] private TriggerEventHandler TriggeredEvent;

        public IUnityEvent<Collider> Triggered { get { return TriggeredEvent; } }

        private void OnTriggerEnter(Collider other)
        {
            TriggeredEvent.Invoke(other);   
        }
    }
}
