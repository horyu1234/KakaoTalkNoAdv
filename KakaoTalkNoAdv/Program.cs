using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
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
            Console.Title = "카카오톡 PC 버전 광고 제거 v1.0.0 [By horyu1234]";

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            Console.WriteLine("프로그램 개발자: horyu1234");
            Console.WriteLine();
            Console.WriteLine("본 프로그램의 소스는 https://github.com/horyu1234/KakaoTalkNoAdv 에서 공개하고 있습니다.");
            Console.WriteLine();
            Console.WriteLine("본 프로그램은 real21c 님의 kakao-noadv 프로젝트의 구조를 참고하여 개발되었습니다.");
            Console.WriteLine();
            Console.WriteLine("==============================");
            Console.WriteLine();

            try
            {
                CheckKakaoTalkIsRunningOrStart();

                Console.WriteLine("카카오톡 PC 버전의 창을 찾는 중입니다.");
                var kakaoTalkWindowId = GetKakaoTalkWindowId();

                if (kakaoTalkWindowId == 0)
                {
                    Console.WriteLine("카카오톡 PC 버전의 창을 찾을 수 없습니다.");
                    Shutdown();
                }

                RemoveAds(kakaoTalkWindowId);
            }
            catch (Exception e)
            {
                Console.WriteLine("작업을 진행하는 중 오류가 발생하였습니다.");
                Console.WriteLine(e.Message);
            }

            Shutdown();
        }

        private static void CheckKakaoTalkIsRunningOrStart()
        {
            Console.WriteLine("카카오톡 PC 버전이 실행 중인지 확인하는 중입니다.");

            var kakaoTalkProcess = Process.GetProcessesByName("kakaotalk");
            if (kakaoTalkProcess.Length == 0)
            {
                Console.WriteLine("카카오톡 PC 버전이 실행 중이 아닙니다.");

                Console.WriteLine("카카오톡 PC 버전을 실행합니다.");
                var classesRoot = Registry.ClassesRoot;
                var subKey = classesRoot.OpenSubKey("kakaoopen\\shell\\open\\command");

                var kakaoTalkStartCommand = subKey.GetValue(null).ToString();
                var kakaoTalkExePath = Regex.Split(Regex.Split(kakaoTalkStartCommand, "\" \"%1\"")[0], "\"")[1];

                Process.Start(kakaoTalkExePath);

                WaitWhileKakaoTalkRunning();
            }
            else
            {
                Console.WriteLine("카카오톡 PC 버전이 실행 중입니다.");
            }
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
            Console.WriteLine("광고를 제거하는 중입니다.");

            var friendList = FindWindowEx(kakaoTalkWindowId, 0, "EVA_ChildWindow", null);
            var evaWindow = FindWindowEx(kakaoTalkWindowId, 0, "EVA_Window", null);

            GetWindowRect(new IntPtr(kakaoTalkWindowId), out var kakaoTalkWindowRect);

            ShowWindow(evaWindow, 0);

            SetWindowPos(new IntPtr(evaWindow), HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE);
            SetWindowPos(new IntPtr(friendList), HWND_BOTTOM, 0, 0,
                kakaoTalkWindowRect.Right - kakaoTalkWindowRect.Left,
                kakaoTalkWindowRect.Bottom - kakaoTalkWindowRect.Top - 36, SWP_NOMOVE);

            Console.WriteLine("광고가 제거되었습니다.");
        }

        private static void Shutdown()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            for (var remainSeconds = 5; remainSeconds > 0; remainSeconds--)
            {
                Console.WriteLine(remainSeconds + " 초 후 프로그램이 종료됩니다.");
                Thread.Sleep(1000);
            }

            Process.GetCurrentProcess().Kill();
        }

        private static void WaitWhileKakaoTalkRunning()
        {
            while (Process.GetProcessesByName("kakaotalk").Length == 0)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }

            for (var remainSeconds = 10; remainSeconds > 0; remainSeconds--)
            {
                Console.WriteLine(remainSeconds + " 초 후 작업을 시작합니다.");
                Thread.Sleep(1000);
            }
        }
    }
}