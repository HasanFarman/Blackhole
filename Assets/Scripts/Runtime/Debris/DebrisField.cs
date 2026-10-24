using System.Collections.Generic;
using UnityEngine;

namespace Blackhole
{
    public class DebrisField : MonoBehaviour
    {
        [SerializeField] private GameObject[] debrisPrefabs;
        [SerializeField] private BlackHoleOrbit orbit;
        [SerializeField] private Transform player;
        [SerializeField] private int maxActiveDebris = 48;

        private readonly List<Debris> active = new();

        public static DebrisField Instance { get; private set; }
        public int ActiveCount => active.Count;

        private void Awake()
        {
            Instance = this;

            if (orbit == null)
            {
                orbit = FindFirstObjectByType<BlackHoleOrbit>();
            }

            if (player == null && orbit != null)
            {
                player = orbit.transform;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Configure(GameObject[] prefabs, BlackHoleOrbit owningOrbit, Transform owner, int maxActive)
        {
            debrisPrefabs = prefabs;
            orbit = owningOrbit;
            player = owner;
            maxActiveDebris = Mathf.Max(1, maxActive);
        }

        public Debris SpawnLoose(Vector3 position, Vector3 velocity)
        {
            if (debrisPrefabs == null || debrisPrefabs.Length == 0)
            {
                return null;
            }

            var prefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];
            if (prefab == null)
            {
                return null;
            }

            var instance = Instantiate(prefab, position, Random.rotation);
            if (!instance.activeSelf)
            {
                instance.SetActive(true);
            }

            var debris = instance.GetComponent<Debris>();
            if (debris == null)
            {
                Destroy(instance);
                return null;
            }

            debris.Configure(orbit, player, this);
            velocity.y = 0f;
            if (debris.Body != null)
            {
                debris.Body.linearVelocity = velocity;
            }

            active.Add(debris);
            TrimToCap();
            return debris;
        }

        public void SpawnBurst(Vector3 position, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var direction = Random.insideUnitCircle.normalized;
                var impulse = new Vector3(direction.x, 0f, direction.y) * Random.Range(2.5f, 5.5f);
                SpawnLoose(position, impulse);
            }
        }

        public void SpawnBonusNearPlayer(int count)
        {
            var center = player != null ? player.position : transform.position;
            for (var i = 0; i < count; i++)
            {
                var angle = Random.Range(0f, 360f);
                var radius = Random.Range(5f, 7f);
                var position = OrbitMath.OrbitPosition(center, radius, angle);
                SpawnLoose(position, Vector3.zero);
            }
        }

        public void SpawnInitialOrbit(int count)
        {
            if (orbit == null)
            {
                return;
            }

            var center = player != null ? player.position : transform.position;
            for (var i = 0; i < count; i++)
            {
                var angle = (360f / Mathf.Max(1, count)) * i + Random.Range(-12f, 12f);
                var radius = Random.Range(orbit.MinOrbitRadius, orbit.MaxOrbitRadius);
                var debris = SpawnLoose(OrbitMath.OrbitPosition(center, radius, angle), Vector3.zero);
                if (debris != null)
                {
                    orbit.Capture(debris);
                }
            }
        }

        internal void ReportDestroyed(Debris debris)
        {
            if (debris == null)
            {
                return;
            }

            active.Remove(debris);
            orbit?.Remove(debris);
        }

        private void TrimToCap()
        {
            while (active.Count > maxActiveDebris)
            {
                var oldest = active[0];
                active.RemoveAt(0);

                if (oldest == null)
                {
                    continue;
                }

                orbit?.Remove(oldest);
                Destroy(oldest.gameObject);
            }
        }
    }
}
