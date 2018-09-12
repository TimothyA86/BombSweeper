using UnityEngine;
using System;

namespace Assets.GameLogic.Core
{
    public class Cover : MonoBehaviour, IInteractable
    {
        public static event Action<Cover> CoverLeftClicked = _ => { };
        public static event Action<Cover> CoverRightClicked = _ => { };

        private GameObject flag;

        private void Awake()
        {
            flag = transform.Find("Flag").gameObject;
        }

        public void ShowFlag(bool show)
        {
            flag.SetActive(show);
        }

        #region IInteractable
        public void OnPointerEnter() { }

        public void OnPointerExit() { }

        public void OnLeftClick()
        {
            CoverLeftClicked(this);
        }

        public void OnRightClick()
        {
            CoverRightClicked(this);
        }
        #endregion
    }
}
