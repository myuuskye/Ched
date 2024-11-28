﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;
using Ched.Localization;

namespace Ched.Plugins
{
    public class GuideReverser : IScorePlugin
    {
        public string DisplayName => PluginStrings.GuideReverser;

        public void Run(IScorePluginArgs args)
        {
            var score = args.GetCurrentScore();
            var range = args.GetSelectedRange();
            int startTick = range.Duration < 0 ? range.StartTick + range.Duration : range.StartTick;
            int endTick = range.Duration < 0 ? range.StartTick : range.StartTick + range.Duration;
            var endStepDic = score.Notes.Guides.ToDictionary(p => p, p => p.StepNotes.OrderByDescending(q => q.TickOffset).First());
            var airStepDic = score.Notes.Airs
                .Where(p => endStepDic.Values.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);
            var airActionStepDic = score.Notes.AirActions
                .Where(p => endStepDic.Values.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);

            var targets = score.Notes.Guides
                .Where(p => p.StartTick >= startTick && p.StartTick + p.GetDuration() <= endTick)
                .Where(p => p.StartLaneIndex >= range.StartLaneIndex && p.StartLaneIndex + p.StartWidth <= range.StartLaneIndex + range.SelectedLanesCount)
                .Where(p => p.StepNotes.All(q => q.LaneIndex >= range.StartLaneIndex && q.LaneIndex + q.Width <= range.StartLaneIndex + range.SelectedLanesCount))
                .Where(p => !airStepDic.ContainsKey(endStepDic[p]) && !airActionStepDic.ContainsKey(endStepDic[p]))
                .ToList();
            List<Guide.StepTap> StepList = new List<Guide.StepTap>();

            foreach (var slide in targets)
            {
                StepList.AddRange(slide.StepNotes);
            }
            var airedStepDic = score.Notes.Airs
                .Where(p => StepList.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);
            if (targets.Count == 0) return;
            var results = targets.Select(p =>
            {
                var ordered = p.StepNotes.OrderByDescending(q => q.TickOffset).ToList();
                var res = new Guide() { StartTick = startTick + (endTick - ordered[0].Tick) };
                res.SetPosition(ordered[0].LaneIndex, ordered[0].Width);
                var trailing = new Guide.StepTap(res) { IsVisible = true, TickOffset = startTick + (endTick - p.StartTick) - res.StartTick };
                trailing.SetPosition(p.StartLaneIndex - res.StartLaneIndex, p.StartWidth - res.StartWidth);
                var steps = ordered.Skip(1).Select(q =>
                {
                    var step = new Guide.StepTap(res) { IsVisible = q.IsVisible, TickOffset = startTick + (endTick - q.Tick) - res.StartTick };
                    step.SetPosition(q.LaneIndex - res.StartLaneIndex, q.Width - res.StartWidth);
                    if (airedStepDic.ContainsKey(q))
                    {
                        score.Notes.Airs.Add(new Air(step) { HorizontalDirection = airedStepDic[q].HorizontalDirection, VerticalDirection = airedStepDic[q].VerticalDirection });
                        score.Notes.Airs.Remove(airedStepDic[q]);
                    }
                    return step;
                })
                .Concat(new[] { trailing });
                res.StepNotes.AddRange(steps);
                return res;
            });

            foreach (var slide in targets) score.Notes.Guides.Remove(slide);
            score.Notes.Guides.AddRange(results);
            args.UpdateScore(score);
        }
    }
}
