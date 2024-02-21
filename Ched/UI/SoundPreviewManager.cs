using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ched.Configuration;
using Ched.Core;
using Ched.Core.Events;
using Ched.Core.Notes;

namespace Ched.UI
{
    /// <remarks>
    /// 公開メソッドはUIスレッド経由での操作前提でスレッドセーフではない。
    /// </remarks>
    public class SoundPreviewManager : IDisposable
    {
        public event EventHandler<TickUpdatedEventArgs> TickUpdated;
        public event EventHandler Started;
        public event EventHandler Finished;
        public event EventHandler ExceptionThrown;

        private int CurrentTick { get; set; }
        private SoundManager SoundManager { get; } = new SoundManager();
        private ISoundPreviewContext PreviewContext { get; set; }
        private LinkedListNode<int?> TickElement;
        private LinkedListNode<int?> TapTickElement;
        private LinkedListNode<int?> ExTapTickElement;
        private LinkedListNode<int?> AirTickElement;
        private LinkedListNode<int?> ExAirTickElement;
        private LinkedListNode<int?> FlickTickElement;
        private LinkedListNode<int?> ExFlickTickElement;
        private LinkedListNode<int?> StepTickElement;
        private LinkedListNode<int?> ExStepTickElement;

        private LinkedListNode<BpmChangeEvent> BpmElement;
        private int LastSystemTick { get; set; }
        private int InitialTick { get; set; }
        private int StartTick { get; set; }
        private int EndTick { get; set; }
        private double elapsedTick;
        private Control SyncControl { get; }
        private Timer Timer { get; } = new Timer() { Interval = 4 };

        public bool Playing { get; private set; }
        public bool IsStopAtLastNote { get; set; }
        public bool IsSupported { get { return SoundManager.IsSupported; } }

        public SoundPreviewManager(Control syncControl)
        {
            SyncControl = syncControl;
            if (ApplicationSettings.Default.IsPJsekaiSounds)
            {
                Timer.Tick += Tick;
                Timer.Tick += Tick2;
                Timer.Tick += Tick3;
                Timer.Tick += Tick4;
                Timer.Tick += Tick5;
                Timer.Tick += Tick6;
                Timer.Tick += Tick7;
                Timer.Tick += Tick8;
            }
            else
            {
                Timer.Tick += GuideTick;
            }
            


            SoundManager.ExceptionThrown += (s, e) => SyncControl.InvokeIfRequired(() =>
            {
                Stop();
                ExceptionThrown?.Invoke(this, EventArgs.Empty);
            });
        }

