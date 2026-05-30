using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EngineRoom.Demo.Singletons.Tests
{
    public class GameManagerTests
    {
        [UnityTest]
        public IEnumerator RegisterTap_IncrementsCount_AndNotifiesSoundAndUi()
        {
            var store = MockDataStoreManager.Install();
            var sound = MockSoundManager.Install();
            var ui = MockUIManager.Install();

            var game = GameManager.Create();

            yield return null;

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

        [UnityTest]
        public IEnumerator RegisterTap_RaisesCountChangedEventOnInterface()
        {
            MockDataStoreManager.Install();
            MockSoundManager.Install();
            MockUIManager.Install();

            var game = GameManager.Create();
            
            yield return null;

            try
            {
                var observed = -1;
                game.CountChanged += value => observed = value;

                game.RegisterTap();
                game.RegisterTap();

                Assert.AreEqual(2, observed);
            }
            finally
            {
                Object.DestroyImmediate(((GameManager)game).gameObject);
            }
        }
    }
}
