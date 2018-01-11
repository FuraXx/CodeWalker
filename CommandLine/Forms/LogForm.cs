using System.Windows.Forms;

namespace CodeWalker.CommandLine.Forms
{
    public partial class LogForm : Form
    {
        public string Content    { get { return richTextBox1.Text; } set { richTextBox1.Text = value; } }
        public string ContentRtf { get { return richTextBox1.Rtf;  } set { richTextBox1.Rtf  = value; } }

        public LogForm()
        {
            InitializeComponent();
        }

    }
}
