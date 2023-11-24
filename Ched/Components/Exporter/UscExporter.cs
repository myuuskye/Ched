using Ched.Configuration;
using Ched.Core;
using Ched.Core.Events;
using Ched.Core.Notes;
using Ched.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Ched.Components.Exporter
{
    public class UscExporter
    {

        protected ScoreBook ScoreBook { get; set; }
        protected BarIndexCalculator BarIndexCalculator { get; }
        protected int StandardBarTick => ScoreBook.Score.TicksPerBeat * 4;

        private int offset = 0;
       

        [Newtonsoft.Json.JsonProperty]
        private USC usc;

        [Newtonsoft.Json.JsonProperty]
        private int version = 2;

       

        internal static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        public UscExporter(ScoreBook book)
        {
            ScoreBook = book;
            BarIndexCalculator = new BarIndexCalculator(book.Score.TicksPerBeat, book.Score.Events.TimeSignatureChangeEvents);
        }

        public void Export(Stream stream)
        {
            
            var book = ScoreBook;
            var notes = book.Score.Notes;
            var objects = new List<USCObject>();
            usc = new USC(offset);

            int guideDefType = ApplicationSettings.Default.GuideDefaultFade;

            var bpmchange = new List<USCObject>();
            var timescalechange = new List<USCObject>();

            foreach (var bpmevent in book.Score.Events.BpmChangeEvents)
            {
                var change = new USCBpmChange((double)bpmevent.Tick / 480, bpmevent.Bpm);

                usc.objects.Add(change);

            }
            Dictionary<int, int> channelAdapt = new Dictionary<int, int>();

            var speedEvents = book.Score.Events.HighSpeedChangeEvents;
            int count = 0;
            if (speedEvents.Count > 0)
            {
                count = book.Score.Events.HighSpeedChangeEvents.OrderBy(p => p.SpeedCh).First().SpeedCh;


                List<USCTimeScale> timeScaleList = new List<USCTimeScale>();
                for (int i = 0; i <= speedEvents.OrderBy(p => p.SpeedCh).Last().SpeedCh; i++)
                {
                    timeScaleList = new List<USCTimeScale>();
                    foreach (var sclaeevent in book.Score.Events.HighSpeedChangeEvents.OrderBy(p => p.SpeedCh).Where(q => i == q.SpeedCh))
                    {
                        var change = new USCTimeScale((double)sclaeevent.Tick / 480, sclaeevent.SpeedRatio);
                        timeScaleList.Add(change);
                    }
                    var timeScaleChange = new USCTimeScaleChange(timeScaleList);

                    usc.objects.Add(timeScaleChange);
                }
            }

            foreach (var note in notes.Taps) //TAP
            {

                bool isOnSlide = false;
                bool isOnGuide = false;
                bool isAir = false;
                bool isAirDown = false;
                bool isOnFlick = false;
                foreach (var note2 in notes.Slides)
                {
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnSlide = true;
                    foreach (var note3 in note2.StepNotes)
                    {
                        if ((note.LaneIndex == note3.LaneIndex) && (note.Tick == note3.Tick)) isOnSlide = true;
                    }
                }
                if (isOnSlide && ApplicationSettings.Default.IsTapHideOnSlide && !note.IsStart) continue; //SlideやStepと重なってたらスキップ
                if (isOnSlide && ApplicationSettings.Default.IsTap2HideOnSlide && note.IsStart) continue; //SlideやStepと重なってたらスキップ(TAP2)

                foreach (var note2 in notes.Guides)
                {
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnGuide = true;
                    foreach (var note3 in note2.StepNotes)
                    {
                        if ((note.LaneIndex == note3.LaneIndex) && (note.Tick == note3.Tick)) isOnGuide = true;
                    }
                }
                if (isOnGuide && ApplicationSettings.Default.IsTapHideOnGuide && note.IsStart) continue;
                foreach (var note2 in notes.Airs)
                {
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick) && (note2.VerticalDirection == VerticalAirDirection.Up)) isAir = true;
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick) && (note2.VerticalDirection == VerticalAirDirection.Down)) isAirDown = true;
                }
                if(isAir) continue; //Airと重なってたらスキップ
               
                if (isAirDown && !isOnGuide) continue; //AirDownと重なっていて、Guideと重なっていなかったらスキップ

                foreach (var note2 in notes.Flicks)
                {
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick)) isOnFlick = true;
                }
                if (isOnFlick) continue; //Flickと重なってたらスキップ

                if (note.IsStart)
                {
                    if (isOnGuide) continue; //Guideと重なってたらスキップ
                }
                var laneIndex = note.LaneIndex - 8 + (float)book.LaneOffset + note.Width / 2;
                var singlenote = new USCSingleNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2, false, false);
                usc.objects.Add(singlenote);
                
            }


            foreach (var note in notes.ExTaps)//ExTAP
            {
                bool isOnSlide = false;
                bool isOnGuide = false;
                bool isAir = false;
                bool isAirDown = false;
                bool isOnFlick = false;
                bool isOnStepNoteTap = false;

                foreach (var note2 in notes.Slides)
                {
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnSlide = true;
                    foreach (var note3 in note2.StepNotes)
                    {
                        if ((note.LaneIndex == note3.LaneIndex) && (note.Tick == note3.Tick)) isOnSlide = true;
                    }
                }
                if (isOnSlide && ApplicationSettings.Default.IsExTapHideOnSlide && !note.IsStart) continue; //Slideと重なっていて、スキップ可能だったらスキップ
                if (isOnSlide && ApplicationSettings.Default.IsExTap2HideOnSlide && note.IsStart) continue; //Slideと重なっていて、スキップ可能だったらスキップ
                foreach (var note2 in notes.Guides)
                {
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnGuide = true;
                    foreach (var note3 in note2.StepNotes)
                    {
                        if ((note.LaneIndex == note3.LaneIndex) && (note.Tick == note3.Tick)) isOnGuide = true;
                    }
                }
                if (isOnGuide && ApplicationSettings.Default.IsExTapHideOnGuide) continue;
                foreach (var note2 in notes.Airs)
                {
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick)) isAir = true;
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick) && (note2.VerticalDirection == VerticalAirDirection.Down)) isAirDown = true;
                }
                if (isAir) continue; //Airと重なってたらスキップ
                if (isAirDown && !isOnGuide) continue; //AirDownと重なっていて、Guideと重なっていなかったらスキップ
                foreach (var note2 in notes.Flicks)
                {
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick)) isOnFlick = true;
                }
                if (isOnFlick) continue; //Flickと重なってたらスキップ
                foreach (var note2 in notes.StepNoteTaps)
                {
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick)) isOnStepNoteTap = true;
                }
                if (isOnStepNoteTap) continue; //Flickと重なってたらスキップ
                if (note.IsStart)
                {
                    if (isOnGuide) continue; //Guideと重なってたらスキップ
                }

                var laneIndex = note.LaneIndex - 8 + (float)book.LaneOffset + note.Width / 2;
                var singlenote = new USCSingleNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2, true, false);
                usc.objects.Add(singlenote);
            }



            foreach (var note in notes.Flicks)
            {
                bool isOnSlide = false;
                bool isOnGuide = false;
                bool isAir = false;
                bool isCritical = false;
                foreach (var note2 in notes.Slides)
                {
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnSlide = true;
                    foreach (var note3 in note2.StepNotes)
                    {
                        if ((note.LaneIndex == note3.LaneIndex) && (note.Tick == note3.Tick)) isOnSlide = true;
                    }
                }
                if (isOnSlide && ApplicationSettings.Default.IsFlickHideOnSlide) continue; //Slideと重なってたらスキップ
                foreach (var note2 in notes.Guides)
                {
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnGuide = true;
                    foreach (var note3 in note2.StepNotes)
                    {
                        if ((note.LaneIndex == note3.LaneIndex) && (note.Tick == note3.Tick)) isOnGuide = true;
                    }
                }
                if (isOnGuide && ApplicationSettings.Default.IsFlickHideOnGuide) continue; //Slideと重なってたらスキップ
                foreach (var note2 in notes.Airs)
                {
                    if ((note.LaneIndex == note2.LaneIndex) && (note.Tick == note2.Tick)) isAir = true;
                }
                if (isAir) continue; //Airと重なってたらスキップ

                foreach (var note2 in notes.ExTaps)
                {
                    if (note.Tick == note2.Tick && note.LaneIndex == note2.LaneIndex) isCritical = true;
                }

                var laneIndex = note.LaneIndex - 8 + (float)book.LaneOffset + note.Width / 2;
                var singlenote = new USCSingleNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2, isCritical, true);
                usc.objects.Add(singlenote);
            }


            foreach (var note in notes.Damages)
            {
                bool isOnSlide = false;
                bool isOnGuide = false;

                foreach (var note2 in notes.Slides)
                {
                    var endNote = note2.StepNotes.OrderBy(p => p.TickOffset).Last();
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnSlide = true;
                    if ((note.LaneIndex == endNote.LaneIndex) && (note.Tick == endNote.Tick)) isOnSlide = true;

                }
                if (isOnSlide && ApplicationSettings.Default.IsDamageHideOnSlide) continue; //Slideと重なってたらスキップ
                foreach (var note2 in notes.Guides)
                {
                    var endNote = note2.StepNotes.OrderBy(p => p.TickOffset).Last();
                    if ((note.LaneIndex == note2.StartNote.LaneIndex) && (note.Tick == note2.StartNote.Tick)) isOnGuide = true;
                    if ((note.LaneIndex == endNote.LaneIndex) && (note.Tick == endNote.Tick)) isOnGuide = true;

                }
                if (isOnGuide && ApplicationSettings.Default.IsDamageHideOnGuide) continue; //Slideと重なってたらスキップ

                var laneIndex = note.LaneIndex - 8 + (float)book.LaneOffset + note.Width / 2;
                var singlenote = new USCDamageNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2);

                usc.objects.Add(singlenote);
            }

            foreach (var note in notes.StepNoteTaps)
            {
                bool isCritical = false;

                foreach (var note2 in notes.ExTaps)
                {
                    if (note.LaneIndex == note2.LaneIndex && note.Tick == note2.Tick) isCritical = true;
                }
                var laneIndex = note.LaneIndex - 8 + (float)book.LaneOffset + note.Width / 2;
                var visiblestepnote = new USCConnectionVisibleTickNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2, isCritical, "linear");

                var slidestart = new USCConnectionStartNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2, isCritical, "linear", "none");
                var slideend = new USCConnectionEndNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2, isCritical, "none");

                var slidenote = new USCSlideNote(isCritical, slidestart, visiblestepnote, slideend);

                usc.objects.Add(slidenote);
            }


            foreach (var note in notes.Airs)
            {
                bool isCritical = false;
                bool isTrace = false;
                bool isOnSlide = false;
                int direction = (int)note.HorizontalDirection;

                foreach (var note2 in notes.ExTaps)
                {
                    if (note.Tick == note2.Tick && note.LaneIndex == note2.LaneIndex) isCritical = true;
                }
                foreach (var note2 in notes.Flicks)
                {
                    if (note.Tick == note2.Tick && note.LaneIndex == note2.LaneIndex)
                    {
                        isTrace = true;
                        if(note.VerticalDirection == VerticalAirDirection.Down)
                        {
                            direction = 3;
                        }
                    }
                    
                }
                if (direction != 3 && note.VerticalDirection == VerticalAirDirection.Down) continue;
                foreach (var note2 in notes.Slides)
                {
                    var endNote = note2.StepNotes.OrderBy(p => p.TickOffset).Last();
                    if ((note.Tick == note2.StartNote.Tick && note.LaneIndex == note2.StartNote.LaneIndex) ||
                        (note.Tick == endNote.Tick && note.LaneIndex == endNote.LaneIndex)) isOnSlide = true;
                    else
                    {
                        foreach(var note3 in note2.StepNotes)
                        {
                            if (note.Tick == note3.Tick && note.LaneIndex == note3.LaneIndex) isOnSlide = true;
                        }
                    }
                }

                if (isOnSlide) continue;

                var laneIndex = note.LaneIndex - 8 + (float)book.LaneOffset + note.Width / 2;
                var air = new USCAirNote((double)note.Tick / 480, note.Channel, laneIndex, note.Width / 2, isCritical, isTrace, direction);

                usc.objects.Add(air);
            }


            foreach(var note in notes.Slides)
            {
                bool isCritical = false;
                bool isEndCritical = false;
                string startJudge = "normal";
                string startEase = "linear";

                string endJudge = "normal";
                int endDirection = 3;
                var endNote = note.StepNotes.OrderBy(p => p.TickOffset).Last();

                var isTapEraceStart = ApplicationSettings.Default.IsTapEraseStart;
                var isExTapEraceStart = ApplicationSettings.Default.IsExTapEraseStart;
                var isTap2EraceStart = ApplicationSettings.Default.IsTap2EraseStart;
                var isExTap2EraceStart = ApplicationSettings.Default.IsExTap2EraseStart;
                var isDamageEraceStart = ApplicationSettings.Default.IsDamageEraseStart;


                var isTapEraceEnd = ApplicationSettings.Default.IsTapEraseEnd;
                var isExTapEraceEnd = ApplicationSettings.Default.IsExTapEraseEnd;
                var isTap2EraceEnd = ApplicationSettings.Default.IsTap2EraseEnd;
                var isExTap2EraceEnd = ApplicationSettings.Default.IsExTap2EraseEnd;
                var isDamageEraceEnd = ApplicationSettings.Default.IsDamageEraseEnd;

                var isFlickTraceStart = ApplicationSettings.Default.IsFlickSlideStartTrace;
                var isFlickTraceEnd = ApplicationSettings.Default.IsFlickSlideEndTrace;

                var slideStartDefType = ApplicationSettings.Default.SlideStartDefaultType;
                var slideEndDefType = ApplicationSettings.Default.SlideEndDefaultType;

                switch (slideStartDefType)
                {
                    case 1:
                        startJudge = "trace";
                        break;
                    case 2:
                        startJudge = "none";
                        break;
                }
                switch (slideEndDefType)
                {
                    case 1:
                        endJudge = "trace";
                        break;
                    case 2:
                        endJudge = "none";
                        break;
                }





                foreach (var note2 in notes.ExTaps)
                {
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex) isCritical = true;
                    if ((endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex) || isCritical ) isEndCritical = true;
                }
                foreach (var note2 in notes.Flicks)
                {
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && isFlickTraceStart) startJudge = "trace";
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex && isFlickTraceEnd) endJudge = "trace";
                }
                foreach (var note2 in notes.Taps)
                {
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && isTapEraceStart && note2.IsStart == false) startJudge = "none";
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && isTap2EraceStart && note2.IsStart == true) startJudge = "none";
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex && isTapEraceEnd && note2.IsStart == false) endJudge = "none";
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex && isTap2EraceEnd && note2.IsStart == true) endJudge = "none";
                }
                foreach (var note2 in notes.ExTaps)
                {
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && isExTapEraceStart && note2.IsStart == false) startJudge = "none";
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && isExTap2EraceStart && note2.IsStart == true) startJudge = "none";
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex && isExTapEraceEnd && note2.IsStart == false) endJudge = "none";
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex && isExTap2EraceEnd && note2.IsStart == true) endJudge = "none";

                }
                foreach (var note2 in notes.Damages)
                {
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && isDamageEraceStart) startJudge = "none";
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex && isDamageEraceEnd) endJudge = "none";
                }
                foreach (var note2 in notes.Airs)
                {
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && note2.VerticalDirection == VerticalAirDirection.Down)
                    {
                        switch(note2.HorizontalDirection)
                        {
                            case HorizontalAirDirection.Center:
                                startEase = "in";
                                break;
                            case HorizontalAirDirection.Left:
                            case HorizontalAirDirection.Right:
                                startEase = "out";
                                break;
                        }
                    }
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex && note2.VerticalDirection == VerticalAirDirection.Up)
                    {
                        endDirection = (int)note2.HorizontalDirection;
                    }
                }

                List<USCConnectionTickNote> ticknotes = new List<USCConnectionTickNote>();
                List<USCConnectionVisibleTickNote> visibleticknotes = new List<USCConnectionVisibleTickNote>();
                List<USCConnectionAttachNote> attachnotes = new List<USCConnectionAttachNote>();
                
                foreach (var step in note.StepNotes)
                {
                    if (step == endNote) continue;
                    string stepEase = "linear";
                    bool isAttach = false;
                    bool isStepCritical = false;
                    var steplaneIndex = step.LaneIndex - 8 + (float)book.LaneOffset + step.Width / 2;
                    var stepnote = new USCConnectionTickNote((double)step.Tick / 480, step.Channel, steplaneIndex, step.Width / 2, stepEase);
                    var visiblestepnote = new USCConnectionVisibleTickNote((double)step.Tick / 480, step.Channel, steplaneIndex, step.Width / 2, isStepCritical , stepEase);
                    var attachnote = new USCConnectionAttachNote((double)step.Tick / 480, step.Channel, steplaneIndex, step.Width / 2, isStepCritical);

                    foreach (var note2 in notes.Flicks) 
                    {
                        if (step.Tick == note2.Tick && step.LaneIndex == note2.LaneIndex) isAttach = true;
                    }
                    foreach(var note2 in notes.ExTaps)
                    {
                        if ((step.Tick == note2.Tick && step.LaneIndex == note2.LaneIndex) || isCritical) isStepCritical = true;
                    }
                    foreach( var note2 in notes.Airs)
                    {
                        if (step.Tick == note2.Tick && step.LaneIndex == note2.LaneIndex && note2.VerticalDirection == VerticalAirDirection.Down)
                        {
                            switch (note2.HorizontalDirection)
                            {
                                case HorizontalAirDirection.Center:
                                    stepEase = "in";
                                    break;
                                case HorizontalAirDirection.Left:
                                case HorizontalAirDirection.Right:
                                    stepEase = "out";
                                    break;
                            }
                        };
                    }
                    if (isAttach)
                    {
                        attachnote = new USCConnectionAttachNote((double)step.Tick / 480, step.Channel, steplaneIndex, step.Width / 2, isStepCritical);
                        attachnotes.Add(attachnote);
                    }
                    else
                    {
                        if (step.IsVisible)
                        {
                            visiblestepnote = new USCConnectionVisibleTickNote((double)step.Tick / 480, step.Channel, steplaneIndex, step.Width / 2, isStepCritical, stepEase);
                            visibleticknotes.Add(visiblestepnote);
                        }
                        else
                        {
                            stepnote = new USCConnectionTickNote((double)step.Tick / 480, step.Channel, steplaneIndex, step.Width / 2, stepEase);
                            ticknotes.Add(stepnote);
                        }
                    }




                }

                var startlaneIndex = note.StartNote.LaneIndex - 8 + (float)book.LaneOffset + note.StartNote.Width / 2;
                var endlaneIndex = endNote.LaneIndex - 8 + (float)book.LaneOffset + endNote.Width / 2;

                var slidestart = new USCConnectionStartNote((double)note.StartNote.Tick / 480, note.Channel, startlaneIndex, note.StartNote.Width / 2, isCritical, startEase, startJudge);
                var slideend = new USCConnectionEndNote((double)endNote.Tick / 480, endNote.Channel, endlaneIndex, endNote.Width / 2, isEndCritical, endJudge);
                var slideairend = new USCConnectionAirEndNote((double)endNote.Tick / 480, endNote.Channel, endlaneIndex, endNote.Width / 2,  isEndCritical, endJudge, endDirection);

                



                var slidenote = new USCSlideNote(isCritical, slidestart, visibleticknotes.ToArray(), ticknotes.ToArray(), attachnotes.ToArray(), slideend);

                if(endDirection != 3)
                {
                    slidenote = new USCSlideNote(isCritical, slidestart, visibleticknotes.ToArray(), ticknotes.ToArray(), attachnotes.ToArray(), slideairend);
                }


                usc.objects.Add(slidenote);
            }


            foreach(var note in notes.Guides)
            {
                string color = "green";
                string fade = "none";
                string startEase = "linear";
                var endNote = note.StepNotes.OrderBy(p => p.TickOffset).Last();

                var isTapChangeFade = ApplicationSettings.Default.IsTapChangeFade;
                var isExTapChangeFade = ApplicationSettings.Default.IsExTapChangeFade;
                var isTap2ChangeFade = ApplicationSettings.Default.IsTap2ChangeFade;
                var isExTap2ChangeFade = ApplicationSettings.Default.IsExTap2ChangeFade;
                var isFlickChangeFade = ApplicationSettings.Default.IsFlickChangeFade;
                var isDamageChangeFade = ApplicationSettings.Default.IsDamageChangeFade;


                switch (guideDefType)
                {
                    case 0:
                    default:
                        fade = "none";
                        break;
                    case 1:
                        fade = "out";
                        break;
                    case 2:
                        fade = "in";
                        break;
                }


                foreach (var note2 in notes.Taps) {
                    
                        if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex) {
                        if (isTapChangeFade && !note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "in";
                                    break;
                                case 1:
                                    fade = "in";
                                    break;
                                case 2:
                                    fade = "out";
                                    break;
                            }
                        }
                        else if(isTap2ChangeFade && note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "in";
                                    break;
                                case 1:
                                    fade = "in";
                                    break;
                                case 2:
                                    fade = "out";
                                    break;
                            }
                        }
                            
                        }
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex)
                    {
                        if (isTapChangeFade && !note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "out";
                                    break;
                                case 1:
                                    fade = "none";
                                    break;
                                case 2:
                                    fade = "none";
                                    break;
                            }
                        }
                        else if (isTap2ChangeFade && note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "out";
                                    break;
                                case 1:
                                    fade = "none";
                                    break;
                                case 2:
                                    fade = "none";
                                    break;
                            }
                        }
                    }
                    
                }

                foreach (var note2 in notes.ExTaps)
                {

                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex)
                    {
                        if (isExTapChangeFade && !note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "in";
                                    break;
                                case 1:
                                    fade = "in";
                                    break;
                                case 2:
                                    fade = "out";
                                    break;
                            }
                        }
                        else if (isExTap2ChangeFade && note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "in";
                                    break;
                                case 1:
                                    fade = "in";
                                    break;
                                case 2:
                                    fade = "out";
                                    break;
                            }
                        }

                    }
                    if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex)
                    {
                        if (isExTapChangeFade && !note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "out";
                                    break;
                                case 1:
                                    fade = "none";
                                    break;
                                case 2:
                                    fade = "none";
                                    break;
                            }
                        }
                        else if (isExTap2ChangeFade && note2.IsStart)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "out";
                                    break;
                                case 1:
                                    fade = "none";
                                    break;
                                case 2:
                                    fade = "none";
                                    break;
                            }
                        }
                    }

                }


                foreach (var note2 in notes.Flicks)
                {
                    if (isFlickChangeFade == true)
                    {
                        if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "in";
                                    break;
                                case 1:
                                    fade = "in";
                                    break;
                                case 2:
                                    fade = "out";
                                    break;
                            }
                        }
                        if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "out";
                                    break;
                                case 1:
                                    fade = "none";
                                    break;
                                case 2:
                                    fade = "none";
                                    break;
                            }

                        }
                    }
                }

                foreach (var note2 in notes.Damages)
                {
                    if (isDamageChangeFade == true)
                    {
                        if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "in";
                                    break;
                                case 1:
                                    fade = "in";
                                    break;
                                case 2:
                                    fade = "out";
                                    break;
                            }
                        }
                        if (endNote.Tick == note2.Tick && endNote.LaneIndex == note2.LaneIndex)
                        {
                            switch (guideDefType)
                            {
                                case 0:
                                    fade = "out";
                                    break;
                                case 1:
                                    fade = "none";
                                    break;
                                case 2:
                                    fade = "none";
                                    break;
                            }

                        }
                    }
                }





                foreach (var note2 in notes.Airs)
                {
                    if (note.StartNote.Tick == note2.Tick && note.StartNote.LaneIndex == note2.LaneIndex && note2.VerticalDirection == VerticalAirDirection.Down)
                    {
                        switch (note2.HorizontalDirection)
                        {
                            case HorizontalAirDirection.Center:
                                startEase = "in";
                                break;
                            case HorizontalAirDirection.Left:
                            case HorizontalAirDirection.Right:
                                startEase = "out";
                                break;
                        }
                    }
                }

               
                /*
                switch (note.GuideColor)
                {
                    case Guide.USCGuideColor.neutral: color = "neutral"; break;
                    case Guide.USCGuideColor.red: color = "red"; break;
                    case Guide.USCGuideColor.green: color = "green"; break;
                    case Guide.USCGuideColor.blue: color = "blue"; break;
                    case Guide.USCGuideColor.yellow: color = "yellow"; break;
                    case Guide.USCGuideColor.purple: color = "purple"; break;
                    case Guide.USCGuideColor.cyan: color = "cyan"; break;
                }
                */

                color = note.GuideColor.ToString();

                var startlaneIndex = note.StartNote.LaneIndex - 8 + (float)book.LaneOffset + note.StartNote.Width / 2;

                List<USCGuideMidpointNote> midpointnotes = new List<USCGuideMidpointNote>();

                var startpoint = new USCGuideMidpointNote((double)note.StartTick / 480, note.Channel, startlaneIndex, note.StartWidth / 2, startEase);
                midpointnotes.Add(startpoint);

                foreach(var step in note.StepNotes.OrderBy(p => p.Tick))
                {
                    string stepEase = "linear";

                    foreach (var note2 in notes.Airs)
                    {
                        if (step.Tick == note2.Tick && step.LaneIndex == note2.LaneIndex && note2.VerticalDirection == VerticalAirDirection.Down)
                        {
                            switch (note2.HorizontalDirection)
                            {
                                case HorizontalAirDirection.Center:
                                    stepEase = "in";
                                    break;
                                case HorizontalAirDirection.Left:
                                case HorizontalAirDirection.Right:
                                    stepEase = "out";
                                    break;
                            }
                        };
                    }
                    var steplaneIndex = step.LaneIndex - 8 + (float)book.LaneOffset + step.Width / 2;

                    var midpoint = new USCGuideMidpointNote((double)step.Tick / 480, step.Channel, steplaneIndex, step.Width / 2, stepEase);
                    midpointnotes.Add(midpoint);
                }

                

                var guidenote = new USCGuideNote(color, fade, midpointnotes.ToArray());
                usc.objects.Add(guidenote);

            }









            string data = JsonConvert.SerializeObject(this, SerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (var jsonstream = new MemoryStream(bytes))
            {
                {
                    jsonstream.CopyTo(stream);
                }
            }

        }

    }
}
