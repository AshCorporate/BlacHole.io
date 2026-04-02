using System.Collections.Generic;
using UnityEngine;
using BlacHole.Core.Infrastructure;
using BlacHole.Gameplay.Player;
using BlacHole.Gameplay.Tail;
using BlacHole.Gameplay.Territory;

namespace BlacHole.Gameplay.States
{
    // ── EventBus payloads ───────────────────────────────────────────────────
    public readonly struct TerritoryChangedEvent
    {
        public readonly int   OwnerId;
        public readonly int   CellsCaptured;
        public TerritoryChangedEvent(int id, int cells) { OwnerId = id; CellsCaptured = cells; }
    }

    /// <summary>
    /// Pure C# state machine that transitions between Safe / Capturing / Dead / Stunned.
    /// </summary>
    public class PlayerStateMachine
    {
        private readonly PlayerEntity     _entity;
        private readonly ITailService     _tail;
        private readonly ITerritoryService _territory;
        private          EventBus          _eventBus;

        public PlayerStateMachine(PlayerEntity entity, ITailService tail, ITerritoryService territory)
        {
            _entity    = entity;
            _tail      = tail;
            _territory = territory;

            // Wire tail events
            if (tail is TailService ts)
            {
                ts.OnTailTimeout       += HandleTailTimeout;
                ts.OnSelfIntersection  += HandleSelfIntersection;
            }
        }

        public void SetEventBus(EventBus bus) => _eventBus = bus;

        public void Tick(float dt)
        {
            switch (_entity.State)
            {
                case PlayerState.Safe:       TickSafe();       break;
                case PlayerState.Capturing:  TickCapturing();  break;
                case PlayerState.Stunned:    TickStunned(dt);  break;
                case PlayerState.Dead:                         break;
            }
        }

        // ── State ticks ──────────────────────────────────────────────────────

        private void TickSafe()
        {
            if (_territory == null) return;
            if (!_territory.IsOnOwnTerritory(_entity.Id, _entity.Position))
            {
                // Left own territory — start capturing
                _entity.State = PlayerState.Capturing;
                _tail.StartTail(_entity.Position);
            }
        }

        private void TickCapturing()
        {
            if (_territory == null) return;
            if (_territory.IsOnOwnTerritory(_entity.Id, _entity.Position))
            {
                // Returned home — capture enclosed area
                var pts  = new List<Vector2>(_tail.TailPoints);
                int captured = _territory.FillEnclosedArea(_entity.Id, pts);
                _tail.StopTail();
                _entity.State = PlayerState.Safe;

                _eventBus?.Fire(new TerritoryChangedEvent(_entity.Id, captured));
            }
        }

        private void TickStunned(float dt)
        {
            // Stun is handled by ControlLockTimer in PlayerController
            if (_entity.ControlLockTimer <= 0f)
                _entity.State = _territory != null && _territory.IsOnOwnTerritory(_entity.Id, _entity.Position)
                    ? PlayerState.Safe
                    : PlayerState.Capturing;
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void HandleTailTimeout()
        {
            _entity.State            = PlayerState.Stunned;
            _entity.ControlLockTimer = 1f;
            _tail.Clear();
        }

        private void HandleSelfIntersection()
        {
            KillPlayer();
        }

        private void KillPlayer()
        {
            _entity.IsAlive = false;
            _entity.State   = PlayerState.Dead;
            _tail.Clear();
        }
    }
}
