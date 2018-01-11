using System;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Reflection;
using CodeWalker.Properties;
using CodeWalker.GameFiles;
using CodeWalker.CommandLine.Arguments;
using CodeWalker.CommandLine.Forms;
using System.Collections.Generic;

namespace CodeWalker.CommandLine
{
    class CommandLine
    {
        public static GameFileCache Cache     = new GameFileCache();
        public static bool          FoundVerb = false;

        public static void Run(string[] args)
        {
            Parse<MenuOptions>   (args, opts => { Application.Run(new MenuForm   ());     });
            Parse<ExploreOptions>(args, opts => { Application.Run(new ExploreForm());     });
            Parse<WorldOptions>  (args, opts => { Application.Run(new WorldForm  ());     });
            Parse<MetaOptions>   (args, opts => { Application.Run(new MetaForm   (opts)); });
            Parse<RpfOptions>    (args, opts => { Application.Run(new RpfForm    (opts)); });
            Parse<Rsc7Options>   (args, opts => { Application.Run(new Rsc7Form   (opts)); });

            if(!FoundVerb && args.Length > 0 && args[0] == "help")
            {
                ShowHelp();
            }

        }

        public static bool Init(Action<string> updateStatus = null, bool skipCache = false, bool enableDlc = false)
        {

            if (!EnsureGTAFolder())
            {
                return false;
            }

            try
            {
                GTA5Keys.LoadFromPath(Settings.Default.GTAFolder);
                
                if(!skipCache)
                {
                    Cache.Init(updateStatus, updateStatus);

                    if(enableDlc)
                    {
                        string dlc = Cache.DlcNameList[Cache.DlcNameList.Count - 1];
                        updateStatus("Loading latest DLC : " + dlc);
                        Cache.SetDlcLevel(dlc, true);
                    }
                }

                return true;
            }
            catch
            {
                MessageBox.Show("Keys not found! This shouldn't happen", "CodeWalker by dexyfex");
                return false;
            }
        }

        /// <summary>
        /// Parse a parsing target T
        /// </summary>
        public static void Parse<T>(string[] args, Action<T> callback) where T : class,new()
        {
            T parsingTarget   = null;
            var verbAttribute = GetVerbAttribute<T>();

            if(args[0] == verbAttribute.Name)
            {
                parsingTarget = new T();
                AssignOptions(parsingTarget, args);
            }

            if(args.Length >= 2 && args[0] == "help" && args[1] == verbAttribute.Name)
            {
                ShowHelp(GenHelp<T>());
            }

            if(parsingTarget != null)
            {
                FoundVerb = true;
                callback(parsingTarget);
            }
        }

        /// <summary>
        /// Get verb attribute from parsing target T
        /// </summary>
        private static VerbAttribute GetVerbAttribute<T>() where T : class
        {
            TypeInfo typeInfo = typeof(T).GetTypeInfo();
            var attrs         = typeInfo.GetCustomAttributes();

            foreach (var attr in attrs)
            {
                if (attr.GetType().Name == "VerbAttribute")
                {
                    return attr as VerbAttribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Get pairs of PropertyInfo / OptionAttribute from parsing target T
        /// </summary>
        private static Tuple<PropertyInfo, OptionAttribute>[] GetOptionAttributes<T>() where T : class
        {
            TypeInfo typeInfo = typeof(T).GetTypeInfo();
            var props         = typeInfo.GetProperties();
            var optionAttrs   = new List<Tuple<PropertyInfo, OptionAttribute>>();

            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes();

                foreach(var attr in attrs)
                {
                    if (attr.GetType().Name == "OptionAttribute")
                    {
                        optionAttrs.Add(new Tuple<PropertyInfo, OptionAttribute>(prop, attr as OptionAttribute));
                        break;
                    }
                }

            }

            return optionAttrs.ToArray();
        }

        /// <summary>
        /// Assign values to parsing target T from arg array
        /// </summary>
        private static void AssignOptions<T>(T obj, string[] args) where T : class
        {
            var optionAttrs = GetOptionAttributes<T>();

            foreach (var attr in optionAttrs)
            {
                bool found = false;

                for(int i=0; i<args.Length; i++)
                {
                    var type = attr.Item1.PropertyType;

                    if(args[i] == "--" + attr.Item2.Name || args[i] == "-" + attr.Item2.ShortName)
                    {
                        found = true;

                        if (type == typeof(bool)) // Parse bool (switch)
                        {
                            attr.Item1.SetValue(obj, !(bool)attr.Item2.Default);

                        }
                        else if (type == typeof(string)) // Parse string
                        {
                            if (args.Length > i + 1 && !args[i + 1].StartsWith("-"))
                            {
                                attr.Item1.SetValue(obj, args[i + 1]);
                            }
                            else
                            {
                                attr.Item1.SetValue(obj, (string)attr.Item2.Default);
                            }
                        }
                        else if (type == typeof(int)) // Parse int
                        {
                            if (args.Length > i + 1 && !args[i + 1].StartsWith("-"))
                            {
                                attr.Item1.SetValue(obj, Convert.ToInt32(args[i + 1]));
                            }
                            else
                            {
                                attr.Item1.SetValue(obj, (string)attr.Item2.Default);
                            }
                        }
                        else if (type == typeof(IEnumerable<string>)) // Parse array of string
                        {
                            var list = new List<string>();

                            while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                i++;
                                list.Add(args[i]);
                            }

                            attr.Item1.SetValue(obj, list);
                        }
                        else if (type == typeof(IEnumerable<int>)) // Parse array of int
                        {
                            var list = new List<int>();

                            while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                i++;
                                list.Add(Convert.ToInt32(args[i]));
                            }

                            attr.Item1.SetValue(obj, list);
                        }

                        break;
                    }

                }

                if(!found)
                {
                    attr.Item1.SetValue(obj, attr.Item2.Default);
                }
            }
        }

