using UnityEngine.Events;

namespace Assets.GameLogic.Core
{
    public interface IUnityEvent
    {
        void AddListener(UnityAction listener);
        void RemoveListener(UnityAction listener);
    }

    public interface IUnityEvent<T>
    {
        void AddListener(UnityAction<T> listener);
        void RemoveListener(UnityAction<T> listener);
    }
}
