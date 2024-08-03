using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ched.Core.Notes;
using Ched.Localization;

namespace Ched.Plugins
{
    public class GuideSplitter : IScorePlugin
    {
        public string DisplayName => PluginStrings.GuideSplitter;

        public void Run(IScorePluginArgs args)
        {
            var score = args.GetCurrentScore();
            var range = args.GetSelectedRange();
            bool modified = false;
            var targets = score.Notes.Guides.Where(p => p.StartTick < range.StartTick && p.StepNotes.OrderByDescending(q => q.TickOffset).First().Tick > range.StartTick);
            var endStepDic = score.Notes.Guides.ToDictionary(p => p, p => p.StepNotes.OrderByDescending(q => q.TickOffset).First());
            var airStepDic = score.Notes.Airs
                .Where(p => endStepDic.Values.Contains(p.ParentNote))
                .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);
            var airActionStepDic = score.Notes.AirActions
               .Where(p => endStepDic.Values.Contains(p.ParentNote))
               .ToDictionary(p => p.ParentNote as Guide.StepTap, p => p);

            foreach (var guide in targets.ToList())
            {
                // カーソル位置に中継点が存在しなければ処理しない
                int offset = range.StartTick - guide.StartTick;
                if (guide.StepNotes.All(p => p.TickOffset != offset)) continue;

                var first = new Guide() { StartTick = guide.StartTick, GuideColor = guide.GuideColor };
                first.SetPosition(guide.StartLaneIndex, guide.StartWidth);
                first.StepNotes.AddRange(guide.StepNotes.OrderBy(p => p.TickOffset).TakeWhile(p => p.TickOffset <= offset).Select(p =>
                {
                    var step = new Guide.StepTap(first) { TickOffset = p.TickOffset, IsVisible = p.IsVisible };
                    step.SetPosition(p.LaneIndexOffset, p.WidthChange);
                    return step;
                }));
                first.StepNotes[first.StepNotes.Count - 1].IsVisible = true;

                var second = new Guide() { StartTick = range.StartTick, GuideColor = guide.GuideColor };
                var trailing = guide.StepNotes.OrderBy(p => p.TickOffset).SkipWhile(p => p.TickOffset < offset).ToList();
                second.SetPosition(trailing[0].LaneIndex, trailing[0].Width);
                second.StepNotes.AddRange(trailing.Skip(1).Select(p =>
                {
                    var step = new Guide.StepTap(second) { TickOffset = p.TickOffset - offset, IsVisible = p.IsVisible };
                    step.SetPosition(p.LaneIndex - second.StartLaneIndex, p.Width - second.StartWidth);
                    return step;
                }));

                // 終点AIRをsecondに挿入
                if (airStepDic.ContainsKey(endStepDic[guide]))
                {
                    var origAir = airStepDic[endStepDic[guide]];
                    var air = new Air(second.StepNotes[second.StepNotes.Count - 1])
                    {
                        VerticalDirection = origAir.VerticalDirection,
                        HorizontalDirection = origAir.HorizontalDirection
                    };
                    score.Notes.Airs.Remove(origAir);
                    score.Notes.Airs.Add(air);
                }
                if (airActionStepDic.ContainsKey(endStepDic[guide]))
                {
                    var origAirAction = airActionStepDic[endStepDic[guide]];
                    var airAction = new AirAction(second.StepNotes[second.StepNotes.Count - 1]);
                    airAction.ActionNotes.AddRange(origAirAction.ActionNotes.Select(p => new AirAction.ActionNote(airAction) { Offset = p.Offset }));
                    score.Notes.AirActions.Remove(origAirAction);
                    score.Notes.AirActions.Add(airAction);
                }

                score.Notes.Guides.Add(first);
                score.Notes.Guides.Add(second);
                score.Notes.Guides.Remove(guide);
                modified = true;
            }
            if (modified) args.UpdateScore(score);
        }
    }
}
