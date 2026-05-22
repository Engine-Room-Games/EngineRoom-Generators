namespace EngineRoom.Demo.Singletons.Tests
{
    public class MockUIManager : IUiManager
    {
        public int LastCount { get; private set; } = -1;

        public static MockUIManager Install()
        {
            var mock = new MockUIManager();
            IUiManager.Instance = mock;
            return mock;
        }

        public void SetCount(int count)
        {
            LastCount = count;
        }
    }
}