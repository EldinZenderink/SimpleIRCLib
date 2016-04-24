using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormExample
{
    public partial class DebugForm : Form
    {

        public DebugForm()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            DebugOutput.Clear();
        }

        /// <summary>
        /// Appends debug messages from irc client to debug richtextbox output box. 
        /// Invoke is needed because this method executes on a different thread!
        /// </summary>
        /// <param name="debugMessage">defines the message to be appended to the debug richtextbox output box</param>
        public void AppendToDebugOutput(string debugMessage)
        {
            if (this.DebugOutput.InvokeRequired)
            {
                this.DebugOutput.Invoke(new MethodInvoker(() => AppendToDebugOutput(debugMessage)));
            }
            else
            {
                this.DebugOutput.AppendText(debugMessage + "\n");
            }
        }

        /// <summary>
        /// Makes sure that the form doesn't actually close, but hides instead, so debug messages can still be appended!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
