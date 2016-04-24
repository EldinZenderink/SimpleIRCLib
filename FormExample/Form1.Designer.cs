namespace FormExample
{
    partial class Form1
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
            this.ServerInput = new System.Windows.Forms.TextBox();
            this.PortInput = new System.Windows.Forms.TextBox();
            this.UsernameInput = new System.Windows.Forms.TextBox();
            this.ChannelsInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelusername = new System.Windows.Forms.Label();
            this.labelsomething = new System.Windows.Forms.Label();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.ChatOutput = new System.Windows.Forms.RichTextBox();
            this.MessageInput = new System.Windows.Forms.TextBox();
            this.SendMessageButton = new System.Windows.Forms.Button();
            this.DownloadsList = new System.Windows.Forms.ListBox();
            this.ShowDebugButton = new System.Windows.Forms.Button();
            this.SetDownloadFolderButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.DownloadProgressBar = new System.Windows.Forms.ProgressBar();
            this.OpenFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // ServerInput
            // 
            this.ServerInput.Location = new System.Drawing.Point(49, 37);
            this.ServerInput.Name = "ServerInput";
            this.ServerInput.Size = new System.Drawing.Size(162, 20);
            this.ServerInput.TabIndex = 0;
            // 
            // PortInput
            // 
            this.PortInput.Location = new System.Drawing.Point(49, 76);
            this.PortInput.Name = "PortInput";
            this.PortInput.Size = new System.Drawing.Size(162, 20);
            this.PortInput.TabIndex = 1;
            // 
            // UsernameInput
            // 
            this.UsernameInput.Location = new System.Drawing.Point(49, 114);
            this.UsernameInput.Name = "UsernameInput";
            this.UsernameInput.Size = new System.Drawing.Size(162, 20);
            this.UsernameInput.TabIndex = 2;
            // 
            // ChannelsInput
            // 
            this.ChannelsInput.Location = new System.Drawing.Point(49, 152);
            this.ChannelsInput.Name = "ChannelsInput";
            this.ChannelsInput.Size = new System.Drawing.Size(162, 20);
            this.ChannelsInput.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(46, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Server";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            // 
            // labelusername
            // 
            this.labelusername.AutoSize = true;
            this.labelusername.Location = new System.Drawing.Point(49, 99);
            this.labelusername.Name = "labelusername";
            this.labelusername.Size = new System.Drawing.Size(55, 13);
            this.labelusername.TabIndex = 6;
            this.labelusername.Text = "Username";
            // 
            // labelsomething
            // 
            this.labelsomething.AutoSize = true;
            this.labelsomething.Location = new System.Drawing.Point(49, 137);
            this.labelsomething.Name = "labelsomething";
            this.labelsomething.Size = new System.Drawing.Size(156, 13);
            this.labelsomething.TabIndex = 7;
            this.labelsomething.Text = "Channel(s) (comma to seperate)";
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(49, 179);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(162, 23);
            this.ConnectButton.TabIndex = 8;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.Location = new System.Drawing.Point(49, 208);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(162, 23);
            this.DisconnectButton.TabIndex = 9;
            this.DisconnectButton.Text = "Disconnect";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // ChatOutput
            // 
            this.ChatOutput.Location = new System.Drawing.Point(244, 37);
            this.ChatOutput.Name = "ChatOutput";
            this.ChatOutput.Size = new System.Drawing.Size(307, 252);
            this.ChatOutput.TabIndex = 10;
            this.ChatOutput.Text = "";
            // 
            // MessageInput
            // 
            this.MessageInput.Location = new System.Drawing.Point(49, 314);
            this.MessageInput.Name = "MessageInput";
            this.MessageInput.Size = new System.Drawing.Size(432, 20);
            this.MessageInput.TabIndex = 11;
            this.MessageInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MessageInput_KeyDown);
            // 
            // SendMessageButton
            // 
            this.SendMessageButton.Location = new System.Drawing.Point(487, 312);
            this.SendMessageButton.Name = "SendMessageButton";
            this.SendMessageButton.Size = new System.Drawing.Size(64, 23);
            this.SendMessageButton.TabIndex = 12;
            this.SendMessageButton.Text = "Send";
            this.SendMessageButton.UseVisualStyleBackColor = true;
            this.SendMessageButton.Click += new System.EventHandler(this.SendMessageButton_Click);
            // 
            // DownloadsList
            // 
            this.DownloadsList.FormattingEnabled = true;
            this.DownloadsList.HorizontalScrollbar = true;
            this.DownloadsList.Location = new System.Drawing.Point(49, 362);
            this.DownloadsList.Name = "DownloadsList";
            this.DownloadsList.ScrollAlwaysVisible = true;
            this.DownloadsList.Size = new System.Drawing.Size(502, 121);
            this.DownloadsList.TabIndex = 13;
            this.DownloadsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.DownloadsList_MouseDoubleClick);
            // 
            // ShowDebugButton
            // 
            this.ShowDebugButton.Location = new System.Drawing.Point(49, 237);
            this.ShowDebugButton.Name = "ShowDebugButton";
            this.ShowDebugButton.Size = new System.Drawing.Size(162, 23);
            this.ShowDebugButton.TabIndex = 14;
            this.ShowDebugButton.Text = "Show Debug";
            this.ShowDebugButton.UseVisualStyleBackColor = true;
            this.ShowDebugButton.Click += new System.EventHandler(this.ShowDebugButton_Click);
            // 
            // SetDownloadFolderButton
            // 
            this.SetDownloadFolderButton.Location = new System.Drawing.Point(49, 266);
            this.SetDownloadFolderButton.Name = "SetDownloadFolderButton";
            this.SetDownloadFolderButton.Size = new System.Drawing.Size(162, 23);
            this.SetDownloadFolderButton.TabIndex = 15;
            this.SetDownloadFolderButton.Text = "Choose Download Folder";
            this.SetDownloadFolderButton.UseVisualStyleBackColor = true;
            this.SetDownloadFolderButton.Click += new System.EventHandler(this.SetDownloadFolderButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(241, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Chat:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(46, 346);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Downloads:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(49, 486);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Progress:";
            // 
            // DownloadProgressBar
            // 
            this.DownloadProgressBar.Location = new System.Drawing.Point(49, 503);
            this.DownloadProgressBar.Name = "DownloadProgressBar";
            this.DownloadProgressBar.Size = new System.Drawing.Size(502, 23);
            this.DownloadProgressBar.TabIndex = 19;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 530);
            this.Controls.Add(this.DownloadProgressBar);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.SetDownloadFolderButton);
            this.Controls.Add(this.ShowDebugButton);
            this.Controls.Add(this.DownloadsList);
            this.Controls.Add(this.SendMessageButton);
            this.Controls.Add(this.MessageInput);
            this.Controls.Add(this.ChatOutput);
            this.Controls.Add(this.DisconnectButton);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.labelsomething);
            this.Controls.Add(this.labelusername);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ChannelsInput);
            this.Controls.Add(this.UsernameInput);
            this.Controls.Add(this.PortInput);
            this.Controls.Add(this.ServerInput);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ServerInput;
        private System.Windows.Forms.TextBox PortInput;
        private System.Windows.Forms.TextBox UsernameInput;
        private System.Windows.Forms.TextBox ChannelsInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelusername;
        private System.Windows.Forms.Label labelsomething;
        private System.Windows.Forms.Button ConnectButton;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.RichTextBox ChatOutput;
        private System.Windows.Forms.TextBox MessageInput;
        private System.Windows.Forms.Button SendMessageButton;
        private System.Windows.Forms.ListBox DownloadsList;
        private System.Windows.Forms.Button ShowDebugButton;
        private System.Windows.Forms.Button SetDownloadFolderButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ProgressBar DownloadProgressBar;
        private System.Windows.Forms.FolderBrowserDialog OpenFolderDialog;
    }
}

