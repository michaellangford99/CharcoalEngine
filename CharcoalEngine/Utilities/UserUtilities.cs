using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharcoalEngine.Utilities
{
    public class UserUtilities
    {
        public const string OBJ_FILTER = "OBJ files (*.obj)|*.obj|All files (*.*)|*.*";

        public static string OpenFile(string filetypefilter)
        {
            System.Windows.Forms.OpenFileDialog d = new System.Windows.Forms.OpenFileDialog();
            d.ValidateNames = true;
            d.CheckFileExists = true;
            d.Filter = filetypefilter;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return null;
            return d.FileName;
        }
    }
}
