using UnityEngine;

namespace Assets.GameLogic.Core
{
    public interface IMovementController
    {
        Vector3 InputAxes { get; }
    }
}