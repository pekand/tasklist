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

        public static int IsOnScreen(TaskListForm form)
        {
            Log.write("FormManager IsOnScreen");
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
    }
}
