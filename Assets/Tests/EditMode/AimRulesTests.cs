using System;
using NUnit.Framework;
using UnityEngine;

namespace Blackhole.Tests
{
    public class AimRulesTests
    {
        [Test]
        public void BestAimIndex_ReturnsMostAlignedPosition()
        {
            var positions = new[]
            {
                new Vector3(5f, 0f, 0f),
                new Vector3(0f, 0f, 5f),
                new Vector3(-5f, 0f, 0f)
            };

            Assert.AreEqual(1, AimRules.BestAimIndex(Vector3.zero, positions, new Vector3(0f, 0f, 1f)));
        }

        [Test]
        public void BestAimIndex_EmptyList_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, AimRules.BestAimIndex(Vector3.zero, Array.Empty<Vector3>(), Vector3.right));
        }

        [Test]
        public void BestAimIndex_ZeroDirection_ReturnsMinusOne()
        {
            var positions = new[] { new Vector3(5f, 0f, 0f) };
            Assert.AreEqual(-1, AimRules.BestAimIndex(Vector3.zero, positions, Vector3.zero));
        }

        [Test]
        public void BestAimIndex_IgnoresVerticalOffset()
        {
            var positions = new[] { new Vector3(0f, 50f, 5f) };
            Assert.AreEqual(0, AimRules.BestAimIndex(new Vector3(0f, 3f, 0f), positions, Vector3.forward));
        }

        [Test]
        public void TryGroundPoint_UpwardRay_ReturnsFalse()
        {
            Assert.IsFalse(AimRules.TryGroundPoint(new Ray(Vector3.zero, Vector3.up), 0f, out _));
        }

        [Test]
        public void TryGroundPoint_DownwardRay_ReturnsPlaneHit()
        {
            Assert.IsTrue(AimRules.TryGroundPoint(new Ray(new Vector3(0f, 5f, 0f), Vector3.down), 0f, out var hit));
            Assert.AreEqual(0f, hit.y, 0.0001f);
        }
    }
}
