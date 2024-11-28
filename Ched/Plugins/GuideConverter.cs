using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;
using Ched.Localization;

namespace Ched.Plugins
{
    public class GuideConverter : IScorePlugin
    {
        public string DisplayName => PluginStrings.GuideConverter;

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

            foreach (var guide in targets)
            {
                StepList.AddRange(guide.StepNotes);
            }
            var airedStepDic = score.Notes.Airs
                .Where(p => StepList.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);
            if (targets.Count == 0) return;
            var results = targets.Select(p =>
            {
                var ordered = p.StepNotes.OrderByDescending(q => q.TickOffset).ToList();
                var res = new Slide() { StartTick = p.StartTick};
                res.SetPosition(p.StartLaneIndex, p.StartWidth);
                var trailing = new Slide.StepTap(res) { IsVisible = true, TickOffset = ordered[0].TickOffset };
                trailing.SetPosition(ordered[0].LaneIndexOffset, ordered[0].WidthChange );
                var steps = ordered.Skip(1).Select(q =>
                {
                    var step = new Slide.StepTap(res) { IsVisible = q.IsVisible, TickOffset = q.TickOffset};
                    step.SetPosition(q.LaneIndexOffset , q.WidthChange);
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

            foreach (var guide in targets) score.Notes.Guides.Remove(guide);
            score.Notes.Slides.AddRange(results);
            args.UpdateScore(score);
        }
    }
}
