using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KakaoTalkNoAdv
{
    internal class Program
    {
        [DllImport("user32.dll")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int FindWindowEx(int hWnd1, int hWnd2, string lpsz1, string lpsz2);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left; // x position of upper-left corner
            public int Top; // y position of upper-left corner
            public int Right; // x position of lower-right corner
            public int Bottom; // y position of lower-right corner
        }

        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const int SWP_NOMOVE = 0x2;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
            int uFlags);

        private static void Main(string[] args)
        {
            try
            {
                CheckKakaoTalkIsRunningOrStart();

                var kakaoTalkWindowId = GetKakaoTalkWindowId();

                if (kakaoTalkWindowId == 0)
                {
                    ShowBalloonTip("카카오톡 PC 버전의 창을 찾을 수 없습니다.", 3000);
                    QuitProgram(5000);
                }

                RemoveAds(kakaoTalkWindowId);

                ShowBalloonTip("광고가 제거되었습니다.", 2000);
            }
            catch (Exception e)
            {
                ShowBalloonTip("작업을 진행하는 중 오류가 발생하였습니다.\n" +
                               e.Message, 3000);
            }

            QuitProgram(5000);

            Console.ReadLine();
        }

        private static void CheckKakaoTalkIsRunningOrStart()
        {
            var kakaoTalkProcess = Process.GetProcessesByName("kakaotalk");
            if (kakaoTalkProcess.Length != 0)
            {
                return;
            }

            ShowBalloonTip("카카오톡 PC 버전이 실행 중이 아닙니다.\n" +
                           "카카오톡 PC 버전을 실행합니다.", 5000);

            var classesRoot = Registry.ClassesRoot;
            var subKey = classesRoot.OpenSubKey("kakaoopen\\shell\\open\\command");

            var kakaoTalkStartCommand = subKey.GetValue(null).ToString();
            var kakaoTalkExePath = Regex.Split(Regex.Split(kakaoTalkStartCommand, "\" \"%1\"")[0], "\"")[1];

            Process.Start(kakaoTalkExePath);

            WaitWhileKakaoTalkRunning();
        }

        private static int GetKakaoTalkWindowId()
        {
            var kakaoTalkWindowTitles = new List<string>()
            {
                "카카오톡", "KakaoTalk", "カカオク"
            };

            foreach (var kakaoTalkWindowTitle in kakaoTalkWindowTitles)
            {
                var kakaoTalkWindow = FindWindow(null, kakaoTalkWindowTitle);
                if (kakaoTalkWindow != 0)
                {
                    return kakaoTalkWindow;
                }
            }

            return 0;
        }

        private static void RemoveAds(int kakaoTalkWindowId)
        {
            var friendList = FindWindowEx(kakaoTalkWindowId, 0, "EVA_ChildWindow", null);
            var evaWindow = FindWindowEx(kakaoTalkWindowId, 0, "EVA_Window", null);

            GetWindowRect(new IntPtr(kakaoTalkWindowId), out var kakaoTalkWindowRect);

            ShowWindow(evaWindow, 0);

            SetWindowPos(new IntPtr(evaWindow), HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE);
            SetWindowPos(new IntPtr(friendList), HWND_BOTTOM, 0, 0,
                kakaoTalkWindowRect.Right - kakaoTalkWindowRect.Left,
                kakaoTalkWindowRect.Bottom - kakaoTalkWindowRect.Top - 36, SWP_NOMOVE);
        }

        private static void WaitWhileKakaoTalkRunning()
        {
            int waitSeconds = 0;
            while (Process.GetProcessesByName("kakaotalk").Length == 0)
            {
                waitSeconds++;

                if (waitSeconds > 60)
                {
                    ShowBalloonTip("카카오톡이 실행 여부를 확인하지 못하였습니다.", 3000);
                    QuitProgram(5000);
                    break;
                }

                Thread.Sleep(1000);
            }

            Thread.Sleep(10000);
        }

        private static void ShowBalloonTip(string message, int timeout)
        {
            new Thread((() =>
            {
                var notification = new NotifyIcon
                {
                    Visible = true,
                    Icon = System.Drawing.SystemIcons.Information,
                    BalloonTipTitle = "KakaoTalkNoAdv v1.1.0 (개발자: horyu1234)",
                    BalloonTipText = message
                };

                notification.ShowBalloonTip(timeout);

                Thread.Sleep(timeout);

                notification.Dispose();
            })).Start();
        }

        private static void QuitProgram(int timeout)
        {
            Thread.Sleep(timeout);

            Process.GetCurrentProcess().Kill();
        }
    }
}