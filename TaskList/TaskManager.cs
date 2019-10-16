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
    public static class TaskManager
    {
        //*******// List windows

        public static IDictionary<IntPtr, string> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate (IntPtr IntPtr, int lParam)
            {
                if (IntPtr == shellWindow) return true;
                if (!IsWindowVisible(IntPtr)) return true;

                int length = GetWindowTextLength(IntPtr);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(IntPtr, builder, length + 1);

                windows[IntPtr] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(IntPtr IntPtr, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr IntPtr, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr IntPtr);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr IntPtr);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        public static string[] GetDesktopWindowsTitles()
        {
            List<string> lstTitles = new List<string>();

            foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value;

                lstTitles.Add(handle + " " + title);
            }

            return lstTitles.ToArray();
        }

        //*******// Show Hide Application

        static IntPtr test;

        public static void hideApp(string name)
        {
            IntPtr IntPtr;
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                if (pr.ProcessName == name)
                {
                    IntPtr = pr.MainWindowHandle;
                    test = IntPtr;
                    ShowWindow(IntPtr, SW_HIDE);
                }
            }
        }

        public static void showApp(string name)
        {
            IntPtr IntPtr;
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                if (pr.ProcessName == name)
                {
                    IntPtr = pr.MainWindowHandle;
                    ShowWindow(IntPtr, SW_SHOW);
                }
            }
        }

        public static void hideApp(IntPtr IntPtr)
        {
            ShowWindow(IntPtr, SW_HIDE);
        }

        public static void showApp(IntPtr IntPtr)
        {
            ShowWindow(IntPtr, SW_SHOW);
        }

        //*******// Activate window

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static IntPtr GetProcessesByName(string name)
        {
            var prc = Process.GetProcessesByName(name);
            if (prc.Length > 0)
            {
                return prc[0].MainWindowHandle;
            }

            return IntPtr.Zero;
        }

        public static void setForegroundWindow(IntPtr hWnd)
        {
            if (TaskManager.isMinimalized(hWnd))
            {
                ShowWindow(hWnd, SW_SHOWNORMAL);
            }

            SetForegroundWindow(hWnd);
        }

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr IntPtr, int nCmdShow);

        //*******// All processies

        public static List<Process> getProcessesNames()
        {
            Process[] processRunning = Process.GetProcesses();

            List<Process> lstTitles = new List<Process>();

            foreach (Process pr in processRunning)
            {
                lstTitles.Add(pr);
            }

            return lstTitles;
        }

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
                return null;
            }
        }


        //*******// Check if is still opened

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public static bool isLive(IntPtr hWnd)
        {
            return IsWindow(hWnd);
        }

        //*******// Check if window is minimalized

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsIconic(IntPtr hWnd);

        public static bool isMinimalized(IntPtr hWnd)
        {
            return IsIconic(hWnd);
        }

        //*******// Minimalize windows

        public static void showDesktop()
        {
            Type typeShell = Type.GetTypeFromProgID("Shell.Application");
            object objShell = Activator.CreateInstance(typeShell);
            typeShell.InvokeMember("MinimizeAll", System.Reflection.BindingFlags.InvokeMethod, null, objShell, null);
        }

        //*******// Serialization

        public static T Deserialize<T>(this string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader textReader = new StringReader(toDeserialize);
            return (T)xmlSerializer.Deserialize(textReader);
        }

        public static string Serialize<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringWriter textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static void Log(string text)
        {
            File.AppendAllText(@"log.txt", text);
        }



    }
}
