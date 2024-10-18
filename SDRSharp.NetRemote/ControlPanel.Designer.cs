namespace SDRSharp.NetRemote
{
    partial class ControlPanel
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			cbNetwork = new System.Windows.Forms.CheckBox();
			cbSerial = new System.Windows.Forms.CheckBox();
			cobSerial = new System.Windows.Forms.ComboBox();
			nudPort = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();
			SuspendLayout();
			// 
			// cbNetwork
			// 
			cbNetwork.Checked = true;
			cbNetwork.CheckState = System.Windows.Forms.CheckState.Checked;
			cbNetwork.Location = new System.Drawing.Point(3, 4);
			cbNetwork.Name = "cbNetwork";
			cbNetwork.Size = new System.Drawing.Size(71, 19);
			cbNetwork.TabIndex = 0;
			cbNetwork.Text = "Network";
			cbNetwork.UseVisualStyleBackColor = true;
			cbNetwork.CheckedChanged += cbNetwork_CheckedChanged;
			// 
			// cbSerial
			// 
			cbSerial.Checked = true;
			cbSerial.CheckState = System.Windows.Forms.CheckState.Checked;
			cbSerial.Location = new System.Drawing.Point(3, 34);
			cbSerial.Name = "cbSerial";
			cbSerial.Size = new System.Drawing.Size(54, 19);
			cbSerial.TabIndex = 1;
			cbSerial.Text = "Serial";
			cbSerial.UseVisualStyleBackColor = true;
			cbSerial.CheckedChanged += cbSerial_CheckedChanged;
			// 
			// cobSerial
			// 
			cobSerial.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			cobSerial.FormattingEnabled = true;
			cobSerial.Location = new System.Drawing.Point(80, 32);
			cobSerial.Name = "cobSerial";
			cobSerial.Size = new System.Drawing.Size(66, 23);
			cobSerial.TabIndex = 2;
			// 
			// nudPort
			// 
			nudPort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nudPort.Location = new System.Drawing.Point(80, 3);
			nudPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			nudPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			nudPort.Name = "nudPort";
			nudPort.Size = new System.Drawing.Size(66, 23);
			nudPort.TabIndex = 3;
			nudPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			nudPort.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// ControlPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoSize = true;
			Controls.Add(cobSerial);
			Controls.Add(cbSerial);
			Controls.Add(cbNetwork);
			Controls.Add(nudPort);
			MinimumSize = new System.Drawing.Size(155, 60);
			Name = "ControlPanel";
			Size = new System.Drawing.Size(155, 60);
			((System.ComponentModel.ISupportInitialize)nudPort).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private System.Windows.Forms.CheckBox cbNetwork;
		private System.Windows.Forms.CheckBox cbSerial;
		private System.Windows.Forms.ComboBox cobSerial;
		private System.Windows.Forms.NumericUpDown nudPort;
	}
}
