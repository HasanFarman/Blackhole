using NUnit.Framework;
using UnityEngine;

namespace Blackhole.Tests
{
    public class PlayerMotionTests
    {
        [Test]
        public void CalculateVelocity_DiagonalInput_IsNotFasterThanMaxSpeed()
        {
            var velocity = PlayerMotion.CalculateVelocity(
                Vector3.zero,
                new Vector2(1f, 1f),
                7f,
                100f,
                1f);

            Assert.AreEqual(7f, velocity.magnitude, 0.01f);
            Assert.AreEqual(0f, velocity.y, 0.0001f);
        }

        [Test]
        public void CalculateVelocity_NoInput_DeceleratesToZero()
        {
            var velocity = PlayerMotion.CalculateVelocity(
                new Vector3(7f, 0f, 0f),
                Vector2.zero,
                7f,
                100f,
                1f);

            Assert.AreEqual(0f, velocity.magnitude, 0.01f);
        }

        [Test]
        public void CalculateVelocity_ShortStep_MovesTowardTarget()
        {
            var velocity = PlayerMotion.CalculateVelocity(
                Vector3.zero,
                new Vector2(1f, 0f),
                7f,
                10f,
                0.1f);

            Assert.AreEqual(1f, velocity.x, 0.001f);
        }
    }
}
