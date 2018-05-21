using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleIRCLib;

namespace FormExample
{
    public partial class DebugForm : Form
    {

        private readonly SimpleIRC _simpleIrc;

        /// <summary>
        /// Constructor for the debug form.
        /// </summary>
        /// <param name="irc">SimpleIRC instance</param>
        public DebugForm(SimpleIRC irc)
        {
            _simpleIrc = irc;
            InitializeComponent();
        }

        /// <summary>
        /// Clears a specific rich textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClearButton_Click(object sender, EventArgs e)
        {
            RichTextBox selectedRtb = (RichTextBox)debugTabs.SelectedTab.Controls[0];
            selectedRtb.Clear();
        }

        /// <summary>
        /// Makes sure that the form doesn't actually close, but hides instead, so debug messages can still be appended!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        /// <summary>
        /// Register event handlers for the IrcClient and DCCClient when the form loads. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DebugForm_Load(object sender, EventArgs e)
        {
            _simpleIrc.IrcClient.OnDebugMessage += OnIrcDebugMessage;
            _simpleIrc.IrcClient.OnRawMessageReceived += OnRawMessageReceived;
            _simpleIrc.DccClient.OnDccDebugMessage += OnDccDebugMessage;
        }

        /// <summary>
        /// Event for receiving debug messages from the IrcClient
        /// </summary>
        /// <param name="source">source class</param>
        /// <param name="args">IrcDebugMessageEventArgs contains debug message and type</param>
        public void OnIrcDebugMessage(object source, IrcDebugMessageEventArgs args)
        {
            OnIrcDebugMessageLocal(args.Type, args.Message);
        }

        /// <summary>
        /// For appending the debug message on the main thread using invoke required.
        /// </summary>
        /// <param name="type">Debug message type, handy for figuring out where the debug message came from</param>
        /// <param name="message">message to append to the rich textbox</param>
        public void OnIrcDebugMessageLocal(string type, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => OnIrcDebugMessageLocal(type, message)));
            }
            else
            {
                if (debugTabs != null)
                {
                    RichTextBox selectedRtb = (RichTextBox)debugTabs.TabPages[0].Controls[0];
                    selectedRtb.AppendText(type + " | " + message + Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// Event for receiving raw messages from the irc server.
        /// </summary>
        /// <param name="source">source class</param>
        /// <param name="args">IrcRawReceivedEventArgs contains the message received</param>
        public void OnRawMessageReceived(object source, IrcRawReceivedEventArgs args)
        {
            OnRawMessageReceivedLocal(args.Message);
        }

        /// <summary>
        /// For appending the rawmessage on the main thread using invoke required.
        /// </summary>
        /// <param name="message">message to append to the rich textbox</param>
        public void OnRawMessageReceivedLocal(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => OnRawMessageReceivedLocal(message)));
            }
            else
            {
                if (debugTabs != null)
                {
                    RichTextBox selectedRtb = (RichTextBox) debugTabs.TabPages[2].Controls[0];
                    selectedRtb.AppendText(message + Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// Event for receiving debug messages from the DccClient
        /// </summary>
        /// <param name="source">source class</param>
        /// <param name="args">DCCDebugMessageArgs contains the debug message and type</param>
        public void OnDccDebugMessage(object source, DCCDebugMessageArgs args)
        {
           OnDccDebugMessageLocal(args.Type, args.Message);
        }

        /// <summary>
        /// For appending the debug message on the main thread using invoke required.
        /// </summary>
        /// <param name="type">Debug message type, handy for figuring out where the debug message came from</param>
        /// <param name="message">message to append to the rich textbox</param>
        public void OnDccDebugMessageLocal(string type, string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => OnDccDebugMessageLocal(type, message)));
            }
            else
            {
                if (debugTabs != null)
                {
                    RichTextBox selectedRtb = (RichTextBox) debugTabs.TabPages[1].Controls[0];
                    selectedRtb.AppendText(type + " | " + message + Environment.NewLine);
                }
            }
        }
    }
}
