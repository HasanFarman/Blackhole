using UnityEngine;

namespace Blackhole.Editor
{
    public static partial class ProjectBootstrap
    {
        private static ParticleSystem CreateDustSystem(
            GameObject parent,
            Material material,
            float radius,
            float rate,
            Color tint,
            bool localSpace,
            float orbitalSpeed = 22f)
        {
            var dustObject = new GameObject("Dust");
            dustObject.transform.SetParent(parent.transform, false);

            var system = dustObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.duration = 4f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.14f);
            main.startColor = tint;
            main.simulationSpace = localSpace
                ? ParticleSystemSimulationSpace.Local
                : ParticleSystemSimulationSpace.World;
            main.maxParticles = 420;
            main.gravityModifier = 0f;

            var emission = system.emission;
            emission.enabled = true;
            emission.rateOverTime = rate;

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.radiusThickness = 0.7f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalY = new ParticleSystem.MinMaxCurve(orbitalSpeed * 0.75f, orbitalSpeed * 1.35f);
            velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.radial = new ParticleSystem.MinMaxCurve(-0.08f, -0.18f);

            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = FadeGradient(tint);

            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, ShrinkCurve());

            var renderer = dustObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingFudge = -8f;

            system.Play();
            return system;
        }

        private static ParticleSystem CreateTrailSystem(GameObject parent, Material material, Color tint)
        {
            var trailObject = new GameObject("Trail");
            trailObject.transform.SetParent(parent.transform, false);

            var system = trailObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.38f);
            main.startColor = tint;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 220;
            main.gravityModifier = 0f;

            var emission = system.emission;
            emission.enabled = false;

            var shape = system.shape;
            shape.enabled = false;

            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = FadeGradient(tint);

            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, ShrinkCurve());

            var renderer = trailObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingFudge = -6f;

            system.Play();
            return system;
        }

        private static GameObject BuildEnemyBurstPrefab(Material warmMaterial, Material coolMaterial)
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/FX");

            var root = new GameObject("EnemyBurst");

            CreateBurstSystem(root, warmMaterial, new Color(1f, 0.78f, 0.45f), 30, 0.35f, 0.95f, 2.6f, 6.8f, 0.06f, 0.16f);
            CreateBurstSystem(root, coolMaterial, new Color(0.55f, 0.85f, 1f), 14, 0.3f, 0.65f, 1.2f, 3.2f, 0.1f, 0.3f);

            var flashObject = new GameObject("Flash");
            flashObject.transform.SetParent(root.transform, false);
            var light = flashObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.78f, 0.48f);
            light.range = 7f;
            light.intensity = 4.5f;
            flashObject.AddComponent<PulseLight>().Configure(0.22f, 4.5f);

            root.AddComponent<AutoDestroy>().Configure(3f);

            return SavePrefab(root, "Assets/Resources/FX/EnemyBurst.prefab");
        }

        private static void CreateBurstSystem(
            GameObject parent,
            Material material,
            Color tint,
            short count,
            float lifetimeMin,
            float lifetimeMax,
            float speedMin,
            float speedMax,
            float sizeMin,
            float sizeMax)
        {
            var burstObject = new GameObject("Sparks");
            burstObject.transform.SetParent(parent.transform, false);

            var system = burstObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = tint;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;
            main.gravityModifier = 0.28f;

            var emission = system.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = FadeGradient(tint);

            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, ShrinkCurve());

            var rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

            var renderer = burstObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingFudge = 2f;
        }

        private static ParticleSystem.MinMaxGradient FadeGradient(Color tint)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(tint, 0f),
                    new GradientColorKey(Color.Lerp(tint, Color.white, 0.35f), 0.25f),
                    new GradientColorKey(tint, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.15f),
                    new GradientAlphaKey(0.8f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });

            return new ParticleSystem.MinMaxGradient(gradient);
        }

        private static AnimationCurve ShrinkCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.7f, 0.5f),
                new Keyframe(1f, 0.15f));
        }
    }
}
