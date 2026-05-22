namespace EngineRoom.Demo.Singletons.Tests
{
    public class MockDataStoreManager : IDataStoreManager
    {
        public int Score { get; private set; }

        public static MockDataStoreManager Install()
        {
            var mock = new MockDataStoreManager();
            IDataStoreManager.Instance = mock;
            return mock;
        }

        public int GetScore()
        {
            return Score;
        }

        public void SetScore(int value)
        {
            Score = value;
        }
    }
}