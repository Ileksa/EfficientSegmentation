using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EfficientSegmentation;

namespace Make3DApplication
{
    public partial class FormForTesting : Form
    {
        public FormForTesting()
        {
            InitializeComponent();
        }

        public void pictureBox1_Click(object sender, EventArgs e)
        {
            //тестирующий код
            int[] segOut;
            Bitmap testBitmap = new SegmentImage(0.7, 500, 200).ExecuteEfficientSegmentation(new Bitmap("inputimage2.jpg"), String.Empty, out segOut);

            Graphics grp = Graphics.FromHwnd(pictureBox1.Handle);
            grp.DrawImage(testBitmap, 0, 0, testBitmap.Width, testBitmap.Height);
        }
    }
}
