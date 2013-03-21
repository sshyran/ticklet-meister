namespace TickletMeister_Clientlet
{
    partial class Clientlet_Window
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.endButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.echoButton = new System.Windows.Forms.Button();
            this.textInputBox = new System.Windows.Forms.TextBox();
            this.textOutputBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // endButton
            // 
            this.endButton.Location = new System.Drawing.Point(205, 238);
            this.endButton.Name = "endButton";
            this.endButton.Size = new System.Drawing.Size(75, 23);
            this.endButton.TabIndex = 1;
            this.endButton.Text = "End Session";
            this.endButton.UseVisualStyleBackColor = true;
            this.endButton.Click += new System.EventHandler(this.endButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(39, 49);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(221, 20);
            this.textBox1.TabIndex = 2;
            // 
            // echoButton
            // 
            this.echoButton.Location = new System.Drawing.Point(124, 238);
            this.echoButton.Name = "echoButton";
            this.echoButton.Size = new System.Drawing.Size(75, 23);
            this.echoButton.TabIndex = 3;
            this.echoButton.Text = "Echo";
            this.echoButton.UseVisualStyleBackColor = true;
            this.echoButton.Click += new System.EventHandler(this.echoButton_Click);
            // 
            // textInputBox
            // 
            this.textInputBox.Location = new System.Drawing.Point(39, 209);
            this.textInputBox.Name = "textInputBox";
            this.textInputBox.Size = new System.Drawing.Size(221, 20);
            this.textInputBox.TabIndex = 4;
            this.textInputBox.Text = "Input";
            // 
            // textOutputBox
            // 
            this.textOutputBox.Location = new System.Drawing.Point(39, 183);
            this.textOutputBox.Name = "textOutputBox";
            this.textOutputBox.Size = new System.Drawing.Size(221, 20);
            this.textOutputBox.TabIndex = 5;
            this.textOutputBox.Text = "Output";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.textOutputBox);
            this.Controls.Add(this.textInputBox);
            this.Controls.Add(this.echoButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.endButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button endButton;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button echoButton;
        private System.Windows.Forms.TextBox textInputBox;
        private System.Windows.Forms.TextBox textOutputBox;
    }
}

