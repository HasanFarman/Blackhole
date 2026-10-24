using NUnit.Framework;
using UnityEngine;

namespace Blackhole.Tests
{
    public class MenuMotionTests
    {
        [Test]
        public void Lean_ClampsToMaxDistance()
        {
            var anchor = new Vector3(2f, 1f, 0f);
            var pointer = new Vector3(40f, 1f, -30f);

            var lean = MenuMotion.Lean(anchor, pointer, 1f, 1.15f);

            Assert.LessOrEqual(lean.magnitude, 1.1501f);
            Assert.Greater(lean.magnitude, 0f);
        }

        [Test]
        public void Lean_IgnoresHeightDifference()
        {
            var anchor = new Vector3(0f, 2f, 0f);
            var highPointer = new Vector3(1f, 40f, 0f);
            var flatPointer = new Vector3(1f, 2f, 0f);

            var high = MenuMotion.Lean(anchor, highPointer, 0.5f, 2f);
            var flat = MenuMotion.Lean(anchor, flatPointer, 0.5f, 2f);

            Assert.AreEqual(flat.x, high.x, 0.0001f);
            Assert.AreEqual(flat.z, high.z, 0.0001f);
            Assert.AreEqual(0f, flat.y, 0.0001f);
        }

        [Test]
        public void Wander_StaysInsideRadius()
        {
            for (var i = 0; i < 120; i++)
            {
                var wander = MenuMotion.Wander(i * 0.25f, 0.6f);
                Assert.LessOrEqual(wander.magnitude, 0.6001f);
            }
        }
    }
}
