﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ched.Core.Notes;
using Ched.Localization;

namespace Ched.Plugins
{
    public class GuideKnitter : IScorePlugin
    {
        public string DisplayName => PluginStrings.GuideKnitter;

        public void Run(IScorePluginArgs args)
        {
            var score = args.GetCurrentScore();
            var range = args.GetSelectedRange();
            var guides = score.Notes.Guides
                .Where(p => p.StartTick <= range.StartTick && p.StepNotes.OrderByDescending(q => q.Tick).First().Tick >= range.StartTick);
            

            foreach (var slide in guides.Where(p => p.StepNotes.Count == 3))
            {
                var steps = slide.StepNotes.OrderBy(p => p.TickOffset).ToList();
                // 始点と2つ目の中継点間の時間が等しいものが対象
                int initInterval = steps[0].TickOffset;
                if (initInterval * 2 != steps[1].TickOffset) continue;
                
                
                bool stepVisible = steps[0].IsVisible;
                int duration = steps[steps.Count - 1].TickOffset;
                int stepsCount = duration / initInterval;

                var air1 = score.Notes.Airs.Where(p => p.ParentNote == steps[0]).FirstOrDefault();
                var air2 = score.Notes.Airs.Where(p => p.ParentNote == steps[1]).FirstOrDefault();

                for (int i = 0; i < stepsCount - 2; i++)
                {
                    int pos = initInterval * (i + 3);
                    if (pos >= duration) break;
                    var step = new Core.Notes.Guide.StepTap(slide)
                    {
                        TickOffset = pos,
                        IsVisible = stepVisible
                    };
                    step.SetPosition(steps[i % 2].LaneIndexOffset, steps[i % 2].WidthChange);
                    step.IsVisible = steps[i % 2].IsVisible;
                    if(i % 2 == 0)
                    {
                        if (air1 != null)
                        {

                            score.Notes.Airs.Add(new Air(step) { HorizontalDirection = air1.HorizontalDirection, VerticalDirection = air1.VerticalDirection });
                        }
                    }
                    else
                    {
                        if (air2 != null)
                        {
                            score.Notes.Airs.Add(new Air(step) { HorizontalDirection = air2.HorizontalDirection, VerticalDirection = air2.VerticalDirection });
                        }
                    }
                    slide.StepNotes.Add(step);

                }
            }

            args.UpdateScore(score);
        }
    }
}
