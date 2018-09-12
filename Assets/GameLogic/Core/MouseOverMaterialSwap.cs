using UnityEngine;

namespace Assets.GameLogic.Core
{
    // TODO: Replace with a better system (Not using unity's message system)
    public class MouseOverMaterialSwap : MonoBehaviour, IInteractable
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material hoverMaterial;
        private Material material;

        private void Awake()
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (meshRenderer == null)
            {
                Debug.LogError(name + " needs to have a MeshRenderer on its gameObject or one of its children!");
                Destroy(this);
                return;
            }

            material = meshRenderer.material;
        }

        #region IInteractable
        public void OnPointerEnter()
        {
            meshRenderer.material = hoverMaterial;
        }

        public void OnPointerExit()
        {
            meshRenderer.material = material;
        }

        public void OnLeftClick() { }
        public void OnRightClick() { }
        #endregion
    }
}
