using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Ched.Core.Notes;
using Ched.Core;
using Ched.Core.Events;
using Ched.Configuration;
using Ched.Localization;
using Ched.Plugins;
using Ched.Properties;
using Ched.UI.Shortcuts;
using Ched.UI.Operations;
using Ched.UI.Windows;
using System.Globalization;
using Ched.Drawing;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Runtime.Versioning;
using System.Windows.Controls;

namespace Ched.UI
{
    public partial class MainForm : Form
    {
        private event EventHandler PreviewModeChanged;
        private event EventHandler SettingChanged;


        private readonly string UserShortcutKeySourcePath = "keybindings.json";

        private readonly string FileExtension = ".chs";

        private string FileTypeFilter => FileFilterStrings.ChedFilter + string.Format("({0})|{1}", "*" + FileExtension, "*" + FileExtension);

        private bool isPreviewMode;

        private ScoreBook ScoreBook { get; set; }
        private OperationManager OperationManager { get; }

        private ScrollBar NoteViewScrollBar { get; }
        private NoteView NoteView { get; }

        private SoundPreviewManager PreviewManager { get; set; }
        private SoundSource CurrentMusicSource;

        private ShortcutManagerHost ShortcutManagerHost { get; }
        private ShortcutManager ShortcutManager => ShortcutManagerHost.ShortcutManager;

        private ExportManager ExportManager { get; } = new ExportManager();

        private int Channel { get; set; } = 1;
        private int ViewChannel { get; set; } = 0;
        private int Stage { get; set; } = 0;
        private int ViewStage { get; set; } = 0;

        private float WidthAmount { get; set; } = 1;
        private float ScrollAmount { get; set; } = ApplicationSettings.Default.ScrollAmount;

        private bool LaneVisual { get; set; } = false;

        public bool FormSpeedbyCh { get; set; } = ApplicationSettings.Default.IsAnotherChannelFormSpeeds;

        private int defaultCh = 1;

        private Plugins.PluginManager PluginManager { get; } = Plugins.PluginManager.GetInstance();

        static System.Data.DataTable _dt = new System.Data.DataTable();

        static decimal Calculate1(string str)
        {
            string s = _dt.Compute("1.0 *" + str, "").ToString();
            return decimal.Parse(s.ToString());
            //https://lets-csharp.com/string-number-calculator/ より
        }

        private bool IsPreviewMode
        {
            get { return isPreviewMode; }
            set
            {
                isPreviewMode = value;
                NoteView.Editable = CanEdit;
                NoteView.LaneBorderLightColor = isPreviewMode ? Color.FromArgb(40, 40, 40) : Color.FromArgb(60, 60, 60);
                NoteView.LaneBorderDarkColor = isPreviewMode ? Color.FromArgb(10, 10, 10) : Color.FromArgb(30, 30, 30);
                NoteView.UnitLaneWidth = isPreviewMode ? 4 : ApplicationSettings.Default.UnitLaneWidth;
                NoteView.ShortNoteHeight = isPreviewMode ? 4 : 5;
                NoteView.UnitBeatHeight = isPreviewMode ? 48 : ApplicationSettings.Default.UnitBeatHeight;
                UpdateThumbHeight();
                PreviewModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool CanWidenLaneWidth => !IsPreviewMode && NoteView.UnitLaneWidth < 60;
        private bool CanNarrowLaneWidth => !IsPreviewMode && NoteView.UnitLaneWidth > 4;
        private bool CanZoomIn => !IsPreviewMode && NoteView.UnitBeatHeight < 9600;
        private bool CanZoomOut => !IsPreviewMode && NoteView.UnitBeatHeight > 30;
        private bool CanEdit => !IsPreviewMode && !PreviewManager.Playing;

        public MainForm()
        {
            InitializeComponent();
            Size = new Size(420, 700);
            Icon = Resources.MainIcon;

            ToolStripManager.RenderMode = ToolStripManagerRenderMode.System;

            OperationManager = new OperationManager();
            OperationManager.OperationHistoryChanged += (s, e) =>
            {
                SetText(ScoreBook.Path);
                NoteView.Invalidate();
            };
            OperationManager.ChangesCommitted += (s, e) => SetText(ScoreBook.Path);

            NoteView = new NoteView(OperationManager)
            {
                Dock = DockStyle.Fill,
                UnitBeatHeight = ApplicationSettings.Default.UnitBeatHeight,
                UnitLaneWidth = ApplicationSettings.Default.UnitLaneWidth,
                InsertAirWithAirAction = ApplicationSettings.Default.InsertAirWithAirAction,
                SelectMethod = ApplicationSettings.Default.SelectMethod,

            };

            PreviewManager = new SoundPreviewManager(this);
            PreviewManager.IsStopAtLastNote = ApplicationSettings.Default.IsPreviewAbortAtLastNote;
            PreviewManager.TickUpdated += (s, e) => NoteView.CurrentTick = e.Tick;
            PreviewManager.ExceptionThrown += (s, e) => MessageBox.Show(this, ErrorStrings.PreviewException, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);

            var commandSource = new ShortcutCommandSource();
            SetupCommands(commandSource);
            var shortcutManager = new ShortcutManager()
            {
                DefaultKeySource = new DefaultShortcutKeySource(),
                CommandSource = commandSource
            };
            ShortcutManagerHost = new ShortcutManagerHost(shortcutManager);
            ShortcutManagerHost.UserShortcutKeySource = LoadUserShortcutKeySource();

            NoteViewScrollBar = new VScrollBar()
            {
                Dock = DockStyle.Right,
                Minimum = -NoteView.UnitBeatTick * 4 * 20,
                SmallChange = NoteView.UnitBeatTick,
            };

            void processScrollBarRangeExtension(ScrollBar s)
            {

                if (NoteViewScrollBar.Value < NoteViewScrollBar.Minimum * 0.9f)
                {
                    NoteViewScrollBar.Minimum = (int)(NoteViewScrollBar.Minimum * 1.2);
                }
                
            }

            NoteView.Resize += (s, e) => UpdateThumbHeight();

            NoteView.MouseWheel += (s, e) =>
            {
                int value = NoteViewScrollBar.Value - e.Delta / 120 * NoteViewScrollBar.SmallChange;
                NoteViewScrollBar.Value = Math.Min(Math.Max(value, NoteViewScrollBar.Minimum), NoteViewScrollBar.GetMaximumValue());
                processScrollBarRangeExtension(NoteViewScrollBar);
            };

            NoteView.DragScroll += (s, e) =>
            {
                NoteViewScrollBar.Value = Math.Max(-NoteView.HeadTick, NoteViewScrollBar.Minimum);
                processScrollBarRangeExtension(NoteViewScrollBar);
            };

            NoteViewScrollBar.ValueChanged += (s, e) =>
            {
                NoteView.HeadTick = -NoteViewScrollBar.Value / 60 * 60; // 60の倍数できれいに表示されるので…
                NoteView.Invalidate();
            };

            NoteViewScrollBar.Scroll += (s, e) =>
            {
                if (e.Type == ScrollEventType.EndScroll)
                {
                    processScrollBarRangeExtension(NoteViewScrollBar);
                }
            };

            NoteView.NewNoteTypeChanged += (s, e) => NoteView.EditMode = EditMode.Edit;
            NoteView.ScoreChanged += (s, e) => NoteView.UpdateScore(ScoreBook.Score);

            AllowDrop = true;
            DragEnter += (s, e) =>
            {
                e.Effect = DragDropEffects.None;
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var items = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (items.Length == 1 && items.All(p => Path.GetExtension(p) == FileExtension  && File.Exists(p)))
                        e.Effect = DragDropEffects.Copy;
                }
            };
            DragDrop += (s, e) =>
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop)).Single();
                if (!ConfirmDiscardChanges()) return;
                LoadFile(path);
            };

            FormClosing += (s, e) =>
            {
                if (!ConfirmDiscardChanges())
                {
                    e.Cancel = true;
                    return;
                }

                ApplicationSettings.Default.Save();
                File.WriteAllText(UserShortcutKeySourcePath, ShortcutManagerHost.UserShortcutKeySource.DumpShortcutKeys());
            };

            using (var manager = this.WorkWithLayout())
            {
                this.MainMenuStrip = CreateMainMenu(NoteView);
                this.Controls.Add(NoteView);
                this.Controls.Add(NoteViewScrollBar);
                this.Controls.Add(CreateNewNoteTypeToolStrip(NoteView));
                this.Controls.Add(CreateMainToolStrip(NoteView));
                this.Controls.Add(MainMenuStrip);
            }

            NoteView.NewNoteType = NoteType.Tap;
            NoteView.EditMode = EditMode.Edit;

            LoadEmptyBook();
            ShortcutManager.NotifyUpdateShortcut();
            SetText();

