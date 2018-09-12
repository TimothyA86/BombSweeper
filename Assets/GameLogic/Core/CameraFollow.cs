using UnityEngine;
using Assets.GameLogic.Updater;

namespace Assets.GameLogic.Core
{
    public class CameraFollow : MonoBehaviour, IUpdatable
    {
        [SerializeField] new private Camera camera;
        [SerializeField] private Vector3 followVector;

        private void Awake()
        {
            if (camera == null)
            {
                Debug.LogWarning(gameObject.name + " was not assigned a camera.");
            }
        }

        private void OnEnable()
        {
            UpdateManager.GeneralUpdater.Register(this);
        }

        private void OnDisable()
        {
            UpdateManager.GeneralUpdater.Deregister(this);
        }

        public void OnUpdate()
        {
            if (camera != null)
            {
                camera.transform.position = transform.position + followVector;
            }
        }
    }
}
