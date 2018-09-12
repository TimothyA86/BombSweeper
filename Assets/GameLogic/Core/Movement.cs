using UnityEngine;
using Assets.GameLogic.Updater;

namespace Assets.GameLogic.Core
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(IMovementController))]
    public class Movement : MonoBehaviour, IUpdatable
    {
        // TODO: Improve handling (braking / accelerating)
        [SerializeField] [Range(0f, 5f)] private float walkForce = 0f;
        [SerializeField] [Range(0f, 5f)] private float stopForce = 0f;
        [SerializeField] [Range(0f, 10f)] private float maxSpeed = 0f;
        private new Rigidbody rigidbody;
        private IMovementController control;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            control = GetComponent<IMovementController>();
        }

        private void OnEnable()
        {
            UpdateManager.MovementUpdater.Register(this);
        }

        private void OnDisable()
        {
            UpdateManager.MovementUpdater.Deregister(this);
        }

        public void OnUpdate()
        {
            ApplyBrakes();
            Accelerate(control.InputAxes.normalized, walkForce);
        }

        private void Accelerate(Vector3 direction, float expectedFriction = 0f)
        {
            var velocity = rigidbody.velocity;
            velocity.y = 0f;

            var predictedVelocity = velocity + direction * walkForce - velocity.normalized * expectedFriction;
            float force = (predictedVelocity.magnitude > maxSpeed)
                ? Mathf.Max(maxSpeed - velocity.magnitude + expectedFriction, 0f)
                : walkForce;

            rigidbody.AddRelativeForce(direction * force, ForceMode.VelocityChange);
        }

        private void ApplyBrakes(float expectedFriction = 0f)
        {
            var velocity = rigidbody.velocity;
            var inputDirection = control.InputAxes.normalized;

            float horizontalDirection = Mathf.Sign(velocity.x);

            if (inputDirection.x == 0 || control.InputAxes.x == -horizontalDirection)
            {
                float force = Mathf.Sign(velocity.x - stopForce * horizontalDirection) == horizontalDirection ? stopForce : Mathf.Abs(velocity.x);
                rigidbody.AddRelativeForce(Vector3.left * horizontalDirection * force, ForceMode.VelocityChange);
            }

            float verticalDirection = Mathf.Sign(velocity.z);

            if (inputDirection.z == 0 || control.InputAxes.z == -verticalDirection)
            {
                float force = Mathf.Sign(velocity.z - stopForce * verticalDirection) == verticalDirection ? stopForce : Mathf.Abs(velocity.z);
                rigidbody.AddRelativeForce(Vector3.back * verticalDirection * force, ForceMode.VelocityChange);
            }
        }
    }
}
