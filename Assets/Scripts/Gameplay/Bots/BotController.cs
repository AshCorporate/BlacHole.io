using UnityEngine;
using BlacHole.Core;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Movement;
using BlacHole.Gameplay.Player;
using BlacHole.Gameplay.States;
using BlacHole.Gameplay.Tail;
using BlacHole.Gameplay.Territory;
using BlacHole.Gameplay.Progression;
using BlacHole.Gameplay.Buffs;
using BlacHole.Data;

namespace BlacHole.Gameplay.Bots
{
    /// <summary>
    /// MonoBehaviour owning a BotEntity.
    /// Runs BotBrain each tick to get a desired direction, feeds it to the movement motor.
    /// </summary>
    [RequireComponent(typeof(PlayerView))]
    public class BotController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private MovementConfig  movementConfig;
        [SerializeField] private PlayerConfig    playerConfig;
        [SerializeField] private TailConfig      tailConfig;
        [SerializeField] private BotConfig       botConfig;

        public BotEntity   Bot    { get; private set; }
        public PlayerEntity Entity => Bot?.Entity;

        private IMovementMotor     _motor;
        private ITailService       _tail;
        private ITerritoryService  _territory;
        private ProgressionService _progression;
        private BuffSystem         _buffs;
        private PlayerStateMachine _stateMachine;
        private BotBrain           _brain;
        private PlayerView         _view;

        public void Initialise(int id, string name, Color color)
        {
            float mass   = playerConfig != null ? playerConfig.StartMass  : 10f;
            float radius = playerConfig != null ? playerConfig.BaseRadius : 0.5f;

            Bot = new BotEntity(id, name, color, mass, radius);

            _view        = GetComponent<PlayerView>();
            _buffs       = new BuffSystem();
            _progression = new ProgressionService(Entity, _buffs, playerConfig);
            _motor       = new TopDownMovementMotor(Entity, movementConfig);

            ITerritoryService territory = null;
            ServiceLocator.TryGet(out territory);
            _territory   = territory;

            _tail        = new TailService(Entity, tailConfig);
            _stateMachine = new PlayerStateMachine(Entity, _tail, _territory);

            _brain = new BotBrain(Bot, botConfig, _territory, null);
        }

        private void Update()
        {
            if (Bot == null || !Entity.IsAlive) return;

            float dt = Time.deltaTime;

            if (Entity.ControlLockTimer > 0f)
                Entity.ControlLockTimer -= dt;

            // Brain
            _brain.Tick(dt);
            Vector2 input = Entity.IsControlLocked ? Vector2.zero : _brain.DesiredInput;

            _buffs.Tick(dt);
            Entity.SpeedModifier = _buffs.TotalSpeedModifier;

            _motor.Tick(input, dt);
            transform.position = new Vector3(Entity.Position.x, Entity.Position.y, 0f);

            _tail.Tick(dt);
            _stateMachine.Tick(dt);
            _view.Refresh(Entity);
        }
    }
}