        /// <summary>
        /// Generic help - Show which verbs can be used
        /// </summary>
        private static string GenHelp()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var sb       = new StringBuilder();

            sb.AppendLine(assembly.Name + " " + assembly.Version + "\n");

            int longerVerbLength = 0;

            foreach (var verb in Verbs.Registered)
            {
                if(verb.Name.Length > longerVerbLength)
                {
                    longerVerbLength = verb.Name.Length;
                }
            }

            foreach (var verb in Verbs.Registered)
            {
                int lengthDiff = longerVerbLength - verb.Name.Length;
                sb.AppendLine("  " + verb.Name + new string(' ', 4 + lengthDiff) + verb.HelpText);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Specific help - Show what args are declared in parsing target T
        /// </summary>
        private static string GenHelp<T>() where T : class
        {
            var optionAttrs          = GetOptionAttributes<T>();
            var assembly             = Assembly.GetExecutingAssembly().GetName();
            var sb                   = new StringBuilder();
            var attrInfos            = new List<string>();
            var attrHelpTexts        = new List<string>();
            int longerAttrInfoLength = 0;

            foreach (var attr in optionAttrs)
            {
                var type      = attr.Item1.PropertyType;
                var shortName = attr.Item2.ShortName;
                var name      = attr.Item2.Name;

                sb.Append("--" + name);

                if (shortName != null)
                {
                    sb.Append(", -" + shortName);
                }

                var attrInfo = sb.ToString();

                if(attrInfo.Length > longerAttrInfoLength)
                {
                    longerAttrInfoLength = attrInfo.Length;
                }

                attrInfos.Add(attrInfo);
                attrHelpTexts.Add(attr.Item2.HelpText);

                sb.Clear();
            }

            sb.AppendLine(assembly.Name + " " + assembly.Version + "\n");

            for (int i=0; i<attrInfos.Count; i++)
            {
                int lengthDiff = longerAttrInfoLength - attrInfos[i].Length;
                sb.AppendLine("  " + attrInfos[i] + new string(' ', 4 + lengthDiff) + attrHelpTexts[i]);
            }

            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Helper method to show help (generic or specific)
        /// </summary>
        private static void ShowHelp(string msg = null)
        {
            if(msg == null)
            {
                msg = GenHelp();
            }

            var logForm     = new LogForm();
            logForm.Text    = "Help - CodeWalker by dexyfex";
            logForm.Content = msg;

            logForm.FormClosed += (object sender, FormClosedEventArgs e) => { Application.Exit(); };

            Application.Run(logForm);
        }

        private static bool EnsureGTAFolder()
        {
            string fldr = Settings.Default.GTAFolder;
            if (string.IsNullOrEmpty(fldr) || !Directory.Exists(fldr))
            {
                if (!ChangeGTAFolder())
                {
                    return false;
                }
                fldr = Settings.Default.GTAFolder;
            }
            if (!Directory.Exists(fldr))
            {
                MessageBox.Show("The specified folder does not exist:\n" + fldr);
                return false;
            }
            if (!File.Exists(fldr + "\\gta5.exe"))
            {
                MessageBox.Show("GTA5.exe not found in folder:\n" + fldr);
                return false;
            }
            Settings.Default.GTAFolder = fldr; //seems ok, save it for later
            return true;
        }

        private static bool ChangeGTAFolder()
        {
            SelectFolderForm f = new SelectFolderForm();
            f.ShowDialog();
            if (f.Result == DialogResult.OK)
            {
                Settings.Default.GTAFolder = f.SelectedFolder;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}