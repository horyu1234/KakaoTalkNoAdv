using System;
using System.Collections.Generic;
using System.Linq;
using Timer = System.Timers.Timer;

namespace KakaoTalkNoAdv
{
    public class AdRemover
    {
        // Timer interval in milliseconds
        private const double INTERVAL = 100D;
        private readonly Timer _timer;
        private bool _prevState;

        public AdRemover()
        {
            _timer = new Timer(INTERVAL)
            {
                AutoReset = true
            };
            _timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            _timer.Enabled = true;
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var id = GetKakaoTalkWindowId();
            if (id != 0 && !_prevState)
            {
                _prevState = RemoveAds(id);
            }
            else if (id == 0)
            {
                _prevState = false;
            }
        }

        private static readonly IReadOnlyList<string> KakaotalkWindowTitles = new List<string>
        {
            "카카오톡", "KakaoTalk", "カカオク"
        };
        private static int GetKakaoTalkWindowId()
        {
            return KakaotalkWindowTitles
                .Select(windowTitle => WinApi.FindWindow(null, windowTitle))
                .FirstOrDefault(windowId => windowId != 0);
        }

        private static bool RemoveAds(int kakaoTalkWindowId)
        {
            var friendList = WinApi.FindWindowEx(kakaoTalkWindowId, 0, "EVA_ChildWindow", null);
            var evaWindow = WinApi.FindWindowEx(kakaoTalkWindowId, 0, "EVA_Window", null);
            var visible = WinApi.IsWindowVisible(new IntPtr(friendList));
            if (!visible) return false;

            WinApi.GetWindowRect(new IntPtr(kakaoTalkWindowId), out var kakaoTalkWindowRect);

            WinApi.ShowWindow(evaWindow, 0);

            WinApi.SetWindowPos(new IntPtr(evaWindow), WinApi.HWND_BOTTOM, 0, 0, 0, 0, WinApi.SWP_NOMOVE);
            WinApi.SetWindowPos(new IntPtr(friendList), WinApi.HWND_BOTTOM, 0, 0,
                kakaoTalkWindowRect.Right - kakaoTalkWindowRect.Left,
                kakaoTalkWindowRect.Bottom - kakaoTalkWindowRect.Top - 36, WinApi.SWP_NOMOVE);
            return true;
        }
    }
}
