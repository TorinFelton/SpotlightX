using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CleanUI.Exceptions;
using CleanUI.Extensions;
using CleanUI.Interfaces;
using CleanUI.Settings;
using MahApps.Metro.IconPacks;

namespace CleanUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        private readonly ISettingsSaver _settingsSaver;
        private static bool _firstActivation = true;
        private readonly UiSettings _fUiSettings;
        private readonly Dictionary<string, Command> _validCommands;
        private readonly List<string> _autocompleteList;
        private List<string> _matchesList;
        private int _matchIndex;

        private bool _multipleAutocompleteOptions;

        //private List<string> _programList;
        private readonly Dictionary<string, string> _programPaths;

        private readonly string _configPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                                              @"\AppData\Roaming\Microsoft\Windows\Start Menu\SpotlightXConfig\config\";

        private bool _clearOnClick = true;

        private bool _recentLaunch;
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
        // ReSharper disable once IdentifierTypo
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        private const int HOTKEY_ID = 9000;


        public MainWindow(ISettingsLoader settingsLoader, ISettingsSaver settingsSaver, ICommandLoader commandLoader,
            IProgramListLoader programListLoader)
        {
            if (settingsLoader == null) throw new ArgumentNullException(nameof(settingsLoader));
            if (commandLoader == null) throw new ArgumentNullException(nameof(commandLoader));
            if (programListLoader == null) throw new ArgumentNullException(nameof(programListLoader));
            _settingsSaver = settingsSaver ?? throw new ArgumentNullException(nameof(settingsSaver));

            _validCommands = new Dictionary<string, Command>(StringComparer.CurrentCultureIgnoreCase);
            _autocompleteList = new List<string>();
            _programPaths = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            //_programList = new List<string>();

            InitializeComponent();
            Activated += CommandTb_GotFocus;
            Deactivated += CommandTb_LostFocus;
            CommandTb.GotFocus += CommandTb_GotFocus;
            CommandTb.LostFocus += CommandTb_LostFocus;

            try
            {
                _fUiSettings = settingsLoader.LoadSettings();
            }
            catch (SettingsLoadException e)
            {
                MessageBox.Show(
                    "Couldn't load the config/settings.json file, is it valid JSON? Re-download it or fix any JSON formatting errors.\nException: " +
                    e.BuildException());
                Application.Current.Shutdown();
            }

            foreach (var toAdd in commandLoader.GetCommands(_fUiSettings))
            {
                try
                {
                    _validCommands.Add(toAdd.Name, toAdd);
                    _autocompleteList.Add(toAdd.Name);
                }
                catch (CommandLoadException e)
                {
                    MessageBox.Show("Unable to load command.\nException is: " + e.BuildException());
                    CommandError();
                }
            }

            foreach (var cmd in _fUiSettings.Commands)
            {
                _validCommands.Add(cmd.Name, cmd);
                _autocompleteList.Add(cmd.Name);
            }

            foreach (var program in programListLoader.GetProgramList())
            {
                _programPaths[Path.GetFileNameWithoutExtension(program).Trim()] = program;
                _autocompleteList.Add(Path.GetFileNameWithoutExtension(program).Trim());
            }
        }

        public void CommandTb_GotFocus(object sender, EventArgs e)
        {
            ClearAutocompleteOptions();
            if (!_firstActivation)
            {
                if (_clearOnClick)
                {
                    CommandTb.Text = "";
                }
            }
            else
            {
                _firstActivation = false;
            }
        }

        public void CommandTb_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommandTb.Text))
            {
                CommandTb.Text = "Type a command...";
                _clearOnClick = true;
            }

            Hide();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void EnterCommand(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) // Escape, close the window
            {
                Hide();
            }
            else if (_multipleAutocompleteOptions && e.Key != Key.Tab)
            {
                ClearAutocompleteOptions();
            }
        }

        private void Autocomplete()
        {
            if (_multipleAutocompleteOptions)
            {
                _matchIndex++;
                if (_matchIndex == _matchesList.Count) _matchIndex = 0;
                CommandTb.Text = _matchesList[_matchIndex] + " ";
                ChangeIcon();
            }
            else
            {
                if (CommandTb.Text.Split(' ').Length == 1)
                {
                    _matchesList = _autocompleteList.Where(x => x.ToLower().StartsWith(CommandTb.Text.ToLower()))
                        .ToList();
                    _matchesList.Sort();
                    _matchesList = _matchesList.Distinct().ToList();
                    if (_matchesList.Count > 0)
                    {
                        CommandTb.Text = _matchesList[0] + " ";
                        ChangeIcon();
                        if (_matchesList.Count > 1)
                        {
                            _multipleAutocompleteOptions = true;
                            _matchIndex = 0;
                        }
                    }
                }
                else // Auto-completing an argument, not a command
                {
                    try
                    {
                        Command thisCommand = _validCommands[CommandTb.Text.Split(' ')[0]];
                        if (thisCommand.Name == "settings") // Autocomplete Settings args
                        {
                            string argument = CommandTb.Text.Split(' ')[1];
                            List<string> autoLines = File.ReadAllLines(_configPath + "ms-settings.txt")
                                .Where(settingLine => settingLine.Substring(12).StartsWith(argument.ToLower()))
                                .ToList();
                            // Only load into memory when needed, and it's not a large file - just a list of settings pages.
                            if (autoLines.Count > 0)
                            {
                                // Replace unfinished argument with full one
                                CommandTb.Text = CommandTb.Text.Split(' ')[0] + " " + autoLines[0].Substring(12);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            CommandTb.Select(CommandTb.Text.Length, 0); // move cursor to end of text box
        }

        private void ClearAutocompleteOptions()
        {
            if (_matchesList == null) _matchesList = new List<string>();
            _matchesList.Clear();
            _multipleAutocompleteOptions = false;
            _matchIndex = 0;
        }

        private void RunCommand(string text)
        {
            string[] splitCommand = SplitArgs(CommandTb.Text);

            if (_validCommands.ContainsKey(splitCommand[0].Trim()) || _programPaths.ContainsKey(text.Trim()))
            {
                try
                {
                    Hide();


                    foreach (var action in _validCommands[splitCommand[0].ToLower()].Actions)
                    {
                        CompleteAction(action.ElementAt(0).Key, action.ElementAt(0).Value);
                    }

                    CommandTb.Text = "Type a command...";
                }
                catch
                {
                    _recentLaunch = true;
                    Hide();
                    CommandTypeIcon.Kind = PackIconMaterialKind.Apps;
                    Process.Start(_programPaths[text.Trim()]);
                    CommandTypeIcon.Kind = PackIconMaterialKind.MicrosoftWindows;
                    CommandTb.Text = "Type a command...";
                }

                _clearOnClick = true;
            }
            else
            {
                CommandError();
            }
        }

        private void CommandError()
        {
            CommandTypeIcon.Kind = PackIconMaterialKind.Exclamation;
        }

        private void CompleteAction(string type, string arguments)
        {
            arguments = StringArgsToArgs(arguments, type); // Replace all _arg1_ and _allargs_ vars to their values

            switch (type)
            {
                case "SEARCH":
                    Process.Start("https://www.google.com/search?q=" +
                                  Uri.EscapeDataString(StringArgsToArgs(arguments, type)));
                    break;
                case "EXIT":
                    Application.Current.Shutdown();
                    break;
                case "PROCESS":
                    Process.Start(arguments.Trim());
                    break;
                case "ADDPATH":
                    _fUiSettings.AppFolders.Add(StringArgsToArgs(arguments, type));
                    _settingsSaver.SaveSettings(_fUiSettings);
                    Restart();
                    break;
                case "REMOVEPATH":
                    _fUiSettings.AppFolders.Remove(StringArgsToArgs(arguments, type));
                    _settingsSaver.SaveSettings(_fUiSettings);
                    Restart();
                    break;
            }
        }

        private static void Restart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private string
            StringArgsToArgs(string arguments,
                string type) // type = command type, e.g PROCESS, SEARCH, so on. This is given to remove it from _allargs_ param
        {
            string[] splitCommand = SplitArgs(CommandTb.Text); // split up args
            int index = 1; // 1st arg
            string pattern;

            foreach (var arg in splitCommand)
            {
                string argNum = "_arg" + index + "_";
                pattern = @"\b" + argNum + @"\b";
                arguments = Regex.Replace(arguments, pattern, arg, RegexOptions.IgnoreCase);
                index++;
            }

            string allargs = @"_allargs_";
            pattern = @"\b" + allargs + @"\b";
            arguments = Regex.Replace(arguments, pattern, CommandTb.Text.Substring(type.Length),
                RegexOptions.IgnoreCase);

            return arguments;
        }

        private void CommandTb_KeyDown(object sender, KeyEventArgs e)
        {
            _clearOnClick = false;
            if (_recentLaunch) _recentLaunch = false;
            else
            {
                switch (e.Key)
                {
                    // Enter the command, run it
                    case Key.Enter:
                        RunCommand(CommandTb.Text);
                        break;
                    case Key.Tab:
                        // Tab to autocomplete word if possible
                        Autocomplete();
                        break;
                    case Key.Space:
                        ChangeIcon();
                        break;
                }
            }
        }

        private void ChangeIcon()
        {
            string[] splitCommand = SplitArgs(CommandTb.Text);

            if (_validCommands.ContainsKey(splitCommand[0].ToLower()))
            {
                try
                {
                    CommandTypeIcon.Kind = (PackIconMaterialKind) Enum.Parse(typeof(PackIconMaterialKind),
                        _validCommands[splitCommand[0].ToLower()].Icon);
                    // This is ugly, but essentially it converts a string of an icon (e.g "Rocket") to MahApps.Metro.IconPacks.PackIconMaterialKind.Rocket and sets that as the new icon
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            else if (_programPaths.ContainsKey(CommandTb.Text.Trim())) // Start menu program
            {
                CommandTypeIcon.Kind = PackIconMaterialKind.ArrowUpCircle;
            }
        }

        private void CommandTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_firstActivation) return;
            string[] splitCommand = SplitArgs(CommandTb.Text);

            if (!_validCommands.ContainsKey(splitCommand[0].ToLower()))
            {
                CommandTypeIcon.Kind = PackIconMaterialKind.MicrosoftWindows;
            }
        }

        private string[] SplitArgs(string text)
        {
            var regex = new Regex("[ ]{2,}",
                RegexOptions.None); // Get rid of extra ' ' and replace it with only one space
            string command = regex.Replace(text, " ");
            string[] splitCommand = command.Split(' '); // Split cmd args
            return splitCommand;
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            Debug.Assert(_source != null, nameof(_source) + " != null");
            _source.AddHook(HWndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HWndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            const uint VK_S = 0x53;
            const uint MOD_ALT = 0x0001;
            if (RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_ALT, VK_S)) return;
            MessageBox.Show("Couldn't register hot-key, closing application.");
            Application.Current.Shutdown();
        }

        // ReSharper disable once IdentifierTypo
        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private IntPtr HWndHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
            Show();
            Activate();
            CommandTb.Focus();
        }
    }
}