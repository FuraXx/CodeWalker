using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CodeWalker.GameFiles;
using Glob;

namespace CodeWalker.CommandLine.Forms
{
    public partial class MetaForm : CodeWalker.CommandLine.Forms.ProgressForm
    {
        private static MetaOptions Options;

        public MetaForm() : this(new MetaOptions())
        {

        }

        public MetaForm(MetaOptions options)
        {
            Options = options;
            InitializeComponent();
        }

        private void MetaForm_Load(object sender, EventArgs e)
        {
            if (Options.Export)
            {
                CmdExport();
            }
            else if (Options.Import)
            {
                CmdImport();
            }
            else if (Options.GenStructs)
            {
                CmdGenStructs();
            }
            else
            {
                MessageBox.Show("Invalid arguments", "CodeWalker by dexyfex");
                Application.Exit();
            }
        }

        private void CmdExport()
        {
            var inputFiles = Options.InputFiles.ToList();

            if (inputFiles == null)
            {
                MessageBox.Show("Please specify at least one input file", "CodeWalker by dexyfex");
                return;
            }

            UpdatePercent("Meta export", 0);

            Task.Run(() =>
            {
                CommandLine.Init(UpdateStatus, false, Options.EnableDlc);

                UpdateStatus("Generating file list");

                var inputFileInfos = FileUtil.Expand(CommandLine.Cache.RpfMan, inputFiles);
                var inputFileCount = inputFileInfos.Length;
                var errors         = new List<string>();

                for (int i = 0; i < inputFileCount; i++)
                {
                    var inputFile = inputFileInfos[i];
                    string name   = Path.GetFileNameWithoutExtension(inputFile.FullName);
                    string directory;

                    UpdateStatus(inputFile.FullName);

                    switch (inputFile.Extension)
                    {
                        case ".ymap":
                        case ".ytyp":
                            {

                                Meta meta = null;

                                try
                                {
                                    if (inputFile is FileInfo)
                                    {
                                        meta = LoadMeta(inputFile.FullName);
                                    }
                                    else if (inputFile is RpfFileInfo)
                                    {
                                        var info           = inputFile  as RpfFileSystemInfo;
                                        RpfFileEntry entry = info.Entry as RpfFileEntry;
                                        byte[] data        = entry.File.ExtractFile(entry);

                                        data = entry.File.ExtractFile(entry);
                                        meta = LoadMeta(data);
                                    }
                                }
                                catch
                                {
                                    errors.Add("Error export (" + i + "/" + inputFileCount + ") " + inputFile.FullName);
                                    continue;
                                }


                                string xml = MetaXml.GetXml(meta);

                                if (Options.OutputDirectory != null)
                                {
                                    directory = Options.OutputDirectory;

                                    if(!Directory.Exists(directory))
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

                                    if (inputFile is FileInfo)
                                    {
                                        directory = Path.GetDirectoryName(inputFile.FullName);
                                    }
                                    else
                                    {
                                        directory = Directory.GetCurrentDirectory();
                                    }
                                }

                                if(!Directory.Exists(directory))
                                {
                                    MessageBox.Show("Directory " + directory + " does not exsists");
                                    return;
                                }

                                string targetFile = FileUtil.GetUniqueFileName(directory + "\\" + name + inputFile.Extension + ".xml");

                                File.WriteAllText(targetFile, xml);

                                break;
                            }


                        default:
                            {
                                break;
                            }
                    }

                    int percent = (int)Math.Round((double)(100 * i) / inputFileCount);

                    UpdatePercent("Meta export", percent);
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

        private void CmdImport()
        {
            var inputFiles = Options.InputFiles.ToList();

            if (inputFiles == null)
            {
                MessageBox.Show("Please specify at least one input file", "CodeWalker by dexyfex");
                return;
            }

            for (int i = 0; i < inputFiles.Count; i++)
            {
                if (inputFiles[i].StartsWith("gta://"))
                {
                    MessageBox.Show("gta:// not supported here", "CodeWalker by dexyfex");
                    Application.Exit();
                }
            }

            UpdatePercent("Meta import", 0);

            Task.Run(() =>
            {
                CommandLine.Init(UpdateStatus, true);

                UpdateStatus("Generating file list");

                var inputFileInfos = FileUtil.Expand(inputFiles);
                var inputFileCount = inputFileInfos.Length;
                var errors         = new List<string>();

                for (int i = 0; i < inputFileCount; i++)
                {
                    var inputFile  = inputFileInfos[i];
                    string name    = Path.GetFileNameWithoutExtension(inputFile.Name).Split('.').First();
                    string fullExt = FileUtil.GetFullExtension(inputFile.Name);
                    string origExt = fullExt.Split('.')[0];
                    string directory;

                    UpdateStatus(inputFile.FullName);

                    switch (fullExt)
                    {
                        case "ymap.xml":
                        case "ytyp.xml":
                            {

                                var doc = new XmlDocument();

                                if (inputFile is FileInfo)
                                {
                                    doc.Load(inputFile.FullName);
                                }

                                Meta meta   = XmlMeta.GetMeta(doc);
                                byte[] data = ResourceBuilder.Build(meta, 2);

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

                                string targetFile = FileUtil.GetUniqueFileName(directory + '\\' + name + '.' + origExt);

                                File.WriteAllBytes(targetFile, data);

                                UpdateStatus(inputFile.FullName);

                                break;
                            }


                        default:
                            {
                                break;
                            }
                    }

                    int percent = (int)Math.Round((double)(100 * i) / inputFileCount);

                    UpdatePercent("Meta import", percent);
                }

                Application.Exit();
            });
        }

        private void CmdGenStructs()
        {
            var inputFiles   = Options.InputFiles.ToList();
            bool cacheNeeded = inputFiles.FindIndex(e => e.Contains("gta://")) != -1;

            if (inputFiles == null)
            {
                MessageBox.Show("Please specify at least one input file", "CodeWalker by dexyfex");
                return;
            }

            UpdatePercent("Meta genstructs", 0);

            Task.Run(() =>
            {
                CommandLine.Init(UpdateStatus, !cacheNeeded, Options.EnableDlc);

                UpdateStatus("Generating file list");

                var inputFileInfos = FileUtil.Expand(CommandLine.Cache.RpfMan, inputFiles);
                var inputFileCount = inputFileInfos.Length;
                var structureInfos = new List<MetaStructureInfo>();
                var enumInfos      = new List<MetaEnumInfo>();

                for (int i = 0; i < inputFileCount; i++)
                {
                    var inputFile = inputFileInfos[i];
                    Meta meta = null;

                    try
                    {
                        if (inputFile is FileInfo)
                        {
                            meta = LoadMeta(inputFile.FullName);
                        }
                        else if (inputFile is RpfFileInfo)
                        {
                            var info           = inputFile as RpfFileSystemInfo;
                            RpfFileEntry entry = info.Entry as RpfFileEntry;
                            byte[] data        = entry.File.ExtractFile(entry);
                            meta               = LoadMeta(data);
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    if (meta.StructureInfos != null)
                    {
                        foreach (var si in meta.StructureInfos)
                        {
                            bool found = false;

                            foreach (var si2 in structureInfos)
                            {
                                if (si.StructureNameHash == si2.StructureNameHash)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                UpdateStatus("STRUCT " + si.StructureNameHash);
                                structureInfos.Add(si);
                            }
                        }
                    }

                    if (meta.EnumInfos != null)
                    {
                        foreach (var ei in meta.EnumInfos)
                        {
                            bool found = false;

                            foreach (var ei2 in enumInfos)
                            {
                                if (ei.EnumNameHash == ei2.EnumNameHash)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                UpdateStatus("ENUM " + ei.EnumNameHash);
                                enumInfos.Add(ei);
                            }

                        }
                    }

                    int percent = (int)Math.Round((double)(100 * i) / inputFileCount);

                    UpdatePercent("Meta genstructs", percent);
                }

                string code = AggregateTypesInitString(structureInfos, enumInfos);

                ShowLog(code);

            });

        }

        private Meta LoadMeta(byte[] data)
        {
            RpfResourceFileEntry resentry = new RpfResourceFileEntry();
            uint rsc7                     = BitConverter.ToUInt32(data, 0);

            if (rsc7 == 0x37435352)
            {
                int version            = BitConverter.ToInt32(data, 4);
                resentry.SystemFlags   = BitConverter.ToUInt32(data, 8);
                resentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);

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

                data = ResourceBuilder.Decompress(data);
            }
            else
            {
                resentry.SystemFlags   = RpfResourceFileEntry.GetFlagsFromSize(data.Length, 0);
                resentry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, 2); //graphics type 2 for ymap
            }

            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            return rd.ReadBlock<Meta>();
        }

        private Meta LoadMeta(string fileName)
        {
            byte[] data = File.ReadAllBytes(fileName);
            return LoadMeta(data);
        }

        private string AggregateTypesInitString(List<MetaStructureInfo> structureInfos, List<MetaEnumInfo> enumInfos)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var si in structureInfos)
            {
                sb.AppendFormat("return new MetaStructureInfo({0}, {1}, {2}, {3},", GetMetaNameString(si.StructureNameHash), si.StructureKey, si.Unknown_8h, si.StructureSize);
                sb.AppendLine();

                for (int i = 0; i < si.Entries.Length; i++)
                {
                    var e = si.Entries[i];

                    sb.AppendFormat("   new MetaStructureEntryInfo_s({0}, {1}, MetaStructureEntryDataType.{2}, {3}, {4}, {5})", GetMetaNameString(e.EntryNameHash), e.DataOffset, e.DataType, e.Unknown_9h, e.ReferenceTypeIndex, GetMetaNameString(e.ReferenceKey));

                    if (i < si.Entries.Length - 1) sb.Append(",");

                    sb.AppendLine();
                }

                sb.AppendFormat(");");
                sb.AppendLine("\n");
            }

            sb.AppendLine();

            foreach (var ei in enumInfos)
            {
                sb.AppendFormat("return new MetaEnumInfo({0}, {1},", GetMetaNameString(ei.EnumNameHash), ei.EnumKey);
                sb.AppendLine();

                for (int i = 0; i < ei.Entries.Length; i++)
                {
                    var e = ei.Entries[i];

                    sb.AppendFormat("   new MetaEnumEntryInfo_s({0}, {1})", GetMetaNameString(e.EntryNameHash), e.EntryValue);

                    if (i < ei.Entries.Length - 1) sb.Append(",");

                    sb.AppendLine();
                }

                sb.AppendFormat(");");
                sb.AppendLine("\n");
            }


            string str = sb.ToString();
            return str;
        }

        private string GetMetaNameString(MetaName name)
        {
            string nameString;

            if (((uint)name).ToString() == name.ToString())
            {
                nameString = "(MetaName) " + name.ToString();
            }
            else
            {
                nameString = "MetaName." + name.ToString();
            }

            return nameString;
        }
    }
}
