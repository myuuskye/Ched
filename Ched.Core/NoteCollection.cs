﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;

namespace Ched.Core
{
    /// <summary>
    /// ノーツを格納するコレクションを表すクラスです。
    /// </summary>
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
    public class NoteCollection
    {
        [Newtonsoft.Json.JsonProperty]
        private List<Tap> taps;
        [Newtonsoft.Json.JsonProperty]
        private List<ExTap> exTaps;
        [Newtonsoft.Json.JsonProperty]
        private List<Hold> holds;
        [Newtonsoft.Json.JsonProperty]
        private List<Slide> slides;
        [Newtonsoft.Json.JsonProperty]
        private List<Guide> guides;
        [Newtonsoft.Json.JsonProperty]
        private List<Marker> markers;
        [Newtonsoft.Json.JsonProperty]
        private List<Flick> flicks;
        [Newtonsoft.Json.JsonProperty]
        private List<Damage> damages;
        [Newtonsoft.Json.JsonProperty]
        private List<Air> airs;
        [Newtonsoft.Json.JsonProperty]
        private List<AirAction> airActions;
        [Newtonsoft.Json.JsonProperty]
        private List<StepNoteTap> stepNoteTaps;
        [Newtonsoft.Json.JsonProperty]
        private List<Slide.StepTap> slidesteps;
        [Newtonsoft.Json.JsonProperty]
        private List<Guide.StepTap> guidesteps;


        public List<Tap> Taps
        {
            get { return taps; }
            set { taps = value; }
        }

        public List<ExTap> ExTaps
        {
            get { return exTaps; }
            set { exTaps = value; }
        }

        public List<Hold> Holds
        {
            get { return holds; }
            set { holds = value; }
        }

        public List<Marker> Markers
        {
            get { return markers; }
            set { markers = value; }
        }
        public List<Slide> Slides
        {
            get { return slides; }
            set { slides = value; }
        }

        public List<Air> Airs
        {
            get { return airs; }
            set { airs = value; }
        }

        public List<AirAction> AirActions
        {
            get { return airActions; }
            set { airActions = value; }
        }

        public List<Flick> Flicks
        {
            get { return flicks; }
            set { flicks = value; }
        }

        public List<Damage> Damages
        {
            get { return damages; }
            set { damages = value; }
        }

        public List<Guide> Guides
        {
            get { return guides; }
            set { guides = value; }
        }

        public List<StepNoteTap> StepNoteTaps
        {
            get { return stepNoteTaps; }
            set { stepNoteTaps = value; }
        }

        public List<Slide.StepTap> SlideSteps
        {
            get { return slidesteps; }
            set { slidesteps = value; }
        }
        public List<Guide.StepTap> GuideSteps
        {
            get { return guidesteps; }
            set { guidesteps = value; }
        }


        public NoteCollection()
        {
            Taps = new List<Tap>();
            ExTaps = new List<ExTap>();
            Holds = new List<Hold>();
            Markers = new List<Marker>();
            Slides = new List<Slide>();
            Guides = new List<Guide>();
            Airs = new List<Air>();
            AirActions = new List<AirAction>();
            Flicks = new List<Flick>();
            Damages = new List<Damage>();
            StepNoteTaps = new List<StepNoteTap>();
            SlideSteps = new List<Slide.StepTap>();
            GuideSteps = new List<Guide.StepTap>();
        }

        public NoteCollection(NoteCollection collection)
        {
            Taps = collection.Taps.ToList();
            ExTaps = collection.ExTaps.ToList();
            Holds = collection.Holds.ToList();
            Markers = collection.Markers.ToList();
            Slides = collection.Slides.ToList();
            Guides = collection.Guides.ToList();
            Airs = collection.Airs.ToList();
            AirActions = collection.AirActions.ToList();
            Flicks = collection.Flicks.ToList();
            Damages = collection.Damages.ToList();
            StepNoteTaps = collection.StepNoteTaps.ToList();
            SlideSteps = collection.SlideSteps.ToList();
            GuideSteps = collection.GuideSteps.ToList();
        }

        public IEnumerable<TappableBase> GetShortNotes()
        {
            return Taps.Cast<TappableBase>().Concat(ExTaps).Concat(Flicks).Concat(Damages).Concat(StepNoteTaps);
        }

        public void UpdateTicksPerBeat(double factor)
        {
            foreach (var note in GetShortNotes())
                note.Tick = (int)(note.Tick * factor);

            foreach (var hold in Holds)
            {
                hold.StartTick = (int)(hold.StartTick * factor);
                hold.Duration = (int)(hold.Duration * factor);
            }
            foreach (var marker in Markers)
            {
                marker.StartTick = (int)(marker.StartTick * factor);
                marker.Duration = (int)(marker.Duration * factor);
            }

            foreach (var slide in Slides)
            {
                slide.StartTick = (int)(slide.StartTick * factor);
                foreach (var step in slide.StepNotes)
                    step.TickOffset = (int)(step.TickOffset * factor);
            }

            foreach (var guide in Guides)
            {
                guide.StartTick = (int)(guide.StartTick * factor);
                foreach (var step in guide.StepNotes)
                    step.TickOffset = (int)(step.TickOffset * factor);
            }

            foreach (var action in AirActions.SelectMany(p => p.ActionNotes))
                action.Offset = (int)(action.Offset * factor);
        }
    }
}
