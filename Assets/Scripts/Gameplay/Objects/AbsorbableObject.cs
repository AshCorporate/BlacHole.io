using UnityEngine;
using BlacHole.Gameplay.Gravity;
using BlacHole.Gameplay.Spawning;

namespace BlacHole.Gameplay.Objects
{
    /// <summary>
    /// MonoBehaviour on every map object.
    /// Implements IGravityAffectable so GravityService can pull it toward the player.
    /// </summary>
    public class AbsorbableObject : MonoBehaviour, IGravityAffectable
    {
        [Header("Data")]
        public string  Category             = "Micro";
        public float   Weight               = 1f;
        public float   XPReward             = 0.1f;
        public float   RequiredConsumeRadius_Backing = 0.2f;

        [HideInInspector] public bool IsBeingPulled_Backing;

        private ObjectSpawner _spawner;

        // ── IGravityAffectable ───────────────────────────────────────────────
        public Vector2  Position
        {
            get => transform.position;
            set => transform.position = new Vector3(value.x, value.y, 0f);
        }
        public float Mass                  => Weight;
        public float RequiredConsumeRadius => RequiredConsumeRadius_Backing;
        public bool  IsBeingPulled
        {
            get => IsBeingPulled_Backing;
            set => IsBeingPulled_Backing = value;
        }
        public bool  IsAvailable           => gameObject.activeSelf;

        // ── Lifecycle ────────────────────────────────────────────────────────

        public void SetSpawner(ObjectSpawner spawner) => _spawner = spawner;

        public void Despawn()
        {
            IsBeingPulled_Backing = false;
            if (_spawner != null)
                _spawner.Return(Category, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