        public bool Start(ISoundPreviewContext context, int startTick, NoteView noteview)
        {
            if (Playing) throw new InvalidOperationException();
            if (context == null) throw new ArgumentNullException("context");
            
            PreviewContext = context;
            SoundManager.Register(context.ClapSource.FilePath);
            SoundManager.Register(context.ClapSourceTap.FilePath);
            SoundManager.Register(context.ClapSourceExTap.FilePath);
            SoundManager.Register(context.ClapSourceAir.FilePath);
            SoundManager.Register(context.ClapSourceExAir.FilePath);
            SoundManager.Register(context.ClapSourceTrace.FilePath);
            SoundManager.Register(context.ClapSourceExTrace.FilePath);
            SoundManager.Register(context.ClapSourceStep.FilePath);
            SoundManager.Register(context.ClapSourceExStep.FilePath);

            SoundManager.Register(context.MusicSource.FilePath);

            var timeCalculator = new TimeCalculator(context.TicksPerBeat, context.BpmDefinitions);
            var ticks = new SortedSet<int>(context.GetGuideTicks(noteview)).ToList();
            var tapticks = new SortedSet<int>(context.GetTapTicks(noteview)).ToList();
            var extapticks = new SortedSet<int>(context.GetExTapTicks(noteview)).ToList();
            var airticks = new SortedSet<int>(context.GetAirTicks(noteview)).ToList();
            var exairticks = new SortedSet<int>(context.GetExAirTicks(noteview)).ToList();
            var flickticks = new SortedSet<int>(context.GetFlickTicks(noteview)).ToList();
            var exflickticks = new SortedSet<int>(context.GetExFlickTicks(noteview)).ToList();
            var stepticks = new SortedSet<int>(context.GetSlideStepTicks(noteview)).ToList();
            var exstepticks = new SortedSet<int>(context.GetExSlideStepTicks(noteview)).ToList();
            TickElement = new LinkedList<int?>(ticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            TapTickElement = new LinkedList<int?>(tapticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            ExTapTickElement = new LinkedList<int?>(extapticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            AirTickElement = new LinkedList<int?>(airticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            ExAirTickElement = new LinkedList<int?>(exairticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            FlickTickElement = new LinkedList<int?>(flickticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            ExFlickTickElement = new LinkedList<int?>(exflickticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            StepTickElement = new LinkedList<int?>(stepticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            ExStepTickElement = new LinkedList<int?>(exstepticks.Where(p => p >= startTick).OrderBy(p => p).Select(p => new int?(p))).First;
            BpmElement = new LinkedList<BpmChangeEvent>(context.BpmDefinitions.OrderBy(p => p.Tick)).First;

            EndTick = IsStopAtLastNote ? ticks[ticks.Count - 1] : timeCalculator.GetTickFromTime(SoundManager.GetDuration(context.MusicSource.FilePath));
            if (EndTick < startTick) return false;

            // スタート時まで進める
            while (TickElement != null && TickElement.Value < startTick) TickElement = TickElement.Next;
            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= startTick) BpmElement = BpmElement.Next;

            int clapLatencyTick = GetLatencyTick(context.ClapSource.Latency, BpmElement.Value.Bpm);
            InitialTick = startTick - clapLatencyTick;
            CurrentTick = InitialTick;
            StartTick = startTick;

            double startTime = timeCalculator.GetTimeFromTick(startTick);
            double headGap = Math.Max(-context.MusicSource.Latency - startTime, 0);
            elapsedTick = 0;
            Task.Run(() =>
            {
                LastSystemTick = Environment.TickCount;
                SyncControl.Invoke((MethodInvoker)(() => Timer.Start()));

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(Math.Max((context.ClapSource.Latency + headGap) / context.Speed, 0)));
                if (!Playing) return;
                SoundManager.Play(context.MusicSource.FilePath, startTime + context.MusicSource.Latency, context.MusicSource.Volume, context.Speed);
            })
            .ContinueWith(p =>
            {
                if (p.Exception != null)
                {
                    Program.DumpExceptionTo(p.Exception, "sound_exception.json");
                    ExceptionThrown?.Invoke(this, EventArgs.Empty);
                }
            });

            Playing = true;
            Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void Stop()
        {
            Timer.Stop();
            Playing = false;
            
            SoundManager.StopAll();
            Finished?.Invoke(this, EventArgs.Empty);
        }

        private void GuideTick(object sender, EventArgs e) //通常
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;

            if (CurrentTick >= EndTick + PreviewContext.TicksPerBeat)
            {
                Stop();
            }

            int latencyTick = GetLatencyTick(PreviewContext.ClapSource.Latency, BpmElement.Value.Bpm);
            if (TickElement == null || TickElement.Value - latencyTick > CurrentTick) return;
            while (TickElement != null && TickElement.Value - latencyTick <= CurrentTick)
            {
                TickElement = TickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSource.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }


        private void Tick(object sender, EventArgs e) //TAP
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;

            if (CurrentTick >= EndTick + PreviewContext.TicksPerBeat)
            {
                Stop();
            }

            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceTap.Latency, BpmElement.Value.Bpm);
            if (TapTickElement == null || TapTickElement.Value - latencyTick > CurrentTick) return;
            while (TapTickElement != null && TapTickElement.Value - latencyTick <= CurrentTick)
            {
                TapTickElement = TapTickElement.Next;
            }


            
            SoundManager.Play(PreviewContext.ClapSourceTap.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }

        private void Tick2(object sender, EventArgs e) //ExTAP
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;


            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceExTap.Latency, BpmElement.Value.Bpm);
            if (ExTapTickElement == null || ExTapTickElement.Value - latencyTick > CurrentTick) return;
            while (ExTapTickElement != null && ExTapTickElement.Value - latencyTick <= CurrentTick)
            {
                ExTapTickElement = ExTapTickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSourceExTap.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }

        private void Tick3(object sender, EventArgs e) // AIR
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;

            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceExAir.Latency, BpmElement.Value.Bpm);
            if (AirTickElement == null || AirTickElement.Value - latencyTick > CurrentTick) return;
            while (AirTickElement != null && AirTickElement.Value - latencyTick <= CurrentTick)
            {
                AirTickElement = AirTickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSourceAir.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }

        private void Tick4(object sender, EventArgs e) // ExAIR
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;

            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceExAir.Latency, BpmElement.Value.Bpm);
            if (ExAirTickElement == null || ExAirTickElement.Value - latencyTick > CurrentTick) return;
            while (ExAirTickElement != null && ExAirTickElement.Value - latencyTick <= CurrentTick)
            {
                ExAirTickElement = ExAirTickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSourceExAir.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }

        private void Tick5(object sender, EventArgs e) //TRACE
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;


            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceTrace.Latency, BpmElement.Value.Bpm);
            if (FlickTickElement == null || FlickTickElement.Value - latencyTick > CurrentTick) return;
            while (FlickTickElement != null && FlickTickElement.Value - latencyTick <= CurrentTick)
            {
                FlickTickElement = FlickTickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSourceTrace.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }

        private void Tick6(object sender, EventArgs e) //ExTrace
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;


            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceExTrace.Latency, BpmElement.Value.Bpm);
            if (ExFlickTickElement == null || ExFlickTickElement.Value - latencyTick > CurrentTick) return;
            while (ExFlickTickElement != null && ExFlickTickElement.Value - latencyTick <= CurrentTick)
            {
                ExFlickTickElement = ExFlickTickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSourceExTrace.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }

        private void Tick7(object sender, EventArgs e) //Step
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;

            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceStep.Latency, BpmElement.Value.Bpm);
            if (StepTickElement == null || StepTickElement.Value - latencyTick > CurrentTick) return;
            while (StepTickElement != null && StepTickElement.Value - latencyTick <= CurrentTick)
            {
                StepTickElement = StepTickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSourceStep.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }

