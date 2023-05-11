using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppDependsOnJava
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Task.Run(this.CheckJava);
        }


        private async Task CheckJava()
        {
            StringBuilder pout = new StringBuilder();

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = @"java.exe";
                p.StartInfo.Arguments = "-version";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.OutputDataReceived += (sender, args) => pout.AppendLine(args.Data);
                p.ErrorDataReceived += (sender, args) => pout.AppendLine(args.Data);

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                pout.AppendLine("Java not found");
            }

            Invoke(new Action(() =>
            {
                this.label1.Text = pout.ToString();
            }));
        }
    }
}
