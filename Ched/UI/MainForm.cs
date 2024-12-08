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

namespace Ched.UI
{
    public partial class MainForm : Form
    {
        private event EventHandler PreviewModeChanged;
        

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

        private float WidthAmount { get; set; } = 1;
        private float ScrollAmount { get; set; } = ApplicationSettings.Default.ScrollAmount;

        private bool LaneVisual { get; set; } = false;

        public bool FormSpeedbyCh { get; set; } = ApplicationSettings.Default.IsAnotherChannelSounds;

        private int defaultCh = 1;


        private Plugins.PluginManager PluginManager { get; } = Plugins.PluginManager.GetInstance();


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
                AllowStepCh = ApplicationSettings.Default.IsAllowStepChannel,
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
                SmallChange = NoteView.UnitBeatTick
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
            NoteViewScrollBar.Value = NoteViewScrollBar.GetMaximumValue();
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
            events.TimeSignatureChangeEvents.Add(new TimeSignatureChangeEvent() { Tick = 0, Numerator = 4, DenominatorExponent = 2 });
            book.LaneOffset = ApplicationSettings.Default.LaneOffset;

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
            NoteViewScrollBar.Maximum = NoteViewScrollBar.LargeChange + NoteView.PaddingHeadTick;
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
            commandSource.RegisterCommand(Commands.PasteFlip, MainFormStrings.PasteFlipped, () => NoteView.PasteFlippedNotes());

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

                if (!FormSpeedbyCh)
                {
                    spratio = NoteView.ScoreEvents.HighSpeedChangeEvents.OrderBy(p => p.Tick).LastOrDefault(p => p.Tick <= NoteView.CurrentTick)?.SpeedRatio ?? 1.0m;
                }

                var form = new HighSpeedSelectionForm()
                {
                    SpeedRatio = spratio,
                    SpeedCh = Channel
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new HighSpeedChangeEvent()
                {
                    Tick = NoteView.CurrentTick,
                    SpeedRatio = form.SpeedRatio,
                    SpeedCh = form.SpeedCh,
                    Type = form.SpeedCh
                };
                UpdateEvent(NoteView.ScoreEvents.HighSpeedChangeEvents, item);
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
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

                var item = new CommentEvent()
                {
                    Tick = NoteView.CurrentTick,
                    Comment = form.Comment,
                    Size = (float)form.TextSize,
                    Color = form.Color,
                    Type = -3

                };
                UpdateEvent(NoteView.ScoreEvents.CommentEvents, item);
            });
            commandSource.RegisterCommand(Commands.InsertMarker, MainFormStrings.Marker, () =>
            {
                var form = new MarkerInsertForm()
                {
                    Name = NoteView.Notes.Markers.OrderBy(p => p.StartTick).LastOrDefault(p => p.StartTick == NoteView.CurrentTick)?.Name ?? "コメント",
                    MarkerWidth = (decimal)(NoteView.Notes.Markers.OrderBy(p => p.StartTick).LastOrDefault(p => p.StartTick == NoteView.CurrentTick)?.StartWidth ?? 9),
                    Color = (NoteView.Notes.Markers.OrderBy(p => p.StartTick).LastOrDefault(p => p.StartTick == NoteView.CurrentTick)?.MarkerColorB ?? 0),
                };
                if (form.ShowDialog(this) != DialogResult.OK) return;

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
                    p.Run(new ScorePluginArgs(() => ScoreBook.Score.Clone(), noteView.SelectedRange, updateScore));
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
                cutItem, copyItem, pasteItem, pasteFlippedItem, new ToolStripSeparator(),
                selectAllItem, selectToEndItem, selectoToBeginningItem, new ToolStripSeparator(),
                flipSelectedNotesItem, removeSelectedNotesItem,  new ToolStripSeparator(),
                copyEventsItem, cutEventsItem, pasteEventsItem, pasteChEventsItem,removeEventsItem, new ToolStripSeparator(),
                insertAirWithAirActionItem, allowStepChItem, new ToolStripSeparator(),
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
                visibleOverlapItem, isUsingBezierCurves
            };


            var insertBpmItem = shortcutItemBuilder.BuildItem(Commands.InsertBpmChange, "BPM");
            var insertHighSpeedItem = shortcutItemBuilder.BuildItem(Commands.InsertHighSpeedChange, MainFormStrings.HighSpeed);
            var insertTimeSignatureItem = shortcutItemBuilder.BuildItem(Commands.InsertTimeSignatureChange, MainFormStrings.TimeSignature);
            var insertCommentItem = shortcutItemBuilder.BuildItem(Commands.InsertComment, MainFormStrings.Comment);


            var insertMenuItems = new ToolStripItem[] { insertBpmItem, insertHighSpeedItem, insertTimeSignatureItem, insertCommentItem };


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


            var uscfadeNone = new ToolStripMenuItem(MainFormStrings.None, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.GuideDefaultFade = 0;
                noteView.GuideDefaultFade = 0;

                item.Checked = noteView.GuideDefaultFade == 0;
            })
            {
                Checked = ApplicationSettings.Default.GuideDefaultFade == 0
            };