        private void Tick8(object sender, EventArgs e) //ExStep
        {
            int now = Environment.TickCount;
            int elapsed = now - LastSystemTick;
            LastSystemTick = now;

            elapsedTick += PreviewContext.TicksPerBeat * BpmElement.Value.Bpm * elapsed * PreviewContext.Speed / 60 / 1000;
            CurrentTick = (int)(InitialTick + elapsedTick);
            if (CurrentTick >= StartTick)
                TickUpdated?.Invoke(this, new TickUpdatedEventArgs(Math.Max(CurrentTick, 0)));

            while (BpmElement.Next != null && BpmElement.Next.Value.Tick <= CurrentTick) BpmElement = BpmElement.Next;

            int latencyTick = GetLatencyTick(PreviewContext.ClapSourceExStep.Latency, BpmElement.Value.Bpm);
            if (ExStepTickElement == null || ExStepTickElement.Value - latencyTick > CurrentTick) return;
            while (ExStepTickElement != null && ExStepTickElement.Value - latencyTick <= CurrentTick)
            {
                ExStepTickElement = ExStepTickElement.Next;
            }



            SoundManager.Play(PreviewContext.ClapSourceExStep.FilePath, 0, PreviewContext.ClapSource.Volume, PreviewContext.Speed);
        }



        private int GetLatencyTick(double latency, double bpm)
        {
            return (int)(PreviewContext.TicksPerBeat * latency * bpm / 60);
        }

        public void Dispose()
        {
            SoundManager.Dispose();
        }
    }

    public interface ISoundPreviewContext
    {
        int TicksPerBeat { get; }
        double Speed { get; }
        IEnumerable<int> GetGuideTicks(NoteView noteview);
        IEnumerable<int> GetTapTicks(NoteView noteview);
        IEnumerable<int> GetExTapTicks(NoteView noteview);
        IEnumerable<int> GetAirTicks(NoteView noteview);
        IEnumerable<int> GetExAirTicks(NoteView noteview);
        IEnumerable<int> GetFlickTicks(NoteView noteview);
        IEnumerable<int> GetExFlickTicks(NoteView noteview);
        IEnumerable<int> GetSlideStepTicks(NoteView noteview);
        IEnumerable<int> GetExSlideStepTicks(NoteView noteview);
        IEnumerable<BpmChangeEvent> BpmDefinitions { get; }
        SoundSource MusicSource { get; }
        SoundSource ClapSource { get; }
        SoundSource ClapSourceTap { get; }
        SoundSource ClapSourceExTap { get; }
        SoundSource ClapSourceAir { get; }
        SoundSource ClapSourceExAir { get; }
        SoundSource ClapSourceTrace { get; }
        SoundSource ClapSourceExTrace { get; }
        SoundSource ClapSourceStep { get; }
        SoundSource ClapSourceExStep { get; }
    }

