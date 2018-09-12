using UnityEngine;
using Assets.GameLogic.Updater;

namespace Assets.GameLogic.Core
{
    public class PlayerControl : MonoBehaviour, IMovementController, IUpdatable
    {
        private Vector3 inputAxes = new Vector3();
        public Vector3 InputAxes { get { return inputAxes; } }

        private void OnEnable()
        {
            UpdateManager.PlayerUpdater.Register(this);
        }

        private void OnDisable()
        {
            UpdateManager.PlayerUpdater.Deregister(this);
            inputAxes *= 0f;
        }

        public void OnUpdate()
        {
            inputAxes.x = GetAxis(KeyCode.D, KeyCode.A);
            inputAxes.z = GetAxis(KeyCode.W, KeyCode.S);
        }

        private float GetAxis(KeyCode positiveKey, KeyCode negativeKey)
        {
            return (Input.GetKey(positiveKey) ? 1f : 0f) - (Input.GetKey(negativeKey) ? 1f : 0f);
        }
    }
}
