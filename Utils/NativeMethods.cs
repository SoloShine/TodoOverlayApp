
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TodoOverlayApp.Utils
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // ShowWindow �����
        public const int SW_RESTORE = 9;  // �ָ���С������
        public const int SW_SHOW = 5;     // ��ʾ����
        public const int SW_MINIMIZE = 6; // ��С������
        public const int SW_MAXIMIZE = 3; // ��󻯴���

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_SHOWWINDOW = 0x0040;

        
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        // ����������ڼ���ڣ���ʹ����ǰ���ǽ��㴰��
        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        // �����������ʹһ���ɼ����޷�����Ĵ�����˸�������û�
        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        // ����������ڻ�ȡ������ʽ
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        // ��������������ô�����ʽ
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        // ������ʽ����
        public const int GWL_STYLE = -16;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_MINIMIZE = 0x20000000;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);


        // GetWindow ����
        public const uint GW_OWNER = 4;
        public const uint GW_HWNDNEXT = 2;
        public const uint GW_HWNDPREV = 3;

        // SetWindowLong ����
        public const int GWL_HWNDPARENT = -8;
        public const int GWL_EXSTYLE = -20;

        // SetWindowPos ��־
        public const uint SWP_NOACTIVATE = 0x0010;


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
