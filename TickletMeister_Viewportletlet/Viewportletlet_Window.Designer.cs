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
            this.axRDPViewer1 = new AxRDPCOMAPILib.AxRDPViewer();
            this.discoButton = new System.Windows.Forms.Button();
            this.textOutputBox = new System.Windows.Forms.TextBox();
            this.textInputBox = new System.Windows.Forms.TextBox();
            this.alertButton = new System.Windows.Forms.Button();
            this.pollButton = new System.Windows.Forms.Button();
            this.tickletSelectionBox = new System.Windows.Forms.TextBox();
            this.selectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.voiceButton = new System.Windows.Forms.Button();
            this.tickList = new System.Windows.Forms.ListBox();
            this.messageButton = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer1)).BeginInit();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(87, 5);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // axRDPViewer1
            // 
            this.axRDPViewer1.Enabled = true;
            this.axRDPViewer1.Location = new System.Drawing.Point(88, 63);
            this.axRDPViewer1.Name = "axRDPViewer1";
            this.axRDPViewer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axRDPViewer1.OcxState")));
            this.axRDPViewer1.Size = new System.Drawing.Size(1041, 635);
            this.axRDPViewer1.TabIndex = 2;
            this.axRDPViewer1.Enter += new System.EventHandler(this.axRDPViewer1_Enter);
            // 
            // discoButton
            // 
            this.discoButton.Location = new System.Drawing.Point(169, 5);
            this.discoButton.Name = "discoButton";
            this.discoButton.Size = new System.Drawing.Size(75, 23);
            this.discoButton.TabIndex = 3;
            this.discoButton.Text = "Disconnect";
            this.discoButton.UseVisualStyleBackColor = true;
            this.discoButton.Click += new System.EventHandler(this.discoButton_Click);
            // 
            // textOutputBox
            // 
            this.textOutputBox.BackColor = System.Drawing.SystemColors.Window;
            this.textOutputBox.Location = new System.Drawing.Point(1135, 63);
            this.textOutputBox.MinimumSize = new System.Drawing.Size(4, 200);
            this.textOutputBox.Multiline = true;
            this.textOutputBox.Name = "textOutputBox";
            this.textOutputBox.ReadOnly = true;
            this.textOutputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textOutputBox.Size = new System.Drawing.Size(167, 501);
            this.textOutputBox.TabIndex = 4;
            this.textOutputBox.Text = "Output";
            this.textOutputBox.TextChanged += new System.EventHandler(this.textOutputBox_TextChanged);
            // 
            // textInputBox
            // 
            this.textInputBox.Location = new System.Drawing.Point(1135, 581);
            this.textInputBox.MinimumSize = new System.Drawing.Size(4, 70);
            this.textInputBox.Multiline = true;
            this.textInputBox.Name = "textInputBox";
            this.textInputBox.Size = new System.Drawing.Size(167, 70);
            this.textInputBox.TabIndex = 5;
            this.textInputBox.Text = "Input";
            this.textInputBox.TextChanged += new System.EventHandler(this.textInputBox_TextChanged);
            // 
            // alertButton
            // 
            this.alertButton.Location = new System.Drawing.Point(331, 5);
            this.alertButton.Name = "alertButton";
            this.alertButton.Size = new System.Drawing.Size(75, 23);
            this.alertButton.TabIndex = 6;
            this.alertButton.Text = "Alert!";
            this.alertButton.UseVisualStyleBackColor = true;
            this.alertButton.Click += new System.EventHandler(this.alertButton_Click);
            // 
            // pollButton
            // 
            this.pollButton.Location = new System.Drawing.Point(169, 34);
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
            this.tickletSelectionBox.Location = new System.Drawing.Point(344, 36);
            this.tickletSelectionBox.Name = "tickletSelectionBox";
            this.tickletSelectionBox.Size = new System.Drawing.Size(989, 20);
            this.tickletSelectionBox.TabIndex = 9;
            this.tickletSelectionBox.TextChanged += new System.EventHandler(this.tickletSelectionBox_TextChanged);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(87, 34);
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
            this.label1.Location = new System.Drawing.Point(254, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Selected Ticklet";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // voiceButton
            // 
            this.voiceButton.Location = new System.Drawing.Point(250, 5);
            this.voiceButton.Name = "voiceButton";
            this.voiceButton.Size = new System.Drawing.Size(75, 23);
            this.voiceButton.TabIndex = 12;
            this.voiceButton.Text = "Voice!";
            this.voiceButton.UseVisualStyleBackColor = true;
            this.voiceButton.Click += new System.EventHandler(this.voiceButton_Click);
            // 
            // tickList
            // 
            this.tickList.FormattingEnabled = true;
            this.tickList.Location = new System.Drawing.Point(12, 5);
            this.tickList.MinimumSize = new System.Drawing.Size(4, 700);
            this.tickList.Name = "tickList";
            this.tickList.Size = new System.Drawing.Size(71, 693);
            this.tickList.TabIndex = 13;
            this.tickList.SelectedIndexChanged += new System.EventHandler(this.tickList_SelectedIndexChanged);
            // 
            // messageButton
            // 
            this.messageButton.Location = new System.Drawing.Point(1135, 657);
            this.messageButton.Name = "messageButton";
            this.messageButton.Size = new System.Drawing.Size(167, 30);
            this.messageButton.TabIndex = 14;
            this.messageButton.Text = "Send Message";
            this.messageButton.UseVisualStyleBackColor = true;
            this.messageButton.Click += new System.EventHandler(this.buttonSubmit_Click);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 706);
            this.splitter1.TabIndex = 15;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(3, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 706);
            this.splitter2.TabIndex = 16;
            this.splitter2.TabStop = false;
            // 
            // Viewportletlet_Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1309, 706);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.messageButton);
            this.Controls.Add(this.tickList);
            this.Controls.Add(this.voiceButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.tickletSelectionBox);
            this.Controls.Add(this.pollButton);
            this.Controls.Add(this.alertButton);
            this.Controls.Add(this.textInputBox);
            this.Controls.Add(this.textOutputBox);
            this.Controls.Add(this.discoButton);
            this.Controls.Add(this.axRDPViewer1);
            this.Controls.Add(this.connectButton);
            this.Name = "Viewportletlet_Window";
            this.Text = "Viewportletlet";
            this.Load += new System.EventHandler(this.Viewportletlet_Window_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private AxRDPCOMAPILib.AxRDPViewer axRDPViewer1;
        private System.Windows.Forms.Button discoButton;
        private System.Windows.Forms.TextBox textOutputBox;
        private System.Windows.Forms.TextBox textInputBox;
        private System.Windows.Forms.Button alertButton;
        private System.Windows.Forms.Button pollButton;
        private System.Windows.Forms.TextBox tickletSelectionBox;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button voiceButton;
        private System.Windows.Forms.ListBox tickList;
        private System.Windows.Forms.Button messageButton;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
    }
}

