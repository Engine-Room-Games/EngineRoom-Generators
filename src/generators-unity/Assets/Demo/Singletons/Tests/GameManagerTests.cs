using NUnit.Framework;
using UnityEngine;

namespace EngineRoom.Demo.Singletons.Tests
{
    public class GameManagerTests
    {
        [Test]
        public void RegisterTap_IncrementsCount_AndNotifiesSoundAndUi()
        {
            var store = MockDataStoreManager.Install();
            var sound = MockSoundManager.Install();
            var ui = MockUIManager.Install();

            var game = GameManager.Create();

            try
            {
                game.RegisterTap();

                Assert.AreEqual(1, game.Count);
                Assert.AreEqual(1, sound.TapPlayCount);
                Assert.AreEqual(1, ui.LastCount);
                Assert.AreEqual(1, store.Score);
            }
            finally
            {
                Object.DestroyImmediate(((GameManager)game).gameObject);
            }
        }
    }
}
