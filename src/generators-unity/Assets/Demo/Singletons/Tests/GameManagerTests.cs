using NUnit.Framework;
using UnityEngine;

namespace EngineRoom.Demo.Singletons.Tests
{
    public class GameManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void RegisterTap_IncrementsCount_AndNotifiesSoundAndUi()
        {
            var sound = MockSoundManager.Install();
            var ui = MockUIManager.Install();
            
            var game = GameManager.Create();

            try
            {
                game.RegisterTap();

                Assert.AreEqual(1, game.Count);
                Assert.AreEqual(1, sound.TapPlayCount);
                Assert.AreEqual(1, ui.LastCount);
            }
            finally
            {
                Object.DestroyImmediate(((GameManager)game).gameObject);
            }
        }

        private class MockSoundManager : ISoundManager
        {
            public int TapPlayCount { get; private set; }
            public int StateChangePlayCount { get; private set; }

            public static MockSoundManager Install()
            {
                var mock = new MockSoundManager();
                ISoundManager.Instance = mock;
                return mock;
            }

            public void PlayTap()
            {
                TapPlayCount++;
            }

            public void PlayStateChange()
            {
                StateChangePlayCount++;
            }
        }

        private class MockUIManager : IUIManager
        {
            public int LastCount { get; private set; } = -1;

            public static MockUIManager Install()
            {
                var mock = new MockUIManager();
                IUIManager.Instance = mock;
                return mock;
            }

            public void SetCount(int count)
            {
                LastCount = count;
            }
        }
    }
}
