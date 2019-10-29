using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TaskList
{

    public class WindowData
    {
        public IntPtr handle = IntPtr.Zero;
        public string title = null;
        public Image image = null;
        public string imageBase = null;
        public Process process = null;
        public string path = null;

        public WindowData(IntPtr handle, string title = null, Image image = null, Process process = null) {
            this.handle = handle;
            this.title = title;
            this.image = image;
            this.process = process;
        }
    }

    public static class TaskManager
    {
        

        /********************************************************************/
        /* List windows */

        private delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsDelegate lpfn, IntPtr lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr IntPtr, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr IntPtr);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr IntPtr);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        public static bool IsWindowPopup(IntPtr hHandle)
        {
            const long WS_POPUP = 0x80000000L;
            long style = (long)GetWindowLongPtr(hHandle, -16);
            bool isPopup = ((style & WS_POPUP) != 0);
            return isPopup;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST

        }

        public static bool isCloacked(IntPtr handle) {
            DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.DWMWA_CLOAKED, out bool isCloacked, Marshal.SizeOf(typeof(bool)));

            return isCloacked;
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        public static List<WindowData> GetOpenWindows()
        {
            Log.write("TaskManager GetOpenWindows");
            IntPtr shellWindow = GetShellWindow();
            List<WindowData> windows = new List<WindowData>();

            IntPtr hDesktop = IntPtr.Zero;
            EnumDesktopWindows(hDesktop, delegate(IntPtr IntPtr, int lParam)
            {

                /*int cls_max_length = 1000;
                StringBuilder classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(IntPtr, classText, cls_max_length + 2);*/

                if (IntPtr == shellWindow)
                    return true;

                if (!IsWindowVisible(IntPtr))
                    return true;

                /*if (IsWindowPopup(IntPtr)) {
                    return true;
                }*/

                if (isCloacked(IntPtr)) // skip window store hidden apps
                    return true;

                int length = GetWindowTextLength(IntPtr);
                if (length == 0) return true;

                windows.Add(new WindowData(IntPtr));
                return true;

            }, IntPtr.Zero);

            return windows;
        }


        /********************************************************************/
        //*******// Activate window

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr IntPtr, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void setForegroundWindow(IntPtr hWnd)
        {
            Log.write("TaskManager setForegroundWindow");

            const int SW_SHOWNORMAL = 1;

            if (TaskManager.isMinimalized(hWnd))
            {
                ShowWindow(hWnd, SW_SHOWNORMAL);
            }

            SetForegroundWindow(hWnd);
        }

        /********************************************************************/
        //*******// Get Icon

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 64 bit version maybe loses significant 64-bit specific information
        /// </summary>
        static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr((long)GetClassLong32(hWnd, nIndex));
            else
                return GetClassLong64(hWnd, nIndex);
        }

        public static Bitmap GetSmallWindowIcon(IntPtr hWnd)
        {
            Log.write("TaskManager GetSmallWindowIcon");
            uint WM_GETICON = 0x007f;
            IntPtr ICON_BIG = new IntPtr(1);
            IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
            int GCL_HICON = -14;

            try
            {
                IntPtr hIcon = default(IntPtr);

                hIcon = SendMessage(hWnd, WM_GETICON, ICON_BIG, IntPtr.Zero);

                if (hIcon == IntPtr.Zero)
                    hIcon = GetClassLongPtr(hWnd, GCL_HICON);

                if (hIcon == IntPtr.Zero)
                    hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00/*IDI_APPLICATION*/);

                if (hIcon != IntPtr.Zero)
                {
                    using (Bitmap image = new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), 16, 16))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, ImageFormat.Bmp);
                            
                            return (Bitmap)image.Clone();
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.write("Exception: "+e.Message);
                return null;
            }
        }

        /********************************************************************/
        //*******// Check if is still opened

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public static bool isLive(IntPtr hWnd)
        {
            Log.write("TaskManager isLive");
            return IsWindow(hWnd);
        }

        /********************************************************************/
        /* Check if window is minimalized */

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        public static bool isMinimalized(IntPtr hWnd)
        {
            Log.write("TaskManager isMinimalized");
            return IsIconic(hWnd);
        }
        
        /********************************************************************/

        public static void ShowDesktop()
        {
            Log.write("TaskManager ShowDesktop");

            const int SW_MINIMIZE = 6;
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

            List<WindowData> windowsList = TaskManager.GetOpenWindows();

            foreach (WindowData window in windowsList)
            {
                if (handle != window.handle) {
                    ShowWindow(window.handle, SW_MINIMIZE);
                }
            }
        }

        /********************************************************************/

        public static void CloseWindow(IntPtr hWnd)
        {
            Log.write("TaskManager CloseWindow");

            uint WM_SYSCOMMAND = 0x0112;
            IntPtr SC_CLOSE = new IntPtr(0xF060);
            SendMessage(hWnd, WM_SYSCOMMAND, SC_CLOSE, IntPtr.Zero);
        }

        /********************************************************************/

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }

        public static List<IntPtr> GetAllChildHandles(IntPtr MainHandle)
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(MainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        public static void FindProcessByWithWindowHandle(List<WindowData> windowData)
        {
            foreach (Process process in Process.GetProcesses())
            {
                
                try
                {
                    if (process.MainWindowHandle == IntPtr.Zero || process.MainWindowTitle == "")
                    {
                        continue;
                    }

                    foreach (WindowData window in windowData)
                    {

                        List<IntPtr> childrenWindows = GetAllChildHandles(process.MainWindowHandle);

                        foreach (IntPtr handle in childrenWindows)
                        {
                            if (handle == window.handle)
                            {
                                window.process = process;
                                continue;
                            }
                        }

                        if (process.MainWindowHandle == window.handle)
                        {
                            window.process = process;
                            continue;
                        }


                    }
                }
                catch (System.InvalidOperationException e)
                {
                    Log.write(e.Message);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    Log.write(e.Message);
                }
            }
        }

        /********************************************************************/

        public static string getWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd);
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }

        /********************************************************************/

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static Process getProcessFromHandle(IntPtr handle) {
            uint lpdwProcessId;
            GetWindowThreadProcessId(handle, out lpdwProcessId);
            return Process.GetProcessById((int)lpdwProcessId);
        }
        /********************************************************************/

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, string lpWindowName);



        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        static IntPtr GetSystemTrayHandle()
        {
            IntPtr hWndTray = FindWindow("Shell_TrayWnd", null);
            if (hWndTray != IntPtr.Zero)
            {
                hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "TrayNotifyWnd", null);
                if (hWndTray != IntPtr.Zero)
                {
                    hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "SysPager", null);
                    if (hWndTray != IntPtr.Zero)
                    {
                        hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);
                        return hWndTray;
                    }
                }
            }

            return IntPtr.Zero;
        }

        public static List<WindowData> getSystemTryWindows() {

            List<WindowData> windows = new List<WindowData>();

            IntPtr systemTrayHandle = GetSystemTrayHandle();
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process process in processes)
            {
                if (process.MainWindowHandle == systemTrayHandle)
                {
                    WindowData data = new WindowData(IntPtr.Zero,process.ProcessName,null,process);
                    windows.Add(data);
                }
            }

            return windows;
        }
    }
}
