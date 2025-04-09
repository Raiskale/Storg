using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Svg;


namespace Salasanakone
{
    public partial class Storg: Form
    {
        public Storg()
        {
            InitializeComponent();
        }

        private void LoadSvgToPictureBox(string svgPath, PictureBox pb)
        {
            SvgDocument svgDoc = SvgDocument.Open(svgPath);
            Bitmap bmp = svgDoc.Draw();
            pb.Image = bmp;
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Storg_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            string path = Application.StartupPath + @"\SVG\1.svg";
            LoadSvgToPictureBox(path, pictureBox3);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click_1(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
