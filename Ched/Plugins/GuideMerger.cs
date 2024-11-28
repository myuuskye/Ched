using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;
using Ched.Localization;

namespace Ched.Plugins
{
    public class GuideMerger : IScorePlugin
    {
        public string DisplayName => PluginStrings.GuideMerger;

        public void Run(IScorePluginArgs args)
        {
            var score = args.GetCurrentScore();
            var range = args.GetSelectedRange();
            bool modified = false;
            int startTick = range.Duration < 0 ? range.StartTick + range.Duration : range.StartTick;
            int endTick = range.Duration < 0 ? range.StartTick : range.StartTick + range.Duration;
            List<Guide.StepTap> StepList = new List<Guide.StepTap>();

            foreach(var slide in score.Notes.Guides)
            {
                StepList.AddRange(slide.StepNotes);
            }

            var endStepDic = score.Notes.Guides.ToDictionary(p => p, p => p.StepNotes.OrderByDescending(q => q.TickOffset).First());
            var airStepDic = score.Notes.Airs
                .Where(p => endStepDic.Values.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);
            var airStepDic2 = score.Notes.Airs
                .Where(p => StepList.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);
            var airActionStepDic = score.Notes.AirActions
                .Where(p => endStepDic.Values.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);
            var startDic = score.Notes.Guides.Where(p => p.StartTick >= startTick && p.StartTick <= endTick)
                .Select(p => new { Position = Tuple.Create(p.StartTick, p.StartLaneIndex, p.StartWidth), Note = p })
                .GroupBy(p => p.Position)
                .ToDictionary(p => p.Key, p => p.Select(q => q.Note).ToList());
            var endDic = score.Notes.Guides.Where(p => endStepDic[p].Tick >= startTick && endStepDic[p].Tick <= endTick)
                .Select(p => new { Position = Tuple.Create(endStepDic[p].Tick, endStepDic[p].LaneIndex, endStepDic[p].Width), Note = p })
                .GroupBy(p => p.Position)
                .ToDictionary(p => p.Key, p => p.Select(q => q.Note).ToList());

            while (endDic.Count > 0)
            {
                var pos = endDic.First().Key;
                if (endDic[pos].Count == 0 || !startDic.ContainsKey(pos) || startDic[pos].Count == 0)
                {
                    endDic.Remove(pos);
                    continue;
                }

                if (startDic[pos].Count > 0)
                {
                    // 終点AIR付きスライドはheadingの対象外
                    while (endDic[pos].Count > 0 && (airStepDic.ContainsKey(endStepDic[endDic[pos][0]]) || airActionStepDic.ContainsKey(endStepDic[endDic[pos][0]])))
                    {
                        endDic[pos].RemoveAt(0);
                    }
                    if (endDic[pos].Count == 0)
                    {
                        endDic.Remove(pos);
                        continue;
                    }

                    var heading = endDic[pos][0];
                    var trailing = startDic[pos][0];
                    var trailingOldSteps = trailing.StepNotes.OrderBy(p => p.TickOffset).ToList();
                    heading.StepNotes.AddRange(trailingOldSteps.Select(p =>
                    {
                        var step = new Guide.StepTap(heading) { TickOffset = p.Tick - heading.StartTick, IsVisible = p.IsVisible };
                        step.SetPosition(p.LaneIndex - heading.StartLaneIndex, p.Width - heading.StartWidth);
                        if (airStepDic2.ContainsKey(p))
                        {
                            Console.WriteLine(p.Tick + " " + p.LaneIndex + " " + p.Width);
                            score.Notes.Airs.Add(new Air(step) { HorizontalDirection = airStepDic2[p].HorizontalDirection, VerticalDirection = airStepDic2[p].VerticalDirection });
                            score.Notes.Airs.Remove(airStepDic2[p]);
                            
                        }

                        return step;
                    }));

                    // trailingにAIRが追加されていれば反映
                    var trailingOldEndStep = trailingOldSteps[trailingOldSteps.Count - 1];
                    var trailingNewEndStep = heading.StepNotes[heading.StepNotes.Count - 1];
                    if (airStepDic.ContainsKey(trailingOldEndStep))
                    {
                        var air = new Air(trailingNewEndStep)
                        {
                            VerticalDirection = airStepDic[trailingOldEndStep].VerticalDirection,
                            HorizontalDirection = airStepDic[trailingOldEndStep].HorizontalDirection
                        };
                        score.Notes.Airs.Add(air);
                        score.Notes.Airs.Remove(airStepDic[trailingOldEndStep]);
                        airStepDic.Add(trailingNewEndStep, air);
                        airStepDic.Remove(trailingOldEndStep);
                    }
                    if (airActionStepDic.ContainsKey(trailingOldEndStep))
                    {
                        var airAction = new AirAction(trailingNewEndStep);
                        airAction.ActionNotes.AddRange(airActionStepDic[trailingOldEndStep].ActionNotes.Select(p => new AirAction.ActionNote(airAction) { Offset = p.Offset }));
                        score.Notes.AirActions.Add(airAction);
                        score.Notes.AirActions.Remove(airActionStepDic[trailingOldEndStep]);
                        airActionStepDic.Add(trailingNewEndStep, airAction);
                        airActionStepDic.Remove(trailingOldEndStep);
                    }

                    

                    endStepDic[heading] = trailingNewEndStep;
                    score.Notes.Guides.Remove(trailing);
                    startDic[pos].Remove(trailing);
                    var trailingEndPos = Tuple.Create(endStepDic[trailing].Tick, endStepDic[trailing].LaneIndex, endStepDic[trailing].Width);
                    if (endDic.ContainsKey(trailingEndPos)) endDic[trailingEndPos].Remove(trailing);
                    endDic[pos].Remove(heading);
                    if (!endDic.ContainsKey(trailingEndPos)) endDic.Add(trailingEndPos, new[] { heading }.ToList());
                    else endDic[trailingEndPos].Add(heading);
                    modified = true;
                }
            }
            if (modified) args.UpdateScore(score);
        }
    }
}
