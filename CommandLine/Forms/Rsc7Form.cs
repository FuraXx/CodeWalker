using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using CodeWalker.GameFiles;

namespace CodeWalker.CommandLine.Forms
{
    public partial class Rsc7Form : CodeWalker.CommandLine.Forms.ProgressForm
    {
        private static Rsc7Options Options;

        public Rsc7Form() : this(new Rsc7Options())
        {

        }

        public Rsc7Form(Rsc7Options options)
        {
            Options = options;
            InitializeComponent();
        }

        private void Rcs7Form_Load(object sender, EventArgs e)
        {
            if (Options.Inflate)
            {
                CmdInflate();
            }
            else if (Options.Deflate)
            {
                CmdDeflate();
            }
            else
            {
                MessageBox.Show("Invalid arguments", "CodeWalker by dexyfex");
                Application.Exit();
            }
        }

        public void CmdInflate()
        {
            var inputFiles = Options.InputFiles?.ToList();

            if (inputFiles == null)
            {
                MessageBox.Show("Please specify at least one input file", "CodeWalker by dexyfex");
                Application.Exit();
            }

            for (int i = 0; i < inputFiles.Count; i++)
            {
                if (inputFiles[i].StartsWith("gta://"))
                {
                    MessageBox.Show("gta:// not supported here", "CodeWalker by dexyfex");
                    Application.Exit();
                }
            }

            UpdatePercent("RSC7 inflate", 0);

            Task.Run(() =>
            {
                CommandLine.Init(UpdateStatus, true);

                UpdateStatus("Generating file list");

                var inputFileInfos = FileUtil.Expand(inputFiles);
                var inputFileCount = inputFileInfos.Length;
                var errors         = new List<string>();

                for (int i = 0; i < inputFileCount; i++)
                {
                    var inputFile    = inputFileInfos[i];
                    string name      = Path.GetFileNameWithoutExtension(inputFile.Name);
                    string directory;

                    byte[] data;

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
                        directory = Path.GetDirectoryName(inputFile.FullName);
                    }

                    if (inputFile is FileInfo)
                    {
                        byte[] deflated = File.ReadAllBytes(inputFile.FullName);

                        try
                        {
                            data = InflateRSC7(deflated);

                            if (data == null)
                            {
                                errors.Add("Error inflate (" + i + "/" + inputFileCount + ") " + inputFile.FullName);
                            }
                            else
                            {
                                string targetFile = FileUtil.GetUniqueFileName(directory + "\\" + name + inputFile.Extension + ".inflated");

                                File.WriteAllBytes(targetFile, data);
                            }


                        }
                        catch
                        {
                            errors.Add("Error inflate (" + i + "/" + inputFileCount + ") " + inputFile.FullName);
                            continue;
                        }

                    }

                    int percent = (int)Math.Round((double)(100 * i) / inputFileCount);

                    UpdatePercent("RSC7 inflate", percent);

                }

                if (errors.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var error in errors)
                    {
                        sb.Append(error + "\r\n");
                    }

                    ShowLog(sb.ToString());

                }
                else
                {
                    Application.Exit();
                }

            });
        }

        public void CmdDeflate()
        {
            var inputFiles = Options.InputFiles?.ToList();

            if (inputFiles == null)
            {
                MessageBox.Show("Please specify at least one input file", "CodeWalker by dexyfex");
                Application.Exit();
            }

            for (int i = 0; i < inputFiles.Count; i++)
            {
                if (inputFiles[i].StartsWith("gta://"))
                {
                    MessageBox.Show("gta:// not supported here", "CodeWalker by dexyfex");
                    Application.Exit();
                }
            }

            UpdatePercent("RCS7 deflate", 0);

            Task.Run(() =>
            {
                CommandLine.Init(UpdateStatus, true);

                UpdateStatus("Generating file list");

                var inputFileInfos = FileUtil.Expand(inputFiles);
                var inputFileCount = inputFileInfos.Length;
                var errors         = new List<string>();

                for (int i = 0; i < inputFileCount; i++)
                {
                    var inputFile    = inputFileInfos[i];
                    string name      = Path.GetFileNameWithoutExtension(inputFile.Name);
                    string directory;

                    byte[] data;

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
                        directory = Path.GetDirectoryName(inputFile.FullName);
                    }

                    if (inputFile is FileInfo)
                    {
                        byte[] inflated = File.ReadAllBytes(inputFile.FullName);

                        try
                        {
                            data = DeflateRSC7(inflated);

                            if (data == null)
                            {
                                errors.Add("Error deflate (" + i + "/" + inputFileCount + ") " + inputFile.FullName);
                            }
                            else
                            {
                                string targetFile = FileUtil.GetUniqueFileName(directory + "\\" + name);
                                File.WriteAllBytes(targetFile, data);
                            }


                        }
                        catch
                        {
                            errors.Add("Error deflate (" + i + "/" + inputFileCount + ") " + inputFile.FullName);
                            continue;
                        }

                    }

                    int percent = (int)Math.Round((double)(100 * i) / inputFileCount);

                    UpdatePercent("RCS7 deflate", percent);

                }

                if (errors.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var error in errors)
                    {
                        sb.Append(error + "\r\n");
                    }

                    ShowLog(sb.ToString());

                }
                else
                {
                    Application.Exit();
                }

            });
        }

        private byte[] InflateRSC7(byte[] data)
        {

            uint rsc7 = BitConverter.ToUInt32(data, 0);

            if (rsc7 == 0x37435352)
            {
                if (data.Length > 16)
                {
                    int newlen     = data.Length - 16;
                    byte[] newdata = new byte[newlen];

                    Buffer.BlockCopy(data, 16, newdata, 0, newlen);

                    data = newdata;
                }
                else
                {
                    data = null;
                }
            }

            return ResourceBuilder.Decompress(data);

        }

        private byte[] DeflateRSC7(byte[] data)
        {
            var entry           = new RpfResourceFileEntry();
            entry.SystemFlags   = RpfResourceFileEntry.GetFlagsFromSize(data.Length, 0);
            entry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, 2);

            data = ResourceBuilder.Compress(data);

            return ResourceBuilder.AddResourceHeader(entry, data);
        }

    }
}