            var uscfadeOut = new ToolStripMenuItem(MainFormStrings.GuideOut, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.GuideDefaultFade = 1;
                noteView.GuideDefaultFade = 1;

                item.Checked = noteView.GuideDefaultFade == 1;
            })
            {
                Checked = ApplicationSettings.Default.GuideDefaultFade == 1
            };

            var uscfadeIn = new ToolStripMenuItem(MainFormStrings.GuideIn, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.GuideDefaultFade = 2;
                noteView.GuideDefaultFade = 2;

                item.Checked = noteView.GuideDefaultFade == 2;
            })
            {
                Checked = ApplicationSettings.Default.GuideDefaultFade == 2
            };

            var uscfadeItems = new ToolStripItem[] { uscfadeNone, uscfadeOut, uscfadeIn };
            var ExportuscfadeItems = new ToolStripMenuItem(MainFormStrings.GuideFadeTypes, null, uscfadeItems);

            var SStypeN = new ToolStripMenuItem(MainFormStrings.SlideNormal, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.SlideStartDefaultType = 0;
                noteView.SlideStartDefault = 0;

                item.Checked = noteView.SlideStartDefault == 0;
            })
            {
                Checked = ApplicationSettings.Default.SlideStartDefaultType == 0
            };

            var SStypeT = new ToolStripMenuItem(MainFormStrings.SlideTrace, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.SlideStartDefaultType = 1;
                noteView.SlideStartDefault = 1;

                item.Checked = noteView.SlideStartDefault == 1;
            })
            {
                Checked = ApplicationSettings.Default.SlideStartDefaultType == 1
            };

            var SStypeE = new ToolStripMenuItem(MainFormStrings.None, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.SlideStartDefaultType = 2;
                noteView.SlideStartDefault = 2;

                item.Checked = noteView.SlideStartDefault == 2;
            })
            {
                Checked = ApplicationSettings.Default.SlideStartDefaultType == 2
            };



            var slideStartTypeItems = new ToolStripItem[]
            {
                SStypeN, SStypeT, SStypeE
            };
            var ExportslidestartTypeItems = new ToolStripMenuItem(MainFormStrings.SlideStartTypes, null, slideStartTypeItems);

            var SEtypeN = new ToolStripMenuItem(MainFormStrings.SlideNormal, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.SlideEndDefaultType = 0;
                noteView.SlideEndDefault = 0;

                item.Checked = noteView.SlideEndDefault == 0;
                
            })
            {
                Checked = ApplicationSettings.Default.SlideEndDefaultType == 0
            };

            var SEtypeT = new ToolStripMenuItem(MainFormStrings.SlideTrace, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.SlideEndDefaultType = 1;
                noteView.SlideEndDefault = 1;

                item.Checked = noteView.SlideEndDefault == 1;
            })
            {
                Checked = ApplicationSettings.Default.SlideEndDefaultType == 1
            };

            var SEtypeE = new ToolStripMenuItem(MainFormStrings.None, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                ApplicationSettings.Default.SlideEndDefaultType = 2;
                noteView.SlideEndDefault = 2;

                item.Checked = noteView.SlideEndDefault == 2;
            })
            {
                Checked = ApplicationSettings.Default.SlideEndDefaultType == 2
            };

            var slideEndTypeItems = new ToolStripItem[]
            {
                SEtypeN, SEtypeT, SEtypeE
            };
            var ExportslideendTypeItems = new ToolStripMenuItem(MainFormStrings.SlideEndTypes, null, slideEndTypeItems);



            var slideHideTap = new ToolStripMenuItem(MainFormStrings.isOnSlide + "TAP" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTapHideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTapHideOnSlide
            };
            var guideHideTap = new ToolStripMenuItem(MainFormStrings.isOnGuide + "TAP" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTapHideOnGuide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTapHideOnGuide
            };
            var airdownHideTap = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "TAP" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTapEraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTapEraseDown
            };

            var slideHideExTap = new ToolStripMenuItem(MainFormStrings.isOnSlide + "ExTAP" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTapHideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTapHideOnSlide
            };
            var guideHideExTap = new ToolStripMenuItem(MainFormStrings.isOnGuide + "ExTAP" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTapHideOnGuide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTapHideOnGuide
            };
            var airdownHideExTap = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "ExTAP" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTapEraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTapEraseDown
            };

            var slideHideTap2 = new ToolStripMenuItem(MainFormStrings.isOnSlide + "TAP2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTap2HideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTap2HideOnSlide
            };
            var airdownHideTap2 = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "TAP2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTap2EraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTap2EraseDown
            };
            var slideHideExTap2 = new ToolStripMenuItem(MainFormStrings.isOnSlide + "ExTAP2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTap2HideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTap2HideOnSlide
            };
            var airdownHideExTap2 = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "ExTAP2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTap2EraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTap2EraseDown
            };


            var slideHideFlick = new ToolStripMenuItem(MainFormStrings.isOnSlide + "Flick" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsFlickHideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsFlickHideOnSlide
            };
            var guideHideFlick = new ToolStripMenuItem(MainFormStrings.isOnGuide + "Flick" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsFlickHideOnGuide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsFlickHideOnGuide
            };
            var airdownHideFlick = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "Flick" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsFlickEraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsFlickEraseDown
            };

            var slideHideDamage = new ToolStripMenuItem(MainFormStrings.isOnSlide + "DAMAGE" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamageHideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamageHideOnSlide
            };
            var guideHideDamage = new ToolStripMenuItem(MainFormStrings.isOnGuide + "DAMAGE" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamageHideOnGuide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamageHideOnGuide
            };
            var airdownHideDamage = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "DAMAGE" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamageEraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamageEraseDown
            };

            var slideHideFlick2 = new ToolStripMenuItem(MainFormStrings.isOnSlide + "Flick2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsFlick2HideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsFlick2HideOnSlide
            };
            var guideHideFlick2 = new ToolStripMenuItem(MainFormStrings.isOnGuide + "Flick2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsFlick2HideOnGuide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsFlick2HideOnGuide
            };
            var airdownHideFlick2 = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "Flick2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsFlick2EraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsFlick2EraseDown
            };

            var slideHideDamage2 = new ToolStripMenuItem(MainFormStrings.isOnSlide + "DAMAGE2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamage2HideOnSlide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamage2HideOnSlide
            };
            var guideHideDamage2 = new ToolStripMenuItem(MainFormStrings.isOnGuide + "DAMAGE2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamage2HideOnGuide = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamage2HideOnGuide
            };
            var airdownHideDamage2 = new ToolStripMenuItem(MainFormStrings.isOnDownAir + "DAMAGE2" + MainFormStrings.Hide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamage2EraseDown = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamage2EraseDown
            };



            var startEraseTap = new ToolStripMenuItem("TAP " + MainFormStrings.EraceSlideStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTapEraseStart = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTapEraseStart
            };
            var startEraseExTap = new ToolStripMenuItem("ExTAP " + MainFormStrings.EraceSlideStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTapEraseStart = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTapEraseStart
            };
            var startEraseTap2 = new ToolStripMenuItem("TAP2 " + MainFormStrings.EraceSlideStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTap2EraseStart = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTap2EraseStart
            };
            var startEraseExTap2 = new ToolStripMenuItem("ExTAP2 " + MainFormStrings.EraceSlideStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTap2EraseStart = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTap2EraseStart
            };
            var startEraseDamage = new ToolStripMenuItem("DAMAGE " + MainFormStrings.EraceSlideStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamageEraseStart = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamageEraseStart
            };

            var endEraseTap = new ToolStripMenuItem("TAP " + MainFormStrings.EraceSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTapEraseEnd = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTapEraseEnd
            };
            var endEraseExTap = new ToolStripMenuItem("ExTAP " + MainFormStrings.EraceSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTapEraseEnd = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTapEraseEnd
            };
            var endEraseTap2 = new ToolStripMenuItem("TAP2 " + MainFormStrings.EraceSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTap2EraseEnd = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTap2EraseEnd
            };
            var endEraseExTap2 = new ToolStripMenuItem("ExTAP2 " + MainFormStrings.EraceSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTap2EraseEnd = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTap2EraseEnd
            };
            var endEraseDamage = new ToolStripMenuItem("DAMAGE " + MainFormStrings.EraceSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamageEraseEnd = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamageEraseEnd
            };



            var changeFadeTap = new ToolStripMenuItem("TAP " + MainFormStrings.ChangeGuideFade, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTapChangeFade = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTapChangeFade
            };
            var changeFadeExTap = new ToolStripMenuItem("ExTAP " + MainFormStrings.ChangeGuideFade, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTapChangeFade = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTapChangeFade
            };
            var changeFadeTap2 = new ToolStripMenuItem("TAP2 " + MainFormStrings.ChangeGuideFade, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsTap2ChangeFade = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsTap2ChangeFade
            };
            var changeFadeExTap2 = new ToolStripMenuItem("ExTAP2 " + MainFormStrings.ChangeGuideFade, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsExTap2ChangeFade = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsExTap2ChangeFade
            };
            var changeFadeFlick = new ToolStripMenuItem("FLICK " + MainFormStrings.ChangeGuideFade, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsFlickChangeFade = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsFlickChangeFade
            };
            var changeFadeDamage = new ToolStripMenuItem("DAMAGE " + MainFormStrings.ChangeGuideFade, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsDamageChangeFade = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsDamageChangeFade
            };


            var SStapCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTapCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTapCritical
            };
            var SStap2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTap2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTap2Critical
            };

            var SSextapCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTapCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTapCritical
            };
            var SSextap2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTap2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTap2Critical
            };
            var SSflickCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlickCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlickCritical
            };
            var SSflick2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlick2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlick2Critical
            };

            var SSdamageCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamageCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamageCritical
            };
            var SSdamage2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamage2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamage2Critical
            };
            var SStapTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTapChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTapChangeTraceS
            };
            var SStapTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTapChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTapChangeTraceE
            };
            var SStap2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTap2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTap2ChangeTraceS
            };
            var SStap2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTap2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTap2ChangeTraceE
            };
            var SSextapTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTapChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTapChangeTraceS
            };
            var SSextapTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTapChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTapChangeTraceE
            };
            var SSextap2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTap2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTap2ChangeTraceS
            };
            var SSextap2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTap2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTap2ChangeTraceE
            };
            var SSflickTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlickChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlickChangeTraceS
            };
            var SSflickTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlickChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlickChangeTraceE
            };
            var SSflick2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlick2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlick2ChangeTraceS
            };
            var SSflick2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlick2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlick2ChangeTraceE
            };
            var SSdamageTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamageChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamageChangeTraceS
            };
            var SSdamageTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamageChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamageChangeTraceE
            };
            var SSdamage2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamage2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamage2ChangeTraceS
            };
            var SSdamage2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamage2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamage2ChangeTraceE
            };

            var SStapDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTapDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTapDeleteS
            };
            var SStapDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTapDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTapDeleteE
            };
            var SStap2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTap2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTap2DeleteS
            };
            var SStap2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsTap2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsTap2DeleteE
            };
            var SSextapDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTapDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTapDeleteS
            };
            var SSextapDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTapDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTapDeleteE
            };
            var SSextap2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTap2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTap2DeleteS
            };
            var SSextap2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsExTap2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsExTap2DeleteE
            };
            var SSflickDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlickDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlickDeleteS
            };
            var SSflickDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlickDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlickDeleteE
            };
            var SSflick2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlick2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlick2DeleteS
            };
            var SSflick2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsFlick2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsFlick2DeleteE
            };
            var SSdamageDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamageDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamageDeleteS
            };
            var SSdamageDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamageDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamageDeleteE
            };
            var SSdamage2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamage2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamage2DeleteS
            };
            var SSdamage2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SSIsDamage2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SSIsDamage2DeleteE
            };

            var STEtapCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsTapCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsTapCritical
            };
            var STEtap2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsTap2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsTap2Critical
            };
            var STEextapCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsExTapCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsExTapCritical
            };
            var STEextap2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsExTap2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsExTap2Critical
            };
            var STEflickCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsFlickCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsFlickCritical
            };
            var STEflick2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsFlick2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsFlick2Critical
            };
            var STEdamageCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsDamageCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsDamageCritical
            };
            var STEdamage2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsDamage2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsDamage2Critical
            };

            var STEtapAttach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsTapAttach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsTapAttach
            };
            var STEtap2Attach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsTap2Attach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsTap2Attach
            };
            var STEextapAttach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsExTapAttach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsExTapAttach
            };
            var STEextap2Attach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsExTap2Attach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsExTap2Attach
            };
            var STEflickAttach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsFlickAttach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsFlickAttach
            };
            var STEflick2Attach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsFlick2Attach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsFlick2Attach
            };
            var STEdamageAttach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsDamageAttach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsDamageAttach
            };
            var STEdamage2Attach = new ToolStripMenuItem(MainFormStrings.MakeAttachStep, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.STEIsDamage2Attach = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.STEIsDamage2Attach
            };

            var SEtapCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTapCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTapCritical
            };
            var SEtap2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTap2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTap2Critical
            };

            var SEextapCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTapCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTapCritical
            };
            var SEextap2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTap2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTap2Critical
            };
            var SEflickCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlickCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlickCritical
            };
            var SEflick2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlick2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlick2Critical
            };

            var SEdamageCritical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamageCritical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamageCritical
            };
            var SEdamage2Critical = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlide, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamage2Critical = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamage2Critical
            };

            var SEtapCriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTapCriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTapCriticalE
            };
            var SEtap2CriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTap2CriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTap2CriticalE
            };

            var SEextapCriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTapCriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTapCriticalE
            };
            var SEextap2CriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTap2CriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTap2CriticalE
            };
            var SEflickCriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlickCriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlickCriticalE
            };
            var SEflick2CriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlick2CriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlick2CriticalE
            };

            var SEdamageCriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamageCriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamageCriticalE
            };
            var SEdamage2CriticalE = new ToolStripMenuItem(MainFormStrings.MakeCriticalSlideEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamage2CriticalE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamage2CriticalE
            };

            var SEtapTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTapChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTapChangeTraceS
            };
            var SEtapTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTapChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTapChangeTraceE
            };
            var SEtap2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTap2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTap2ChangeTraceS
            };
            var SEtap2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTap2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTap2ChangeTraceE
            };
            var SEextapTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTapChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTapChangeTraceS
            };
            var SEextapTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTapChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTapChangeTraceE
            };
            var SEextap2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTap2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTap2ChangeTraceS
            };
            var SEextap2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTap2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTap2ChangeTraceE
            };
            var SEflickTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlickChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlickChangeTraceS
            };
            var SEflickTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlickChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlickChangeTraceE
            };
            var SEflick2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlick2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlick2ChangeTraceS
            };
            var SEflick2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlick2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlick2ChangeTraceE
            };
            var SEdamageTraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamageChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamageChangeTraceS
            };
            var SEdamageTraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamageChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamageChangeTraceE
            };
            var SEdamage2TraceS = new ToolStripMenuItem(MainFormStrings.SetTraceStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamage2ChangeTraceS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamage2ChangeTraceS
            };
            var SEdamage2TraceE = new ToolStripMenuItem(MainFormStrings.SetTraceEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamage2ChangeTraceE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamage2ChangeTraceE
            };

            var SEtapDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTapDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTapDeleteS
            };
            var SEtapDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTapDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTapDeleteE
            };
            var SEtap2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTap2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTap2DeleteS
            };
            var SEtap2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsTap2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsTap2DeleteE
            };
            var SEextapDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTapDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTapDeleteS
            };
            var SEextapDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTapDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTapDeleteE
            };
            var SEextap2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTap2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTap2DeleteS
            };
            var SEextap2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsExTap2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsExTap2DeleteE
            };
            var SEflickDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlickDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlickDeleteS
            };
            var SEflickDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlickDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlickDeleteE
            };
            var SEflick2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlick2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlick2DeleteS
            };
            var SEflick2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsFlick2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsFlick2DeleteE
            };
            var SEdamageDeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamageDeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamageDeleteS
            };
            var SEdamageDeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamageDeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamageDeleteE
            };
            var SEdamage2DeleteS = new ToolStripMenuItem(MainFormStrings.DeleteStart, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamage2DeleteS = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamage2DeleteS
            };
            var SEdamage2DeleteE = new ToolStripMenuItem(MainFormStrings.DeleteEnd, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.SEIsDamage2DeleteE = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.SEIsDamage2DeleteE
            };

            var GStapChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTapFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTapFadeChange
            };
            var GStapChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTapFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTapFadeN
            };
            var GStapChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTapFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTapFadeO
            };
            var GStapChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTapFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTapFadeI
            };

            var GStap2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTap2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTap2FadeChange
            };
            var GStap2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTap2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTap2FadeN
            };
            var GStap2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTap2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTap2FadeO
            };
            var GStap2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsTap2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsTap2FadeI
            };


            var GSextapChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTapFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTapFadeChange
            };
            var GSextapChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTapFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTapFadeN
            };
            var GSextapChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTapFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTapFadeO
            };
            var GSextapChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTapFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTapFadeI
            };

            var GSextap2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTap2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTap2FadeChange
            };
            var GSextap2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTap2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTap2FadeN
            };
            var GSextap2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTap2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTap2FadeO
            };
            var GSextap2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsExTap2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsExTap2FadeI
            };


            var GSflickChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlickFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlickFadeChange
            };
            var GSflickChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlickFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlickFadeN
            };
            var GSflickChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlickFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlickFadeO
            };
            var GSflickChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlickFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlickFadeI
            };

            var GSflick2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlick2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlick2FadeChange
            };
            var GSflick2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlick2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlick2FadeN
            };
            var GSflick2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlick2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlick2FadeO
            };
            var GSflick2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsFlick2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsFlick2FadeI
            };

            var GSdamageChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamageFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamageFadeChange
            };
            var GSdamageChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamageFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamageFadeN
            };
            var GSdamageChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamageFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamageFadeO
            };
            var GSdamageChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamageFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamageFadeI
            };

            var GSdamage2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamage2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamage2FadeChange
            };
            var GSdamage2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamage2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamage2FadeN
            };
            var GSdamage2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamage2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamage2FadeO
            };
            var GSdamage2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSIsDamage2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSIsDamage2FadeI
            };


            var GEtapChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTapFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTapFadeChange
            };
            var GEtapChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTapFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTapFadeN
            };
            var GEtapChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTapFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTapFadeO
            };
            var GEtapChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTapFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTapFadeI
            };

            var GEtap2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTap2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTap2FadeChange
            };
            var GEtap2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTap2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTap2FadeN
            };
            var GEtap2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTap2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTap2FadeO
            };
            var GEtap2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsTap2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsTap2FadeI
            };


            var GEextapChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTapFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTapFadeChange
            };
            var GEextapChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTapFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTapFadeN
            };
            var GEextapChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTapFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTapFadeO
            };
            var GEextapChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTapFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTapFadeI
            };

            var GEextap2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTap2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTap2FadeChange
            };
            var GEextap2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTap2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTap2FadeN
            };
            var GEextap2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTap2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTap2FadeO
            };
            var GEextap2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsExTap2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsExTap2FadeI
            };


            var GEflickChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlickFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlickFadeChange
            };
            var GEflickChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlickFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlickFadeN
            };
            var GEflickChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlickFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlickFadeO
            };
            var GEflickChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlickFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlickFadeI
            };

            var GEflick2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlick2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlick2FadeChange
            };
            var GEflick2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlick2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlick2FadeN
            };
            var GEflick2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlick2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlick2FadeO
            };
            var GEflick2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsFlick2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsFlick2FadeI
            };

            var GEdamageChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamageFadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamageFadeChange
            };
            var GEdamageChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamageFadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamageFadeN
            };
            var GEdamageChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamageFadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamageFadeO
            };
            var GEdamageChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamageFadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamageFadeI
            };

            var GEdamage2ChangeFade = new ToolStripMenuItem(MainFormStrings.FadeChange, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamage2FadeChange = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamage2FadeChange
            };
            var GEdamage2ChangeFadeN = new ToolStripMenuItem(MainFormStrings.FadeN, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamage2FadeN = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamage2FadeN
            };
            var GEdamage2ChangeFadeO = new ToolStripMenuItem(MainFormStrings.FadeO, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamage2FadeO = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamage2FadeO
            };
            var GEdamage2ChangeFadeI = new ToolStripMenuItem(MainFormStrings.FadeI, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GEIsDamage2FadeI = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GEIsDamage2FadeI
            };

            var GSTEistap = new ToolStripMenuItem("TAP", null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSTIsTap = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSTIsTap
            };
            var GSTEisextap = new ToolStripMenuItem("ExTAP", null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSTIsExTap = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSTIsExTap
            };
            var GSTEistrace = new ToolStripMenuItem("FLICK", null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSTIsFlick = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSTIsFlick
            };
            var GSTEisdamage = new ToolStripMenuItem("DAMAGE", null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.GSTIsDamage = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.GSTIsDamage
            };




            var SStapNoteMenu = new ToolStripMenuItem[]
            {
                SStapCritical,
                SStapTraceS, SStapTraceE,
                SStapDeleteS, SStapDeleteE,
            };
            var STEtapNoteMenu = new ToolStripMenuItem[]
            {
                STEtapCritical, STEtapAttach
            };
            var SEtapNoteMenu = new ToolStripMenuItem[]
            {
                SEtapCritical, SEtapCriticalE,
                SEtapTraceS, SEtapTraceE,
                SEtapDeleteS, SEtapDeleteE,
            };
            var SSextapNoteMenu = new ToolStripMenuItem[]
            {
                SSextapCritical,
                SSextapTraceS, SSextapTraceE,
                SSextapDeleteS, SSextapDeleteE,
            };
            var STEextapNoteMenu = new ToolStripMenuItem[]
            {
                STEextapCritical, STEextapAttach
            };
            var SEextapNoteMenu = new ToolStripMenuItem[]
            {
                SEextapCritical, SEextapCriticalE,
                SEextapTraceS, SEextapTraceE,
                SEextapDeleteS, SEextapDeleteE,
            };
            var SStap2NoteMenu = new ToolStripMenuItem[]
            {
                SStap2Critical,
                SStap2TraceS, SStap2TraceE,
                SStap2DeleteS, SStap2DeleteE,
            };
            var STEtap2NoteMenu = new ToolStripMenuItem[]
            {
                STEtap2Critical, STEtap2Attach
            };
            var SEtap2NoteMenu = new ToolStripMenuItem[]
            {
                SEtap2Critical, SEtap2CriticalE,
                SEtap2TraceS, SEtap2TraceE,
                SEtap2DeleteS, SEtap2DeleteE,
            };
            var SSextap2NoteMenu = new ToolStripMenuItem[]
            {
                SSextap2Critical,
                SSextap2TraceS,  SSextap2TraceE,
                SSextap2DeleteS, SSextap2DeleteE,
            };
            var STEextap2NoteMenu = new ToolStripMenuItem[]
            {
                STEextap2Critical, STEextap2Attach
            };
            var SEextap2NoteMenu = new ToolStripMenuItem[]
            {
                SEextap2Critical, SEextap2CriticalE,
                SEextap2TraceS, SEextap2TraceE,
                SEextap2DeleteS, SEextap2DeleteE,
            };
            var SSflickNoteMenu = new ToolStripMenuItem[]
            {
                SSflickCritical,
                SSflickTraceS, SSflickTraceE,
                SSflickDeleteS, SSflickDeleteE,
            };
            var STEflickNoteMenu = new ToolStripMenuItem[]
            {
                STEflickCritical, STEflickAttach
            };
            var SEflickNoteMenu = new ToolStripMenuItem[]
            {
                SEflickCritical, SEflickCriticalE,
                SEflickTraceS, SEflickTraceE,
                SEflickDeleteS, SEflickDeleteE,
            };
            var SSflick2NoteMenu = new ToolStripMenuItem[]
            {
                SSflick2Critical,
                SSflick2TraceS, SSflick2TraceE,
                SSflick2DeleteS, SSflick2DeleteE,
            };
            var STEflick2NoteMenu = new ToolStripMenuItem[]
            {
                STEflick2Critical, STEflick2Attach
            };
            var SEflick2NoteMenu = new ToolStripMenuItem[]
            {
                SEflick2Critical, SEflick2CriticalE,
                SEflick2TraceS, SEflick2TraceE,
                SEflick2DeleteS, SEflick2DeleteE,
            };
            var SSdamageNoteMenu = new ToolStripMenuItem[]
            {
                SSdamageCritical,
                SSdamageTraceS, SSdamageTraceE,
                SSdamageDeleteS, SSdamageDeleteE,
            };
            var STEdamageNoteMenu = new ToolStripMenuItem[]
            {
                STEdamageCritical, STEdamageAttach
            };
            var SEdamageNoteMenu = new ToolStripMenuItem[]
            {
                SEdamageCritical, SEdamageCriticalE,
                SEdamageTraceS, SEdamageTraceE,
                SEdamageDeleteS, SEdamageDeleteE,
            };
            var SSdamage2NoteMenu = new ToolStripMenuItem[]
            {
                SSdamage2Critical,
                SSdamage2TraceS, SSdamage2TraceE,
                SSdamage2DeleteS, SSdamage2DeleteE,
            };
            var STEdamage2NoteMenu = new ToolStripMenuItem[]
            {
                STEdamage2Critical, STEdamage2Attach
            };
            var SEdamage2NoteMenu = new ToolStripMenuItem[]
            {
                SEdamage2Critical, SEdamage2CriticalE,
                SEdamage2TraceS, SEdamage2TraceE,
                SEdamage2DeleteS, SEdamage2DeleteE,
            };

            var GStapNoteMenu = new ToolStripMenuItem[]
            {
                GStapChangeFade,
                GStapChangeFadeN, GStapChangeFadeO, GStapChangeFadeI
            };
            var GEtapNoteMenu = new ToolStripMenuItem[]
            {
                GEtapChangeFade,
                GEtapChangeFadeN, GEtapChangeFadeO, GEtapChangeFadeI
            };
            var GStap2NoteMenu = new ToolStripMenuItem[]
            {
                GStap2ChangeFade,
                GStap2ChangeFadeN, GStap2ChangeFadeO, GStap2ChangeFadeI
            };
            var GEtap2NoteMenu = new ToolStripMenuItem[]
            {
                GEtap2ChangeFade,
                GEtap2ChangeFadeN, GEtap2ChangeFadeO, GEtap2ChangeFadeI
            };

            var GSextapNoteMenu = new ToolStripMenuItem[]
            {
                GSextapChangeFade,
                GSextapChangeFadeN, GSextapChangeFadeO, GSextapChangeFadeI
            };
            var GEextapNoteMenu = new ToolStripMenuItem[]
            {
                GEextapChangeFade,
                GEextapChangeFadeN, GEextapChangeFadeO, GEextapChangeFadeI
            };
            var GSextap2NoteMenu = new ToolStripMenuItem[]
            {
                GSextap2ChangeFade,
                GSextap2ChangeFadeN, GSextap2ChangeFadeO, GSextap2ChangeFadeI
            };
            var GEextap2NoteMenu = new ToolStripMenuItem[]
            {
                GEextap2ChangeFade,
                GEextap2ChangeFadeN, GEextap2ChangeFadeO, GEextap2ChangeFadeI
            };
            var GSflickNoteMenu = new ToolStripMenuItem[]
            {
                GSflickChangeFade,
                GSflickChangeFadeN, GSflickChangeFadeO, GSflickChangeFadeI
            };
            var GEflickNoteMenu = new ToolStripMenuItem[]
            {
                GEflickChangeFade,
                GEflickChangeFadeN, GEflickChangeFadeO, GEflickChangeFadeI
            };
            var GSflick2NoteMenu = new ToolStripMenuItem[]
            {
                GSflick2ChangeFade,
                GSflick2ChangeFadeN, GSflick2ChangeFadeO, GSflick2ChangeFadeI
            };
            var GEflick2NoteMenu = new ToolStripMenuItem[]
            {
                GEflick2ChangeFade,
                GEflick2ChangeFadeN, GEflick2ChangeFadeO, GEflick2ChangeFadeI
            };
            var GSdamageNoteMenu = new ToolStripMenuItem[]
            {
                GSdamageChangeFade,
                GSdamageChangeFadeN, GSdamageChangeFadeO, GSdamageChangeFadeI
            };
            var GEdamageNoteMenu = new ToolStripMenuItem[]
            {
                GEdamageChangeFade,
                GEdamageChangeFadeN, GEdamageChangeFadeO, GEdamageChangeFadeI
            };
            var GSdamage2NoteMenu = new ToolStripMenuItem[]
            {
                GSdamage2ChangeFade,
                GSdamage2ChangeFadeN, GSdamage2ChangeFadeO, GSdamage2ChangeFadeI
            };
            var GEdamage2NoteMenu = new ToolStripMenuItem[]
            {
                GEdamage2ChangeFade,
                GEdamage2ChangeFadeN, GEdamage2ChangeFadeO, GEdamage2ChangeFadeI
            };
            var GSTEnotes = new ToolStripMenuItem[]
            {
                GSTEistap, GSTEisextap,GSTEistrace,GSTEisdamage,
            };




            var SSTapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SStapNoteMenu);
            var STETapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEtapNoteMenu);
            var SETapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEtapNoteMenu);
            var SSExTapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SSextapNoteMenu);
            var STEExTapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEextapNoteMenu);
            var SEExTapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEextapNoteMenu);

            var SSTap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SStap2NoteMenu);
            var STETap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEtap2NoteMenu);
            var SETap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEtap2NoteMenu);
            var SSExTap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SSextap2NoteMenu);
            var STEExTap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEextap2NoteMenu);
            var SEExTap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEextap2NoteMenu);

            var SSFlickNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SSflickNoteMenu);
            var STEFlickNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEflickNoteMenu);
            var SEFlickNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEflickNoteMenu);
            var SSFlick2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SSflick2NoteMenu);
            var STEFlick2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEflick2NoteMenu);
            var SEFlick2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEflick2NoteMenu);
            var SSDamageNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SSdamageNoteMenu);
            var STEDamageNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEdamageNoteMenu);
            var SEDamageNoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEdamageNoteMenu);
            var SSDamage2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStart, null, SSdamage2NoteMenu);
            var STEDamage2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideStep, null, STEdamage2NoteMenu);
            var SEDamage2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnSlideEnd, null, SEdamage2NoteMenu);


            var GSTapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GStapNoteMenu);
            var GETapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEtapNoteMenu);
            var GSExTapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GSextapNoteMenu);
            var GEExTapNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEextapNoteMenu);

            var GSTap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GStap2NoteMenu);
            var GETap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEtap2NoteMenu);
            var GSExTap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GSextap2NoteMenu);
            var GEExTap2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEextap2NoteMenu);

            var GSFlickNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GSflickNoteMenu);
            var GEFlickNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEflickNoteMenu);
            var GSFlick2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GSflick2NoteMenu);
            var GEFlick2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEflick2NoteMenu);
            var GSDamageNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GSdamageNoteMenu);
            var GEDamageNoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEdamageNoteMenu);
            var GSDamage2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideStart, null, GSdamage2NoteMenu);
            var GEDamage2NoteItem = new ToolStripMenuItem(MainFormStrings.isOnGuideEnd, null, GEdamage2NoteMenu);

            var GSTENoteItem = new ToolStripMenuItem(MainFormStrings.GuideStepNote, null, GSTEnotes);


            var tapNoteMenu = new ToolStripMenuItem[]
            {
                slideHideTap,
                guideHideTap,
                airdownHideTap,
                SSTapNoteItem,
                STETapNoteItem,
                SETapNoteItem,
                GSTapNoteItem,
                GETapNoteItem,

            };


            var extapNoteMenu = new ToolStripMenuItem[]
            {
                slideHideExTap,
                guideHideExTap,
                airdownHideExTap,
                SSExTapNoteItem,
                STEExTapNoteItem,
                SEExTapNoteItem,
                GSExTapNoteItem,
                GEExTapNoteItem,

            };
            var tap2NoteMenu = new ToolStripMenuItem[]
            {
                slideHideTap2,
                airdownHideTap2,
                SSTap2NoteItem,
                STETap2NoteItem,
                SETap2NoteItem,
                GSTap2NoteItem,
                GETap2NoteItem,
            };
            var extap2NoteMenu = new ToolStripMenuItem[]
            {
                slideHideExTap2,
                airdownHideExTap2,
                SSExTap2NoteItem,
                STEExTap2NoteItem,
                SEExTap2NoteItem,
                GSExTap2NoteItem,
                GEExTap2NoteItem,
            };
            var flickNoteMenu = new ToolStripMenuItem[]
            {
                slideHideFlick,
                guideHideFlick,
                airdownHideFlick,
                SSFlickNoteItem,
                STEFlickNoteItem,
                SEFlickNoteItem,
                GSFlickNoteItem,
                GEFlickNoteItem,

            };
            var flick2NoteMenu = new ToolStripMenuItem[]
            {
                slideHideFlick2,
                guideHideFlick2,
                airdownHideFlick2,
                SSFlick2NoteItem,
                STEFlick2NoteItem,
                SEFlick2NoteItem,
                GSFlick2NoteItem,
                GEFlick2NoteItem,

            };
            var damageNoteMenu = new ToolStripMenuItem[]
            {
                slideHideDamage,
                guideHideDamage,
                airdownHideDamage,
                SSDamageNoteItem,
                STEDamageNoteItem,
                SEDamageNoteItem,
                GSDamageNoteItem,
                GEDamageNoteItem,
            };
            var damage2NoteMenu = new ToolStripMenuItem[]
            {
                slideHideDamage2,
                guideHideDamage2,
                airdownHideDamage2,
                SSDamage2NoteItem,
                STEDamage2NoteItem,
                SEDamage2NoteItem,
                GSDamage2NoteItem,
                GEDamage2NoteItem,
            };
            var slideNoteMenu = new ToolStripMenuItem[]
            {
                ExportslidestartTypeItems,
                ExportslideendTypeItems
            };
            var guideNoteMenu = new ToolStripMenuItem[]
            {
                ExportuscfadeItems, GSTENoteItem
            };
            var Accuratedjudge = new ToolStripMenuItem(MainFormStrings.Accuratejudge, null, (s, e) =>
            {
                var item = s as ToolStripMenuItem;
                item.Checked = !item.Checked;
                ApplicationSettings.Default.IsAccurateOverlap = item.Checked;
            })
            {
                Checked = ApplicationSettings.Default.IsAccurateOverlap
            };

            var TapNoteItem = new ToolStripMenuItem("TAP", Resources.TapIcon, tapNoteMenu);
            var ExTapNoteItem = new ToolStripMenuItem("ExTAP", Resources.ExTapIcon, extapNoteMenu);
            var Tap2NoteItem = new ToolStripMenuItem("TAP2", Resources.TapIcon2, tap2NoteMenu);
            var ExTap2NoteItem = new ToolStripMenuItem("ExTAP2", Resources.ExTapIcon2, extap2NoteMenu);
            var FlickNoteItem = new ToolStripMenuItem("FLICK", Resources.FlickIcon, flickNoteMenu);
            var Flick2NoteItem = new ToolStripMenuItem("FLICK2", Resources.FlickIcon, flick2NoteMenu);
            var DamageNoteItem = new ToolStripMenuItem("DAMAGE", Resources.DamgeIcon, damageNoteMenu);
            var Damage2NoteItem = new ToolStripMenuItem("DAMAGE2", Resources.DamgeIcon, damage2NoteMenu);

            var SlideNoteItem = new ToolStripMenuItem("SLIDE", Resources.SlideIcon, slideNoteMenu);
            var GuideNoteItem = new ToolStripMenuItem("GUIDE", Resources.GuideGreen, guideNoteMenu);

            var NoteItems = new ToolStripMenuItem[]
            {
                TapNoteItem, ExTapNoteItem, Tap2NoteItem, ExTap2NoteItem, FlickNoteItem, DamageNoteItem, Flick2NoteItem, Damage2NoteItem,
                SlideNoteItem, GuideNoteItem,
            };

            var ExportNotesItems = new ToolStripMenuItem(MainFormStrings.Notes, null, NoteItems);




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
                new ToolStripMenuItem(MainFormStrings.VersionInfo, null, (s, e) => new VersionInfoForm().ShowDialog(this))
            };


            var themeMenuItems = new ToolStripItem[] { themeBlack, themeWhite};

            var channelMenuItems = new ToolStripItem[] { channelMovableItem, channelSoundsItem, noteVisualModeItem, changeChannelSelectedNotesItem, isFormSpeedItem };

            var exportMenuItems = new ToolStripItem[] { ExportNotesItems, Accuratedjudge };



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

            noteView.GuideFademodeChanged += (s, e) =>
            {
                uscfadeNone.Checked = noteView.GuideDefaultFade == 0;
                uscfadeOut.Checked = noteView.GuideDefaultFade == 1;
                uscfadeIn.Checked = noteView.GuideDefaultFade == 2;
                
            };
            noteView.SlideStartChanged+= (s, e) =>
            {
                SStypeN.Checked = noteView.SlideStartDefault == 0;
                SStypeT.Checked = noteView.SlideStartDefault == 1;
                SStypeE.Checked = noteView.SlideStartDefault == 2;
                
            };
            noteView.SlideEndChanged += (s, e) =>
            {
                SEtypeN.Checked = noteView.SlideEndDefault == 0;
                SEtypeT.Checked = noteView.SlideEndDefault == 1;
                SEtypeE.Checked = noteView.SlideEndDefault == 2;

            };
            noteView.ChannelVisualChanged += (s, e) =>
            {
                notDisplay.Checked = noteView.NoteVisualMode == 0;
                translucentDisplay.Checked = noteView.NoteVisualMode == 1;
                Display.Checked = noteView.NoteVisualMode == 2;

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

            var zoomInButton = shortcutItemBuilder.BuildItem(Commands.ZoomIn, MainFormStrings.ZoomIn, Resources.ZoomInIcon);
            zoomInButton.Enabled = CanZoomIn;

            var zoomOutButton = shortcutItemBuilder.BuildItem(Commands.ZoomOut, MainFormStrings.ZoomOut, Resources.ZoomOutIcon);
            zoomOutButton.Enabled = CanZoomOut;

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
                penButton, selectionButton, eraserButton, paintButton, propertyButton, markerButton, stepeditorButton, new ToolStripSeparator(),
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
                new ToolStripMenuItem("AIR", Resources.AirOtherIcon, (s, e) => noteView.AirDirection = new AirDirection(VerticalAirDirection.Other, HorizontalAirDirection.Center))
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
                quantizeComboBox, widthSetBox, new ToolStripSeparator(), speedChBox, viewChBox,  laneVisible, widthAmountBox, deleteChhistory, nameChannel
            });
            }

            return menu;
        }
    }
}
