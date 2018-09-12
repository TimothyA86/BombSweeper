using UnityEngine;
using Assets.GameLogic.Updater;

namespace Assets.GameLogic.Core
{
    public sealed class UpdateManager : MonoBehaviour
    {
        private static UpdateManager instance;
        
        private static Updater<IUpdatable> generalUpdater = new Updater<IUpdatable>();
        private static Updater<IUpdatable> generalFixedUpdater = new Updater<IUpdatable>();
        private static Updater<PlayerControl> playerUpdater = new Updater<PlayerControl>();
        private static Updater<Movement> movementUpdater = new Updater<Movement>();

        public static IUpdater<IUpdatable> GeneralUpdater { get { return generalUpdater; } }
        public static IUpdater<IUpdatable> GeneralFixedUpdater { get { return generalFixedUpdater; } }
        public static IUpdater<PlayerControl> PlayerUpdater { get { return playerUpdater; } }
        public static IUpdater<Movement> MovementUpdater { get { return movementUpdater; } }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else if (instance != this)
            {
                Debug.LogWarning("Instance of " + GetType().Name + " already exists. Removed.");
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            playerUpdater.Update();
            generalUpdater.Update();
        }

        private void FixedUpdate()
        {
            movementUpdater.Update();
            generalFixedUpdater.Update();
        }
    }
}