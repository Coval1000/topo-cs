namespace TopoCS
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows
       
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel2 = new System.Windows.Forms.Panel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.button_BackgroundColour = new System.Windows.Forms.Button();
            this.button_Mode = new System.Windows.Forms.Button();
            this.numericUpDown_Sensivity = new System.Windows.Forms.NumericUpDown();
            this.radioButton_AxleX = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButton_AxleZ = new System.Windows.Forms.RadioButton();
            this.radioButton_AxleY = new System.Windows.Forms.RadioButton();
            this.button_GridColor = new System.Windows.Forms.Button();
            this.button_Save = new System.Windows.Forms.Button();
            this.textBox_SaveWidth = new System.Windows.Forms.TextBox();
            this.textBox_SaveHeight = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sensivity)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Location = new System.Drawing.Point(13, 123);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1040, 545);
            this.panel2.TabIndex = 9;
            this.panel2.SizeChanged += new System.EventHandler(this.panel2_SizeChanged);
            this.panel2.MouseEnter += new System.EventHandler(this.panel2_MouseEnter);
            this.panel2.MouseLeave += new System.EventHandler(this.panel2_MouseLeave);
            // 
            // timer1
            // 
            this.timer1.Interval = 8;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // button_BackgroundColour
            // 
            this.button_BackgroundColour.Location = new System.Drawing.Point(13, 11);
            this.button_BackgroundColour.Name = "button_BackgroundColour";
            this.button_BackgroundColour.Size = new System.Drawing.Size(80, 30);
            this.button_BackgroundColour.TabIndex = 10;
            this.button_BackgroundColour.Text = "Kolor Tła";
            this.button_BackgroundColour.UseVisualStyleBackColor = true;
            this.button_BackgroundColour.Click += new System.EventHandler(this.button_BackgroundColour_Click);
            // 
            // button_Mode
            // 
            this.button_Mode.Location = new System.Drawing.Point(204, 11);
            this.button_Mode.Name = "button_Mode";
            this.button_Mode.Size = new System.Drawing.Size(85, 30);
            this.button_Mode.TabIndex = 11;
            this.button_Mode.Text = "Tryb";
            this.button_Mode.UseVisualStyleBackColor = true;
            this.button_Mode.Click += new System.EventHandler(this.button_Mode_Click);
            // 
            // numericUpDown_Sensivity
            // 
            this.numericUpDown_Sensivity.DecimalPlaces = 2;
            this.numericUpDown_Sensivity.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numericUpDown_Sensivity.Location = new System.Drawing.Point(13, 47);
            this.numericUpDown_Sensivity.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDown_Sensivity.Name = "numericUpDown_Sensivity";
            this.numericUpDown_Sensivity.Size = new System.Drawing.Size(125, 26);
            this.numericUpDown_Sensivity.TabIndex = 12;
            this.numericUpDown_Sensivity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_Sensivity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_Sensivity.ValueChanged += new System.EventHandler(this.numericUpDown_Sensivity_ValueChanged);
            // 
            // radioButton_AxleX
            // 
            this.radioButton_AxleX.AutoSize = true;
            this.radioButton_AxleX.Location = new System.Drawing.Point(3, 3);
            this.radioButton_AxleX.Name = "radioButton_AxleX";
            this.radioButton_AxleX.Size = new System.Drawing.Size(62, 24);
            this.radioButton_AxleX.TabIndex = 13;
            this.radioButton_AxleX.TabStop = true;
            this.radioButton_AxleX.Text = "Oś X";
            this.radioButton_AxleX.UseVisualStyleBackColor = true;
            this.radioButton_AxleX.CheckedChanged += new System.EventHandler(this.radioButton_AxleX_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButton_AxleZ);
            this.panel1.Controls.Add(this.radioButton_AxleY);
            this.panel1.Controls.Add(this.radioButton_AxleX);
            this.panel1.Location = new System.Drawing.Point(583, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(465, 93);
            this.panel1.TabIndex = 14;
            // 
            // radioButton_AxleZ
            // 
            this.radioButton_AxleZ.AutoSize = true;
            this.radioButton_AxleZ.Location = new System.Drawing.Point(3, 63);
            this.radioButton_AxleZ.Name = "radioButton_AxleZ";
            this.radioButton_AxleZ.Size = new System.Drawing.Size(61, 24);
            this.radioButton_AxleZ.TabIndex = 15;
            this.radioButton_AxleZ.TabStop = true;
            this.radioButton_AxleZ.Text = "Oś Z";
            this.radioButton_AxleZ.UseVisualStyleBackColor = true;
            this.radioButton_AxleZ.CheckedChanged += new System.EventHandler(this.radioButton_AxleZ_CheckedChanged);
            // 
            // radioButton_AxleY
            // 
            this.radioButton_AxleY.AutoSize = true;
            this.radioButton_AxleY.Location = new System.Drawing.Point(3, 33);
            this.radioButton_AxleY.Name = "radioButton_AxleY";
            this.radioButton_AxleY.Size = new System.Drawing.Size(62, 24);
            this.radioButton_AxleY.TabIndex = 14;
            this.radioButton_AxleY.TabStop = true;
            this.radioButton_AxleY.Text = "Oś Y";
            this.radioButton_AxleY.UseVisualStyleBackColor = true;
            this.radioButton_AxleY.CheckedChanged += new System.EventHandler(this.radioButton_AxleY_CheckedChanged);
            // 
            // button_GridColor
            // 
            this.button_GridColor.Location = new System.Drawing.Point(99, 11);
            this.button_GridColor.Name = "button_GridColor";
            this.button_GridColor.Size = new System.Drawing.Size(99, 30);
            this.button_GridColor.TabIndex = 15;
            this.button_GridColor.Text = "Kolor Siatki";
            this.button_GridColor.UseVisualStyleBackColor = true;
            this.button_GridColor.Click += new System.EventHandler(this.button_GridColor_Click);
            // 
            // button_Save
            // 
            this.button_Save.Location = new System.Drawing.Point(13, 79);
            this.button_Save.Name = "button_Save";
            this.button_Save.Size = new System.Drawing.Size(78, 27);
            this.button_Save.TabIndex = 16;
            this.button_Save.Text = "Save";
            this.button_Save.UseVisualStyleBackColor = true;
            this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
            // 
            // textBox_SaveWidth
            // 
            this.textBox_SaveWidth.Location = new System.Drawing.Point(97, 80);
            this.textBox_SaveWidth.Name = "textBox_SaveWidth";
            this.textBox_SaveWidth.Size = new System.Drawing.Size(65, 26);
            this.textBox_SaveWidth.TabIndex = 17;
            this.textBox_SaveWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_SaveWidth_KeyPress);
            // 
            // textBox_SaveHeight
            // 
            this.textBox_SaveHeight.Location = new System.Drawing.Point(168, 80);
            this.textBox_SaveHeight.Name = "textBox_SaveHeight";
            this.textBox_SaveHeight.Size = new System.Drawing.Size(65, 26);
            this.textBox_SaveHeight.TabIndex = 18;
            this.textBox_SaveHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_SaveHeight_KeyPress);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(295, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(15, 15);
            this.button1.TabIndex = 19;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            // 
            // Form1
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(1062, 680);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox_SaveHeight);
            this.Controls.Add(this.textBox_SaveWidth);
            this.Controls.Add(this.button_Save);
            this.Controls.Add(this.button_GridColor);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.numericUpDown_Sensivity);
            this.Controls.Add(this.button_Mode);
            this.Controls.Add(this.button_BackgroundColour);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(850, 400);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TopoCS";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sensivity)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button button_BackgroundColour;
        private System.Windows.Forms.Button button_Mode;
        private System.Windows.Forms.NumericUpDown numericUpDown_Sensivity;
        private System.Windows.Forms.RadioButton radioButton_AxleX;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButton_AxleZ;
        private System.Windows.Forms.RadioButton radioButton_AxleY;
        private System.Windows.Forms.Button button_GridColor;
        private System.Windows.Forms.Button button_Save;
        private System.Windows.Forms.TextBox textBox_SaveWidth;
        private System.Windows.Forms.TextBox textBox_SaveHeight;
        private System.Windows.Forms.Button button1;
    }
}

