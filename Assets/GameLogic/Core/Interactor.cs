using UnityEngine;
using System.Collections.Generic;
using Assets.GameLogic.Updater;

namespace Assets.GameLogic.Core
{
    public sealed class Interactor : MonoBehaviour, IUpdatable
    {
        [SerializeField] private uint range = 1;
        [SerializeField] private LayerMask interactMask;
        [SerializeField] new private Camera camera;
        private GameObject previousGameObject;
        private List<IInteractable> previousInteractables = new List<IInteractable>();
        private List<IInteractable> interactables = new List<IInteractable>();

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

        private void InvokeOnPointerEnter(List<IInteractable> interactables)
        {
            foreach (var interactable in interactables)
            {
                interactable.OnPointerEnter();
            }
        }

        private void InvokeOnPointerExit(List<IInteractable> interactables)
        {
            foreach (var interactable in interactables)
            {
                interactable.OnPointerExit();
            }
        }

        private void InvokeOnLeftClick(List<IInteractable> interactables)
        {
            foreach (var interactable in interactables)
            {
                interactable.OnLeftClick();
            }
        }

        private void InvokeOnRightClick(List<IInteractable> interactables)
        {
            foreach (var interactable in interactables)
            {
                interactable.OnRightClick();
            }
        }

        public void OnUpdate()
        {
            interactables.Clear();

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, interactMask))
            {
                var gameObject = hit.collider.gameObject;
                int distX = Mathf.Abs((int)transform.position.x - (int)gameObject.transform.position.x);
                int distZ = Mathf.Abs((int)transform.position.z - (int)gameObject.transform.position.z);

                if (distX <= range && distZ <= range)
                {
                    interactables.AddRange(gameObject.GetComponents<IInteractable>());

                    if (previousGameObject != gameObject)
                    {
                        InvokeOnPointerExit(previousInteractables);
                        InvokeOnPointerEnter(interactables);

                        previousGameObject = gameObject;
                        previousInteractables.Clear();
                        previousInteractables.AddRange(interactables);
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        InvokeOnRightClick(interactables);
                    }
                    else if (Input.GetMouseButtonDown(0))
                    {
                        InvokeOnLeftClick(interactables);
                    }

                    return;
                }
            }

            InvokeOnPointerExit(previousInteractables);

            previousGameObject = null;
            previousInteractables.Clear();
        }
    }
}