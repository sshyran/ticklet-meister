namespace TickletMeister_Viewportletlet
{
    partial class Viewportletlet_Window
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
            if (axRDPViewer1 != null)
            {
                axRDPViewer1.Disconnect();
            }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Viewportletlet_Window));
            this.connectButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.axRDPViewer1 = new AxRDPCOMAPILib.AxRDPViewer();
            this.discoButton = new System.Windows.Forms.Button();
            this.textOutputBox = new System.Windows.Forms.TextBox();
            this.textInputBox = new System.Windows.Forms.TextBox();
            this.alertButton = new System.Windows.Forms.Button();
            this.endButton = new System.Windows.Forms.Button();
            this.pollButton = new System.Windows.Forms.Button();
            this.tickletSelectionBox = new System.Windows.Forms.TextBox();
            this.selectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.voiceButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer1)).BeginInit();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(262, 17);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(44, 17);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(208, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // axRDPViewer1
            // 
            this.axRDPViewer1.Enabled = true;
            this.axRDPViewer1.Location = new System.Drawing.Point(44, 101);
            this.axRDPViewer1.Name = "axRDPViewer1";
            this.axRDPViewer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axRDPViewer1.OcxState")));
            this.axRDPViewer1.Size = new System.Drawing.Size(1454, 709);
            this.axRDPViewer1.TabIndex = 2;
            // 
            // discoButton
            // 
            this.discoButton.Location = new System.Drawing.Point(350, 17);
            this.discoButton.Name = "discoButton";
            this.discoButton.Size = new System.Drawing.Size(75, 23);
            this.discoButton.TabIndex = 3;
            this.discoButton.Text = "Disconnect";
            this.discoButton.UseVisualStyleBackColor = true;
            this.discoButton.Click += new System.EventHandler(this.discoButton_Click);
            // 
            // textOutputBox
            // 
            this.textOutputBox.Location = new System.Drawing.Point(455, 19);
            this.textOutputBox.Name = "textOutputBox";
            this.textOutputBox.Size = new System.Drawing.Size(947, 20);
            this.textOutputBox.TabIndex = 4;
            this.textOutputBox.Text = "Output";
            // 
            // textInputBox
            // 
            this.textInputBox.Location = new System.Drawing.Point(455, 45);
            this.textInputBox.Name = "textInputBox";
            this.textInputBox.Size = new System.Drawing.Size(947, 20);
            this.textInputBox.TabIndex = 5;
            this.textInputBox.Text = "Input";
            // 
            // alertButton
            // 
            this.alertButton.Location = new System.Drawing.Point(1409, 41);
            this.alertButton.Name = "alertButton";
            this.alertButton.Size = new System.Drawing.Size(75, 23);
            this.alertButton.TabIndex = 6;
            this.alertButton.Text = "Alert!";
            this.alertButton.UseVisualStyleBackColor = true;
            this.alertButton.Click += new System.EventHandler(this.alertButton_Click);
            // 
            // endButton
            // 
            this.endButton.Location = new System.Drawing.Point(1409, 13);
            this.endButton.Name = "endButton";
            this.endButton.Size = new System.Drawing.Size(75, 23);
            this.endButton.TabIndex = 7;
            this.endButton.Text = "Close";
            this.endButton.UseVisualStyleBackColor = true;
            this.endButton.Click += new System.EventHandler(this.endButton_Click);
            // 
            // pollButton
            // 
            this.pollButton.Location = new System.Drawing.Point(262, 46);
            this.pollButton.Name = "pollButton";
            this.pollButton.Size = new System.Drawing.Size(75, 23);
            this.pollButton.TabIndex = 8;
            this.pollButton.Text = "Poll";
            this.pollButton.UseVisualStyleBackColor = true;
            this.pollButton.Click += new System.EventHandler(this.pollButton_Click);
            // 
            // tickletSelectionBox
            // 
            this.tickletSelectionBox.Enabled = false;
            this.tickletSelectionBox.Location = new System.Drawing.Point(177, 75);
            this.tickletSelectionBox.Name = "tickletSelectionBox";
            this.tickletSelectionBox.Size = new System.Drawing.Size(1321, 20);
            this.tickletSelectionBox.TabIndex = 9;
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(177, 46);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(75, 23);
            this.selectButton.TabIndex = 10;
            this.selectButton.Text = "Select";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Selected Ticklet";
            // 
            // voiceButton
            // 
            this.voiceButton.Location = new System.Drawing.Point(350, 46);
            this.voiceButton.Name = "voiceButton";
            this.voiceButton.Size = new System.Drawing.Size(75, 23);
            this.voiceButton.TabIndex = 12;
            this.voiceButton.Text = "Voice!";
            this.voiceButton.UseVisualStyleBackColor = true;
            this.voiceButton.Click += new System.EventHandler(this.voiceButton_Click);
            // 
            // Viewportletlet_Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1536, 837);
            this.Controls.Add(this.voiceButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.tickletSelectionBox);
            this.Controls.Add(this.pollButton);
            this.Controls.Add(this.endButton);
            this.Controls.Add(this.alertButton);
            this.Controls.Add(this.textInputBox);
            this.Controls.Add(this.textOutputBox);
            this.Controls.Add(this.discoButton);
            this.Controls.Add(this.axRDPViewer1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.connectButton);
            this.Name = "Viewportletlet_Window";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox textBox1;
        private AxRDPCOMAPILib.AxRDPViewer axRDPViewer1;
        private System.Windows.Forms.Button discoButton;
        private System.Windows.Forms.TextBox textOutputBox;
        private System.Windows.Forms.TextBox textInputBox;
        private System.Windows.Forms.Button alertButton;
        private System.Windows.Forms.Button endButton;
        private System.Windows.Forms.Button pollButton;
        private System.Windows.Forms.TextBox tickletSelectionBox;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button voiceButton;
    }
}

