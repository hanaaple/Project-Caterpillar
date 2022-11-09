namespace Utility.SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        private int _hp;
        public int hp => _hp;

        public void SetHp(int hp)
        {
            _hp = hp;
        }
    }
}