    public class SoundPreviewContext : ISoundPreviewContext
    {
        private Core.Score score;

        public int TicksPerBeat => score.TicksPerBeat;
        public double Speed { get; private set; } = 1.0;
        public IEnumerable<BpmChangeEvent> BpmDefinitions => score.Events.BpmChangeEvents;
        public SoundSource MusicSource { get; }
        public SoundSource ClapSource { get; }
        public SoundSource ClapSourceTap { get; }
        public SoundSource ClapSourceExTap { get; }
        public SoundSource ClapSourceAir { get; }
        public SoundSource ClapSourceExAir { get; }
        public SoundSource ClapSourceTrace { get; }
        public SoundSource ClapSourceExTrace { get; }
        public SoundSource ClapSourceStep { get; }
        public SoundSource ClapSourceExStep { get; }


        public SoundPreviewContext(Core.Score score, SoundSource musicSource, SoundSource clapSource, SoundSource clapSource2, SoundSource clapSource3, SoundSource clapSource4, SoundSource clapSource5, SoundSource clapSource6, SoundSource clapSource7, SoundSource clapSource8, SoundSource clapSource9)
        {
            this.score = score;
            MusicSource = musicSource;
            ClapSource = clapSource;

            ClapSourceTap = clapSource2;
            ClapSourceExTap = clapSource3;
            ClapSourceAir = clapSource4;
            ClapSourceExAir = clapSource5;
            ClapSourceTrace = clapSource6;
            ClapSourceExTrace = clapSource7;
            ClapSourceStep = clapSource8;
            ClapSourceExStep = clapSource9;


            Speed = Configuration.ApplicationSettings.Default.IsSlowDownPreviewEnabled ? 0.5 : 1.0;
        }
        public SoundPreviewContext(Core.Score score, SoundSource musicSource, SoundSource clapSource)
        {
            this.score = score;
            MusicSource = musicSource;
            ClapSource = clapSource;
            ClapSourceTap = clapSource;
            ClapSourceExTap = clapSource;
            ClapSourceAir = clapSource;
            ClapSourceExAir = clapSource;
            ClapSourceTrace = clapSource;
            ClapSourceExTrace = clapSource;
            ClapSourceStep = clapSource;
            ClapSourceExStep = clapSource;

            Speed = Configuration.ApplicationSettings.Default.IsSlowDownPreviewEnabled ? 0.5 : 1.0;
        }


        public IEnumerable<int> GetGuideTicks(NoteView noteview) => GetGuideTicks(score.Notes, noteview);

        private IEnumerable<int> GetGuideTicks(Core.NoteCollection notes, NoteView noteview)
        {

            var shortNotesTick = notes.Taps.Cast<TappableBase>().Where(p => !p.IsStart).Concat(notes.ExTaps).Where(p => !p.IsStart).Concat(notes.Flicks).Concat(notes.StepNoteTaps).Select(p => p.Tick);
            var holdsTick = notes.Holds.SelectMany(p => new int[] { p.StartTick, p.StartTick + p.Duration });
            var slidesTick = notes.Slides.SelectMany(p => new int[] { p.StartTick }.Concat(p.StepNotes.Where(q => q.IsVisible).Select(q => q.Tick)));
            var airActionsTick = notes.AirActions.SelectMany(p => p.ActionNotes.Select(q => p.StartTick + q.Offset));

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                shortNotesTick = notes.Taps.Cast<TappableBase>().Where(p => p.Channel == noteview.ViewChannel || allch && !p.IsStart).Concat(notes.ExTaps.Where(p => p.Channel == noteview.ViewChannel || allch && !p.IsStart)).Concat(notes.Flicks.Where(p => p.Channel == noteview.ViewChannel || allch)).Select(p => p.Tick);
                holdsTick = notes.Holds.SelectMany(p => new int[] { p.StartTick, p.StartTick + p.Duration });
                slidesTick = notes.Slides.Where(p => p.Channel == noteview.ViewChannel || allch).SelectMany(p => new int[] { p.StartTick }.Concat(p.StepNotes.Where(q => q.IsVisible).Select(q => q.Tick)));
                airActionsTick = notes.AirActions.SelectMany(p => p.ActionNotes.Select(q => p.StartTick + q.Offset));
            }


