namespace Assets.GameLogic.Core
{
    public interface IInteractable
    {
        void OnPointerEnter();
        void OnPointerExit();
        void OnLeftClick();
        void OnRightClick();
    }
}
