using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Windows.Interop;
using Microsoft.Win32;
using Newtonsoft.Json.Serialization;


namespace CleanUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static bool FirstActivation = true;
        private Settings FSettings;
        private MainWindowViewModel ViewModel;
        private Dictionary<string, Command> ValidCommands = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);
        private List<String> AutocompleteList = new List<String>();
        private List<String> MatchesList = new List<String>();
        private int MatchIndex = 0;
        private bool MultipleAutocompleteOptions = false;
        private List<String> ProgramList = new List<String>();
        private Dictionary<String, String> ProgramPaths = new Dictionary<String, String>(StringComparer.InvariantCultureIgnoreCase);
        private bool ClearOnClick = true;
        private bool recentLaunch = false;
        /*
         *  recentLaunch var is being used to a strange event that kept happening: 
         *  - When running the RunCommand method upon pressing enter and being a valid command/program, if Process.Start() was used to run
         *  the program typed in then the RunCommand method would be somehow ran again. If Process.Start() was replaced with a simple Console.WriteLine()
         *  with the same arguments input, it would only run once (as it is supposed to). To counteract this weird double-running, after stepping through
         *  every single line, the only thing I can do is leave a boolean temporarily set to counteract the second running of it on that specific method being run.
         *  You will see this at the top of the KeyDown event for CommandTb.
         */


        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
        [In] IntPtr hWnd,
        [In] int id,
        [In] uint fsModifiers,
        [In] uint vk
        );

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        private string ConfigPath 
        { 
            get
            { 
                var folderPath = System.IO.File.Exists(Constants.UserConfigPath) ? Constants.UserConfigPath : Constants.DefaultConfigPath;
                MessageBox.Show("Exists:" + System.IO.File.Exists(Constants.UserConfigPath) + folderPath);
                
                return System.IO.Path.Combine(folderPath, "settings.json");
            }
        }

        private string MsSettingsPath
        {
            get
            {
                var folderPath = System.IO.File.Exists(Constants.UserConfigPath) ? Constants.UserConfigPath : Constants.DefaultConfigPath;

                return System.IO.Path.Combine(folderPath, "ms-settings.txt");
            }
        }

        private string StylePath
        {
            get
            {
                var folderPath = System.IO.File.Exists(Constants.UserConfigPath) ? Constants.UserConfigPath : Constants.DefaultConfigPath;

                return System.IO.Path.Combine(folderPath, "style.json");
            }
        }

        public MainWindow()
        {
            RefreshSettings();
            RefreshStyle();

            this.DataContext = ViewModel;


            InitializeComponent();
            this.Activated += new EventHandler(CommandTb_GotFocus);
            this.Deactivated += new EventHandler(CommandTb_LostFocus);
            CommandTb.GotFocus += new RoutedEventHandler(CommandTb_GotFocus);
            CommandTb.LostFocus += new RoutedEventHandler(CommandTb_LostFocus);


            foreach (string folder in FSettings.AppFolders) 
            {
                string[] files;
                try
                {
                    files = Directory.GetFiles(folder);
                } catch
                {
                    CommandTb.Text = "Couldn't load folder: " + folder;
                    CommandError();
                    files = new string[0];
                }

                foreach (string file in files) { // Add all the programs in their folders to the autocomplete & valid commands list.
                    Command toAdd = new Command();
                    toAdd.Name = System.IO.Path.GetFileNameWithoutExtension(file);
                    toAdd.Actions = new List<Dictionary<String, String>>();
                    toAdd.Actions.Add(new Dictionary<string, string> {
                        { "PROCESS", file }
                    });
                    toAdd.Icon = "Apps";
                    ValidCommands.Add(toAdd.Name, toAdd);
                    AutocompleteList.Add(toAdd.Name);
                }
            }
            

            foreach (Command cmd in FSettings.Commands)
            {
                ValidCommands.Add(cmd.Name, cmd);
                AutocompleteList.Add(cmd.Name);
            }

            string UserStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs"; // Also automatically add all startmenu programs
            string StartMenuPath = System.IO.Path.GetPathRoot(Environment.SystemDirectory) + @"ProgramData\Microsoft\Windows\Start Menu\Programs";

            ProgramList.AddRange(Directory.GetFiles(StartMenuPath).Where(x => x.EndsWith("lnk")).ToList<string>());
            ProgramList.AddRange(Directory.GetFiles(UserStartMenuPath).Where(x => x.EndsWith("lnk")).ToList<string>());

            List<DirectoryInfo> StartMenuDirs = new DirectoryInfo(StartMenuPath).GetDirectories().Where(x => (x.Attributes & FileAttributes.Hidden) == 0).ToList<DirectoryInfo>();
            StartMenuDirs.AddRange(new DirectoryInfo(UserStartMenuPath).GetDirectories().Where(x => (x.Attributes & FileAttributes.Hidden) == 0).ToList<DirectoryInfo>());

            foreach (DirectoryInfo dir in StartMenuDirs)
            {
                ProgramList.AddRange(Directory.GetFiles(dir.FullName).Where(x => (x.Split(' ').Length < 5 && x.EndsWith("lnk") ) ) ); // Short-named program shortcuts to make sure the autocomplete isn't too long

                
            }



            foreach (string prog in ProgramList)
            {
                ProgramPaths[System.IO.Path.GetFileNameWithoutExtension(prog).ToString().Trim()] = prog; // Add file name (without extension) and file path to dict
                AutocompleteList.Add(System.IO.Path.GetFileNameWithoutExtension(prog).ToString().Trim());
            }

        }

        public void RefreshSettings()
        {
            try
            {
                FSettings = JsonConvert.DeserializeObject<Settings>(System.IO.File.ReadAllText(ConfigPath));
            }
            catch (Exception e)
            {
                MessageBox.Show("SpotlightX couldn't load the config/settings.json file, is it valid JSON? Redownload it or fix any JSON formatting errors. Exception: " + e);
                Application.Current.Shutdown();
            }
        }

        public void RefreshStyle()
        {
            try
            {
                ViewModel = JsonConvert.DeserializeObject<MainWindowViewModel>(System.IO.File.ReadAllText(StylePath));
            }
            catch (Exception e)
            {
                MessageBox.Show("SpotlightX couldn't load the config/style.json file, is it valid JSON? Redownload it or fix any JSON formatting errors. Exception: " + e);
                Application.Current.Shutdown();
            }
        }

        public void CommandTb_GotFocus(object sender, EventArgs e)
        {

            ClearAutocompleteOptions();
            if (!FirstActivation)
            {
                if (ClearOnClick)
                {
                    CommandTb.Text = "";
                }
            }
            else
            {
                FirstActivation = false;
            }
        }

        public void CommandTb_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommandTb.Text))
            {
                CommandTb.Text = "Type a command...";
                ClearOnClick = true;
            }
            this.Hide();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();

        }

        private void EnterCommand(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) // Escape, close the window
            {
                this.Hide();
            } else if (MultipleAutocompleteOptions && e.Key != Key.Tab)
            {
                ClearAutocompleteOptions();
            }
        }

        private void Autocomplete()
        {
            if (MultipleAutocompleteOptions)
            {
                MatchIndex++;
                if (MatchIndex == MatchesList.Count) MatchIndex = 0;
                CommandTb.Text = MatchesList[MatchIndex] + " ";
                changeIcon();
            }
            else
            {
                if (CommandTb.Text.Split(' ').Length == 1)
                {
                    MatchesList = AutocompleteList.Where(x => x.ToLower().StartsWith(CommandTb.Text.ToLower())).ToList();
                    MatchesList.Sort();
                    MatchesList = MatchesList.Distinct().ToList();
                    if (MatchesList.Count > 0)
                    {
                        CommandTb.Text = MatchesList[0] + " ";
                        changeIcon();
                        if (MatchesList.Count > 1)
                        {
                            MultipleAutocompleteOptions = true;
                            MatchIndex = 0;
                        }
                    }
                }
                else // Autocompleting an argument, not a command
                {
                    try
                    {
                        Command thisCommand = ValidCommands[CommandTb.Text.Split(' ')[0]];
                        if (thisCommand.Name == "settings") // Autocomplete Settings args
                        {
                            String argument = CommandTb.Text.Split(' ')[1];
                            List<string> autoLines = System.IO.File.ReadAllLines(MsSettingsPath).Where(settingLine => settingLine.Substring(12).StartsWith(argument.ToLower())).ToList<string>();
                            // Only load into memory when needed, and it's not a large file - just a list of settings pages.
                            if (autoLines.Count > 0)
                            {
                                // Replace unfinished argument with full one
                                CommandTb.Text = CommandTb.Text.Split(' ')[0] + " " + autoLines[0].Substring(12);
                            }
                        }
                    }
                    catch { }
                }
            }
            CommandTb.Select(CommandTb.Text.Length, 0); // move cursor to end of textbox
        }

        private void ClearAutocompleteOptions()
        {
            MatchesList.Clear();
            MultipleAutocompleteOptions = false;
            MatchIndex = 0;
        }

        private void RunCommand(string text)
        {
            string[] SplitCommand = SplitArgs(CommandTb.Text);

            if (ValidCommands.ContainsKey(SplitCommand[0].Trim()) || ProgramPaths.ContainsKey(text.Trim()) || ValidCommands.ContainsKey(text.Trim())) // If it is either a command word (e.g 'search'), a program name, or file
            {
                try
                {
                    this.Hide();

                    Command toRun;
                    if (ValidCommands.ContainsKey(SplitCommand[0].Trim())) toRun = ValidCommands[SplitCommand[0].ToLower()]; // The command is a command word ('search')
                    else toRun = ValidCommands[text.Trim()]; // The command is to open a file 

                    foreach (Dictionary<string, string> action in toRun.Actions)
                    {
                        CompleteAction(action.ElementAt(0).Key, action.ElementAt(0).Value);
                    }
                    CommandTb.Text = "Type a command...";

                }
                catch
                {
                    recentLaunch = true;
                    this.Hide();
                    CommandTypeIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Apps;
                    Process.Start(ProgramPaths[text.Trim()]);
                    CommandTypeIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.MicrosoftWindows;
                    CommandTb.Text = "Type a command...";
                }
                ClearOnClick = true;
            }
            else
            {
                CommandError();
            }
        }

        private void CommandError()
        {
            CommandTypeIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Exclamation;
        }

        private void CompleteAction(string type, string arguments)
        {
            arguments = StringArgsToArgs(arguments, type); // Replace all _arg1_ and _allargs_ vars to their values

            // General Actions
            if (type == "SEARCH")
            {
                Process.Start("https://www.google.com/search?q=" + Uri.EscapeDataString(StringArgsToArgs(arguments, type)));
            } else if (type == "EXIT")
            {
                System.Windows.Application.Current.Shutdown();
            }
            else if (type == "PROCESS")
            {
                Process.Start(arguments.Trim());
            }

            // App-Specific Actions
            else if (type == "ADDPATH")
            {
                FSettings.AppFolders.Add(arguments);
                System.IO.File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(FSettings, Formatting.Indented)); // Append path to settings.json
                Restart();
            }
            else if (type == "REMOVEPATH")
            {
                FSettings.AppFolders.Remove(arguments);
                System.IO.File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(FSettings, Formatting.Indented)); // Append path to settings.json
                Restart();
            }
            else if (type == "RELOAD")
            {
                Restart(); // Restart THIS program, not the whole computer - this is why the command is 'reload' not 'restart'
            }

            // Customisation Actions
            else if (type == "BACKGROUND")
            {
                string[] Gradients = arguments.Split(' '); // To save the operation being done 4 times on top of the other splits

                // Colours
                ViewModel.G1Col = Gradients[0].Split(':')[0];
                ViewModel.G2Col = Gradients[1].Split(':')[0];


                // Offsets
                ViewModel.G1Offset = Gradients[0].Split(':')[1];
                ViewModel.G2Offset = Gradients[1].Split(':')[1];


                System.IO.File.WriteAllText(StylePath, JsonConvert.SerializeObject(ViewModel, Formatting.Indented)); // Append path to style.json
                Restart();
            }
            else if (type == "OPACITY")
            {
                ViewModel.Opacity = arguments;
                System.IO.File.WriteAllText(StylePath, JsonConvert.SerializeObject(ViewModel, Formatting.Indented)); // Append path to style.json
                Restart();
            }
            else if (type == "TEXTCOL")
            {
                ViewModel.TextCol = arguments;
                System.IO.File.WriteAllText(StylePath, JsonConvert.SerializeObject(ViewModel, Formatting.Indented)); // Append path to style.json
                Restart();
            }

        }

        public void Restart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        public string StringArgsToArgs(String arguments, String type) // type = command type, e.g PROCESS, SEARCH, so on. This is given to remove it from _allargs_ param
        {
            string[] SplitCommand = SplitArgs(CommandTb.Text); // split up args
            int index = 1; // 1st arg
            string pattern = "";

            foreach (string arg in SplitCommand)
            {
                string argnum = "_arg" + index.ToString() + "_";
                pattern = @"\b" + argnum + @"\b";
                arguments = Regex.Replace(arguments, pattern, arg, RegexOptions.IgnoreCase);
                index++;
            }

            string allargs = @"_allargs_";
            pattern = @"\b" + allargs + @"\b";
            arguments = Regex.Replace(arguments, pattern, CommandTb.Text.Substring(type.Length), RegexOptions.IgnoreCase);

            return arguments;
        }

        private void CommandTb_KeyDown(object sender, KeyEventArgs e)
        {
            ClearOnClick = false;
            if (recentLaunch) recentLaunch = false;
            else
            {
                if (e.Key == Key.Enter) // Enter the command, run it
                {
                    RunCommand(CommandTb.Text);
                }
                else if (e.Key == Key.Tab)
                { // Tab to autocomplete word if possible
                    Autocomplete();
                }
                else if (e.Key == Key.Space)
                {
                    changeIcon();
                }
            }
        }

        private void changeIcon()
        {
            string[] SplitCommand = SplitArgs(CommandTb.Text);

            if (ValidCommands.ContainsKey(SplitCommand[0].ToLower()))
            {
                try
                {
                    CommandTypeIcon.Kind = (MahApps.Metro.IconPacks.PackIconMaterialKind)Enum.Parse(typeof(MahApps.Metro.IconPacks.PackIconMaterialKind), ValidCommands[SplitCommand[0].ToLower()].Icon);
                    // This is ugly, but essentially it converts a string of an icon (e.g "Rocket") to MahApps.Metro.IconPacks.PackIconMaterialKind.Rocket and sets that as the new icon
                }
                catch (Exception e) { }
            }
            else if (ProgramPaths.ContainsKey(CommandTb.Text.Trim())) // Startmenu program
            {
                CommandTypeIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ArrowUpCircle;
            }
        }

        private void CommandTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!FirstActivation)
            {

                string[] SplitCommand = SplitArgs(CommandTb.Text);

                if (!ValidCommands.ContainsKey(SplitCommand[0].ToLower()))
                {
                    CommandTypeIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.MicrosoftWindows;
                }
            }
        }

        private String[] SplitArgs(String text)
        {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None); // Get rid of extra ' ' and replace it with only one space
            string command = regex.Replace(text, " ");
            String[] SplitCommand = command.Split(' '); // Split cmd args
            return SplitCommand;
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            const uint VK_S = 0x53;
            const uint MOD_ALT = 0x0001;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_ALT, VK_S))
            {
                MessageBox.Show("SpotlightX couldn't register hotkey, closing application. Is the app already running?");
                Application.Current.Shutdown();
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            this.Show();
            this.Activate();
            CommandTb.Focus();
        }

    }
}
