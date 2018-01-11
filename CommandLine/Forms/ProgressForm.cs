using System;
using System.Windows.Forms;

namespace CodeWalker.CommandLine.Forms
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        protected void UpdateStatus(string text)
        {
            if (!IsDisposed)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdateStatus(text); }));
                }
                else
                {
                    label1.Text = text;
                }
            }
        }

        protected void UpdatePercent(string prefix, int percent)
        {
            if(!IsDisposed)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { UpdatePercent(prefix, percent); }));
                }
                else
                {
                    Text               = prefix + " - " + percent + "% - CodeWalker by dexyfex";
                    progressBar1.Value = percent;
                }
            }

        }

        protected void ShowLog(string text)
        {
            if(!IsDisposed)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { ShowLog(text); }));
                }
                else
                {
                    Hide();

                    var logForm     = new LogForm();
                    logForm.Content = text;

                    logForm.Show();

                    logForm.FormClosed += (object sender, FormClosedEventArgs e) => { Application.Exit(); };
                }
            }

        }

        protected void ShowLogRtf(string rtfText)
        {
            if (!IsDisposed)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { ShowLogRtf(rtfText); }));
                }
                else
                {
                    Hide();

                    var logForm        = new LogForm();
                    logForm.ContentRtf = rtfText;

                    logForm.Show();

                    logForm.FormClosed += (object sender, FormClosedEventArgs e) => { Application.Exit(); };
                }
            }

        }

    }
}
