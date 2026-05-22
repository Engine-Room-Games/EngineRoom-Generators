namespace EngineRoom.Demo.Singletons.Tests
{
    public class MockSoundManager : ISoundManager
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
}