            return shortNotesTick.Concat(holdsTick).Concat(slidesTick).Concat(airActionsTick);

        }


        public IEnumerable<int> GetTapTicks(NoteView noteview) => GetTapTicks(score.Notes, noteview);

        private IEnumerable<int> GetTapTicks(Core.NoteCollection notes, NoteView noteview)
        {
            var tapList = new List<Tap>();
            
            foreach(var note in notes.Taps)
            {
                bool isAir = false;
                foreach (var note2 in notes.Airs)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isAir = true;
                }
                if (isAir) continue;
                if(note.IsStart) continue;
                

                tapList.Add(note);
            }


            var slidesTick = notes.Slides.SelectMany(p => new int[] { p.StartTick }.Concat(p.StepNotes.Where(q => q.Equals(p.StepNotes.OrderBy(s => s.Tick).Last())).Select(s => s.Tick)));

            var shortNotesTick = tapList.Cast<TappableBase>().Select(p => p.Tick);

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                shortNotesTick = tapList.Cast<TappableBase>().Where(p => p.Channel == noteview.ViewChannel || allch).Concat(notes.ExTaps.Where(p => p.Channel == noteview.ViewChannel || allch)).Concat(notes.Flicks.Where(p => p.Channel == noteview.ViewChannel || allch)).Select(p => p.Tick);
                slidesTick = notes.Slides.Where(p => p.Channel == noteview.ViewChannel || allch).SelectMany(p => new int[] { p.StartTick }.Concat(p.StepNotes.Where(q => q.Equals(p.StepNotes.OrderBy(s =>s.Tick).Last())).Select(s => s.Tick)));
            }
            

            return shortNotesTick.Concat(slidesTick);
        }

        public IEnumerable<int> GetExTapTicks(NoteView noteview) => GetExTapTicks(score.Notes, noteview);

        private IEnumerable<int> GetExTapTicks(Core.NoteCollection notes, NoteView noteview)
        {

            var tapList = new List<ExTap>();

            foreach (var note in notes.ExTaps)
            {
                bool isAir = false;
                bool onTrace = false;
                bool onSlide = false;
                bool onStep = false;
                foreach (var note2 in notes.Airs)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isAir = true;
                }
                if (isAir) continue;
                foreach (var note2 in notes.Flicks)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) onTrace = true;
                }
                if (onTrace) continue;
                foreach (var note2 in notes.StepNoteTaps)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) onStep = true;
                }
                if (onStep) continue;
                foreach (var note2 in notes.Slides)
                {
                    if (note.Tick == note2.StartTick && note.Channel == note2.Channel && note.LaneIndex == note2.StartLaneIndex) onSlide = true;
                }
                if (onSlide) continue;
                if (note.IsStart) continue;

                tapList.Add(note);
            }

            var exNotesTick = tapList.Cast<TappableBase>().Select(p => p.Tick);

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                exNotesTick = tapList.Cast<TappableBase>().Where(p => p.Channel == noteview.ViewChannel || allch).Select(p => p.Tick);
            }


            return exNotesTick;
        }

        public IEnumerable<int> GetAirTicks(NoteView noteview) => GetAirTicks(score.Notes, noteview);

        private IEnumerable<int> GetAirTicks(Core.NoteCollection notes, NoteView noteview)
        {
            var airNotesList = new List<Air>();

            foreach (var note in notes.Airs)
            {
                if (note.VerticalDirection != VerticalAirDirection.Up) continue;
                bool isEx = false;
                bool isSlideExAir = false;
                
                foreach (var note2 in notes.ExTaps)
                {
                    
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isEx = true;
                }
                if (isEx) continue;
                foreach (var note2 in notes.Slides)
                {
                    bool isSlideEx = false;
                    bool onSlide = false;
                    var end = note2.StepNotes.OrderBy(p => p.Tick).Last();
                    foreach(var note3 in notes.ExTaps)
                    {
                        if (note2.StartTick == note3.Tick && note2.Channel == note3.Channel && note2.StartLaneIndex == note3.LaneIndex) isSlideEx = true;
                    }
                    
                    if (end.Tick == note.Tick && end.Channel == note.Channel && end.LaneIndex == note.LaneIndex) onSlide = true;
                    isSlideExAir = isSlideEx && onSlide;
                }
                if (isSlideExAir) continue;




                airNotesList.Add(note);
            }

            var airNotesTick = airNotesList.Select(p => p.Tick);

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                airNotesTick = airNotesList.Where(p => p.Channel == noteview.ViewChannel || allch).Select(p => p.Tick);
            }


            return airNotesTick;
        }

        public IEnumerable<int> GetExAirTicks(NoteView noteview) => GetExAirTicks(score.Notes, noteview);

        private IEnumerable<int> GetExAirTicks(Core.NoteCollection notes, NoteView noteview)
        {
            var airNotesList = new List<Air>();

            foreach (var note in notes.Airs)
            {
                if (note.VerticalDirection != VerticalAirDirection.Up) continue;
                bool isEx = false;
                bool isSlideExAir = false;

                foreach (var note2 in notes.ExTaps)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isEx = true;
                }
                

                foreach (var note2 in notes.Slides)
                {
                    bool isSlideEx = false;
                    foreach (var note3 in notes.ExTaps)
                    {
                        if (note2.StartTick == note3.Tick && note2.Channel == note3.Channel && note2.StartLaneIndex == note3.LaneIndex) isSlideEx = true;
                    }
                    if (!isSlideEx) continue;
                    var end = note2.StepNotes.OrderBy(p => p.Tick).Last();
                    if (end.Tick == note.Tick && end.Channel == note.Channel && end.LaneIndex == note.LaneIndex) isSlideExAir = true;
                }
                if (isEx || isSlideExAir)
                {
                    airNotesList.Add(note);
                }
            }



            var airNotesTick = airNotesList.Select(p => p.Tick);

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                airNotesTick = airNotesList.Where(p => p.Channel == noteview.ViewChannel || allch).Select(p => p.Tick);
            }


            return airNotesTick;
        }

        public IEnumerable<int> GetFlickTicks(NoteView noteview) => GetFlickTicks(score.Notes, noteview);

        private IEnumerable<int> GetFlickTicks(Core.NoteCollection notes, NoteView noteview)
        {
            var flickNotesList = new List<Flick>();

            foreach (var note in notes.Flicks)
            {
                bool isEx = false;
                bool isOnStep = false;
                foreach (var note2 in notes.ExTaps)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isEx = true;
                }
                if (isEx) continue;
                foreach(var note2 in notes.Slides)
                {
                    foreach (var note3 in note2.StepNotes)
                    {
                        if (note.Tick == note3.Tick && note.Channel == note3.Channel && note.LaneIndex == note3.LaneIndex) isOnStep = true;
                    }
                }
                if (!isOnStep)
                {
                    flickNotesList.Add(note);
                }

            }

            var flickNotesTick = flickNotesList.Select(p => p.Tick);

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                flickNotesTick = flickNotesList.Where(p => p.Channel == noteview.ViewChannel || allch).Select(p => p.Tick);
            }


            return flickNotesTick;
        }

        public IEnumerable<int> GetExFlickTicks(NoteView noteview) => GetExFlickTicks(score.Notes, noteview);

        private IEnumerable<int> GetExFlickTicks(Core.NoteCollection notes, NoteView noteview)
        {
            var flickNotesList = new List<Flick>();

            foreach (var note in notes.Flicks)
            {
                bool isEx = false;
                bool isOnStep = false;
                foreach (var note2 in notes.ExTaps)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isEx = true;
                }
                if (!isEx) continue;
                foreach (var note2 in notes.Slides)
                {
                    foreach (var note3 in note2.StepNotes)
                    {
                        if (note.Tick == note3.Tick && note.Channel == note3.Channel && note.LaneIndex == note3.LaneIndex) isOnStep = true;
                    }
                }
                if (!isOnStep)
                {
                    flickNotesList.Add(note);
                }
            }

            var flickNotesTick = flickNotesList.Select(p => p.Tick);

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                flickNotesTick = flickNotesList.Where(p => p.Channel == noteview.ViewChannel || allch).Select(p => p.Tick);
            }


            return flickNotesTick;
        }

        public IEnumerable<int> GetSlideStepTicks(NoteView noteview) => GetSlideStepTicks(score.Notes, noteview);

        private IEnumerable<int> GetSlideStepTicks(Core.NoteCollection notes, NoteView noteview)
        {

            var tapList = new List<Slide>();
            var tapList2 = new List<StepNoteTap>();

            foreach (var note in notes.Slides)
            {
                bool isEx = false;
                
                foreach (var note2 in notes.ExTaps)
                {
                    if (note.StartTick == note2.Tick && note.Channel == note2.Channel && note.StartLaneIndex == note2.LaneIndex) isEx = true;
                }
                if (!isEx) //ifにしたのは鳴ってないと勘違いして書き換えちゃったから
                {
                    tapList.Add(note);
                }

            }

            foreach(var note in notes.StepNoteTaps)
            {
                bool isEx2 = false;
                foreach (var note2 in notes.ExTaps)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isEx2 = true;
                }
                if (!isEx2)
                {
                    tapList2.Add(note);
                }
            }


            var stepNotesTick = tapList.SelectMany(p => p.StepNotes.Where(s => s.IsVisible && p.StepNotes.OrderBy(o => o.Tick).Last() != s).Select(q => q.Tick)).Concat(tapList2.Select(q => q.Tick));

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                stepNotesTick = tapList.Where(p => p.Channel == noteview.ViewChannel || allch).SelectMany(p => p.StepNotes.Where(s => s.IsVisible && p.StepNotes.OrderBy(o => o.Tick).Last() != s).Select(q => q.Tick)).Concat(tapList2.Where(p => p.Channel == noteview.ViewChannel || allch).Select(q => q.Tick));
            }


            return stepNotesTick;
        }

        public IEnumerable<int> GetExSlideStepTicks(NoteView noteview) => GetExSlideStepTicks(score.Notes, noteview);

        private IEnumerable<int> GetExSlideStepTicks(Core.NoteCollection notes, NoteView noteview)
        {

            var tapList = new List<Slide>();
            var tapList2 = new List<StepNoteTap>();

            foreach (var note in notes.Slides)
            {
                bool isEx = false;
                foreach (var note2 in notes.ExTaps)
                {
                    if (note.StartTick == note2.Tick && note.Channel == note2.Channel && note.StartLaneIndex == note2.LaneIndex) isEx = true;
                }
                if (isEx)
                {
                    tapList.Add(note);
                }
            }
            foreach (var note in notes.StepNoteTaps)
            {
                bool isEx2 = false;
                foreach (var note2 in notes.ExTaps)
                {
                    if (note.Tick == note2.Tick && note.Channel == note2.Channel && note.LaneIndex == note2.LaneIndex) isEx2 = true;
                }
                if (isEx2)
                {
                    tapList2.Add(note);
                }
            }

            var stepNotesTick = tapList.SelectMany(p => p.StepNotes.Where(s => s.IsVisible && p.StepNotes.OrderBy(o => o.Tick).Last() != s)).Select(q => q.Tick).Concat(tapList2.Select(q => q.Tick));

            var allch = noteview.ViewChannel == -1;
            if (noteview.SoundbyCh)
            {
                stepNotesTick = tapList.Where(p => p.Channel == noteview.ViewChannel || allch).SelectMany(p => p.StepNotes.Where(s => s.IsVisible && p.StepNotes.OrderBy(o => o.Tick).Last() != s).Select(q => q.Tick)).Concat(tapList2.Where(p => p.Channel == noteview.ViewChannel || allch).Select(q => q.Tick));
            }


            return stepNotesTick;
        }


    }

    public class TickUpdatedEventArgs : EventArgs
    {
        public int Tick { get; }

        public TickUpdatedEventArgs(int tick)
        {
            Tick = tick;
        }
    }
}
