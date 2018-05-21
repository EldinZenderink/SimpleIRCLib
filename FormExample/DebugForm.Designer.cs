namespace FormExample
{
    partial class DebugForm
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
            this.ClearButton = new System.Windows.Forms.Button();
            this.debugTabs = new System.Windows.Forms.TabControl();
            this.ircDebug = new System.Windows.Forms.TabPage();
            this.dccDebug = new System.Windows.Forms.TabPage();
            this.ircDebugRichTextbox = new System.Windows.Forms.RichTextBox();
            this.dccDebugRichTextBox = new System.Windows.Forms.RichTextBox();
            this.ircRawOutput = new System.Windows.Forms.TabPage();
            this.rawIrcOutput = new System.Windows.Forms.RichTextBox();
            this.debugTabs.SuspendLayout();
            this.ircDebug.SuspendLayout();
            this.dccDebug.SuspendLayout();
            this.ircRawOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(12, 394);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(449, 23);
            this.ClearButton.TabIndex = 1;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // debugTabs
            // 
            this.debugTabs.Controls.Add(this.ircDebug);
            this.debugTabs.Controls.Add(this.dccDebug);
            this.debugTabs.Controls.Add(this.ircRawOutput);
            this.debugTabs.Location = new System.Drawing.Point(12, 0);
            this.debugTabs.Name = "debugTabs";
            this.debugTabs.SelectedIndex = 0;
            this.debugTabs.Size = new System.Drawing.Size(460, 388);
            this.debugTabs.TabIndex = 2;
            // 
            // ircDebug
            // 
            this.ircDebug.Controls.Add(this.ircDebugRichTextbox);
            this.ircDebug.Location = new System.Drawing.Point(4, 22);
            this.ircDebug.Name = "ircDebug";
            this.ircDebug.Padding = new System.Windows.Forms.Padding(3);
            this.ircDebug.Size = new System.Drawing.Size(452, 362);
            this.ircDebug.TabIndex = 0;
            this.ircDebug.Text = "IRC Debug";
            this.ircDebug.UseVisualStyleBackColor = true;
            // 
            // dccDebug
            // 
            this.dccDebug.Controls.Add(this.dccDebugRichTextBox);
            this.dccDebug.Location = new System.Drawing.Point(4, 22);
            this.dccDebug.Name = "dccDebug";
            this.dccDebug.Padding = new System.Windows.Forms.Padding(3);
            this.dccDebug.Size = new System.Drawing.Size(452, 362);
            this.dccDebug.TabIndex = 1;
            this.dccDebug.Text = "DCC Debug";
            this.dccDebug.UseVisualStyleBackColor = true;
            // 
            // ircDebugRichTextbox
            // 
            this.ircDebugRichTextbox.Location = new System.Drawing.Point(6, 6);
            this.ircDebugRichTextbox.Name = "ircDebugRichTextbox";
            this.ircDebugRichTextbox.Size = new System.Drawing.Size(439, 350);
            this.ircDebugRichTextbox.TabIndex = 0;
            this.ircDebugRichTextbox.Text = "";
            // 
            // dccDebugRichTextBox
            // 
            this.dccDebugRichTextBox.Location = new System.Drawing.Point(6, 6);
            this.dccDebugRichTextBox.Name = "dccDebugRichTextBox";
            this.dccDebugRichTextBox.Size = new System.Drawing.Size(440, 353);
            this.dccDebugRichTextBox.TabIndex = 0;
            this.dccDebugRichTextBox.Text = "";
            // 
            // ircRawOutput
            // 
            this.ircRawOutput.Controls.Add(this.rawIrcOutput);
            this.ircRawOutput.Location = new System.Drawing.Point(4, 22);
            this.ircRawOutput.Name = "ircRawOutput";
            this.ircRawOutput.Size = new System.Drawing.Size(452, 362);
            this.ircRawOutput.TabIndex = 2;
            this.ircRawOutput.Text = "Irc Raw Output";
            this.ircRawOutput.UseVisualStyleBackColor = true;
            // 
            // rawIrcOutput
            // 
            this.rawIrcOutput.Location = new System.Drawing.Point(4, 4);
            this.rawIrcOutput.Name = "rawIrcOutput";
            this.rawIrcOutput.Size = new System.Drawing.Size(441, 355);
            this.rawIrcOutput.TabIndex = 0;
            this.rawIrcOutput.Text = "";
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 427);
            this.Controls.Add(this.debugTabs);
            this.Controls.Add(this.ClearButton);
            this.Name = "DebugForm";
            this.Text = "DebugForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebugForm_FormClosing);
            this.Load += new System.EventHandler(this.DebugForm_Load);
            this.debugTabs.ResumeLayout(false);
            this.ircDebug.ResumeLayout(false);
            this.dccDebug.ResumeLayout(false);
            this.ircRawOutput.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.TabControl debugTabs;
        private System.Windows.Forms.TabPage ircDebug;
        private System.Windows.Forms.RichTextBox ircDebugRichTextbox;
        private System.Windows.Forms.TabPage dccDebug;
        private System.Windows.Forms.RichTextBox dccDebugRichTextBox;
        private System.Windows.Forms.TabPage ircRawOutput;
        private System.Windows.Forms.RichTextBox rawIrcOutput;
    }
}