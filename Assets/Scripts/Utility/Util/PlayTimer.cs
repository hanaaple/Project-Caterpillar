using System;
using UnityEngine;

namespace Utility.Util
{
    public static class PlayTimer
    {
        private static DateTime _startDateTime;
        
        private static TimeSpan _playTime;

        public static void ReStart()
        {
            _playTime = TimeSpan.Zero;
            _startDateTime = DateTime.Now;
        }
        
        public static void SetTime(TimeSpan timeSpan)
        {
            _startDateTime = DateTime.Now;
            _playTime = timeSpan;
        }

        public static TimeSpan GetPlayTime()
        {
            var playTime = DateTime.Now - _startDateTime + _playTime;
            Debug.Log($"시작: {_startDateTime}, 현재: {DateTime.Now}, 이전 플레이 시간 : {_playTime}, 총 : {playTime}");
            return playTime;
        }
    }
}