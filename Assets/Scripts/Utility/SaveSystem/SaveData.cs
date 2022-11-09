namespace Utility.SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        private string _scenario;
        
        public string scenario => _scenario;
        
        private int _playTime;
        
        public int playTime => playTime;
        
        private int _hp;
        public int hp => _hp;

        public void SetScenario(string scenario)
        {
            _scenario = scenario;
        }
        public void SetPlayTime(int playTime)
        {
            _playTime = playTime;
        }
        public void SetHp(int hp)
        {
            _hp = hp;
        }
    }
}
