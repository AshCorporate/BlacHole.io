using UnityEngine;
using BlacHole.Core;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Movement;
using BlacHole.Gameplay.States;
using BlacHole.Gameplay.Tail;
using BlacHole.Gameplay.Territory;
using BlacHole.Gameplay.Progression;
using BlacHole.Gameplay.Buffs;
using BlacHole.Data;

namespace BlacHole.Gameplay.Player
{
    /// <summary>
    /// MonoBehaviour that bridges Unity input to the movement motor and owns PlayerEntity data.
    /// </summary>
    [RequireComponent(typeof(PlayerView))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private MovementConfig  movementConfig;
        [SerializeField] private PlayerConfig    playerConfig;
        [SerializeField] private TailConfig      tailConfig;
        [SerializeField] private TerritoryConfig territoryConfig;

        public PlayerEntity Entity { get; private set; }

        private IMovementMotor      _motor;
        private ITailService        _tail;
        private ITerritoryService   _territory;
        private ProgressionService  _progression;
        private BuffSystem          _buffs;
        private PlayerStateMachine  _stateMachine;
        private PlayerView          _view;

        private void Awake()
        {
            Entity = new PlayerEntity
            {
                Id          = 0,
                Name        = "Player",
                Mass        = playerConfig != null ? playerConfig.StartMass    : 10f,
                Radius      = playerConfig != null ? playerConfig.BaseRadius   : 0.5f,
                PlayerColor = Color.cyan,
                IsAlive     = true
            };

            _view        = GetComponent<PlayerView>();
            _buffs       = new BuffSystem();
            _progression = new ProgressionService(Entity, _buffs, playerConfig);
            _motor       = new TopDownMovementMotor(Entity, movementConfig);
            _tail        = ServiceLocator.TryGet<ITailService>(out var ts) ? ts : new TailService(Entity, tailConfig);
            _territory   = ServiceLocator.TryGet<ITerritoryService>(out var ter) ? ter : null;
            _stateMachine = new PlayerStateMachine(Entity, _tail, _territory);

            ServiceLocator.Register<PlayerController>(this);
        }

        private void Update()
        {
            if (!Entity.IsAlive) return;

            float dt = Time.deltaTime;

            // Tick control-lock timer
            if (Entity.ControlLockTimer > 0f)
                Entity.ControlLockTimer -= dt;

            // Read input
            Vector2 input = Vector2.zero;
            if (!Entity.IsControlLocked)
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // Tick buffs & recalc speed modifier
            _buffs.Tick(dt);
            Entity.SpeedModifier = _buffs.TotalSpeedModifier;

            // Move
            _motor.Tick(input, dt);
            transform.position = new Vector3(Entity.Position.x, Entity.Position.y, 0f);

            // Tick tail
            _tail.Tick(dt);

            // Tick state machine
            _stateMachine.Tick(dt);

            // Sync view
            _view.Refresh(Entity);
        }
    }
}
