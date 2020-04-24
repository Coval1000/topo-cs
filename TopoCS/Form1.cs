using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;



namespace TopoCS
{
    public partial class Form1 : Form
    {
        App app;
        Axle axlPTR;
        public Form1()
        {
            InitializeComponent();
            app = new App();
            app.Initialize(panel2.Handle, panel2.Width, panel2.Height, @"data\100percent_grafen_6H-sic_HV_Z_256_000_Topo.dat");
            app.SaveWidth = 1920;
            app.SaveHeight = 1080;
            timer1.Start();
        }
        ~Form1()
        {
            app.Shutdown();
        }


        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!app.Frame())
            {
                app.Shutdown();
                Dispose();
                Close();
            }
        }
        #region Buttons
        private void button_BackgroundColour_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                app.BackgroundColour = new SharpDX.Color4(
                    colorDialog1.Color.R / 255.0f,
                    colorDialog1.Color.G / 255.0f,
                    colorDialog1.Color.B / 255.0f,
                    1.0f);
        }

        private void button_GridColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                app.graph.terrain.Grid.SolidColor = new SharpDX.Color4(
                    colorDialog1.Color.R / 255.0f,
                    colorDialog1.Color.G / 255.0f,
                    colorDialog1.Color.B / 255.0f,
                    1.0f);
        }

        private void button_Mode_Click(object sender, EventArgs e)
        {
            app.Button_Mode();
        }
        #endregion
        private void numericUpDown_Sensivity_ValueChanged(object sender, EventArgs e)
        {
            app.graph.terrain.Sensivity = (float)numericUpDown_Sensivity.Value;
        }

        private void radioButton_AxleX_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton_AxleX.Checked) return;
            axlPTR = app.graph.AxleX;
        }

        private void radioButton_AxleY_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton_AxleY.Checked) return;
            axlPTR = app.graph.AxleY;
        }

        private void radioButton_AxleZ_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton_AxleZ.Checked) return;
            axlPTR = app.graph.AxleZ;
        }

        private void panel2_MouseEnter(object sender, EventArgs e)
        {
            app.Focus = true;
            app.CoordFix.X = Location.X + panel2.Location.X + 8;
            app.CoordFix.Y = Location.Y + panel2.Location.Y + 31;
        }

        private void panel2_MouseLeave(object sender, EventArgs e)
        {
            app.Focus = false;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void textBox_SaveWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_SaveHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void panel2_SizeChanged(object sender, EventArgs e)
        {
            app.Resize(panel2.Handle, panel2.Width, panel2.Height);
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            Int32.TryParse(textBox_SaveHeight.Text, out app.SaveHeight);
            Int32.TryParse(textBox_SaveWidth.Text, out app.SaveWidth);
            app.PrintScreen = true;
        }
    }
}