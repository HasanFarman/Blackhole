using NUnit.Framework;
using UnityEngine;

namespace Blackhole.Tests
{
    public class OrbitMathTests
    {
        [Test]
        public void OrbitPosition_AtZeroDegrees_IsOnPositiveX()
        {
            var position = OrbitMath.OrbitPosition(Vector3.zero, 2f, 0f);

            Assert.AreEqual(2f, position.x, 0.0001f);
            Assert.AreEqual(0f, position.z, 0.0001f);
        }

        [Test]
        public void OrbitPosition_AtNinetyDegrees_IsOnPositiveZ()
        {
            var position = OrbitMath.OrbitPosition(Vector3.zero, 2f, 90f);

            Assert.AreEqual(0f, position.x, 0.0001f);
            Assert.AreEqual(2f, position.z, 0.0001f);
        }

        [Test]
        public void WrapDegrees_WrapsPositiveAndNegativeValues()
        {
            Assert.AreEqual(10f, OrbitMath.WrapDegrees(370f), 0.0001f);
            Assert.AreEqual(350f, OrbitMath.WrapDegrees(-10f), 0.0001f);
        }
    }
}
