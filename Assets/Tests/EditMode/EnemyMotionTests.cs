using NUnit.Framework;
using UnityEngine;

namespace Blackhole.Tests
{
    public class EnemyMotionTests
    {
        [Test]
        public void SeekDirection_NoWobble_PointsAtTarget()
        {
            var direction = EnemyMotion.SeekDirection(
                Vector3.zero,
                new Vector3(5f, 0f, 0f),
                0f,
                0f,
                0f);

            Assert.AreEqual(1f, direction.x, 0.001f);
            Assert.AreEqual(0f, direction.z, 0.001f);
        }

        [Test]
        public void SeekDirection_WithWobble_StaysUnitLength()
        {
            var direction = EnemyMotion.SeekDirection(
                Vector3.zero,
                new Vector3(5f, 0f, 0f),
                0.5f,
                0f,
                0.5f);

            Assert.AreEqual(1f, direction.magnitude, 0.001f);
            Assert.That(Mathf.Abs(direction.z), Is.GreaterThan(0.001f));
        }

        [Test]
        public void SeekDirection_AtTarget_ReturnsZero()
        {
            var direction = EnemyMotion.SeekDirection(
                Vector3.zero,
                Vector3.zero,
                1f,
                0f,
                0.5f);

            Assert.AreEqual(Vector3.zero, direction);
        }
    }
}
