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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.echoButton = new System.Windows.Forms.Button();
            this.textInputBox = new System.Windows.Forms.TextBox();
            this.textOutputBox = new System.Windows.Forms.TextBox();
            this.submitButton = new System.Windows.Forms.Button();
            this.endButton = new System.Windows.Forms.Button();
            this.chatOutputBox = new System.Windows.Forms.TextBox();
            this.chatInputBox = new System.Windows.Forms.TextBox();
            this.chatMessageButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
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
            this.echoButton.Visible = false;
            this.echoButton.Click += new System.EventHandler(this.echoButton_Click);
            // 
            // textInputBox
            // 
            this.textInputBox.Location = new System.Drawing.Point(39, 209);
            this.textInputBox.Name = "textInputBox";
            this.textInputBox.Size = new System.Drawing.Size(221, 20);
            this.textInputBox.TabIndex = 4;
            this.textInputBox.Text = "Input";
            this.textInputBox.Visible = false;
            // 
            // textOutputBox
            // 
            this.textOutputBox.Location = new System.Drawing.Point(39, 183);
            this.textOutputBox.Name = "textOutputBox";
            this.textOutputBox.Size = new System.Drawing.Size(221, 20);
            this.textOutputBox.TabIndex = 5;
            this.textOutputBox.Text = "Output";
            this.textOutputBox.Visible = false;
            // 
            // submitButton
            // 
            this.submitButton.Location = new System.Drawing.Point(75, 97);
            this.submitButton.Name = "submitButton";
            this.submitButton.Size = new System.Drawing.Size(142, 55);
            this.submitButton.TabIndex = 6;
            this.submitButton.Text = "Submit Ticklet!";
            this.submitButton.UseVisualStyleBackColor = true;
            this.submitButton.Click += new System.EventHandler(this.submitButton_Click);
            // 
            // endButton
            // 
            this.endButton.Location = new System.Drawing.Point(205, 238);
            this.endButton.Name = "endButton";
            this.endButton.Size = new System.Drawing.Size(75, 23);
            this.endButton.TabIndex = 1;
            this.endButton.Text = "End Session";
            this.endButton.UseVisualStyleBackColor = true;
            this.endButton.Visible = false;
            this.endButton.Click += new System.EventHandler(this.endButton_Click);
            // 
            // chatOutputBox
            // 
            this.chatOutputBox.BackColor = System.Drawing.SystemColors.Window;
            this.chatOutputBox.Location = new System.Drawing.Point(306, 7);
            this.chatOutputBox.MinimumSize = new System.Drawing.Size(4, 200);
            this.chatOutputBox.Multiline = true;
            this.chatOutputBox.Name = "chatOutputBox";
            this.chatOutputBox.ReadOnly = true;
            this.chatOutputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.chatOutputBox.Size = new System.Drawing.Size(306, 206);
            this.chatOutputBox.TabIndex = 7;
            this.chatOutputBox.Text = "MESSAGES!";
            this.chatOutputBox.TextChanged += new System.EventHandler(this.chatOutputBox_TextChanged);
            // 
            // chatInputBox
            // 
            this.chatInputBox.Location = new System.Drawing.Point(306, 219);
            this.chatInputBox.MinimumSize = new System.Drawing.Size(4, 70);
            this.chatInputBox.Multiline = true;
            this.chatInputBox.Name = "chatInputBox";
            this.chatInputBox.Size = new System.Drawing.Size(230, 86);
            this.chatInputBox.TabIndex = 8;
            // 
            // chatMessageButton
            // 
            this.chatMessageButton.Location = new System.Drawing.Point(529, 219);
            this.chatMessageButton.Name = "chatMessageButton";
            this.chatMessageButton.Size = new System.Drawing.Size(83, 86);
            this.chatMessageButton.TabIndex = 15;
            this.chatMessageButton.Text = "Send Message";
            this.chatMessageButton.UseVisualStyleBackColor = true;
            this.chatMessageButton.Click += new System.EventHandler(this.chatMessageButton_Click);
            // 
            // Clientlet_Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 306);
            this.Controls.Add(this.chatMessageButton);
            this.Controls.Add(this.chatInputBox);
            this.Controls.Add(this.chatOutputBox);
            this.Controls.Add(this.submitButton);
            this.Controls.Add(this.textOutputBox);
            this.Controls.Add(this.textInputBox);
            this.Controls.Add(this.echoButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.endButton);
            this.Name = "Clientlet_Window";
            this.Text = "Clientlet";
            this.Load += new System.EventHandler(this.Clientlet_Window_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button echoButton;
        private System.Windows.Forms.TextBox textInputBox;
        private System.Windows.Forms.TextBox textOutputBox;
        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.Button endButton;
        private System.Windows.Forms.TextBox chatOutputBox;
        private System.Windows.Forms.TextBox chatInputBox;
        private System.Windows.Forms.Button chatMessageButton;
    }
}