            if (!PreviewManager.IsSupported)
                MessageBox.Show(this, ErrorStrings.PreviewNotSupported, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (PluginManager.FailedFiles.Count > 0)
            {
                MessageBox.Show(this, string.Join("\n", new[] { ErrorStrings.PluginLoadError }.Concat(PluginManager.FailedFiles)), Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (PluginManager.InvalidFiles.Count > 0)
            {
                MessageBox.Show(this, string.Join("\n", new[] { ErrorStrings.PluginNotSupported }.Concat(PluginManager.InvalidFiles)), Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public MainForm(string filePath) : this()
        {
            LoadFile(filePath);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ShortcutManager.ExecuteCommand(keyData)) return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected void LoadFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            Console.WriteLine("Mainform loadfile " + extension);
            try
            {

                if (!ScoreBook.IsCompatible(filePath))
                {
                    MessageBox.Show(this, ErrorStrings.FileNotCompatible, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (ScoreBook.IsUpgradeNeeded(filePath))
                {
                    if (MessageBox.Show(this, ErrorStrings.FileUpgradeNeeded, Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return;
                }
                
                LoadBook(ScoreBook.LoadFile(filePath));
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(this, ErrorStrings.FileNotAccessible, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadEmptyBook();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ErrorStrings.FileLoadError, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.DumpExceptionTo(ex, "file_exception.json");
                LoadEmptyBook();
            }
        }

        protected void LoadBook(ScoreBook book)
        {
            ScoreBook = book;
            OperationManager.Clear();
            ExportManager.Load(book);
            NoteView.Initialize(book.Score);
            NoteViewScrollBar.Value = NoteView.PaddingHeadTick;//NoteViewScrollBar.GetMaximumValue();
            NoteViewScrollBar.Minimum = -Math.Max(NoteView.UnitBeatTick * 4 * 20, NoteView.Notes.GetLastTick());
            NoteViewScrollBar.SmallChange = NoteView.UnitBeatTick;
            if(ScoreBook.ChannelNames.Count < 11)
            {
                ScoreBook.ChannelNames = new Dictionary<int, string>()
            {
            { 0, "Ch0" },
            { 1, "Ch1" },
            { 2, "Ch2" },
            { 3, "Ch3" },
            { 4, "Ch4" },
            { 5, "Ch5" },
            { 6, "Ch6" },
            { 7, "Ch7" },
            { 8, "Ch8" },
            { 9, "Ch9" },
            { 10, "Ch10" },
            };
            }
            if (ScoreBook.Stages.Count < 1)
            {
                ScoreBook.Stages = new List<Stage>() { new Stage() { ID = 0, Name = "#0", Number = 0, FromStart = true, UntilEnd = true } };
                ScoreBook.StageCount++;
            }

            NoteView.Stages = ScoreBook.Stages;

            NoteView.Stage = ScoreBook.Stages[0].ID;
            NoteView.ViewStage = -1;

            var defset = new ExportSetting();
            if (ApplicationSettings.Default.DefaultExportSettings == null)
            {
                ApplicationSettings.Default.DefaultExportSettings = new Dictionary<int, IExportSetting>();

                if (ApplicationSettings.Default.DefaultExportSettings.Count < defset.SettingColumns.Count)
                {
                    foreach (var s in defset.SettingColumns.OrderBy(p => p.Key))
                    {

                        if (!ApplicationSettings.Default.DefaultExportSettings.TryGetValue(s.Key, out var set))
                        {
                            s.Value.Value = s.Value.Default;
                            ApplicationSettings.Default.DefaultExportSettings.Add(s.Key, s.Value);
                        }
                         Console.WriteLine(defset.SettingColumns[s.Key].Title + " : " + defset.SettingColumns[s.Key].Value);
                    }
                }
            }
            else
            {
                foreach(var def in ApplicationSettings.Default.DefaultExportSettings)
                {
                    Console.WriteLine(def.Key + " : " + def.Value.Title + " " + def.Value.Value[0] + " " + def.Value.Default[0]);
                }

                if (ApplicationSettings.Default.DefaultExportSettings.Count < defset.SettingColumns.Count)
                {
                    foreach (var s in defset.SettingColumns.OrderBy(p => p.Key))
                    {

                        if (!ApplicationSettings.Default.DefaultExportSettings.TryGetValue(s.Key, out var set))
                        {
                            ApplicationSettings.Default.DefaultExportSettings.Add(s.Key, s.Value);
                        }
                        Console.WriteLine(defset.SettingColumns[s.Key].Title + " : " + defset.SettingColumns[s.Key].Value);
                    }
                }
            }
            


            


            UpdateThumbHeight();
            SetText(book.Path);
            CurrentMusicSource = new SoundSource();
            if (!string.IsNullOrEmpty(book.Path))
            {
                SoundSettings.Default.ScoreSound.TryGetValue(book.Path, out SoundSource src);
                if (src != null)
                {
                    if (src.Volume == 0) src.Volume = 1;
                    CurrentMusicSource = src;
                }
            }
        }

        protected void LoadEmptyBook()
        {
            var book = new ScoreBook();
            var events = book.Score.Events;
            events.BpmChangeEvents.Add(new BpmChangeEvent() { Tick = 0, Bpm = 120 });
            events.TimeSignatureChangeEvents.Add(new TimeSignatureChangeEvent() { Tick = 0, Numerator = 4, DenominatorExponent = 2, Type = -2 });
            book.LaneOffset = ApplicationSettings.Default.LaneOffset;
            book.HP = 1000;

            book.ChannelNames = new Dictionary<int, string>()
            {
            { 0, "Ch0" },
            { 1, "Ch1" },
            { 2, "Ch2" },
            { 3, "Ch3" },
            { 4, "Ch4" },
            { 5, "Ch5" },
            { 6, "Ch6" },
            { 7, "Ch7" },
            { 8, "Ch8" },
            { 9, "Ch9" },
            { 10, "Ch10" },
            };
            
            book.Stages = new List<Stage>() { new Stage() { ID = 0, Name = "#0", Number = 0, FromStart = true, UntilEnd = true } };
            book.StageCount = 1;

            LoadBook(book);
        }

        protected void OpenFile()
        {
            if (!ConfirmDiscardChanges()) return;
            if (!TrySelectOpeningFile(FileTypeFilter, out string path)) return;
            LoadFile(path);
        }

        protected bool TrySelectOpeningFile(string filter, out string path)
        {
            path = null;

            var dialog = new OpenFileDialog()
            {
                Filter = filter
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                path = dialog.FileName;
                return true;
            }
            return false;
        }

        protected void SaveAs()
        {
            var dialog = new SaveFileDialog()
            {
                Filter = FileTypeFilter
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ScoreBook.Path = dialog.FileName;
                SaveFile();
                SetText(ScoreBook.Path);
            }
        }

        protected void SaveFile()
        {
            if (string.IsNullOrEmpty(ScoreBook.Path))
            {
                SaveAs();
                return;
            }
            CommitChanges();
            ScoreBook.Save();
            OperationManager.CommitChanges();

            SoundSettings.Default.ScoreSound[ScoreBook.Path] = CurrentMusicSource;
            SoundSettings.Default.Save();
        }

        protected void ExportAs(IScoreBookExportPlugin exportPlugin)
        {
            if (exportPlugin.ID == 1)
            {
                
                if (ScoreBook.Score.Notes.AirActions.Count > 0) MessageBox.Show(this, ErrorStrings.AirActionInfo, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            var dialog = new SaveFileDialog() { Filter = exportPlugin.FileFilter };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            HandleExport(ScoreBook, ExportManager.PrepareExport(exportPlugin, dialog.FileName));
        }

        private void HandleExport(ScoreBook book, ExportContext context)
        {
            CommitChanges();
            string message;
            bool hasError = true;
            
            try
            {
                context.Export(book);
                message = ErrorStrings.ExportComplete;
                hasError = false;
                ExportManager.CommitExported(context);
            }
            catch (UserCancelledException)
            {
                // Do nothing
                return;
            }
            catch (InvalidTimeSignatureException ex)
            {
                int beatAt = ex.Tick / ScoreBook.Score.TicksPerBeat + 1;
                message = string.Format(ErrorStrings.InvalidTimeSignature, beatAt);
            }
            catch (Exception ex)
            {
                Program.DumpExceptionTo(ex, "export_exception.json");
                message = ErrorStrings.ExportFailed + Environment.NewLine + ex.Message;
            }

            ShowDiagnosticsResult(MainFormStrings.Export, message, hasError, context.Diagnostics);
        }

        protected void HandleImport(IScoreBookImportPlugin plugin, ScoreBookImportPluginArgs args)
        {
            string message;
            bool hasError = true;
            try
            {
                var book = plugin.Import(args);
                LoadBook(book);
                message = ErrorStrings.ImportComplete;
                hasError = false;
            }
            catch (Exception ex)
            {
                Program.DumpExceptionTo(ex, "import_exception.json");
                LoadEmptyBook();
                message = ErrorStrings.ImportFailed + Environment.NewLine + ex.Message;
            }

            ShowDiagnosticsResult(MainFormStrings.Import, message, hasError, args.Diagnostics);
        }

        protected void ShowDiagnosticsResult(string title, string message, bool hasError, IReadOnlyCollection<Diagnostic> diagnostics)
        {
            if (diagnostics.Count > 0)
            {
                var vm = new DiagnosticsWindowViewModel()
                {
                    Title = title,
                    Message = message,
                    Diagnostics = new System.Collections.ObjectModel.ObservableCollection<Diagnostic>(diagnostics)
                };
                var window = new DiagnosticsWindow()
                {
                    DataContext = vm
                };
                window.ShowDialog(this);
            }
            else
            {
                MessageBox.Show(this, message, title, MessageBoxButtons.OK, hasError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
            }
        }

        protected void CommitChanges()
        {
            ScoreBook.Score.Notes = NoteView.Notes.Reposit();
            // Eventsは参照渡ししてますよん
        }

        protected void ClearFile()
        {
            if (!ConfirmDiscardChanges()) return;
            LoadEmptyBook();
        }

        protected bool ConfirmDiscardChanges()
        {
            if (!OperationManager.IsChanged) return true;
            return MessageBox.Show(this, ErrorStrings.FileDiscardConfirmation, Program.ApplicationName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.OK;
        }

        protected void SetText()
        {
            SetText(null);
        }

        protected void SetText(string filePath)
        {
            Text = "Ched SkEdition" + (string.IsNullOrEmpty(filePath) ? "" : " - " + Path.GetFileName(filePath)) + (OperationManager.IsChanged ? " *" : "");
        }

        private void UpdateThumbHeight()
        {
            NoteViewScrollBar.LargeChange = NoteView.TailTick - NoteView.HeadTick;
            NoteViewScrollBar.Maximum = NoteViewScrollBar.LargeChange + NoteView.PaddingHeadTick * 194;
        }

        private void PlayPreview()
        {

            if (string.IsNullOrEmpty(CurrentMusicSource?.FilePath))
            {
                MessageBox.Show(this, ErrorStrings.MusicSourceNull, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(CurrentMusicSource.FilePath))
            {
                MessageBox.Show(this, ErrorStrings.SourceFileNotFound, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (PreviewManager.Playing)
            {
                PreviewManager.Stop();
                return;
            }

            int startTick = NoteView.CurrentTick;
            void lambda(object p, EventArgs q)
            {
                PreviewManager.Finished -= lambda;
                NoteView.CurrentTick = startTick;
                NoteView.Editable = CanEdit;
            }

            try
            {
                CommitChanges();
                var context = new SoundPreviewContext(ScoreBook.Score, CurrentMusicSource, SoundSettings.Default.GuideSound);
                if (ApplicationSettings.Default.IsPJsekaiSounds)
                {
                    context = new SoundPreviewContext(ScoreBook.Score, CurrentMusicSource, SoundSettings.Default.GuideSound, SoundSettings.Default.TapSound, SoundSettings.Default.ExTapSound, SoundSettings.Default.AirSound, SoundSettings.Default.ExAirSound, SoundSettings.Default.TraceSound, SoundSettings.Default.ExTraceSound, SoundSettings.Default.StepSound, SoundSettings.Default.ExStepSound);
                }
                else
                {
                    context = new SoundPreviewContext(ScoreBook.Score, CurrentMusicSource, SoundSettings.Default.GuideSound);
                }
                Console.WriteLine(context);

                if (!PreviewManager.Start(context, startTick, NoteView)) return;
                PreviewManager.Finished += lambda;
                NoteView.Editable = CanEdit;
            }
            catch (Exception ex)
            {
                Program.DumpExceptionTo(ex, "sound_exception.json");
            }
        }

        private UserShortcutKeySource LoadUserShortcutKeySource()
        {
            if (File.Exists(UserShortcutKeySourcePath))
            {
                return new UserShortcutKeySource(File.ReadAllText(UserShortcutKeySourcePath));
            }
            return new UserShortcutKeySource();
        }

        private void SetupCommands(ShortcutCommandSource commandSource)
        {
            commandSource.RegisterCommand(Commands.NewFile, MainFormStrings.NewFile, ClearFile);
            commandSource.RegisterCommand(Commands.OpenFile, MainFormStrings.OpenFile, OpenFile);
            commandSource.RegisterCommand(Commands.Save, MainFormStrings.SaveFile, SaveFile);
            commandSource.RegisterCommand(Commands.SaveAs, MainFormStrings.SaveAs, SaveAs);
            commandSource.RegisterCommand(Commands.ReExport, MainFormStrings.Export, () =>
            {
                if (!ExportManager.CanReExport)
                {
                    if (PluginManager.ScoreBookExportPlugins.Count() == 1)
                    {

                        ExportAs(PluginManager.ScoreBookExportPlugins.Single());
                        return;
                    }
                    MessageBox.Show(this, ErrorStrings.NotExported, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                HandleExport(ScoreBook, ExportManager.PrepareReExport());
            });
            commandSource.RegisterCommand(Commands.SUSExport, "SUS" +MainFormStrings.Export, () =>
            {
                if (!ExportManager.CanReExport)
                {
                    ExportAs(PluginManager.ScoreBookExportPlugins.Where(p => p.FileFilter == "Sliding Universal Score (*.sus)|*.sus").First());
                    return;
                }
                    
                HandleExport(ScoreBook, ExportManager.PrepareReExport());
                
            });
            commandSource.RegisterCommand(Commands.USCExport, "USC" + MainFormStrings.Export, () =>
            {
                if (!ExportManager.CanReExport)
                {
                    MessageBox.Show(this, ErrorStrings.AirActionInfo, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ExportAs(PluginManager.ScoreBookExportPlugins.Where(p => p.FileFilter == "Universal Sekai Chart (*.usc)|*.usc").First());
                    return;
                }

                HandleExport(ScoreBook, ExportManager.PrepareReExport());
            });
            commandSource.RegisterCommand(Commands.ShowScoreBookProperties, MainFormStrings.BookProperty, () =>
            {
                var vm = new BookPropertiesWindowViewModel(ScoreBook, CurrentMusicSource);
                var window = new BookPropertiesWindow() { DataContext = vm };
                window.ShowDialog(this);
            });
            commandSource.RegisterCommand(Commands.ShowShortcutSettings, MainFormStrings.KeyboardShortcuts, () => ConfigureKeyboardShortcut());

            commandSource.RegisterCommand(Commands.Undo, MainFormStrings.Undo, () => { if (OperationManager.CanUndo) OperationManager.Undo(); });
            commandSource.RegisterCommand(Commands.Redo, MainFormStrings.Redo, () => { if (OperationManager.CanRedo) OperationManager.Redo(); });

            commandSource.RegisterCommand(Commands.Cut, MainFormStrings.Cut, () => NoteView.CutSelectedNotes());
            commandSource.RegisterCommand(Commands.Copy, MainFormStrings.Copy, () => NoteView.CopySelectedNotes());
            commandSource.RegisterCommand(Commands.Paste, MainFormStrings.Paste, () => NoteView.PasteNotes());
            commandSource.RegisterCommand(Commands.PasteChannel, MainFormStrings.PasteChannel, () => NoteView.PasteChNotes());
            commandSource.RegisterCommand(Commands.PasteFlip, MainFormStrings.PasteFlipped, () => NoteView.PasteFlippedNotes());
            commandSource.RegisterCommand(Commands.PasteFlipChannel, MainFormStrings.PasteFlipChannel, () => NoteView.PasteFlippedChNotes());

            commandSource.RegisterCommand(Commands.CutEvents, MainFormStrings.Event + MainFormStrings.Cut, () => NoteView.CutSelectedEvents());
            commandSource.RegisterCommand(Commands.CopyEvents, MainFormStrings.Event + MainFormStrings.Copy, () => NoteView.CopySelectedEvents());
            commandSource.RegisterCommand(Commands.PasteEvents, MainFormStrings.Event + MainFormStrings.Paste, () => NoteView.PasteEvents());
            commandSource.RegisterCommand(Commands.PasteChEvents, MainFormStrings.Event + MainFormStrings.Paste + "(" + MainFormStrings.CurrentChannel + ")", () => NoteView.PasteChEvents());

            commandSource.RegisterCommand(Commands.SelectAll, MainFormStrings.SelectAll, () => NoteView.SelectAll());
            commandSource.RegisterCommand(Commands.SelectToBegin, MainFormStrings.SelectToBeginning, () => NoteView.SelectToBeginning());
            commandSource.RegisterCommand(Commands.SelectToEnd, MainFormStrings.SelectToEnd, () => NoteView.SelectToEnd());

            commandSource.RegisterCommand(Commands.FlipSelectedNotes, MainFormStrings.FlipSelectedNotes, () => NoteView.FlipSelectedNotes());
            commandSource.RegisterCommand(Commands.RemoveSelectedNotes, MainFormStrings.RemoveSelectedNotes, () => NoteView.RemoveSelectedNotes());
            commandSource.RegisterCommand(Commands.RemoveSelectedEvents, MainFormStrings.RemoveEvents, () => NoteView.RemoveSelectedEvents());
            commandSource.RegisterCommand(Commands.ChangeChannelSelectedNotes, MainFormStrings.ChangeChannelSelectedNotes, () => NoteView.ChangeChannelSelectedNotes());

            commandSource.RegisterCommand(Commands.SwitchScorePreviewMode, MainFormStrings.ScorePreview, () => IsPreviewMode = !IsPreviewMode);

            commandSource.RegisterCommand(Commands.WidenLaneWidth, MainFormStrings.WidenLaneWidth, () =>
            {
                if (!CanWidenLaneWidth) return;
                NoteView.UnitLaneWidth += 4;
                ApplicationSettings.Default.UnitLaneWidth = NoteView.UnitLaneWidth;
            });
            commandSource.RegisterCommand(Commands.NarrowLaneWidth, MainFormStrings.NarrowLaneWidth, () =>
            {
                if (!CanNarrowLaneWidth) return;
                NoteView.UnitLaneWidth -= 4;
                ApplicationSettings.Default.UnitLaneWidth = NoteView.UnitLaneWidth;
            });


            commandSource.RegisterCommand(Commands.InsertBpmChange, "BPM", () =>
            {
                var form = new BpmSelectionForm()
                {
                    Bpm = NoteView.ScoreEvents.BpmChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.Bpm ?? 120
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new BpmChangeEvent()
                {
                    Tick = NoteView.CurrentTick,
                    Bpm = form.Bpm,
                    Type = -1
                };
                UpdateEvent(NoteView.ScoreEvents.BpmChangeEvents, item);
            });
            commandSource.RegisterCommand(Commands.InsertHighSpeedChange, MainFormStrings.HighSpeed, () =>
            {
                var spratio = NoteView.ScoreEvents.HighSpeedChangeEvents.Where(q => q.SpeedCh == NoteView.Channel).OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.SpeedRatio ?? 1.0m;
                var spcustom = NoteView.ScoreEvents.HighSpeedChangeEvents.Where(q => q.SpeedCh == NoteView.Channel).OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.CustomArgs ?? "";
                var spskip = NoteView.ScoreEvents.HighSpeedChangeEvents.Where(q => q.SpeedCh == NoteView.Channel).OrderBy(p => p.Tick).LastOrDefault(p => p.Tick == NoteView.CurrentTick)?.Skip ?? 0;
                var spease = NoteView.ScoreEvents.HighSpeedChangeEvents.Where(q => q.SpeedCh == NoteView.Channel).OrderBy(p => p.Tick).LastOrDefault(p => p.Tick == NoteView.CurrentTick)?.Ease ?? 0;
                var sphide = NoteView.ScoreEvents.HighSpeedChangeEvents.Where(q => q.SpeedCh == NoteView.Channel).OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.HideNotes ?? 0;
                var splane = NoteView.ScoreEvents.HighSpeedChangeEvents.Where(q => q.SpeedCh == NoteView.Channel).OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.EditLaneIndex ?? 8;

                if (!FormSpeedbyCh)
                {
                    spratio = NoteView.ScoreEvents.HighSpeedChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.SpeedRatio ?? 1.0m;
                    spcustom = NoteView.ScoreEvents.HighSpeedChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.CustomArgs ?? "";
                    spskip = NoteView.ScoreEvents.HighSpeedChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick == NoteView.CurrentTick)?.Skip ?? 0;
                    spease = NoteView.ScoreEvents.HighSpeedChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick == NoteView.CurrentTick)?.Ease ?? 0;
                    sphide = NoteView.ScoreEvents.HighSpeedChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.HideNotes ?? 0;
                    splane = NoteView.ScoreEvents.HighSpeedChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.EditLaneIndex ?? 8;
                }

                var form = new HighSpeedSelectionForm()
                {
                    SpeedRatio = spratio,
                    SpeedCh = Channel,
                    CustomArgs = spcustom,
                    SkipBeats = spskip,
                    Ease = spease,
                    HideNotes = sphide == 1 ? true : false,
                    EditorLane = splane
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;
                var speedratio = form.SpeedRatio;
                var bpm = NoteView.ScoreEvents.BpmChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.Bpm ?? 120;
                var customArgs = form.CustomArgs;
                if (form.CustomArgs.Length > 0)
                {
                    customArgs = customArgs.Replace("{channel}", NoteView.Channel.ToString()).Replace("{spchannel}", form.SpeedCh.ToString()).Replace("{bpm}", bpm.ToString());
                    try
                    {
                        speedratio = Calculate1(customArgs);
                    }
                    catch
                    {
                        speedratio = form.SpeedRatio;
                        MessageBox.Show(this, ErrorStrings.ArgsException, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                 
                }
                var item = new HighSpeedChangeEvent()
                {
                    Tick = NoteView.CurrentTick,
                    SpeedRatio = speedratio,
                    SpeedCh = form.SpeedCh,
                    Type = form.SpeedCh,
                    CustomArgs = form.CustomArgs,
                    Skip = form.SkipBeats,
                    Ease = form.Ease,
                    HideNotes = form.HideNotes ? 1 : 0,
                    EditLaneIndex = form.EditorLane
                };
                UpdateHighSpeed(NoteView.ScoreEvents.HighSpeedChangeEvents, item);
            });
            commandSource.RegisterCommand(Commands.InsertTimeSignatureChange, MainFormStrings.TimeSignature, () =>
            {
                var form = new TimeSignatureSelectionForm();
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new TimeSignatureChangeEvent()
                {
                    Tick = NoteView.CurrentTick,
                    Numerator = form.Numerator,
                    DenominatorExponent = form.DenominatorExponent,
                    Type = -2
                };
                UpdateEvent(NoteView.ScoreEvents.TimeSignatureChangeEvents, item);
            });

            commandSource.RegisterCommand(Commands.InsertComment, MainFormStrings.Comment, () =>
            {
                var form = new CommentInsertForm()
                {
                    Comment = NoteView.ScoreEvents.CommentEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick == NoteView.CurrentTick)?.Comment ?? "コメント",
                    TextSize = (decimal)(NoteView.ScoreEvents.CommentEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick == NoteView.CurrentTick)?.Size ?? 9),
                    Color = (NoteView.ScoreEvents.CommentEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick == NoteView.CurrentTick)?.Color ?? 0),
                    LaneIndex = 12
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new CommentEvent()
                {
                    Tick = NoteView.CurrentTick,
                    Comment = form.Comment,
                    Size = (float)form.TextSize,
                    Color = form.Color,
                    LaneIndex = form.LaneIndex,
                    Type = -3

                };
                UpdateEvent(NoteView.ScoreEvents.CommentEvents, item);
            });
            /*
            commandSource.RegisterCommand(Commands.InsertMarker, MainFormStrings.Marker, () =>
            {
                var form = new MarkerInsertForm()
                {
                    Name = NoteView.Notes.Markers.OrderBy(p => p.StartTick).LastOrDefault(p => p.StartTick == NoteView.CurrentTick)?.Name ?? "コメント",
                    MarkerWidth = (decimal)(NoteView.Notes.Markers.OrderBy(p => p.StartTick).LastOrDefault(p => p.StartTick == NoteView.CurrentTick)?.StartWidth ?? 9)
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

            });
            */
            commandSource.RegisterCommand(Commands.InsertSkill, "Skill", () =>
            {
                var form = new SkillEventForm();
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new SkillEvent()
                {
                    Tick = NoteView.CurrentTick,
                    Skill = form.Skill,
                    Level = form.Level,
                    Type = -1
                };
                UpdateEvent(NoteView.ScoreEvents.SkillEvents, item);
            });
            commandSource.RegisterCommand(Commands.InsertFever, "Fever", () =>
            {
                var form = new FeverEventForm();
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new FeverEvent()
                {
                    Tick = NoteView.CurrentTick,
                    Start = form.Start,
                    Force = form.Force,
                    Type = -1
                };
                UpdateEvent(NoteView.ScoreEvents.FeverEvents, item);
            });


            void UpdateEvent<T>(List<T> list, T item) where T : EventBase
            {

                var prev = list.SingleOrDefault(p => p.Tick == item.Tick && p.Type == item.Type);

                var insertOp = new InsertEventOperation<T>(list, item);
                if (prev == null)
                {
                    OperationManager.InvokeAndPush(insertOp);
                }
                else
                {
                    var removeOp = new RemoveEventOperation<T>(list, prev);
                    OperationManager.InvokeAndPush(new CompositeOperation(insertOp.Description, new IOperation[] { removeOp, insertOp }));
                }
                NoteView.Invalidate();
            }
            void UpdateHighSpeed(List<HighSpeedChangeEvent> list, HighSpeedChangeEvent item)
            {

                HighSpeedChangeEvent prev = null;

                if(list.FindAll(p => p.Tick == item.Tick && p.SpeedCh == item.SpeedCh).Count > 1)
                {
                    prev = list.FindLast(p => p.Tick == item.Tick && p.SpeedCh == item.SpeedCh);
                    list.RemoveAll(p => p.Tick == item.Tick && p.SpeedCh == item.SpeedCh && !p.Equals(item));
                }
                

                var insertOp = new InsertEventOperation<HighSpeedChangeEvent>(list, item);
                if (prev == null)
                {
                    OperationManager.InvokeAndPush(insertOp);
                }
                else
                {
                    var removeOp = new RemoveEventOperation<HighSpeedChangeEvent>(list, prev);
                    OperationManager.InvokeAndPush(new CompositeOperation(insertOp.Description, new IOperation[] { removeOp, insertOp }));
                }
                NoteView.Invalidate();
            }

            commandSource.RegisterCommand(Commands.NoteCollection, "NoteCollection", () =>
            {
                var form = new NoteCollectionForm()
                {
                    BeforeBpm = NoteView.ScoreEvents.BpmChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.Bpm * 2 ?? 240,
                    AfterBpm = NoteView.ScoreEvents.BpmChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.Bpm ?? 120
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var before = ScoreBook.Score;

                var BpmTick1 = NoteView.ScoreEvents.BpmChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.Tick ?? 0;//選択場所のBPM
                var BpmTick2 = NoteView.ScoreEvents.BpmChangeEvents.OrderBy(p => p.Tick).FirstOrDefault(p => p.Tick >= NoteView.CurrentTick)?.Tick ?? int.MaxValue; //選択場所の次のBPM

                var BeforeBpm = form.BeforeBpm;
                var AfterBpm = form.AfterBpm;

                var Oplist = new List<IOperation>();
                var notes = ScoreBook.Score.Notes;

                var dicShortNotes = notes.GetShortNotes().ToDictionary(q => q, q => new MoveShortNoteOperation.NotePosition(q.Tick, q.LaneIndex));
                var dicHolds = notes.Holds.ToDictionary(q => q, q => new MoveHoldOperation.NotePosition(q.StartTick, q.LaneIndex, q.Width));
                var dicSlides = notes.Slides.ToDictionary(q => q, q => new MoveSlideOperation.NotePosition(q.StartTick, q.StartLaneIndex, q.StartWidth));
                var dicGuides = notes.Guides.ToDictionary(q => q, q => new MoveGuideOperation.NotePosition(q.StartTick, q.StartLaneIndex, q.StartWidth));
                var referenced = new NoteCollection(notes);

                var opShortNotes = dicShortNotes.Select(p =>
                {
                    p.Key.Tick = (int)(p.Key.Tick * (AfterBpm / BeforeBpm));
                    var after = new MoveShortNoteOperation.NotePosition(p.Key.Tick, p.Key.LaneIndex);
                    return new MoveShortNoteOperation(p.Key, p.Value, after);
                });

                var opHolds = dicHolds.Select(p =>
                {
                    p.Key.StartTick = (int)(p.Key.StartTick * (AfterBpm / BeforeBpm));
                    var after = new MoveHoldOperation.NotePosition(p.Key.StartTick, p.Key.LaneIndex, p.Key.Width);
                    return new MoveHoldOperation(p.Key, p.Value, after);
                });

                var opSlides = dicSlides.Select(p =>
                {
                    p.Key.StartTick = (int)(p.Key.StartTick * (AfterBpm / BeforeBpm));
                    var after = new MoveSlideOperation.NotePosition(p.Key.StartTick, p.Key.StartLaneIndex, p.Key.StartWidth);
                    return new MoveSlideOperation(p.Key, p.Value, after);
                });
                var opGuides = dicGuides.Select(p =>
                {
                    p.Key.StartTick = (int)(p.Key.StartTick * (AfterBpm / BeforeBpm));
                    var after = new MoveGuideOperation.NotePosition(p.Key.StartTick, p.Key.StartLaneIndex, p.Key.StartWidth);
                    return new MoveGuideOperation(p.Key, p.Value, after);
                });

                foreach (var note in NoteView.Notes.Taps.Where(p => (p.Tick >= BpmTick1) && (p.Tick < BpmTick2)))
                {

                    note.Tick = (int)(note.Tick * (AfterBpm / BeforeBpm));
                }
                foreach (var note in NoteView.Notes.ExTaps.Where(p => (p.Tick >= BpmTick1) && (p.Tick < BpmTick2)))
                {

                    note.Tick = (int)(note.Tick * (AfterBpm / BeforeBpm));
                }
                foreach (var note in NoteView.Notes.Flicks.Where(p => (p.Tick >= BpmTick1) && (p.Tick < BpmTick2)))
                {

                    note.Tick = (int)(note.Tick * (AfterBpm / BeforeBpm));
                }
                foreach (var note in NoteView.Notes.Damages.Where(p => (p.Tick >= BpmTick1) && (p.Tick < BpmTick2)))
                {

                    note.Tick = (int)(note.Tick * (AfterBpm / BeforeBpm));
                }
                foreach (var note in NoteView.Notes.Slides.Where(p => (p.StartTick >= BpmTick1) && (p.StartTick < BpmTick2)))
                {

                    note.StartTick = (int)(note.StartTick * (AfterBpm / BeforeBpm));

                    foreach (var step in note.StepNotes)
                    {
                        step.TickOffset = (int)(step.TickOffset * (AfterBpm / BeforeBpm));
                    }
                }
                foreach (var note in NoteView.Notes.Guides.Where(p => (p.StartTick >= BpmTick1) && (p.StartTick < BpmTick2)))
                {

                    note.StartTick = (int)(note.StartTick * (AfterBpm / BeforeBpm));

                    foreach (var step in note.StepNotes)
                    {
                        step.TickOffset = (int)(step.TickOffset * (AfterBpm / BeforeBpm));
                    }
                }
                foreach (var @event in NoteView.ScoreEvents.HighSpeedChangeEvents)
                {
                    @event.Tick = (int)(@event.Tick * (AfterBpm / BeforeBpm));
                }
                foreach (var @event in NoteView.ScoreEvents.TimeSignatureChangeEvents)
                {
                    @event.Tick = (int)(@event.Tick * (AfterBpm / BeforeBpm));
                }
                foreach (var @event in NoteView.ScoreEvents.CommentEvents)
                {
                    @event.Tick = (int)(@event.Tick * (AfterBpm / BeforeBpm));
                }

                var opList = opShortNotes.Cast<IOperation>().Concat(opHolds).Concat(opSlides).Concat(opGuides).ToList();
                OperationManager.InvokeAndPush(new CompositeOperation("ノーツの移動反転", opList));

            });

            commandSource.RegisterCommand(Commands.PlayPreview, MainFormStrings.Play, () => PlayPreview());

            commandSource.RegisterCommand(Commands.ShowHelp, MainFormStrings.Help, () => System.Diagnostics.Process.Start("https://github.com/myuuskye/Ched/wiki"));

            commandSource.RegisterCommand(Commands.SelectPen, MainFormStrings.Pen, () => NoteView.EditMode = EditMode.Edit);
            commandSource.RegisterCommand(Commands.SelectSelection, MainFormStrings.Selection, () => NoteView.EditMode = EditMode.Select);
            commandSource.RegisterCommand(Commands.SelectEraser, MainFormStrings.Eraser, () => NoteView.EditMode = EditMode.Erase);
            commandSource.RegisterCommand(Commands.SelectPaint, MainFormStrings.Paint, () => NoteView.EditMode = EditMode.Paint);
            commandSource.RegisterCommand(Commands.SelectProperty, MainFormStrings.Property, () => NoteView.EditMode = EditMode.Property);
            commandSource.RegisterCommand(Commands.SelectMarker, MainFormStrings.Marker, () => NoteView.EditMode = EditMode.Marker);
            commandSource.RegisterCommand(Commands.SelectStepEditor, MainFormStrings.StepEditor, () => NoteView.EditMode = EditMode.StepEdit);
            commandSource.RegisterCommand(Commands.SelectEventEditor, MainFormStrings.EventEditor, () => NoteView.EditMode = EditMode.EventEdit);

            commandSource.RegisterCommand(Commands.ZoomIn, MainFormStrings.ZoomIn, () =>
            {
                if (!CanZoomIn) return;
                NoteView.UnitBeatHeight *= 2;
                ApplicationSettings.Default.UnitBeatHeight = (int)NoteView.UnitBeatHeight;
                UpdateThumbHeight();
            });
            commandSource.RegisterCommand(Commands.ZoomOut, MainFormStrings.ZoomOut, () =>
            {
                if (!CanZoomOut) return;
                NoteView.UnitBeatHeight /= 2;
                ApplicationSettings.Default.UnitBeatHeight = (int)NoteView.UnitBeatHeight;
                UpdateThumbHeight();
            });

            commandSource.RegisterCommand(Commands.SelectTap, "TAP", () =>
            {
                NoteView.NewNoteType = NoteType.Tap;
                NoteView.IsNewNoteStart = false;
            });
            commandSource.RegisterCommand(Commands.SelectExTap, "ExTAP", () =>
            {
                NoteView.NewNoteType = NoteType.ExTap;
                NoteView.IsNewNoteStart = false;
            });

            commandSource.RegisterCommand(Commands.SelectHold, "HOLD", () => NoteView.NewNoteType = NoteType.Hold);
            commandSource.RegisterCommand(Commands.SelectSlide, "SLIDE", () =>
            {
                NoteView.NewNoteType = NoteType.Slide;
                NoteView.IsNewSlideStepVisible = false;
            });
            commandSource.RegisterCommand(Commands.SelectSlideStep, MainFormStrings.SlideStep, () =>
            {
                NoteView.NewNoteType = NoteType.Slide;
                NoteView.IsNewSlideStepVisible = true;
            });
            commandSource.RegisterCommand(Commands.SelectAir, "AIR", () =>
            {
                if (NoteView.NewNoteType != NoteType.Air)
                {
                    NoteView.NewNoteType = NoteType.Air;
                    return;
                }
                if (NoteView.AirDirection.HorizontalDirection == HorizontalAirDirection.Left)
                {
                    NoteView.AirDirection = new AirDirection(
                        NoteView.AirDirection.VerticalDirection == VerticalAirDirection.Up ? VerticalAirDirection.Down : VerticalAirDirection.Up,
                        GetNextHorizontalDirection(NoteView.AirDirection.HorizontalDirection));
                    return;
                }
                HandleHorizontalAirDirection(NoteView.AirDirection.VerticalDirection);
            });
            commandSource.RegisterCommand(Commands.SelectAirUp, MainFormStrings.AirUp, () => HandleHorizontalAirDirection(VerticalAirDirection.Up));
            commandSource.RegisterCommand(Commands.SelectAirDown, MainFormStrings.AirDown, () => HandleHorizontalAirDirection(VerticalAirDirection.Down)); 
            commandSource.RegisterCommand(Commands.SelectAirOther, MainFormStrings.AirHandy, () => HandleHorizontalAirDirection(VerticalAirDirection.Other));
            commandSource.RegisterCommand(Commands.SelectAirAction, "AIR-ACTION", () => NoteView.NewNoteType = NoteType.AirAction);
            commandSource.RegisterCommand(Commands.SelectFlick, "FLICK", () => { NoteView.NewNoteType = NoteType.Flick; NoteView.IsNewNoteStart = false; });
            commandSource.RegisterCommand(Commands.SelectDamage, "DAMAGE", () => { NoteView.NewNoteType = NoteType.Damage; NoteView.IsNewNoteStart = false; });
            commandSource.RegisterCommand(Commands.SelectStepNoteTap, "STEPNOTETAP", () => NoteView.NewNoteType = NoteType.StepNoteTap);

            commandSource.RegisterCommand(Commands.SelectGuide, "GUIDE", () =>
            {
                NoteView.NewNoteType = NoteType.Guide;
                NoteView.IsNewGuideStepVisible = false;
            });
            commandSource.RegisterCommand(Commands.SelectGuideStep, "GUIDESTEP", () =>
            {
                NoteView.NewNoteType = NoteType.Guide;
                NoteView.IsNewGuideStepVisible = true;
            });

            commandSource.RegisterCommand(Commands.SelectTap2, "TAP2", () => 
            {
                NoteView.NewNoteType = NoteType.Tap;
                NoteView.IsNewNoteStart = true;
            });
            commandSource.RegisterCommand(Commands.SelectExTap2, "ExTAP2", () =>
            {
                NoteView.NewNoteType = NoteType.ExTap;
                NoteView.IsNewNoteStart = true;
            });
            commandSource.RegisterCommand(Commands.SelectFlick2, "FLICK2", () =>
            {
                NoteView.NewNoteType = NoteType.Flick;
                NoteView.IsNewNoteStart = true;
            });
            commandSource.RegisterCommand(Commands.SelectDamage2, "DAMAGE2", () =>
            {
                NoteView.NewNoteType = NoteType.Damage;
                NoteView.IsNewNoteStart = true;
            });
            commandSource.RegisterCommand(Commands.SelectEventSelection, "Event Select", () =>
            {
                NoteView.EventMode = EventEditMode.Select;
            });
            commandSource.RegisterCommand(Commands.SelectEventEdit, "Event Edit", () =>
            {
                NoteView.EventMode = EventEditMode.Select;
            });
            commandSource.RegisterCommand(Commands.SelectEventEracer, "Event Erace", () =>
            {
                NoteView.EventMode = EventEditMode.Erase;
            });
            commandSource.RegisterCommand(Commands.SelectHighspeed, "Highspeed Event", () =>
            {
                NoteView.EventMode = EventEditMode.Highspeed;
            });
            commandSource.RegisterCommand(Commands.SelectBpm, "Bpm Event", () =>
            {
                NoteView.EventMode = EventEditMode.Bpm;
            });
            commandSource.RegisterCommand(Commands.SelectComment, "Comment Event", () =>
            {
                NoteView.EventMode = EventEditMode.Comment;
            });
            commandSource.RegisterCommand(Commands.SelectSkill, "Skill Event", () =>
            {
                NoteView.EventMode = EventEditMode.Skill;
            });
            commandSource.RegisterCommand(Commands.SelectFever, "Fever Event", () =>
            {
                NoteView.EventMode = EventEditMode.Fever;
            });
            commandSource.RegisterCommand(Commands.SelectCamera, "Camera Event", () =>
            {
                NoteView.EventMode = EventEditMode.Camera;
            });
            commandSource.RegisterCommand(Commands.SelectStageMask, "Stage Mask Event", () =>
            {
                NoteView.EventMode = EventEditMode.StageMask;
            });
            commandSource.RegisterCommand(Commands.SelectStagePivot, "Stage Pivot Event", () =>
            {
                NoteView.EventMode = EventEditMode.StagePivot;
            });
            commandSource.RegisterCommand(Commands.SelectStageStyle, "Stage Style Event", () =>
            {
                NoteView.EventMode = EventEditMode.StageStyle;
            });
            commandSource.RegisterCommand(Commands.SelectStageTransform, "Stage Transform Event", () =>
            {
                NoteView.EventMode = EventEditMode.StageTransform;
            });



            void HandleHorizontalAirDirection(VerticalAirDirection verticalDirection)
            {
                if (NoteView.NewNoteType != NoteType.Air)
                {
                    NoteView.NewNoteType = NoteType.Air;
                    NoteView.AirDirection = new AirDirection(verticalDirection, NoteView.AirDirection.HorizontalDirection);
                    return;
                }
                var horizontalDirection = NoteView.AirDirection.HorizontalDirection;
                if (verticalDirection == NoteView.AirDirection.VerticalDirection)
                    horizontalDirection = GetNextHorizontalDirection(horizontalDirection);
                NoteView.AirDirection = new AirDirection(verticalDirection, horizontalDirection);
            }

            HorizontalAirDirection GetNextHorizontalDirection(HorizontalAirDirection direction)
            {
                switch (direction)
                {
                    case HorizontalAirDirection.Center:
                        return HorizontalAirDirection.Right;

                    case HorizontalAirDirection.Right:
                        return HorizontalAirDirection.Left;

                    case HorizontalAirDirection.Left:
                        return HorizontalAirDirection.Center;
                }
                throw new ArgumentException();
            }
        }

        private void ConfigureKeyboardShortcut()
        {
            var vm = new ShortcutSettingsWindowViewModel(ShortcutManagerHost);
            var window = new ShortcutSettingsWindow() { DataContext = vm };
            window.ShowDialog(this);
            ShortcutManager.NotifyUpdateShortcut();
        }

        private MenuStrip CreateMainMenu(NoteView noteView)
        {
            var shortcutItemBuilder = new ToolStripMenuItemBuilder(ShortcutManager);

            var importPluginItems = PluginManager.ScoreBookImportPlugins.Select(p => new ToolStripMenuItem(p.DisplayName, null, (s, e) =>
            {
                if (!ConfirmDiscardChanges()) return;
                if (!TrySelectOpeningFile(p.FileFilter, out string path)) return;

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var args = new ScoreBookImportPluginArgs(stream, stream.Name);
                    HandleImport(p, args);

                }
            })).ToArray();

            var exportPluginItems = PluginManager.ScoreBookExportPlugins.Select(p => new ToolStripMenuItem(p.DisplayName, null, (s, e) =>
            {
                ExportAs(p);
            })).ToArray();

            var bookPropertiesMenuItem = shortcutItemBuilder.BuildItem(Commands.ShowScoreBookProperties, MainFormStrings.BookProperty);

            var fileMenuItems = new ToolStripItem[]
            {
                shortcutItemBuilder.BuildItem(Commands.NewFile, MainFormStrings.NewFile + "(&N)"),
                shortcutItemBuilder.BuildItem(Commands.OpenFile, MainFormStrings.OpenFile + "(&O)"),
                shortcutItemBuilder.BuildItem(Commands.Save, MainFormStrings.SaveFile + "(&S)"),
                shortcutItemBuilder.BuildItem(Commands.SaveAs, MainFormStrings.SaveAs + "(&A)"),
                new ToolStripSeparator(),
                new ToolStripMenuItem(MainFormStrings.Import, null, importPluginItems) { Enabled = importPluginItems.Length > 0 },
                new ToolStripMenuItem(MainFormStrings.Export, null, exportPluginItems) { Enabled = exportPluginItems.Length > 0 },
                new ToolStripSeparator(),
                bookPropertiesMenuItem,
                new ToolStripSeparator(),
                shortcutItemBuilder.BuildItem(Commands.ShowShortcutSettings, MainFormStrings.KeyboardShortcuts),
                new ToolStripSeparator(),
                new ToolStripMenuItem(MainFormStrings.Exit + "(&X)", null, (s, e) => this.Close())
            };

            var undoItem = shortcutItemBuilder.BuildItem(Commands.Undo, MainFormStrings.Undo);
            undoItem.Enabled = false;

            var redoItem = shortcutItemBuilder.BuildItem(Commands.Redo, MainFormStrings.Redo);
            redoItem.Enabled = false;

            var cutItem = shortcutItemBuilder.BuildItem(Commands.Cut, MainFormStrings.Cut);
            var copyItem = shortcutItemBuilder.BuildItem(Commands.Copy, MainFormStrings.Copy);
            var pasteItem = shortcutItemBuilder.BuildItem(Commands.Paste, MainFormStrings.Paste);
            var pasteFlippedItem = shortcutItemBuilder.BuildItem(Commands.PasteFlip, MainFormStrings.PasteFlipped);
            var pasteChannelItem = shortcutItemBuilder.BuildItem(Commands.PasteChannel, MainFormStrings.PasteChannel);
            var pasteFlippedChannelItem = shortcutItemBuilder.BuildItem(Commands.PasteFlipChannel, MainFormStrings.PasteFlipChannel);

            var selectAllItem = shortcutItemBuilder.BuildItem(Commands.SelectAll, MainFormStrings.SelectAll);
            var selectToEndItem = shortcutItemBuilder.BuildItem(Commands.SelectToEnd, MainFormStrings.SelectToEnd);
            var selectoToBeginningItem = shortcutItemBuilder.BuildItem(Commands.SelectToBegin, MainFormStrings.SelectToBeginning);

            var flipSelectedNotesItem = shortcutItemBuilder.BuildItem(Commands.FlipSelectedNotes, MainFormStrings.FlipSelectedNotes);
            var removeSelectedNotesItem = shortcutItemBuilder.BuildItem(Commands.RemoveSelectedNotes, MainFormStrings.RemoveSelectedNotes);
            var copyEventsItem = shortcutItemBuilder.BuildItem(Commands.CopyEvents, MainFormStrings.CopyEvents);
            var cutEventsItem = shortcutItemBuilder.BuildItem(Commands.CutEvents, MainFormStrings.CutEvents);
            var pasteEventsItem = shortcutItemBuilder.BuildItem(Commands.PasteEvents, MainFormStrings.PasteEvents);
            var pasteChEventsItem = shortcutItemBuilder.BuildItem(Commands.PasteChEvents, MainFormStrings.PasteChEvents);
            var removeEventsItem = shortcutItemBuilder.BuildItem(Commands.RemoveSelectedEvents, MainFormStrings.RemoveEvents);
            var changeChannelSelectedNotesItem = shortcutItemBuilder.BuildItem(Commands.ChangeChannelSelectedNotes, MainFormStrings.ChangeChannelSelectedNotes);
            var noteCollectionItem = shortcutItemBuilder.BuildItem(Commands.NoteCollection, "NoteCollection");

            var insertAirWithAirActionItem = new ToolStripMenuItem(MainFormStrings.InsertAirWithAirAction, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                NoteView.InsertAirWithAirAction = item.Checked;
                ApplicationSettings.Default.InsertAirWithAirAction = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.InsertAirWithAirAction
            };

            var allowStepChItem = new ToolStripMenuItem(MainFormStrings.AllowStepCh, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                NoteView.AllowStepCh = item.Checked;
                ApplicationSettings.Default.IsAllowStepChannel = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsAllowStepChannel
            };
            var anotherSelectMethodItem = new ToolStripMenuItem(MainFormStrings.AnotherSelectMethod, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                NoteView.SelectMethod = item.Checked;
                ApplicationSettings.Default.SelectMethod = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SelectMethod
            };
            var editOutLaneItem = new ToolStripMenuItem(MainFormStrings.EditOutLane, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                NoteView.EditableOutLane = item.Checked;
                ApplicationSettings.Default.EditableOutLane = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.EditableOutLane
            };

            var pluginItems = PluginManager.ScorePlugins.Select(p => new ToolStripMenuItem(p.DisplayName, null, (s, e) =>
            {
                CommitChanges();
                void updateScore(Score newScore)
                {

                    var op = new UpdateScoreOperation(ScoreBook.Score, newScore, score =>
                    {
                        ScoreBook.Score = score;
                        noteView.UpdateScore(score);
                    });
                    OperationManager.InvokeAndPush(op);
                }

                try
                {
                    p.Run(new ScorePluginArgs(() => ScoreBook.Score.Clone(), noteView.SelectedRange, updateScore), noteView.EditbyCh, noteView.Channel);
                }
                catch (Exception ex)
                {
                    Program.DumpExceptionTo(ex, "plugin_exception.json");
                    MessageBox.Show(this, ErrorStrings.PluginException, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            })).ToArray();
            var pluginItem = new ToolStripMenuItem(MainFormStrings.Plugin, null, pluginItems) { Enabled = pluginItems.Length > 0 };

            var editMenuItems = new ToolStripItem[]
            {
                undoItem, redoItem, new ToolStripSeparator(),
                cutItem, copyItem, pasteItem, pasteFlippedItem, pasteChannelItem, pasteFlippedChannelItem, new ToolStripSeparator(),
                selectAllItem, selectToEndItem, selectoToBeginningItem, new ToolStripSeparator(),
                flipSelectedNotesItem, removeSelectedNotesItem,   new ToolStripSeparator(),
                copyEventsItem, cutEventsItem, pasteEventsItem, pasteChEventsItem,removeEventsItem, new ToolStripSeparator(),
                insertAirWithAirActionItem, allowStepChItem, anotherSelectMethodItem, editOutLaneItem, new ToolStripSeparator(),
                pluginItem
            };

            var viewModeItem = shortcutItemBuilder.BuildItem(Commands.SwitchScorePreviewMode, MainFormStrings.ScorePreview);
            PreviewModeChanged += (s, e) => viewModeItem.Checked = IsPreviewMode;

            var widenLaneWidthMenuItem = shortcutItemBuilder.BuildItem(Commands.WidenLaneWidth, MainFormStrings.WidenLaneWidth);
            var narrowLaneWidthMenuItem = shortcutItemBuilder.BuildItem(Commands.NarrowLaneWidth, MainFormStrings.NarrowLaneWidth);
            var visibleOverlapItem = new ToolStripMenuItem(MainFormStrings.VisibleOverlap, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsVisibleOverlap = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsVisibleOverlap
            };
            var isUsingBezierCurves = new ToolStripMenuItem(MainFormStrings.UsingBezierCurves, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsUsingBezierCurves = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsUsingBezierCurves
            };
            var isCustomizedSlide = new ToolStripMenuItem(MainFormStrings.CustomizedSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.CustomizeSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.CustomizeSlide
            };
            var isInvisibleSteps = new ToolStripMenuItem(MainFormStrings.InvisibleStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.InvisibleSteps = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.InvisibleSteps
            };
            var isEventLine = new ToolStripMenuItem(MainFormStrings.EventLineVisual, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsEventHasLine = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsEventHasLine
            };

            NoteView.UnitLaneWidthChanged += (s, e) =>
            {
                widenLaneWidthMenuItem.Enabled = CanWidenLaneWidth;
                narrowLaneWidthMenuItem.Enabled = CanNarrowLaneWidth;
            };

            var viewMenuItems = new ToolStripItem[]
            {
                viewModeItem,
                new ToolStripSeparator(),
                widenLaneWidthMenuItem, narrowLaneWidthMenuItem,
                visibleOverlapItem, isUsingBezierCurves, isCustomizedSlide, isInvisibleSteps, isEventLine
            };



            var insertBpmItem = shortcutItemBuilder.BuildItem(Commands.InsertBpmChange, "BPM");
            var insertHighSpeedItem = shortcutItemBuilder.BuildItem(Commands.InsertHighSpeedChange, MainFormStrings.HighSpeed);
            var insertTimeSignatureItem = shortcutItemBuilder.BuildItem(Commands.InsertTimeSignatureChange, MainFormStrings.TimeSignature);
            var insertCommentItem = shortcutItemBuilder.BuildItem(Commands.InsertComment, MainFormStrings.Comment);
            var insertSkillItem = shortcutItemBuilder.BuildItem(Commands.InsertSkill, "Skill");
            var insertFeverItem = shortcutItemBuilder.BuildItem(Commands.InsertFever, "Fever");


            var insertMenuItems = new ToolStripItem[] { insertBpmItem, insertHighSpeedItem, insertTimeSignatureItem, insertCommentItem, insertSkillItem, insertFeverItem };


            var playItem = shortcutItemBuilder.BuildItem(Commands.PlayPreview, MainFormStrings.Play);

            var stopItem = new ToolStripMenuItem(MainFormStrings.Stop, null, (s, e) => PreviewManager.Stop());


            Rectangle ImageSize = new Rectangle(0, 0, 16, 16);

            Bitmap img = new Bitmap(16, 16);
            Graphics graph = Graphics.FromImage(img);
            graph.FillRectangle(Brushes.Black, ImageSize);

            Bitmap img2 = new Bitmap(16, 16);
            Graphics graph2 = Graphics.FromImage(img2);
            graph2.FillRectangle(Brushes.White, ImageSize);



            var themeBlack = new ToolStripMenuItem(MainFormStrings.ThemeBlack, img, (s, e) => noteView.Theme = 0);
            var themeWhite = new ToolStripMenuItem(MainFormStrings.ThemeWhite, img2, (s, e) => noteView.Theme = 1);
            var themePJsekai = new ToolStripMenuItem(MainFormStrings.PJsekai, img2, (s, e) => noteView.Theme = 2);

            var slowDownPreviewItem = new ToolStripMenuItem(MainFormStrings.SlowDownPreview, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsSlowDownPreviewEnabled = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsSlowDownPreviewEnabled
            };

            var isAbortAtLastNoteItem = new ToolStripMenuItem(MainFormStrings.AbortAtLastNote, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                PreviewManager.IsStopAtLastNote = item.Checked;
                ApplicationSettings.Default.IsPreviewAbortAtLastNote = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsPreviewAbortAtLastNote
            };

            var isPjsekaiSounds = new ToolStripMenuItem("PJsekaiSounds", null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsPJsekaiSounds = item.Checked;
                PreviewManager.Stop();
                PreviewManager.Dispose();
                PreviewManager = new SoundPreviewManager(this);
                PreviewManager.IsStopAtLastNote = ApplicationSettings.Default.IsPreviewAbortAtLastNote;
                PreviewManager.TickUpdated += (a, i) => NoteView.CurrentTick = i.Tick;
                PreviewManager.ExceptionThrown += (a, i) => MessageBox.Show(this, ErrorStrings.PreviewException, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            })
            {
                Checked = ApplicationSettings.Default.IsPJsekaiSounds
            };


            PreviewManager.Started += (s, e) => isAbortAtLastNoteItem.Enabled = false;
            PreviewManager.Finished += (s, e) => isAbortAtLastNoteItem.Enabled = true;

            var channelMovableItem = new ToolStripMenuItem(MainFormStrings.ChannelMovable, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsAnotherChannelEditable = item.Checked;
                noteView.EditbyCh = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsAnotherChannelEditable
            };
            var channelSoundsItem = new ToolStripMenuItem(MainFormStrings.ChannelSounds, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsAnotherChannelSounds = item.Checked;
                noteView.SoundbyCh = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsAnotherChannelSounds
            };

            var isFormSpeedItem = new ToolStripMenuItem(MainFormStrings.ChannelSpeeds, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsAnotherChannelFormSpeeds = item.Checked;
                FormSpeedbyCh = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsAnotherChannelFormSpeeds
            };



            var defset = new ExportSetting();
            if (ApplicationSettings.Default.DefaultExportSettings == null) ApplicationSettings.Default.DefaultExportSettings = new Dictionary<int, IExportSetting>();
            if (ApplicationSettings.Default.DefaultExportSettings.Count < defset.SettingColumns.Count)
            {
                foreach (var s in defset.SettingColumns.OrderBy(p => p.Key))
                {

                    if (!ApplicationSettings.Default.DefaultExportSettings.TryGetValue(s.Key, out var set))
                    {
                        ApplicationSettings.Default.DefaultExportSettings.Add(s.Key, s.Value);
                    }
                    // Console.WriteLine(defset.SettingColumns[s.Key].Title + " : " + defset.SettingColumns[s.Key].Value);
                }
            }
            var ExportSettings = ApplicationSettings.Default.DefaultExportSettings;
            /*ScoreBook.ExportsSettings はデフォルトとの差分だけ保存しているやつなんでデフォルトからそこだけ変える処理
            foreach ( var setting in ScoreBook.ExportSettings) 
            {
                ExportSettings[setting.Key] = setting.Value;
            }
            ですがよく考えたらデフォルトのやつ置くだけなので必要なかった
            */

            var tapSettings = new List<ToolStripMenuItem>();
            var tap2Settings = new List<ToolStripMenuItem>();
            var extapSettings = new List<ToolStripMenuItem>();
            var extap2Settings = new List<ToolStripMenuItem>();
            var flickSettings = new List<ToolStripMenuItem>();
            var flick2Settings = new List<ToolStripMenuItem>();
            var damageSettings = new List<ToolStripMenuItem>();
            var damage2Settings = new List<ToolStripMenuItem>();
            var airSettings = new List<ToolStripMenuItem>();
            var slideSettings = new List<ToolStripMenuItem>();
            var guideSettings = new List<ToolStripMenuItem>();


            foreach (var setting in ExportSettings.Where(p => p.Value.Category2 == 0).OrderBy(p => p.Value.ID))
            {
                switch (setting.Value.Type)
                {

                    case SettingTypes.b:
                        var settingboolitem = new ToolStripMenuItem(setting.Value.Title, null, (s, e) =>
                        {
                            var item = s as ToolStripMenuItem;

                            setting.Value.Value[0] = (!item.Checked).ToString(); 
                            ApplicationSettings.Default.DefaultExportSettings[setting.Key] = setting.Value;
                            item.Tag = new object[] { setting.Value.ID, bool.Parse(setting.Value.Value[0]), 0 };

                            SettingChanged?.Invoke(this, EventArgs.Empty);
                        })
                        {
                            Checked = bool.Parse(setting.Value.Value[0]),
                            Tag = new object[] { setting.Value.ID, bool.Parse(setting.Value.Value[0]), 0}
                            
                        };
                        switch (setting.Value.Category)
                        {

                            case "TAP":

                                tapSettings.Add(settingboolitem);
                                break;
                            case "TAP2":
                                tap2Settings.Add(settingboolitem);
                                break;
                            case "ExTAP":
                                extapSettings.Add(settingboolitem);
                                break;
                            case "ExTAP2":
                                extap2Settings.Add(settingboolitem);
                                break;
                            case "FLICK":
                                flickSettings.Add(settingboolitem);
                                break;
                            case "FLICK2":
                                flick2Settings.Add(settingboolitem);
                                break;
                            case "DAMAGE":
                                damageSettings.Add(settingboolitem);
                                break;
                            case "DAMAGE2":
                                damage2Settings.Add(settingboolitem);
                                break;
                            case "SLIDE":
                                slideSettings.Add(settingboolitem);
                                break;
                            case "GUIDE":
                                guideSettings.Add(settingboolitem);
                                break;
                            default:
                                break;
                        }
                        break;
                    case SettingTypes.i:
                        var choices = new List<ToolStripMenuItem>();
                        foreach (var choice in ((ExportIntSetting)setting.Value).Choices)
                        {
                            var choiceitem = new ToolStripMenuItem(choice, null, (s, e) =>
                            {
                                var item = s as ToolStripMenuItem;

                                setting.Value.Value[0] = ((ExportIntSetting)setting.Value).Choices.IndexOf(choice).ToString();

                                Console.WriteLine(setting.Value.Default[0] + " : " + ((ExportIntSetting)setting.Value).Choices.IndexOf(choice));
                                ApplicationSettings.Default.DefaultExportSettings[setting.Key] = setting.Value;
                                item.Tag = new object[] { setting.Value.ID, ((ExportIntSetting)setting.Value).Choices.IndexOf(choice), 1 };
                                SettingChanged?.Invoke(this, EventArgs.Empty );


                            })
                            {
                                Checked = setting.Value.Default[0] == ((ExportIntSetting)setting.Value).Choices.IndexOf(choice).ToString(),
                                Tag = new object[] { setting.Value.ID, ((ExportIntSetting)setting.Value).Choices.IndexOf(choice), 1 }
                            };
                            choices.Add(choiceitem);
                        }
                        //ドロップダウンを動かしたときに全部修正できればいいよなー(次回予告)
                        var settingintitem = new ToolStripMenuItem(setting.Value.Title, null, choices.ToArray()) { Tag = new object[] { setting.Value.ID, -1, 1 } };
                        switch (setting.Value.Category)
                        {

                            case "TAP":

                                tapSettings.Add(settingintitem);
                                break;
                            case "TAP2":
                                tap2Settings.Add(settingintitem);
                                break;
                            case "ExTAP":
                                extapSettings.Add(settingintitem);
                                break;
                            case "ExTAP2":
                                extap2Settings.Add(settingintitem);
                                break;
                            case "FLICK":
                                flickSettings.Add(settingintitem);
                                break;
                            case "FLICK2":
                                flick2Settings.Add(settingintitem);
                                break;
                            case "DAMAGE":
                                damageSettings.Add(settingintitem);
                                break;
                            case "DAMAGE2":
                                damage2Settings.Add(settingintitem);
                                break;
                            case "SLIDE":
                                slideSettings.Add(settingintitem);
                                break;
                            case "GUIDE":
                                guideSettings.Add(settingintitem);
                                break;
                            default:
                                break;
                        }
                        break;
                    case SettingTypes.list:
                        break;
                    default:
                        break;
                }
                
            }

            
            //カテゴリー2を設定するだけでネストさせたい、ディクショナリーを使って存在確認するとか、ExportSettingの方にtype.setting でmenu 
            //デフォルトの設定の方にここで決定した設定を入れる(次回予告)
            var TapmenuDic = new Dictionary<int, List<ToolStripMenuItem>>();
            var TapmenuDic2 = new Dictionary<int, List<ToolStripMenuItem>>();
            var TapmenuDicsub = new Dictionary<int, Dictionary<int, List<ToolStripMenuItem>>>();

            var TapCate2Dic = new Dictionary<int, List<ToolStripMenuItem>>();
            var TapCate3Dic = new Dictionary<int, List<ToolStripMenuItem>>();
            var TapTotalDic = new Dictionary<int, Dictionary<int, List<ToolStripMenuItem>>>();
            //2-0 3-0 2-1 3-0 2-1 3-1

            foreach (var setting in ExportSettings.Where(p => p.Value.Category2 > 0).OrderBy(p => p.Value.ID))
            {

                switch (setting.Value.Type)
                {

                    case SettingTypes.b:
                        var settingboolitem = new ToolStripMenuItem(setting.Value.Title, null, (s, e) =>
                        {
                            var item = s as ToolStripMenuItem;

                            setting.Value.Value[0] = (!item.Checked).ToString();

                            ApplicationSettings.Default.DefaultExportSettings[setting.Key] = setting.Value;
                            item.Tag = new object[] { setting.Value.ID, bool.Parse(setting.Value.Value[0]), 0 };

                            SettingChanged?.Invoke(this, EventArgs.Empty);
                        })
                        {
                            Checked = bool.Parse(setting.Value.Value[0]),
                            Tag = new object[] { setting.Value.ID, bool.Parse(setting.Value.Value[0]), 0}
                        };
                        switch (setting.Value.Category)
                        {

                            case "TAP":
                                //Console.WriteLine(setting.Value.Title + " bool " + setting.Value.Category2 + " " + setting.Value.Category3);
                                if (!TapTotalDic.ContainsKey(setting.Value.Category2)) //カテゴリ2が登録されていない場合
                                {
                                    var cate2dic = new Dictionary<int, List<ToolStripMenuItem>>(); 
                                    TapTotalDic.Add(setting.Value.Category2, cate2dic); //カテゴリ2の辞書を作成し追加、この辞書の数値はカテゴリ3のもの
                                }

                                if (!TapTotalDic[setting.Value.Category2].ContainsKey(setting.Value.Category3)) //カテゴリ3が登録されていない場合
                                {
                                     var cate3list = new List<ToolStripMenuItem>();
                                     TapTotalDic[setting.Value.Category2].Add(setting.Value.Category3, cate3list);
                                }
                                 TapTotalDic[setting.Value.Category2][setting.Value.Category3].Add(settingboolitem);


                                break;
                            case "TAP2":
                                break;
                            case "ExTAP":
                                break;
                            case "ExTAP2":
                                break;
                            default:
                                break;
                        }
                        break;
                    case SettingTypes.i:
                        
                        var choices = new List<ToolStripMenuItem>();
                        foreach(var choice in ((ExportIntSetting)setting.Value).Choices)
                        {
                            var choiceitem = new ToolStripMenuItem(choice, null, (s, e) =>
                            {
                                var item = s as ToolStripMenuItem;

                                setting.Value.Value[0] = ((ExportIntSetting)setting.Value).Choices.IndexOf(choice).ToString();

                                Console.WriteLine(setting.Value.Default[0] + " : " + ((ExportIntSetting)setting.Value).Choices.IndexOf(choice));

                                ApplicationSettings.Default.DefaultExportSettings[setting.Key] = setting.Value;
                                item.Tag = new object[] { setting.Value.ID, ((ExportIntSetting)setting.Value).Choices.IndexOf(choice), 1 };
                                SettingChanged?.Invoke(this, EventArgs.Empty);
                            })
                            {
                                Checked = setting.Value.Default[0] == ((ExportIntSetting)setting.Value).Choices.IndexOf(choice).ToString(),
                                Tag = new object[] { setting.Value.ID, ((ExportIntSetting)setting.Value).Choices.IndexOf(choice), 1 }
                            };
                            choices.Add(choiceitem);
                        }
                        
                        var settingintitem = new ToolStripMenuItem(setting.Value.Title, null, choices.ToArray()) { Tag = new object[] { setting.Value.ID, -1, 1 } };
                        switch (setting.Value.Category)
                        {

                            case "TAP":
                                //Console.WriteLine(setting.Value.Title + " int " + setting.Value.Category2 + " " + setting.Value.Category3);
                                if (!TapTotalDic.ContainsKey(setting.Value.Category2)) //カテゴリ2が登録されていない場合
                                {
                                    var cate2dic = new Dictionary<int, List<ToolStripMenuItem>>();
                                    TapTotalDic.Add(setting.Value.Category2, cate2dic); //カテゴリ2の辞書を作成し追加、この辞書の数値はカテゴリ3のもの
                                }

                                if (!TapTotalDic[setting.Value.Category2].ContainsKey(setting.Value.Category3)) //カテゴリ3が登録されていない場合
                                {
                                    var cate3list = new List<ToolStripMenuItem>();
                                    TapTotalDic[setting.Value.Category2].Add(setting.Value.Category3, cate3list);
                                }
                                TapTotalDic[setting.Value.Category2][setting.Value.Category3].Add(settingintitem);
                                //Console.WriteLine("int " + settingintitem.Text + " " + TapTotalDic[setting.Value.Category2][setting.Value.Category3].Last().Text);



                                break;
                            case "TAP2":
                                break;
                            case "ExTAP":
                                break;
                            case "ExTAP2":
                                break;
                            default:
                                break;
                        }


                        break;
                    case SettingTypes.list:
                        break;
                    default:
                        break;
                }

            }

            foreach (var cate2items in TapTotalDic.Values) // カテゴリ2ごとに
            {
                var cate2Items = new List<ToolStripMenuItem>();
                var cate3Items = new List<ToolStripMenuItem>();
                foreach (var cate3items in cate2items) //カテゴリ2からカテゴリ3を取り出す、0はカテゴリ2に直
                {



                    if (cate3items.Key > 0)//カテゴリ3が存在する場合
                    {
                        var cate3Item = new ToolStripMenuItem(ExportSettings[int.Parse(((object[])cate3items.Value.First().Tag)[0].ToString())].Category3Name, ExportSettings[int.Parse(((object[])cate3items.Value.First().Tag)[0].ToString())].Category3Image, cate3items.Value.ToArray()) { Tag = new object[] { ((object[])cate3items.Value.First().Tag)[0], -1 } };
                        cate2Items.Add(cate3Item);
                    }
                    else//しない場合 カテゴリ2に直で配置
                    {
                        foreach (ToolStripMenuItem cate3item in cate3items.Value)
                        {
                            cate2Items.Add(cate3item);
                        }
                    }
                    
                }
                var cate2Item = new ToolStripMenuItem(ExportSettings[int.Parse(((object[])cate2Items.First().Tag)[0].ToString())].Category2Name, ExportSettings[int.Parse(((object[])cate2Items.First().Tag)[0].ToString())].Category2Image, cate2Items.ToArray());
                tapSettings.Add(cate2Item);
            }

            
            var Accuratedjudge = new ToolStripMenuItem(MainFormStrings.Accuratejudge, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsAccurateOverlap = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsAccurateOverlap
            };
            var ResetSettings = new ToolStripMenuItem(MainFormStrings.ResetSettings, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                if (MessageBox.Show(this, ErrorStrings.Reset, Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
                ApplicationSettings.Default.DefaultExportSettings = new Dictionary<int, IExportSetting>();
                if (ApplicationSettings.Default.DefaultExportSettings.Count < defset.SettingColumns.Count)
                {
                    foreach (var setting in defset.SettingColumns.OrderBy(p => p.Key))
                    {

                        if (!ApplicationSettings.Default.DefaultExportSettings.TryGetValue(setting.Key, out var set))
                        {
                            ApplicationSettings.Default.DefaultExportSettings.Add(setting.Key, setting.Value);
                            Console.WriteLine(setting.Value.Title + " " + setting.Value.Value[0]);
                        }
                    }
                }
            });

            var TapNoteItem = new ToolStripMenuItem("TAP", Resources.TapIcon, tapSettings.ToArray());
            var ExTapNoteItem = new ToolStripMenuItem("ExTAP", Resources.ExTapIcon, extapSettings.ToArray());
            var Tap2NoteItem = new ToolStripMenuItem("TAP2", Resources.TapIcon2, tap2Settings.ToArray());
            var ExTap2NoteItem = new ToolStripMenuItem("ExTAP2", Resources.ExTapIcon2, extap2Settings.ToArray());
            var FlickNoteItem = new ToolStripMenuItem("FLICK", Resources.FlickIcon, flickSettings.ToArray());
            var Flick2NoteItem = new ToolStripMenuItem("FLICK2", Resources.FlickIcon, flick2Settings.ToArray());
            var DamageNoteItem = new ToolStripMenuItem("DAMAGE", Resources.DamgeIcon, damageSettings.ToArray());
            var Damage2NoteItem = new ToolStripMenuItem("DAMAGE2", Resources.DamgeIcon, damage2Settings.ToArray());

            var SlideNoteItem = new ToolStripMenuItem("SLIDE", Resources.SlideIcon, slideSettings.ToArray());
            var GuideNoteItem = new ToolStripMenuItem("GUIDE", Resources.GuideGreen, guideSettings.ToArray());

            var NoteItems = new ToolStripMenuItem[]
            {
                TapNoteItem, ExTapNoteItem, Tap2NoteItem, ExTap2NoteItem, FlickNoteItem, DamageNoteItem, Flick2NoteItem, Damage2NoteItem,
                SlideNoteItem, GuideNoteItem,
            };

            var ExportNotesItems = new ToolStripMenuItem(MainFormStrings.Notes, null, NoteItems);

            var OpenStageManager = new ToolStripMenuItem(MainFormStrings.StageManager, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                var vm = new StageManagerWindowViewModel(ScoreBook);
                var window = new StageManagerWindow()
                {
                    DataContext = vm
                };
                if (window.ShowDialog(this) ?? false)
                {
                    ScoreBook = ((StageManagerWindowViewModel)window.DataContext).ScoreBook;
                    NoteView.Stages = ScoreBook.Stages;
                }
            });




            noteView.NoteVisualMode = ApplicationSettings.Default.NoteVisualMode;
            var notDisplay = new ToolStripMenuItem(MainFormStrings.Visual1, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.NoteVisualMode = 0;
                noteView.NoteVisualMode = 0;
                item.Checked = ApplicationSettings.Default.NoteVisualMode == 0;
            })
            {
                Checked = ApplicationSettings.Default.NoteVisualMode == 0
            };
            var translucentDisplay = new ToolStripMenuItem(MainFormStrings.Visual2, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.NoteVisualMode = 1;
                noteView.NoteVisualMode = 1;
                item.Checked = ApplicationSettings.Default.NoteVisualMode == 1;
            })
            {
                Checked = ApplicationSettings.Default.NoteVisualMode == 1
            };
            var Display = new ToolStripMenuItem(MainFormStrings.Visual3, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.NoteVisualMode = 2;
                noteView.NoteVisualMode = 2;
                item.Checked = ApplicationSettings.Default.NoteVisualMode == 2;
            })
            {
                Checked = ApplicationSettings.Default.NoteVisualMode == 2
            };
            var noteVisualModeItems = new ToolStripMenuItem[]
            {
             notDisplay, translucentDisplay, Display   
        };
            var noteVisualModeItem = new ToolStripMenuItem(MainFormStrings.ChannelNote, null, noteVisualModeItems);


            var playMenuItems = new ToolStripItem[]
            {
                playItem, stopItem, new ToolStripSeparator(),
                slowDownPreviewItem, isAbortAtLastNoteItem, isPjsekaiSounds
            };

            var helpMenuItems = new ToolStripItem[]
            {
                shortcutItemBuilder.BuildItem(Commands.ShowHelp, MainFormStrings.Help),
                new ToolStripMenuItem(MainFormStrings.VersionInfo, null, (s, e) => new VersionInfoForm().ShowDialog())
            };


            var themeMenuItems = new ToolStripItem[] { themeBlack, themeWhite};

            var channelMenuItems = new ToolStripItem[] { channelMovableItem, channelSoundsItem, noteVisualModeItem, changeChannelSelectedNotesItem, isFormSpeedItem };

            var exportMenuItems = new ToolStripItem[] { ExportNotesItems, Accuratedjudge, ResetSettings };




            OperationManager.OperationHistoryChanged += (s, e) =>
            {
                redoItem.Enabled = OperationManager.CanRedo;
                undoItem.Enabled = OperationManager.CanUndo;
            };

            var menu = new MenuStrip()
            {
                BackColor = Color.White,
                RenderMode = ToolStripRenderMode.Professional
            };

            noteView.ChannelVisualChanged += (s, e) =>
            {
                notDisplay.Checked = noteView.NoteVisualMode == 0;
                translucentDisplay.Checked = noteView.NoteVisualMode == 1;
                Display.Checked = noteView.NoteVisualMode == 2;

            };
            SettingChanged += (s, e) =>
            {
                foreach(var setting in tapSettings)
                {
                    //setting.Checked = true;
                    if (setting.Tag == null)
                    {
                        Console.WriteLine(setting.Text + " " + setting.HasDropDownItems + " 0");
                    }
                    else
                    {
                        Console.WriteLine(setting.Text + " " + setting.HasDropDownItems +  " " + ((object[])setting.Tag).Count());
                    }
                    
                    if (setting.HasDropDownItems)
                    {
                        foreach(ToolStripMenuItem cate2item in setting.DropDownItems)
                        {
                            if (cate2item.Tag == null)
                            {
                                Console.WriteLine(cate2item.Text + " " + cate2item.HasDropDownItems + " 0");
                            }
                            else
                            {
                                Console.WriteLine(cate2item.Text + " " + cate2item.HasDropDownItems + " " + ((object[])cate2item.Tag).Count());
                            }
                            if (cate2item.HasDropDownItems)
                            {
                                foreach (ToolStripMenuItem cate3item in cate2item.DropDownItems)
                                {
                                    if (cate3item.Tag == null)
                                    {
                                        Console.WriteLine(cate3item.Text + " " + cate3item.HasDropDownItems + " 0");
                                    }
                                    else
                                    {
                                        Console.WriteLine(cate3item.Text + " " + cate3item.HasDropDownItems + " " + ((object[])cate3item.Tag).Count());
                                    }
                                    if (cate3item.HasDropDownItems) //カテゴリー3中のintオプションの想定
                                    {
                                        foreach (ToolStripMenuItem cate4item in cate3item.DropDownItems)
                                        {
                                            switch ((int)((object[])cate4item.Tag)[2])
                                            {
                                                case 0: //bool

                                                    cate4item.Checked = (bool.Parse(ExportSettings[(int)((object[])cate4item.Tag)[0]].Value[0]) && bool.Parse(((object[])cate4item.Tag)[1].ToString()));
                                                    break;
                                                case 1: //int
                                                    cate4item.Checked = (ExportSettings[(int)((object[])cate4item.Tag)[0]].Value[0] == ((object[])cate4item.Tag)[1].ToString());
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        switch ((int)((object[])cate3item.Tag)[2])
                                        {
                                            case 0: //bool

                                                cate3item.Checked = (bool.Parse(ExportSettings[(int)((object[])cate3item.Tag)[0]].Value[0]) && bool.Parse(((object[])cate3item.Tag)[1].ToString()));
                                                break;
                                            case 1: //int
                                                cate3item.Checked = (ExportSettings[(int)((object[])cate3item.Tag)[0]].Value[0] == ((object[])cate3item.Tag)[1].ToString());
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                switch ((int)((object[])cate2item.Tag)[2])
                                {
                                    case 0: //bool

                                        cate2item.Checked = (bool.Parse(ExportSettings[(int)((object[])cate2item.Tag)[0]].Value[0]) && bool.Parse(((object[])cate2item.Tag)[1].ToString()));
                                        break;
                                    case 1: //int
                                        cate2item.Checked = (ExportSettings[(int)((object[])cate2item.Tag)[0]].Value[0] == ((object[])cate2item.Tag)[1].ToString());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }else
                    {
                        Console.WriteLine(ExportSettings[(int)((object[])setting.Tag)[0]].Title + " " + ExportSettings[(int)((object[])setting.Tag)[0]].Value[0] + " : " + ((object[])setting.Tag)[1].ToString());
                        //if(ExportSettings[(int)((object[])setting.Tag)[0]].Value[0])
                        switch((int)((object[])setting.Tag)[2]){
                            case 0: //bool
                                
                                setting.Checked = (bool.Parse(ExportSettings[(int)((object[])setting.Tag)[0]].Value[0]) && bool.Parse(((object[])setting.Tag)[1].ToString()));
                                break;
                            case 1: //int
                                setting.Checked = (ExportSettings[(int)((object[])setting.Tag)[0]].Value[0] == ((object[])setting.Tag)[1].ToString());
                                break;
                            default:
                                break;
                        }
                        
                    }
                    
                }
            };


            menu.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem(MainFormStrings.FileMenu, null, fileMenuItems),
                new ToolStripMenuItem(MainFormStrings.EditMenu, null, editMenuItems),
                new ToolStripMenuItem(MainFormStrings.ViewMenu, null, viewMenuItems),
                new ToolStripMenuItem(MainFormStrings.InsertMenu, null, insertMenuItems),
                // PreviewManager初期化後じゃないといけないのダメ設計でしょ
                new ToolStripMenuItem(MainFormStrings.PlayMenu, null, playMenuItems) { Enabled = PreviewManager.IsSupported },
                new ToolStripMenuItem(MainFormStrings.HelpMenu, null, helpMenuItems),
                new ToolStripMenuItem(MainFormStrings.ThemeMenu, null, themeMenuItems),
                new ToolStripMenuItem(MainFormStrings.ChannelMenu, null, channelMenuItems),
                new ToolStripMenuItem("USC" +  MainFormStrings.Export, null, exportMenuItems),
                new ToolStripMenuItem( MainFormStrings.Stage, null, OpenStageManager),
            });
            return menu;
        }

        private ToolStrip CreateMainToolStrip(NoteView noteView)
        {
            var shortcutItemBuilder = new ToolStripButtonBuilder(ShortcutManager);

            var newFileButton = shortcutItemBuilder.BuildItem(Commands.NewFile, MainFormStrings.NewFile, Resources.NewFileIcon);
            var openFileButton = shortcutItemBuilder.BuildItem(Commands.OpenFile, MainFormStrings.OpenFile, Resources.OpenFileIcon);
            var saveFileButton = shortcutItemBuilder.BuildItem(Commands.Save, MainFormStrings.SaveFile, Resources.SaveFileIcon);
            var exportButton = shortcutItemBuilder.BuildItem(Commands.ReExport, MainFormStrings.Export, Resources.ExportIcon);

            var cutButton = shortcutItemBuilder.BuildItem(Commands.Cut, MainFormStrings.Cut, Resources.CutIcon);
            var copyButton = shortcutItemBuilder.BuildItem(Commands.Copy, MainFormStrings.Copy, Resources.CopyIcon);
            var pasteButton = shortcutItemBuilder.BuildItem(Commands.Paste, MainFormStrings.Paste, Resources.PasteIcon);

            var undoButton = shortcutItemBuilder.BuildItem(Commands.Undo, MainFormStrings.Undo, Resources.UndoIcon);
            undoButton.Enabled = false;
            var redoButton = shortcutItemBuilder.BuildItem(Commands.Redo, MainFormStrings.Redo, Resources.RedoIcon);
            redoButton.Enabled = false;

            var penButton = shortcutItemBuilder.BuildItem(Commands.SelectPen, MainFormStrings.Pen, Resources.EditIcon);
            var selectionButton = shortcutItemBuilder.BuildItem(Commands.SelectSelection, MainFormStrings.Selection, Resources.SelectionIcon);
            var eraserButton = shortcutItemBuilder.BuildItem(Commands.SelectEraser, MainFormStrings.Eraser, Resources.EraserIcon);
            var paintButton = shortcutItemBuilder.BuildItem(Commands.SelectPaint, MainFormStrings.Paint, Resources.PaintIcon);
            var propertyButton = shortcutItemBuilder.BuildItem(Commands.SelectProperty, MainFormStrings.Property, Resources.PropertyIcon);
            var markerButton = shortcutItemBuilder.BuildItem(Commands.SelectMarker, MainFormStrings.Marker, Resources.MarkerIcon);
            var stepeditorButton = shortcutItemBuilder.BuildItem(Commands.SelectStepEditor, MainFormStrings.StepEditor, Resources.StepEditorIcon);
            var eventeditorButton = shortcutItemBuilder.BuildItem(Commands.SelectEventEditor, MainFormStrings.EventEditor, Resources.EventEditor);

            var zoomInButton = shortcutItemBuilder.BuildItem(Commands.ZoomIn, MainFormStrings.ZoomIn, Resources.ZoomInIcon);
            zoomInButton.Enabled = CanZoomIn;

            var zoomOutButton = shortcutItemBuilder.BuildItem(Commands.ZoomOut, MainFormStrings.ZoomOut, Resources.ZoomOutIcon);
            zoomOutButton.Enabled = CanZoomOut;

            var eventKind = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            eventKind.Text = MainFormStrings.Event;
            eventKind.Click += (s, e) => { 
                noteView.EditMode = EditMode.EventEdit;
                if((int)noteView.EventMode > 7)
                    noteView.EventMode = EventEditMode.Edit;
            };
            eventKind.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem(MainFormStrings.Edit, Resources.EditIcon, (s, e) => noteView.EventMode = EventEditMode.Edit),
                new ToolStripMenuItem(MainFormStrings.Selection, Resources.SelectionIcon, (s, e) => noteView.EventMode = EventEditMode.Select),
                new ToolStripMenuItem(MainFormStrings.Eraser, Resources.EraserIcon, (s, e) => noteView.EventMode = EventEditMode.Erase),
                new ToolStripMenuItem(MainFormStrings.HighSpeed, Resources.Highspeed, (s, e) => noteView.EventMode = EventEditMode.Highspeed),
                new ToolStripMenuItem("BPM", Resources.Bpm, (s, e) => noteView.EventMode = EventEditMode.Bpm),
                new ToolStripMenuItem(MainFormStrings.Comment, Resources.Comment, (s, e) => noteView.EventMode = EventEditMode.Comment),
                new ToolStripMenuItem(MainFormStrings.Skill, Resources.Skill, (s, e) => noteView.EventMode = EventEditMode.Skill),
                new ToolStripMenuItem(MainFormStrings.Fever, Resources.Fever, (s, e) => noteView.EventMode = EventEditMode.Fever),
            });
            eventKind.Image = Resources.EditIcon;
            ShortcutManager.ShortcutUpdated += (s, e) =>
            {
                if (ShortcutManager.ResolveShortcutKey(Commands.SelectEvent, out Keys key))
                {
                    eventKind.Text = $"Event ({key.ToShortcutChar()})";
                    return;
                }
                eventKind.Text = "Event";
            };

            var eventKind2 = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            eventKind2.Text = MainFormStrings.Event;
            eventKind2.Click += (s, e) => {
                noteView.EditMode = EditMode.EventEdit;
                if ((int)noteView.EventMode < 8)
                    noteView.EventMode = EventEditMode.Camera;
            };
            eventKind2.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Camera Event", Resources.Camera, (s, e) => noteView.EventMode = EventEditMode.Camera),
                new ToolStripMenuItem("Stage Mask Event", Resources.Stage_mask, (s, e) => noteView.EventMode = EventEditMode.StageMask),
                new ToolStripMenuItem("Stage Pivot Event", Resources.Stage_pivot, (s, e) => noteView.EventMode = EventEditMode.StagePivot),
                new ToolStripMenuItem("Stage Style Event", Resources.Stage_style, (s, e) => noteView.EventMode = EventEditMode.StageStyle),
                new ToolStripMenuItem("Stage Transform Event", Resources.Stage_transform, (s, e) => noteView.EventMode = EventEditMode.StageTransform),
            });
            eventKind2.Image = Resources.StageEditorIcon;
            ShortcutManager.ShortcutUpdated += (s, e) =>
            {
                if (ShortcutManager.ResolveShortcutKey(Commands.SelectStageEvent, out Keys key))
                {
                    eventKind2.Text = $"StageEvent ({key.ToShortcutChar()})";
                    return;
                }
                eventKind2.Text = "StageEvent";
            };


            NoteView.UnitBeatHeightChanged += (s, e) =>
            {
                zoomOutButton.Enabled = CanZoomOut;
                zoomInButton.Enabled = CanZoomIn;
                
                if (noteView.UnitBeatHeight > 240)
                {
                    if(noteView.UnitBeatHeight >= 960)
                    NoteViewScrollBar.SmallChange = 60;
                    else
                    NoteViewScrollBar.SmallChange = 120;
                }
                else
                {
                    NoteViewScrollBar.SmallChange = 480;
                }
            };

            OperationManager.OperationHistoryChanged += (s, e) =>
            {
                undoButton.Enabled = OperationManager.CanUndo;
                redoButton.Enabled = OperationManager.CanRedo;
                
            };

            noteView.EditModeChanged += (s, e) =>
            {
                selectionButton.Checked = noteView.EditMode == EditMode.Select;
                penButton.Checked = noteView.EditMode == EditMode.Edit;
                eraserButton.Checked = noteView.EditMode == EditMode.Erase;
                paintButton.Checked = noteView.EditMode == EditMode.Paint;
                propertyButton.Checked = noteView.EditMode == EditMode.Property;
                markerButton.Checked = noteView.EditMode == EditMode.Marker;
                stepeditorButton.Checked = noteView.EditMode == EditMode.StepEdit;
                eventeditorButton.Checked = noteView.EditMode == EditMode.EventEdit;
                if(noteView.EditMode == EditMode.EventEdit)
                {
                    NoteView.EventMode = NoteView.EventMode;
                    if ((int)noteView.EventMode < 8 )
                    eventKind.Checked = true;
                    else eventKind2.Checked = true;
                }
            };
            noteView.EventEditModeChanged += (s, e) =>
            {
                switch (noteView.EventMode)
                {
                    case EventEditMode.Select:
                        eventKind.Image = Resources.SelectionIcon;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Edit:
                        eventKind.Image = Resources.EditIcon;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Erase:
                        eventKind.Image = Resources.EraserIcon;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Highspeed:
                        eventKind.Image = Resources.Highspeed;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Bpm:
                        eventKind.Image = Resources.Bpm;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Comment:
                        eventKind.Image = Resources.Comment;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Skill:
                        eventKind.Image = Resources.Skill;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Fever:
                        eventKind.Image = Resources.Fever;
                        eventKind2.Image = Resources.StageEditorIcon;
                        eventKind.Checked = true;
                        eventKind2.Checked = false;
                        break;
                    case EventEditMode.Camera:
                        eventKind.Image = Resources.EditIcon;
                        eventKind2.Image = Resources.Camera;
                        eventKind.Checked = false;
                        eventKind2.Checked = true;
                        break;
                    case EventEditMode.StageMask:
                        eventKind.Image = Resources.EditIcon;
                        eventKind2.Image = Resources.Stage_mask;
                        eventKind.Checked = false;
                        eventKind2.Checked = true;
                        break;
                    case EventEditMode.StagePivot:
                        eventKind.Image = Resources.EditIcon;
                        eventKind2.Image = Resources.Stage_pivot;
                        eventKind.Checked = false;
                        eventKind2.Checked = true;
                        break;
                    case EventEditMode.StageStyle:
                        eventKind.Image = Resources.EditIcon;
                        eventKind2.Image = Resources.Stage_style;
                        eventKind.Checked = false;
                        eventKind2.Checked = true;
                        break;
                    case EventEditMode.StageTransform:
                        eventKind.Image = Resources.EditIcon;
                        eventKind2.Image = Resources.Stage_transform;
                        eventKind.Checked = false;
                        eventKind2.Checked = true;

                        break;
                }

            };



            var scrollAmountCounts = new float[]
            {
                0.1f, 0.3f, 0.5f, 0.8f, 1f, 1.5f, 1.8f, 2f, 2.3f, 2.5f, 2.8f, 3f, 3.3f, 3.5f, 3.8f, 4f, 4.3f, 4.5f, 4.8f, 5f, 5.3f, 5.5f, 5.8f, 6f, 6.5f, 7f, 7.5f, 8f, 8.5f, 9f, 9.5f, 10f
            };


            var scrollAmountBox = new ToolStripComboBox("スクロール変化量")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 60
            };
            scrollAmountBox.Items.AddRange(scrollAmountCounts.Select(p => "/ " + p).ToArray());

            scrollAmountBox.SelectedIndexChanged += (s, e) =>
            {

                ScrollAmount = scrollAmountCounts[scrollAmountBox.SelectedIndex];
                noteView.ScrollAmount = ScrollAmount;
                ApplicationSettings.Default.ScrollAmount = ScrollAmount;
                noteView.Update();
                noteView.Focus();
            };

            scrollAmountBox.SelectedIndex = scrollAmountCounts.ToList().IndexOf(ApplicationSettings.Default.ScrollAmount);



            return new ToolStrip(new ToolStripItem[]
            {
                newFileButton, openFileButton, saveFileButton, exportButton, new ToolStripSeparator(),
                cutButton, copyButton, pasteButton, new ToolStripSeparator(),
                undoButton, redoButton, new ToolStripSeparator(),
                penButton, selectionButton, eraserButton, paintButton, propertyButton, markerButton, stepeditorButton, eventeditorButton, new ToolStripSeparator(),
                eventKind,eventKind2, new ToolStripSeparator(),
                zoomInButton, zoomOutButton, new ToolStripSeparator(),
                scrollAmountBox
                
            });

        }

        private ToolStrip CreateNewNoteTypeToolStrip(NoteView noteView)
        {
            var shortcutItemBuilder = new ToolStripButtonBuilder(ShortcutManager);

            var tapButton = shortcutItemBuilder.BuildItem(Commands.SelectTap, "TAP", Resources.TapIcon);
            var exTapButton = shortcutItemBuilder.BuildItem(Commands.SelectExTap, "ExTAP", Resources.ExTapIcon);
            var holdButton = shortcutItemBuilder.BuildItem(Commands.SelectHold, "HOLD", Resources.HoldIcon);
            var slideButton = shortcutItemBuilder.BuildItem(Commands.SelectSlide, "SLIDE", Resources.SlideIcon);
            var slideStepButton = shortcutItemBuilder.BuildItem(Commands.SelectSlideStep, MainFormStrings.SlideStep, Resources.SlideStepIcon);
            var airActionButton = shortcutItemBuilder.BuildItem(Commands.SelectAirAction, "AIR-ACTION", Resources.AirActionIcon);
            var flickButton = shortcutItemBuilder.BuildItem(Commands.SelectFlick, "FLICK", Resources.FlickIcon);
            var damageButton = shortcutItemBuilder.BuildItem(Commands.SelectDamage, "DAMAGE", Resources.DamgeIcon);
            var guideButton = shortcutItemBuilder.BuildItem(Commands.SelectGuide, "GUIDE", Resources.GuideNeutral);
            var guideStepButton = shortcutItemBuilder.BuildItem(Commands.SelectGuideStep, "GUIDESTEP", Resources.GuideStepIcon);
            var tap2Button = shortcutItemBuilder.BuildItem(Commands.SelectTap2, "TAP2", Resources.TapIcon2);
            var exTap2Button = shortcutItemBuilder.BuildItem(Commands.SelectExTap2, "ExTAP2", Resources.ExTapIcon2);
            var stepNoteTapButton = shortcutItemBuilder.BuildItem(Commands.SelectStepNoteTap, "StepNoteTAP", Resources.ExTapIcon);
            var flick2Button = shortcutItemBuilder.BuildItem(Commands.SelectFlick2, "FLICK2", Resources.FlickIcon2);
            var damage2Button = shortcutItemBuilder.BuildItem(Commands.SelectDamage2, "DAMAGE2", Resources.DamgeIcon2);

            var eventSelectionButton = shortcutItemBuilder.BuildItem(Commands.SelectEventSelection, "Event Select Tool", Resources.SelectionIcon);
            var eventeditButton = shortcutItemBuilder.BuildItem(Commands.SelectEventEdit, "Event Edit Tool", Resources.EditIcon);
            var eventEraceButton = shortcutItemBuilder.BuildItem(Commands.SelectEventEracer, "Event Erace Tool", Resources.EraserIcon);
            var eventHighspeedButton = shortcutItemBuilder.BuildItem(Commands.SelectHighspeed, "Highspeed Tool", Resources.Highspeed);
            var eventBpmButton = shortcutItemBuilder.BuildItem(Commands.SelectBpm, "Bpm Tool", Resources.Bpm);
            var eventCommentButton = shortcutItemBuilder.BuildItem(Commands.SelectComment, "Comment Tool", Resources.Comment);
            var eventSkillButton = shortcutItemBuilder.BuildItem(Commands.SelectSkill, "Skill Tool", Resources.Skill);
            var eventFeverButton = shortcutItemBuilder.BuildItem(Commands.SelectFever, "Fever Tool", Resources.Fever);
            var eventCameraButton = shortcutItemBuilder.BuildItem(Commands.SelectCamera, "Camera Event Tool", Resources.Camera);
            var eventStageMaskButton = shortcutItemBuilder.BuildItem(Commands.SelectStageMask, "Camera Mask Event Tool", Resources.Stage_mask);
            var eventStagePivotButton = shortcutItemBuilder.BuildItem(Commands.SelectStagePivot, "Camera Pivot Event Tool", Resources.Stage_pivot);
            var eventStageStyleButton = shortcutItemBuilder.BuildItem(Commands.SelectStageStyle, "Camera Style Event Tool", Resources.Stage_style);
            var eventStageTransformButton = shortcutItemBuilder.BuildItem(Commands.SelectStageTransform, "Camera Transform Event Tool", Resources.Stage_transform);




            var airKind = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            airKind.Text = "AIR";
            airKind.Click += (s, e) => noteView.NewNoteType = NoteType.Air;
            airKind.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem(MainFormStrings.AirUp, Resources.AirUpIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Up, HorizontalAirDirection.Center)),
                new ToolStripMenuItem(MainFormStrings.AirLeftUp, Resources.AirLeftUpIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Up, HorizontalAirDirection.Left)),
                new ToolStripMenuItem(MainFormStrings.AirRightUp, Resources.AirRightUpIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Up, HorizontalAirDirection.Right)),
                new ToolStripMenuItem(MainFormStrings.AirDown, Resources.AirDownIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Down, HorizontalAirDirection.Center)),
                new ToolStripMenuItem(MainFormStrings.AirLeftDown, Resources.AirLeftDownIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Down, HorizontalAirDirection.Left)),
                new ToolStripMenuItem(MainFormStrings.AirRightDown, Resources.AirRightDownIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Down, HorizontalAirDirection.Right)),
                new ToolStripMenuItem(MainFormStrings.AirHandy, Resources.AirOtherIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Other, HorizontalAirDirection.Center))
            });
            airKind.Image = Resources.AirUpIcon;
            ShortcutManager.ShortcutUpdated += (s, e) =>
            {
                if (ShortcutManager.ResolveShortcutKey(Commands.SelectAir, out Keys key))
                {
                    airKind.Text = $"AIR ({key.ToShortcutChar()})";
                    return;
                }
                airKind.Text = "AIR";
            };
            


            var guideKind = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image
            };
            guideKind.Text = "GUIDE";
            guideKind.Click += (s, e) =>
            {
                noteView.NewNoteType = NoteType.Guide;
                noteView.IsNewGuideStepVisible = false;
                

            };
            guideKind.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem(MainFormStrings.ColorNeutral, Resources.GuideNeutral, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.neutral),
                new ToolStripMenuItem(MainFormStrings.ColorRed, Resources.GuideRed, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.red),
                new ToolStripMenuItem(MainFormStrings.ColorGreen, Resources.GuideGreen, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.green),
                new ToolStripMenuItem(MainFormStrings.ColorBlue, Resources.GuideBlue, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.blue),
                new ToolStripMenuItem(MainFormStrings.ColorYellow, Resources.GuideYellow, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.yellow),
                new ToolStripMenuItem(MainFormStrings.ColorPurple, Resources.GuidePurple, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.purple),
                new ToolStripMenuItem(MainFormStrings.ColorCyan, Resources.GuideCyan, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.cyan),
                new ToolStripMenuItem(MainFormStrings.ColorBlack, Resources.GuideBlack, (s, e) => noteView.NewGuideColor = Guide.USCGuideColor.black),
            });
            guideKind.Image = Resources.GuideNeutral;
            ShortcutManager.ShortcutUpdated += (s, e) =>
            {
                if (ShortcutManager.ResolveShortcutKey(Commands.SelectGuide, out Keys key))
                {
                    guideKind.Text = $"GUIDE ({key.ToShortcutChar()})";
                    return;
                }
                guideKind.Text = "GUIDE";
            };

            



            var quantizeTicks = new int[]
            {
                4, 8, 12, 16, 24, 32, 48, 64, 96, 128, 144, 192, 240, 256, 384, 480, 512, 576, 768, 960, 1024, 1152, 1920
            };
            var quantizeComboBox = new ToolStripComboBox("クォンタイズ")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 80
            };
            quantizeComboBox.Items.AddRange(quantizeTicks.Select(p => p + MainFormStrings.Division).ToArray());
            quantizeComboBox.Items.Add(MainFormStrings.Custom);
            quantizeComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (quantizeComboBox.SelectedIndex == quantizeComboBox.Items.Count - 1)
                {
                    // ユーザー定義
                    var form = new CustomQuantizeSelectionForm(ScoreBook.Score.TicksPerBeat * 4);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        noteView.QuantizeTick = form.QuantizeTick;
                    }
                }
                else
                {
                    noteView.QuantizeTick = noteView.UnitBeatTick * 4 / quantizeTicks[quantizeComboBox.SelectedIndex];
                }
                noteView.Focus();
            };
            quantizeComboBox.SelectedIndex = 1;

            noteView.NewNoteTypeChanged += (s, e) =>
            {
                tapButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Tap) && !noteView.IsNewNoteStart;
                exTapButton.Checked = noteView.NewNoteType.HasFlag(NoteType.ExTap) && !noteView.IsNewNoteStart;
                holdButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Hold);
                slideButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Slide) && !noteView.IsNewSlideStepVisible;
                slideStepButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Slide) && noteView.IsNewSlideStepVisible;
                airKind.Checked = noteView.NewNoteType.HasFlag(NoteType.Air);
                airActionButton.Checked = noteView.NewNoteType.HasFlag(NoteType.AirAction);
                flickButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Flick) && !noteView.IsNewNoteStart;
                damageButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Damage) && !noteView.IsNewNoteStart;
                stepNoteTapButton.Checked = noteView.NewNoteType.HasFlag(NoteType.StepNoteTap);
                guideButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Guide) && !noteView.IsNewGuideStepVisible;
                guideStepButton.Checked = noteView.NewNoteType.HasFlag(NoteType.Guide) && noteView.IsNewGuideStepVisible;
                tap2Button.Checked = noteView.NewNoteType.HasFlag(NoteType.Tap) && noteView.IsNewNoteStart;
                exTap2Button.Checked = noteView.NewNoteType.HasFlag(NoteType.ExTap) && noteView.IsNewNoteStart;
                guideKind.Checked = noteView.NewNoteType.HasFlag(NoteType.Guide) && !noteView.IsNewGuideStepVisible;
                flick2Button.Checked = noteView.NewNoteType.HasFlag(NoteType.Flick) && noteView.IsNewNoteStart;
                damage2Button.Checked = noteView.NewNoteType.HasFlag(NoteType.Damage) && noteView.IsNewNoteStart;
            };

            noteView.AirDirectionChanged += (s, e) =>
            {
                switch (noteView.AirDirection.HorizontalDirection)
                {
                    case HorizontalAirDirection.Center:
                        airKind.Image = noteView.AirDirection.VerticalDirection == VerticalAirDirection.Up ? Resources.AirUpIcon : Resources.AirDownIcon;
                        if (noteView.AirDirection.VerticalDirection == VerticalAirDirection.Other) airKind.Image = Resources.AirOtherIcon;
                        break;

                    case HorizontalAirDirection.Left:
                        airKind.Image = noteView.AirDirection.VerticalDirection == VerticalAirDirection.Up ? Resources.AirLeftUpIcon : Resources.AirLeftDownIcon;
                        break;

                    case HorizontalAirDirection.Right:
                        airKind.Image = noteView.AirDirection.VerticalDirection == VerticalAirDirection.Up ? Resources.AirRightUpIcon : Resources.AirRightDownIcon;
                        break;
                }
            };

            noteView.GuideColorChanged += (s, e) =>
            {
                switch (noteView.NewGuideColor)
                {
                    case Guide.USCGuideColor.neutral:
                        guideKind.Image = Resources.GuideNeutral;
                        break;
                    case Guide.USCGuideColor.red:
                        guideKind.Image = Resources.GuideRed;
                        break;
                    case Guide.USCGuideColor.green:
                        guideKind.Image = Resources.GuideGreen;
                        break;
                    case Guide.USCGuideColor.blue:
                        guideKind.Image = Resources.GuideBlue;
                        break;
                    case Guide.USCGuideColor.yellow:
                        guideKind.Image = Resources.GuideYellow;
                        break;
                    case Guide.USCGuideColor.purple:
                        guideKind.Image = Resources.GuidePurple;
                        break;
                    case Guide.USCGuideColor.cyan:
                        guideKind.Image = Resources.GuideCyan;
                        break;
                    case Guide.USCGuideColor.black:
                        guideKind.Image = Resources.GuideBlack;
                        break;
                }
            };

            




            var speedchCounts = new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };


            var speedChBox = new ToolStripComboBox("ハイスピードチャンネル")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Width = 60
            };
            if(ScoreBook == null)
            {
                speedChBox.Items.AddRange(speedchCounts.Select(p => "Ch" + p).ToArray());
                Console.WriteLine("not");
            }
            else
            {
                speedChBox.Items.AddRange(ScoreBook.ChannelNames.Values.Select(p => "Ch" + p).ToArray());
                Console.WriteLine(ScoreBook.ChannelNames);
            }
            

            
            speedChBox.Items.Add(MainFormStrings.Custom);

            speedChBox.SelectedIndexChanged += (s, e) =>
            {
                if (speedChBox.SelectedIndex == speedChBox.Items.Count - 1)
                {
                    var form = new CustomChSelectionForm();
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Channel = form.SpeedCh;
                        if(!speedChBox.Items.Contains(form.SpeedCh))
                        speedChBox.Items.Insert(speedChBox.Items.Count - 1, form.SpeedCh);
                        speedChBox.SelectedItem = form.SpeedCh;
                    }
                }
                else
                {
                    if (speedChBox.SelectedIndex > 10)
                    {
                        Channel = int.Parse(speedChBox.SelectedItem.ToString());
                    }
                    else
                    {
                        Channel = speedchCounts[speedChBox.SelectedIndex];
                    }
                    
                }
                noteView.Channel = Channel;
                noteView.Update();
                noteView.Focus();
            };
            
            speedChBox.SelectedIndex = 0;


            var viewchCounts = new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
            };


            var viewChBox = new ToolStripComboBox("表示チャンネル")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Width = 60
            };
            viewChBox.Items.Add(MainFormStrings.All);
            viewChBox.Items.AddRange(viewchCounts.Select(p => "Ch" + p).ToArray());
            viewChBox.Items.Add(MainFormStrings.Custom);

            viewChBox.SelectedIndexChanged += (s, e) =>
            {
                if (viewChBox.SelectedIndex == viewChBox.Items.Count - 1)
                {
                    var form = new CustomViewChSelectionForm();
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        ViewChannel = form.SpeedCh;
                        if (!viewChBox.Items.Contains(form.SpeedCh))
                            viewChBox.Items.Insert(viewChBox.Items.Count - 1, form.SpeedCh);
                        viewChBox.SelectedItem = form.SpeedCh;
                    }
                }
                else if (viewChBox.SelectedIndex == 0)
                {
                    ViewChannel = -1;
                }
                else
                {
                    if (viewChBox.SelectedIndex > 11)
                    {
                        ViewChannel = int.Parse(viewChBox.SelectedItem.ToString());
                    }
                    else
                    {
                        ViewChannel = viewchCounts[viewChBox.SelectedIndex - 1];
                    }
                }
                noteView.ViewChannel = ViewChannel;
                noteView.Update();
                noteView.Focus();
            };
            viewChBox.SelectedIndex = 0;


            var widthAmountCounts = new float[]
            {
                2, 1, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f, 0.1f, 0.05f, 0.01f
            };


            var widthAmountBox = new ToolStripComboBox("幅変化量")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 60
            };
            widthAmountBox.Items.AddRange(widthAmountCounts.Select(p => "" + p).ToArray());

            widthAmountBox.SelectedIndexChanged += (s, e) =>
            {

                    WidthAmount= widthAmountCounts[widthAmountBox.SelectedIndex];
                noteView.WidthAmount = WidthAmount;
                noteView.Update();
                noteView.Focus();
            };
            widthAmountBox.SelectedIndex = 1;

            ToolStripMenuItem laneVisible = new ToolStripMenuItem(MainFormStrings.LaneVisual, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                NoteView.LaneVisual = item.Checked;
                item.Text = MainFormStrings.LaneVisual + ": " + item.Checked;
            })
            {
                Checked = false
            };


            ToolStripMenuItem deleteChhistory = new ToolStripMenuItem(MainFormStrings.ChannelReload, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                speedChBox.Items.Clear();
                viewChBox.Items.Clear();
                /*
                speedChBox.Items.AddRange(speedchCounts.Select(p => "Ch" + p).ToArray());
                speedChBox.Items.Add(MainFormStrings.Custom);

                viewChBox.Items.Add(MainFormStrings.All);
                viewChBox.Items.AddRange(viewchCounts.Select(p => "Ch" + p).ToArray());
                viewChBox.Items.Add(MainFormStrings.Custom);
                */

                viewChBox.Items.Add(MainFormStrings.All);
                viewChBox.Items.AddRange(ScoreBook.ChannelNames.Values.Select(p =>  p).ToArray());
                viewChBox.Items.Add(MainFormStrings.Custom);


                speedChBox.Items.AddRange(ScoreBook.ChannelNames.Values.Select(p =>p).ToArray());
                speedChBox.Items.Add(MainFormStrings.Custom);


                speedChBox.SelectedIndex = 0;
                viewChBox.SelectedIndex = 0;
            })
            {
                Checked = false
            };
            ToolStripMenuItem nameChannel = new ToolStripMenuItem(MainFormStrings.ChannelSetName, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                if(noteView.Channel < 11)
                {
                    var form = new NameChannelForm()
                    {
                        ChannelName = ScoreBook.ChannelNames[noteView.Channel]
                    };

                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var spS = speedChBox.SelectedIndex;
                        var viS = viewChBox.SelectedIndex;
                        ScoreBook.ChannelNames[noteView.Channel] = form.ChannelName;
                        speedChBox.Items.Clear();
                        viewChBox.Items.Clear();
                        viewChBox.Items.Add(MainFormStrings.All);
                        viewChBox.Items.AddRange(ScoreBook.ChannelNames.Values.Select(p => p).ToArray());
                        viewChBox.Items.Add(MainFormStrings.Custom);


                        speedChBox.Items.AddRange(ScoreBook.ChannelNames.Values.Select(p => p).ToArray());
                        speedChBox.Items.Add(MainFormStrings.Custom);
                        speedChBox.SelectedIndex = spS;
                        viewChBox.SelectedIndex = viS;
                    }
                }
                else
                {
                    MessageBox.Show(this, ErrorStrings.NotNameChannel, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            })
            {
                Checked = false
            };

            /*
            var laneVisible2 = new CheckableToolStripSplitButton()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            laneVisible2.Text = "レーン表示";
            laneVisible2.DropDown.Items.AddRange(new ToolStripItem[]
            {
                laneVisible

            });
            */
            var widthSetCounts = new float[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
            };


            var widthSetBox = new ToolStripComboBox("設置するノーツの幅")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Width = 30
            };
            widthSetBox.Items.Add(MainFormStrings.DeleteHistory);
            widthSetBox.Items.AddRange(widthSetCounts.Select(p => p.ToString()).ToArray());
            widthSetBox.Items.Add(MainFormStrings.Custom);

            widthSetBox.SelectedIndexChanged += (s, e) =>
            {

                if (widthSetBox.SelectedIndex == widthSetBox.Items.Count - 1)
                {
                    var form = new CustomWidthSetSelectionForm(NoteView.Notes, NoteView.CurrentTick, NoteView.HeadTick, NoteView.HeadTick + (int)(ClientSize.Height * NoteView.UnitBeatTick / NoteView.UnitBeatHeight)) { Width = NoteView.LastWidth};
                    Console.WriteLine(NoteView.Notes.Taps.Count);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        if(form.SelectNote != null)
                        {
                            NoteView.LastWidth = form.SelectNote.Width;
                            if (!widthSetBox.Items.Contains(form.SelectNote.Width.ToString()))
                            {
                                widthSetBox.Items.Insert(widthSetBox.Items.Count - 1, form.SelectNote.Width.ToString());
                            }
                            widthSetBox.SelectedItem = form.SelectNote.Width.ToString();
                        }
                        else
                        {
                            NoteView.LastWidth = form.Width;
                            if (!widthSetBox.Items.Contains(form.Width.ToString()))
                            {
                                widthSetBox.Items.Insert(widthSetBox.Items.Count - 1, form.Width.ToString());
                            }
                            widthSetBox.SelectedItem = form.Width.ToString();
                        }
                        
                    }
                }
                else if (widthSetBox.SelectedIndex == 0)
                {
                    widthSetBox.Items.Clear();
                    widthSetBox.Items.Add(MainFormStrings.DeleteHistory);
                    widthSetBox.Items.AddRange(widthSetCounts.Select(p => p.ToString()).ToArray());
                    widthSetBox.Items.Add(MainFormStrings.Custom);
                    widthSetBox.SelectedItem = noteView.LastWidth.ToString();
                }
                else
                {
                    
                    noteView.LastWidth = float.Parse(widthSetBox.SelectedItem.ToString());

                }
                noteView.Update();
                noteView.Focus();
            };
            widthSetBox.SelectedItem = noteView.LastWidth.ToString();

            noteView.LastWidthChanged += (s, e) =>
            {
                
                if (!widthSetBox.Items.Contains(noteView.LastWidth.ToString()))
                    widthSetBox.Items.Insert(widthSetBox.Items.Count - 1, noteView.LastWidth.ToString());

                widthSetBox.SelectedItem = noteView.LastWidth.ToString();
            };

            
            var stageBox = new ToolStripComboBox(MainFormStrings.Stage)
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Width = 60
            };
            stageBox.Items.Add(0);
            stageBox.ComboBox.DisplayMember = "Name";
            stageBox.ComboBox.ValueMember = "ID";
            noteView.StageChanged += (s, e) =>
            {
                stageBox.ComboBox.DataSource = ScoreBook.Stages;
            };

            stageBox.SelectedIndexChanged += (s, e) =>
            {

                if (stageBox.SelectedItem is Stage selectedItem)
                {

                    noteView.Stage = selectedItem.ID;
                    noteView.Update();
                    noteView.Focus();
                }
            };

            stageBox.SelectedIndex = 0;


            var viewStageBox = new ToolStripComboBox("表示チャンネル")
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = true,
                Width = 60
            };
            viewStageBox.Items.Add(MainFormStrings.All);
            viewStageBox.ComboBox.DisplayMember = "Name";
            viewStageBox.ComboBox.ValueMember = "ID";
            noteView.StageChanged += (s, e) =>
            {
                List<Stage> stages = new List<Stage>() { new Stage() { ID = -1, Name = MainFormStrings.All } };
                stages.AddRange(ScoreBook.Stages);
                viewStageBox.ComboBox.DataSource = stages;
            };

            viewStageBox.SelectedIndexChanged += (s, e) =>
            {
                if (viewStageBox.SelectedIndex == 0)
                {
                    ViewStage = -1;
                }
                else
                {
                    if (viewStageBox.SelectedItem is Stage selectedItem)
                    {

                        noteView.Stage = selectedItem.ID;
                        noteView.Update();
                        noteView.Focus();
                    }
                }
                noteView.ViewStage = ViewStage;
                noteView.Update();
                noteView.Focus();
            };
            viewStageBox.SelectedIndex = 0;




            var menu = new ToolStrip(new ToolStripItem[] {});

            if (bool.Parse(ConfigurationManager.AppSettings["ShortCutNoteExtend"]))
            {
                menu = new ToolStrip(new ToolStripItem[]
            {
                tapButton, exTapButton, holdButton, slideButton, slideStepButton, airKind, airActionButton, flickButton, damageButton, guideKind,
                 guideStepButton, tap2Button, exTap2Button, flick2Button, damage2Button,
                new ToolStripSeparator(), stepNoteTapButton,
                quantizeComboBox, widthSetBox, new ToolStripSeparator(), speedChBox, viewChBox,  laneVisible, widthAmountBox, deleteChhistory, nameChannel
            });
            }
            else
            {
                menu = new ToolStrip(new ToolStripItem[]
            {
                tapButton, exTapButton, holdButton, slideButton, slideStepButton, airKind, airActionButton, flickButton, damageButton, guideKind,
                 guideStepButton, tap2Button, exTap2Button, flick2Button, damage2Button,
                quantizeComboBox, widthSetBox, new ToolStripSeparator(), speedChBox, viewChBox,  laneVisible, widthAmountBox, stageBox, viewStageBox
            });
            }

            return menu;
        }
    }
}
