using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Ched.Components.Exporter;
using Ched.UI.Windows;
using Ched.Core;
using Ched.UI;
using System.IO;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Ched.Core.Events;
using Ched.Core.Notes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Runtime.Remoting.Channels;
using Ched.Configuration;
using static Ched.Core.Notes.Guide;
using static Ched.Core.Notes.Slide;

namespace Ched.Plugins
{
    public class UscImportPlugin : IScoreBookImportPlugin
    {
        public string DisplayName => "Universal Sekai Chart (*.usc)";

        public string FileFilter => "Universal Sekai Chart (*.usc)|*.usc";


        ScoreBook IScoreBookImportPlugin.Import(IScoreBookImportPluginArgs args)
        {
            //Console.WriteLine(args.Path);
            ScoreBook result = new ScoreBook();

            string data = null;

            using (var file = new FileStream(args.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                data = Encoding.UTF8.GetString(stream.ToArray());
            }
            var doc = JObject.Parse(data);
            var objects = doc["usc"]["objects"];


            var events = result.Score.Events;

            int count = 0;
            int timescaleforcount = 0;

            result.Offset = doc["usc"].Value<double>("offset");

            foreach (var obj in objects)
            {
                count++;
                //Console.WriteLine(obj.Value<string>("type"));
                switch (obj.Value<string>("type"))
                {
                    case "bpm":
                        var bpmevent = new BpmChangeEvent() { Tick = (int)(obj.Value<double>("beat") * 480), Bpm = obj.Value<double>("bpm") };
                        events.BpmChangeEvents.Add(bpmevent);
                        break;
                    case "timeScaleGroup":
                        var timescaleevents = obj.Value<JArray>("changes");
                        foreach (var timescaleevent in timescaleevents)
                        {
                            var highspeedevent = new HighSpeedChangeEvent() { Tick = (int)(timescaleevent.Value<double>("beat") * 480), SpeedRatio = timescaleevent.Value<decimal>("timeScale"), SpeedCh = timescaleforcount };
                            events.HighSpeedChangeEvents.Add(highspeedevent);
                        }
                        timescaleforcount++;
                        break;
                    case "single":
                        var tapnote = new Tap();
                        var exnote = new ExTap();
                        var tracenote = new Flick();
                        bool isTrace = obj.Value<bool>("trace");
                        bool isCritical = obj.Value<bool>("critical");

                        if (isCritical)
                        {
                            exnote = new ExTap() { Tick = (int)(obj.Value<double>("beat") * 480), Channel = obj.Value<int>("timeScaleGroup"), LaneIndex = obj.Value<float>("lane") + 8 - obj.Value<float>("size"), Width = obj.Value<float>("size") * 2 };
                            result.Score.Notes.ExTaps.Add(exnote);
                        }
                        else
                        {
                            if (!isTrace)
                            {
                                tapnote = new Tap() { Tick = (int)(obj.Value<double>("beat") * 480), Channel = obj.Value<int>("timeScaleGroup"), LaneIndex = obj.Value<float>("lane") + 8 - obj.Value<float>("size"), Width = obj.Value<float>("size") * 2 };
                                result.Score.Notes.Taps.Add(tapnote);
                            }
                        }
                        if (isTrace)
                        {
                            tracenote = new Flick() { Tick = (int)(obj.Value<double>("beat") * 480), Channel = obj.Value<int>("timeScaleGroup"), LaneIndex = obj.Value<float>("lane") + 8 - obj.Value<float>("size"), Width = obj.Value<float>("size") * 2 };
                            result.Score.Notes.Flicks.Add(tracenote);
                        }
                        switch (obj.Value<string>("direction"))
                        {
                            case "left":
                                if (isTrace)
                                {
                                    result.Score.Notes.Airs.Add(new Air(tracenote) { HorizontalDirection = HorizontalAirDirection.Left, VerticalDirection = VerticalAirDirection.Up });
                                }
                                else
                                {
                                    if (obj.Value<bool>("critical"))
                                    {
                                        result.Score.Notes.Airs.Add(new Air(exnote) { HorizontalDirection = HorizontalAirDirection.Left, VerticalDirection = VerticalAirDirection.Up });
                                    }
                                    else
                                    {
                                        result.Score.Notes.Airs.Add(new Air(tapnote) { HorizontalDirection = HorizontalAirDirection.Left, VerticalDirection = VerticalAirDirection.Up });
                                    }
                                }
                                

                                break;
                            case "up":
                                if (isTrace)
                                {
                                    result.Score.Notes.Airs.Add(new Air(tracenote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Up });
                                }
                                else
                                {
                                    if (obj.Value<bool>("critical"))
                                    {
                                        result.Score.Notes.Airs.Add(new Air(exnote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Up });
                                    }
                                    else
                                    {
                                        result.Score.Notes.Airs.Add(new Air(tapnote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Up });
                                    }
                                }
                                
                                break;
                            case "right":
                                if (isTrace)
                                {
                                    result.Score.Notes.Airs.Add(new Air(tracenote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Up });
                                }
                                else
                                {
                                    if (obj.Value<bool>("critical"))
                                    {
                                        result.Score.Notes.Airs.Add(new Air(exnote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Up });
                                    }
                                    else
                                    {
                                        result.Score.Notes.Airs.Add(new Air(tapnote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Up });
                                    }
                                }
                                
                                break;
                            case "none":
                                if (isTrace)
                                {
                                    result.Score.Notes.Airs.Add(new Air(tracenote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });
                                }
                                else
                                {
                                    if (obj.Value<bool>("critical"))
                                    {
                                        result.Score.Notes.Airs.Add(new Air(exnote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });
                                    }
                                    else
                                    {
                                        result.Score.Notes.Airs.Add(new Air(tapnote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });
                                    }
                                }
                                
                                break;
                            default:
                                break;
                        }


                        break;
                    case "damage":
                        var damagenote = new Damage();
                        damagenote = new Damage() { Tick = (int)(obj.Value<double>("beat") * 480), Channel = obj.Value<int>("timeScaleGroup"), LaneIndex = obj.Value<float>("lane") + 8 - obj.Value<float>("size"), Width = obj.Value<float>("size") * 2 };
                        result.Score.Notes.Damages.Add(damagenote);
                        break;
                    case "slide":
                        bool isSlideCritical = obj.Value<bool>("critical");
                        var slideconnections = obj.Value<JArray>("connections");

                        var start = obj.Value<JArray>("connections").Where(p => p.Value<string>("type") == "start").First();

                        Slide slide = new Slide()
                        {
                            StartTick = (int)(start.Value<double>("beat") * 480),
                            StartLaneIndex = start.Value<float>("lane") + 8 - start.Value<float>("size"),
                            StartWidth = start.Value<float>("size") * 2,
                            Channel = start.Value<int>("timeScaleGroup"),
                        };
                        foreach (var slideconnection in slideconnections)
                        {
                            //Console.WriteLine(slideconnection);
                            switch (slideconnection.Value<string>("type"))
                            {
                                case "start":
                                    var exNote = new ExTap() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                    if (slideconnection.Value<bool>("critical")) result.Score.Notes.ExTaps.Add(exNote);
                                    
                                    switch (slideconnection.Value<string>("judgeType"))
                                    {
                                        case "trace":
                                            var judgeTrace = new Flick() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                            result.Score.Notes.Flicks.Add(judgeTrace);
                                            switch (slideconnection.Value<string>("ease"))
                                            {
                                                case "out":
                                                    result.Score.Notes.Airs.Add(new Air(judgeTrace) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Down });

                                                    break;
                                                case "in":
                                                    result.Score.Notes.Airs.Add(new Air(judgeTrace) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });

                                                    break;
                                            }

                                            break;
                                        case "none":
                                            var judgeDamage = new Damage() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                            result.Score.Notes.Damages.Add(judgeDamage);
                                            switch (slideconnection.Value<string>("ease"))
                                            {
                                                case "out":
                                                    result.Score.Notes.Airs.Add(new Air(judgeDamage) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Down });

                                                    break;
                                                case "in":
                                                    result.Score.Notes.Airs.Add(new Air(judgeDamage) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });

                                                    break;
                                            }

                                            break;
                                        case "normal":
                                            switch (slideconnection.Value<string>("ease"))
                                            {
                                                case "out":
                                                    var airedNote = new Tap() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                                    if (isSlideCritical)
                                                    {
                                                        result.Score.Notes.Airs.Add(new Air(exNote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Down });
                                                    }
                                                    else
                                                    {
                                                        result.Score.Notes.Taps.Add(airedNote);
                                                        result.Score.Notes.Airs.Add(new Air(airedNote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Down });
                                                    }
                                                    

                                                    break;
                                                case "in":
                                                    var airedNote2 = new Tap() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                                    if (isSlideCritical)
                                                    {
                                                        result.Score.Notes.Airs.Add(new Air(exNote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });
                                                    }
                                                    else
                                                    {
                                                        result.Score.Notes.Taps.Add(airedNote2);
                                                        result.Score.Notes.Airs.Add(new Air(airedNote2) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });
                                                    }
                                                    

                                                    break;
                                            }
                                            break;
                                    }
                                    break;


                                case "tick":

                                    Slide.StepTap stepTap = new Slide.StepTap(slide)
                                    {
                                        IsVisible = ( slideconnection.Value<string>("critical") == "True") || (slideconnection.Value<string>("critical") == "False"),
                                        TickOffset = (int)(slideconnection.Value<double>("beat") * 480) - slide.StartTick,
                                        Channel = slideconnection.Value<int>("timeScaleGroup")
                                    };
                                    stepTap.SetPosition(slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size") - slide.StartLaneIndex, slideconnection.Value<float>("size") * 2 - slide.StartWidth);
                                    
                                    slide.StepNotes.Add(stepTap);
                                    var stepairedNote= new Tap() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                    
                                    switch (slideconnection.Value<string>("ease"))
                                    {
                                        case "out":
                                            result.Score.Notes.Taps.Add(stepairedNote);
                                            result.Score.Notes.Airs.Add(new Air(stepairedNote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Down });

                                            break;
                                        case "in":
                                            result.Score.Notes.Taps.Add(stepairedNote);
                                            result.Score.Notes.Airs.Add(new Air(stepairedNote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });

                                            break;
                                    }

                                    break;
                                case "attach":

                                    Slide.StepTap attachStep = new Slide.StepTap(slide)
                                    {
                                        IsVisible = true,
                                        TickOffset = (int)(slideconnection.Value<double>("beat") * 480) - slide.StartTick,
                                        Channel = slideconnection.Value<int>("timeScaleGroup")
                                    };
                                    attachStep.SetPosition(slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size") - slide.StartLaneIndex, slideconnection.Value<float>("size") * 2 - slide.StartWidth);
                                    slide.StepNotes.Add(attachStep);
                                    var attachFlick= new Flick() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                    result.Score.Notes.Flicks.Add(attachFlick);

                                    break;
                                case "end":
                                    Slide.StepTap endTap = new Slide.StepTap(slide)
                                    {
                                        IsVisible = true,
                                        TickOffset = (int)(slideconnection.Value<double>("beat") * 480) - slide.StartTick,
                                        Channel = slideconnection.Value<int>("timeScaleGroup")
                                    };
                                    endTap.SetPosition(slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size") - slide.StartLaneIndex, slideconnection.Value<float>("size") * 2 - slide.StartWidth);

                                    slide.StepNotes.Add(endTap);
                                    
                                    switch (slideconnection.Value<string>("judgeType"))
                                    {
                                        case "trace":
                                            var judgeTrace = new Flick() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                            result.Score.Notes.Flicks.Add(judgeTrace);
                                            break;
                                        case "none":
                                            var judgeDamage = new Damage() { Channel = slideconnection.Value<int>("timeScaleGroup"), Tick = (int)(slideconnection.Value<double>("beat") * 480), LaneIndex = slideconnection.Value<float>("lane") + 8 - slideconnection.Value<float>("size"), Width = slideconnection.Value<float>("size") * 2 };
                                            result.Score.Notes.Damages.Add(judgeDamage);
                                            break;
                                    }
                                    switch (slideconnection.Value<string>("direction"))
                                    {
                                        case "left":
                                            result.Score.Notes.Airs.Add(new Air(endTap) { VerticalDirection = VerticalAirDirection.Up, HorizontalDirection = HorizontalAirDirection.Left });
                                            break;
                                        case "up":
                                            result.Score.Notes.Airs.Add(new Air(endTap) { VerticalDirection = VerticalAirDirection.Up, HorizontalDirection = HorizontalAirDirection.Center });
                                            break;
                                        case "right":
                                            result.Score.Notes.Airs.Add(new Air(endTap) { VerticalDirection = VerticalAirDirection.Up, HorizontalDirection = HorizontalAirDirection.Right });
                                            break;
                                    }

                                    break;
                            }
                        }
                        

                        result.Score.Notes.Slides.Add(slide);

                        break;
                    case "guide":
                        string guideColor = obj.Value<string>("color");
                        string guideFade = obj.Value<string>("fade");

                        int guideDefType = ApplicationSettings.Default.GuideDefaultFade;

                        var guidemidpoints = obj.Value<JArray>("midpoints");

                        var guidestart = obj.Value<JArray>("midpoints").OrderBy(p => p.Value<double>("beat")).First();
                        var guidelast = obj.Value<JArray>("midpoints").OrderBy(p => p.Value<double>("beat")).Last();

                        Guide guide = new Guide()
                        {
                            StartTick = (int)(guidestart.Value<double>("beat") * 480),
                            StartLaneIndex = guidestart.Value<float>("lane") + 8 - guidestart.Value<float>("size"),
                            StartWidth = guidestart.Value<float>("size") * 2,
                            Channel = guidestart.Value<int>("timeScaleGroup"),
                            GuideColor = (USCGuideColor)Enum.Parse(typeof(USCGuideColor), guideColor, true)
                        };

                        

                        foreach (var gm in guidemidpoints.OrderBy(p => p.Value<double>("beat")))
                        {
                            Guide.StepTap guideStepTap = new Guide.StepTap(guide)
                            {
                                IsVisible = false,
                                TickOffset = (int)(gm.Value<double>("beat") * 480) - guide.StartTick,
                                Channel = gm.Value<int>("timeScaleGroup")
                            };
                            guideStepTap.SetPosition(gm.Value<float>("lane") + 8 - gm.Value<float>("size") - guide.StartLaneIndex, gm.Value<float>("size") * 2 - guide.StartWidth);

                            if (guideStepTap.Tick == guideStepTap.ParentNote.StartTick && guideStepTap.LaneIndex == guideStepTap.ParentNote.StartLaneIndex && guideStepTap.Width == guideStepTap.ParentNote.StartWidth)
                            {
                                var airedNote = new Tap() { Channel = gm.Value<int>("timeScaleGroup"), Tick = (int)(gm.Value<double>("beat") * 480), LaneIndex = gm.Value<float>("lane") + 8 - gm.Value<float>("size"), Width = gm.Value<float>("size") * 2, IsStart = true };
                                bool isexist = false;
                                switch (gm.Value<string>("ease"))
                                {
                                    case "out":
                                        foreach(var tap in result.Score.Notes.Taps.Concat(result.Score.Notes.ExTaps).Where(p => p.Tick == guideStepTap.Tick && p.LaneIndex == guideStepTap.LaneIndex && p.Width == guideStepTap.Width))
                                        {
                                            airedNote = tap; 
                                            isexist = true;
                                            break;
                                        }
                                        if (!isexist) result.Score.Notes.Taps.Add(airedNote);
                                        result.Score.Notes.Airs.Add(new Air(airedNote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Down });

                                        break;
                                    case "in":
                                        foreach (var tap in result.Score.Notes.Taps.Concat(result.Score.Notes.ExTaps).Where(p => p.Tick == guideStepTap.Tick && p.LaneIndex == guideStepTap.LaneIndex && p.Width == guideStepTap.Width))
                                        {
                                            airedNote = tap;
                                            isexist = true;
                                            break;
                                        }
                                        if (!isexist) result.Score.Notes.Taps.Add(airedNote);
                                        result.Score.Notes.Airs.Add(new Air(airedNote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });
                                        break;
                                }
                                continue;
                            }

                            guide.StepNotes.Add(guideStepTap);
                            var stepairedNote = new Tap() { Channel = gm.Value<int>("timeScaleGroup"), Tick = (int)(gm.Value<double>("beat") * 480), LaneIndex = gm.Value<float>("lane") + 8 - gm.Value<float>("size"), Width = gm.Value<float>("size") * 2, IsStart = true };

                            switch (gm.Value<string>("ease"))
                            {
                                case "out":
                                    result.Score.Notes.Taps.Add(stepairedNote);
                                    result.Score.Notes.Airs.Add(new Air(stepairedNote) { HorizontalDirection = HorizontalAirDirection.Right, VerticalDirection = VerticalAirDirection.Down });

                                    break;
                                case "in":
                                    result.Score.Notes.Taps.Add(stepairedNote);
                                    result.Score.Notes.Airs.Add(new Air(stepairedNote) { HorizontalDirection = HorizontalAirDirection.Center, VerticalDirection = VerticalAirDirection.Down });

                                    break;
                            }

                        }

                        

                        guide.StepNotes.OrderBy(p => p.Tick).Last().IsVisible = true;

                        var fadeOutDamage = new Damage()
                        {
                            Tick = (int)(guidelast.Value<double>("beat") * 480),
                            LaneIndex = guidelast.Value<float>("lane") + 8 - guidelast.Value<float>("size"),
                            Width = guidelast.Value<float>("size") * 2,
                            Channel = guidelast.Value<int>("timeScaleGroup"),
                        };
                        var fadeInDamage = new Damage()
                        {
                            Tick = (int)(guidestart.Value<double>("beat") * 480),
                            LaneIndex = guidestart.Value<float>("lane") + 8 - guidestart.Value<float>("size"),
                            Width = guidestart.Value<float>("size") * 2,
                            Channel = guidestart.Value<int>("timeScaleGroup"),
                        };

                        switch (guideFade)
                        {
                            case "out":
                                switch (guideDefType)
                                {
                                    case 0:
                                        result.Score.Notes.Damages.Add(fadeOutDamage);
                                        break;
                                    case 1:
                                        break;
                                    case 2:
                                        result.Score.Notes.Damages.Add(fadeOutDamage);
                                        break;
                                }
                                
                                break;
                            case "in":
                                switch (guideDefType)
                                {
                                    case 0:
                                        result.Score.Notes.Damages.Add(fadeInDamage);
                                        break;
                                    case 1:
                                        result.Score.Notes.Damages.Add(fadeInDamage);
                                        break;
                                    case 2:
                                        break;
                                }
                                
                                break;
                            case "none":
                                switch (guideDefType)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                        result.Score.Notes.Damages.Add(fadeOutDamage);
                                        break;
                                    case 2:
                                        result.Score.Notes.Damages.Add(fadeOutDamage);
                                        break;
                                }
                                break;

                        }

                        result.Score.Notes.Guides.Add(guide);


                        break;
            
            }
        }
            

            
            events.TimeSignatureChangeEvents.Add(new TimeSignatureChangeEvent() { Tick = 0, Numerator = 4, DenominatorExponent = 2 });

            return result;
        }
    }
}
