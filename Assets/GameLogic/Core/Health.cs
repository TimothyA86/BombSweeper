using UnityEngine;
using UnityEngine.Events;
using System;

namespace Assets.GameLogic.Core
{
    public class Health : MonoBehaviour
    {
        private const uint MaxHealth = 100;

        [Serializable] private class HealthDepletedHandler : UnityEvent, IUnityEvent { }
        [Serializable] private class HealthChangedHandler : UnityEvent<uint>, IUnityEvent<uint> { }

        [SerializeField] [Range(1, MaxHealth)] private uint value = 1;
        [SerializeField] private HealthDepletedHandler DepletedEvent;
        [SerializeField] private HealthChangedHandler ValueChangedEvent;

        public uint Value { get { return value; } }
        public IUnityEvent Depleted { get { return DepletedEvent; } }
        public IUnityEvent<uint> ValueChanged { get { return ValueChangedEvent; } }

        public void Damage(uint amount)
        {
            value -= amount;

            if (value <= 0)
            {
                value = 0;
                ValueChangedEvent.Invoke(value);
                DepletedEvent.Invoke();
            }
            else
            {
                ValueChangedEvent.Invoke(value);
            }

            Debug.Log(gameObject.name + " OUCH");
        }

        public void Heal(uint amount)
        {
            value = Math.Min(value + amount, MaxHealth);
            ValueChangedEvent.Invoke(value);
        }
    }
}
