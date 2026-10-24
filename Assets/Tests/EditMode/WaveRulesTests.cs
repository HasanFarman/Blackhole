using NUnit.Framework;

namespace Blackhole.Tests
{
    public class WaveRulesTests
    {
        [Test]
        public void EnemyCount_WaveOne_ReturnsBaseCount()
        {
            Assert.AreEqual(5, WaveRules.EnemyCount(1, 5, 3));
        }

        [Test]
        public void EnemyCount_WaveFour_AddsThreePerWave()
        {
            Assert.AreEqual(14, WaveRules.EnemyCount(4, 5, 3));
        }

        [Test]
        public void EnemyCount_NonPositiveWave_ClampsToBaseCount()
        {
            Assert.AreEqual(5, WaveRules.EnemyCount(0, 5, 3));
        }
    }
}
