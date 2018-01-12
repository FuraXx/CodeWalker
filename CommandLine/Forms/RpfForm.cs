using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using Glob;

namespace CodeWalker.CommandLine.Forms
{
    public partial class RpfForm : CodeWalker.CommandLine.Forms.ProgressForm
    {
        RpfOptions Options;

        public RpfForm() : this(new RpfOptions())
        {

        }

        public RpfForm(RpfOptions options)
        {
            Options = options;
            InitializeComponent();
        }

        private void RpfForm_Load(object sender, EventArgs e)
        {
            if(Options.Extract)
            {
                CmdExtract();
            }
            else
            {
                MessageBox.Show("Invalid arguments", "CodeWalker by dexyfex");
                Application.Exit();
            }
        }

        public void CmdExtract()
        {
            var inputFiles = Options.InputFiles?.ToList();

            if (inputFiles == null)
            {
                MessageBox.Show("Please specify at least one input file", "CodeWalker by dexyfex");
                Application.Exit();
            }

            UpdatePercent("RPF extract", 0);

            Task.Run(() =>
            {
                CommandLine.Init(UpdateStatus, false, Options.EnableDlc);

                UpdateStatus("Generating file list");

                var inputFileInfos = FileUtil.Expand(CommandLine.Cache.RpfMan, inputFiles);
                var inputFileCount = inputFileInfos.Length;

                for (int i = 0; i < inputFileCount; i++)
                {
                    var inputFile    = inputFileInfos[i];
                    string name      = Path.GetFileNameWithoutExtension(inputFile.FullName);
                    string directory;
                    string targetFile;

                    UpdateStatus(inputFile.FullName);

                    if (Options.OutputDirectory != null)
                    {
                        directory = Options.OutputDirectory;

                        if (!Directory.Exists(directory))
                        {
                            try
                            {
                                Directory.CreateDirectory(directory);
                            }
                            catch
                            {
                                MessageBox.Show("Could not create directory " + directory);
                                return;
                            }
                        }
                    }
                    else
                    {
                        directory = Environment.CurrentDirectory;
                    }

                    targetFile = FileUtil.GetUniqueFileName(directory + "\\" + name + inputFile.Extension);

                    if (inputFile is FileInfo)
                    {
                        File.Copy(inputFile.FullName, targetFile);
                    }
                    else if (inputFile is RpfFileInfo)
                    {
                        var info           = inputFile  as RpfFileSystemInfo;
                        RpfFileEntry entry = info.Entry as RpfFileEntry;
                        byte[] data        = entry.File.ExtractFile(entry);

                        File.WriteAllBytes(targetFile, data);
                    }

                    int percent = (int)Math.Round((double)(100 * i) / inputFileCount);

                    UpdatePercent("RPF extract", percent);

                }

                Application.Exit();

            });

        }

    }
}
