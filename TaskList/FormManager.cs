using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskList
{
    class FormManager
    {

        public static int IsOnScreen(TaskList form)
        {
            Screen[] screens = Screen.AllScreens;
            int i = 0;
            foreach (Screen screen in screens)
            {
                Rectangle formRectangle = new Rectangle(form.Left, form.Top, 50, 50);

                if (screen.WorkingArea.Contains(formRectangle))
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public static void saveFormPosition(TaskList form)
        {
            if (form.WindowState == FormWindowState.Maximized)
            {
                Properties.Settings.Default.Location = form.RestoreBounds.Location;
                Properties.Settings.Default.Size = form.RestoreBounds.Size;
                Properties.Settings.Default.Maximised = true;
                Properties.Settings.Default.Minimised = false;
            }
            else if (form.WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = form.Location;
                Properties.Settings.Default.Size = form.Size;
                Properties.Settings.Default.Maximised = false;
                Properties.Settings.Default.Minimised = false;
            }
            else
            {
                Properties.Settings.Default.Location = form.RestoreBounds.Location;
                Properties.Settings.Default.Size = form.RestoreBounds.Size;
                Properties.Settings.Default.Maximised = false;
                Properties.Settings.Default.Minimised = true;
            }

            Properties.Settings.Default.mostTop = form.TopMost;

            Properties.Settings.Default.Save();
        }

        public static void restoreFormPosition(TaskList form)
        {
            if (Properties.Settings.Default.firstRun)
            {
                Properties.Settings.Default.firstRun = false;
            }
            else if (Properties.Settings.Default.Maximised)
            {
                form.WindowState = FormWindowState.Maximized;
                form.Location = Properties.Settings.Default.Location;
                form.Size = Properties.Settings.Default.Size;
            }
            else if (Properties.Settings.Default.Minimised)
            {
                form.WindowState = FormWindowState.Minimized;
                form.Location = Properties.Settings.Default.Location;
                form.Size = Properties.Settings.Default.Size;
            }
            else
            {
                form.Location = Properties.Settings.Default.Location;
                form.Size = Properties.Settings.Default.Size;
            }

            if (FormManager.IsOnScreen(form) == -1)
            {
                form.WindowState = FormWindowState.Normal;
                form.Left = 100;
                form.Top = 100;
                form.Width = 500;
                form.Height = 500;
            }

            form.setTopMost(Properties.Settings.Default.mostTop);
        }

        public static Process[] getProcessies() {
            Process[] tasks = System.Diagnostics.Process.GetProcesses();

            return tasks;
        } 
    }
}
