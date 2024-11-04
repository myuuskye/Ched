using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

using Ched.Core.Notes;
using Ched.UI;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using static Ched.Core.Notes.Guide;
using Ched.Core;
using System.Net;
using System.Reflection;
using System.Collections;

namespace Ched.Drawing
{

    public static class NoteGraphics
    {




        public static void DrawTap(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.TapColor, dc.ColorProfile.BorderColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.InvTapColor, dc.ColorProfile.InvBorderColor);
                        break;
                    case 2:
                        dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.TapColor, dc.ColorProfile.BorderColor);
                        break;

                }
                
            }
            
        }

        public static void DrawExTap(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.ExTapColor, dc.ColorProfile.BorderColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.InvExTapColor, dc.ColorProfile.InvBorderColor);
                        break;
                    case 2:
                        dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.ExTapColor, dc.ColorProfile.BorderColor);
                        break;
                }
            }
            
        }

        public static void DrawFlick(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                var foregroundRect = new RectangleF(rect.Left + rect.Width / 4, rect.Top, rect.Width / 2, rect.Height);
                dc.Graphics.DrawNoteBase(rect, dc.ColorProfile.FlickColor.Item1);
                dc.Graphics.DrawNoteBase(foregroundRect, dc.ColorProfile.FlickColor.Item2);
                dc.Graphics.DrawBorder(rect, dc.ColorProfile.BorderColor);
                dc.Graphics.DrawTapSymbol(foregroundRect, mode);
            }
            else
            {
                var foregroundRect = new RectangleF(rect.Left + rect.Width / 4, rect.Top, rect.Width / 2, rect.Height);
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        
                        dc.Graphics.DrawNoteBase(rect, dc.ColorProfile.InvFlickColor.Item1);
                        dc.Graphics.DrawNoteBase(foregroundRect, dc.ColorProfile.InvFlickColor.Item2);
                        dc.Graphics.DrawBorder(rect, dc.ColorProfile.InvBorderColor);
                        dc.Graphics.DrawTapSymbol(foregroundRect, mode);
                        break;
                    case 2:
                        dc.Graphics.DrawNoteBase(rect, dc.ColorProfile.FlickColor.Item1);
                        dc.Graphics.DrawNoteBase(foregroundRect, dc.ColorProfile.FlickColor.Item2);
                        dc.Graphics.DrawBorder(rect, dc.ColorProfile.BorderColor);
                        dc.Graphics.DrawTapSymbol(foregroundRect, mode);
                        break;
                }

            }
            
        }

        public static void DrawDamage(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                dc.Graphics.DrawSquarishNote(rect, dc.ColorProfile.DamageColor, dc.ColorProfile.BorderColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawSquarishNote(rect, dc.ColorProfile.InvDamageColor, dc.ColorProfile.InvBorderColor);
                        break;
                    case 2:
                        dc.Graphics.DrawSquarishNote(rect, dc.ColorProfile.DamageColor, dc.ColorProfile.BorderColor);
                        break;
                }
                
            }
            
        }


        public static void DrawStepNoteTap(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.StepNoteTapColor, dc.ColorProfile.BorderColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.InvStepNoteTapColor, dc.ColorProfile.InvBorderColor);
                        break;
                    case 2:
                        dc.Graphics.DrawTappableNote(rect, dc.ColorProfile.StepNoteTapColor, dc.ColorProfile.BorderColor);
                        break;

                }

            }

        }

        public static void DrawHoldBegin(this DrawingContext dc, RectangleF rect, int mode)
        {
            dc.DrawHoldEnd(rect);
            dc.Graphics.DrawTapSymbol(rect, mode);
        }

        public static void DrawHoldEnd(this DrawingContext dc, RectangleF rect)
        {
            dc.Graphics.DrawNote(rect, dc.ColorProfile.HoldColor, dc.ColorProfile.BorderColor);
        }

        public static void DrawHoldBackground(this DrawingContext dc, RectangleF rect)
        {
            Color BackgroundEdgeColor = dc.ColorProfile.HoldBackgroundColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.HoldBackgroundColor.LightColor;

            var prevMode = dc.Graphics.SmoothingMode;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(rect, BackgroundEdgeColor, BackgroundMiddleColor, LinearGradientMode.Vertical))
            {
                var blend = new ColorBlend(4)
                {
                    Colors = new Color[] { BackgroundEdgeColor, BackgroundMiddleColor, BackgroundMiddleColor, BackgroundEdgeColor },
                    Positions = new float[] { 0.0f, 0.3f, 0.7f, 1.0f }
                };
                brush.InterpolationColors = blend;
                dc.Graphics.FillRectangle(brush, rect);
            }
            dc.Graphics.SmoothingMode = prevMode;
        }

        public static void DrawSlideBegin(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            dc.DrawSlideStep(rect, isch, mode);
            dc.Graphics.DrawTapSymbol(rect, mode);

        }

        public static void DrawSlideStep(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                dc.Graphics.DrawNote(rect, dc.ColorProfile.SlideColor, dc.ColorProfile.BorderColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawNote(rect, dc.ColorProfile.InvSlideColor, dc.ColorProfile.InvBorderColor);
                        break;
                    case 2:
                        dc.Graphics.DrawNote(rect, dc.ColorProfile.SlideColor, dc.ColorProfile.BorderColor);
                        break;
                }
                
            }
            
        }



        /// <summary>
        /// SLIDEの背景を描画します。
        /// </summary>
        /// <param name="dc">処理対象の<see cref="DrawingContext"/></param>
        /// <param name="steps">全ての中継点位置からなるリスト</param>
        /// <param name="visibleSteps">可視中継点のY座標からなるリスト</param>
        /// <param name="noteHeight">ノート描画高さ</param>
        public static void DrawSlideBackground(this DrawingContext dc, IEnumerable<SlideStepElement> steps, IEnumerable<float> visibleSteps, float noteHeight, bool isch, int mode, bool usingBezier , IReadOnlyCollection<Air> airs, IReadOnlyCollection<Flick> flicks)
        {

            var prevMode = dc.Graphics.SmoothingMode;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color BackgroundEdgeColor = dc.ColorProfile.SlideBackgroundColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.SlideBackgroundColor.LightColor;
            Color SlideLineColor = dc.ColorProfile.SlideLineColor;
            //mode 0:非表示, 1:半透明, 2:表示

            if (!isch)
            {
                if (mode == 0) return;
                if(mode == 1)
                {
                    BackgroundEdgeColor = dc.ColorProfile.InvSlideBackgroundColor.DarkColor;
                    BackgroundMiddleColor = dc.ColorProfile.InvSlideBackgroundColor.LightColor;
                    SlideLineColor = dc.ColorProfile.InvSlideLineColor;
                }
            }
            

            var orderedSteps = steps.OrderBy(p => p.Point.Y).ToList();
            var orderedVisibleSteps = visibleSteps.OrderBy(p => p).ToList();
            


            //スライド背景
            using (var path = new GraphicsPath())
            {
                var left = orderedSteps.Select(p => p.Point);
                var right = orderedSteps.Select(p => new PointF(p.Point.X + p.Width, p.Point.Y)).Reverse();


                float head = orderedVisibleSteps[0];
                float height = orderedVisibleSteps[orderedVisibleSteps.Count - 1] - head;
                var pathBounds = path.GetBounds();
                var blendBounds = new RectangleF(pathBounds.X, head, pathBounds.Width + 0.1f, height + 0.1f);
                

                if (usingBezier)
                {
                    int i = 0;
                    foreach (var step in orderedSteps)
                    {
                        foreach (var flick in flicks)
                        {
                            if (visibleSteps.Contains(step.Point.Y) && orderedVisibleSteps.Last() != step.Point.Y && orderedSteps.First().Point.Y != step.Point.Y && step.Tick == flick.Tick && step.LaneIndex == flick.LaneIndex)
                            {
                                step.Skippable = true;
                            }

                        }
                    }
                    var lineSteps = orderedSteps.Where(q => !q.Skippable).ToList();
                    foreach (var step in orderedSteps)
                    {
                        if (step.Skippable) continue;
                        if (i + 2 > orderedSteps.Count)
                            continue;
                        int curvetype1 = 0;
                        int curvetype2 = 0;
                        var air = airs.Where(p => p.ParentNote.Tick == step.Tick && p.ParentNote.LaneIndex == step.LaneIndex).FirstOrDefault();
                        if (air != null)
                        {
                            curvetype1 = (int)air.HorizontalDirection + 1;
                            curvetype2 = (int)air.VerticalDirection;
                            if (curvetype2 == 0) curvetype1 += 3; //DOWN  center1 left,right 2,3 UP center4 left, right5,6
                        }
                        step.CurveType = curvetype1;

                        if (flicks.Where(q => visibleSteps.Contains(step.Point.Y) && orderedVisibleSteps.Last() != step.Point.Y && orderedSteps.First().Point.Y != step.Point.Y && step.Tick == q.Tick && step.LaneIndex == q.LaneIndex).FirstOrDefault() != null)
                        {
                            step.Skippable = true; //もしフリックが重なってたらスキップ可能に

                        }

                        while (!orderedSteps[i + 1].Skippable)
                        {

                            //Console.WriteLine("i: " + i + " " + !orderedSteps[i + 1].Skippable);
                            if (!orderedSteps[i + 1].Skippable)
                            {
                                break;
                            }
                            else
                            {
                                i++;
                            }

                        }

                        if (step.Skippable) { i++; continue; };
                        //Console.WriteLine(lineSteps.IndexOf(step) + " count:" + lineSteps.Count);
                        var nextstep = lineSteps.OrderBy(q => q.Tick).ToList()[Math.Min(lineSteps.IndexOf(step) + 1, lineSteps.Count - 1)];

                        float un = nextstep.Point.Y - step.Point.Y;
                        float ln = nextstep.Point.X - step.Point.X;

                        var startpoint = step.Point;
                        var steppoint1 = new PointF(step.Point.X, step.Point.Y + un / 3);
                        var steppoint2 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var steppoint3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var endpoint = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var startpointr = new PointF(step.Point.X + step.Width, step.Point.Y);
                        var steppoint1r = new PointF(step.Point.X + step.Width, step.Point.Y + un / 3);
                        var steppoint2r = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                        var endpointr = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);

                        var start = step.Point;
                        var step1 = step.Point;
                        var step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                        var end = new PointF(step.Point.X + step.Width, step.Point.Y);
                        var step3 = step.Point;
                        var step4 = new PointF(step.Point.X + step.Width / 2, step.Point.Y + un / 3);
                        var end2 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var step5 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var step6 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                        var end3 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                        var step7 = new PointF(step.Point.X, step.Point.Y + un / 5 * 3);
                        var step8 = new PointF(nextstep.Point.X, nextstep.Point.Y - un / 5 * 3);
                        PointF[] points = { start, step1, step2, end };
                        PointF[] points2 = { start, step1, step2, end };

                        switch (step.CurveType)
                        {
                            case 0:
                                points = new[] { step.Point, end, end3, end2 };
                                path.AddPolygon(points);
                                path.CloseFigure();
                                break;
                            case 1:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step4 = new PointF(step.Point.X + step.Width, step.Point.Y + un / 5 * 3);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(step.Point.X, step.Point.Y + un / 5 * 3);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step6, step7, start };

                                path.AddBeziers(points);
                                path.CloseFigure();
                                break;

                            case 2:
                            case 3:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step4 = new PointF(nextstep.Point.X + nextstep.Width, step.Point.Y + un / 5 * 2);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(nextstep.Point.X, step.Point.Y + un / 5 * 2);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step6, step7, start };
                                path.AddBeziers(points);
                                path.CloseFigure();
                                break;

                            case 4:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(step.Point.X + step.Width, step.Point.Y + un / 5 * 3);
                                step4 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y - un / 5 * 3);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(step.Point.X, step.Point.Y + un / 5 * 3);
                                step8 = new PointF(nextstep.Point.X, nextstep.Point.Y - un / 5 * 3);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step8, step7, start };

                                path.AddBeziers(points);
                                path.CloseFigure();

                                break;

                            case 5:
                            case 6:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(nextstep.Point.X + nextstep.Width, step.Point.Y + un / 5 * 3);
                                step4 = new PointF(step.Point.X + step.Width, nextstep.Point.Y - un / 5 * 3);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(step.Point.X, nextstep.Point.Y - un / 5 * 3);
                                step8 = new PointF(nextstep.Point.X, step.Point.Y + un / 5 * 3);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step7, step8, start };

                                path.AddBeziers(points);
                                path.CloseFigure();

                                break;

                            default:
                                //Console.WriteLine("default");
                                points = new[] { step.Point, end, end3, end2 };
                                path.AddPolygon(points);
                                path.CloseFigure();
                                break;

                        }



                        i++;
                    }

                }
                else
                {
                    path.AddPolygon(left.Concat(right).ToArray());
                }






                using (var brush = new LinearGradientBrush(blendBounds, Color.Black, Color.Black, LinearGradientMode.Vertical))
                {
                    var heights = orderedVisibleSteps.Zip(orderedVisibleSteps.Skip(1), (p, q) => Tuple.Create(p, q - p));
                    var absPos = new[] { head }.Concat(heights.SelectMany(p => new[] { p.Item1 + p.Item2 * 0.3f, p.Item1 + p.Item2 * 0.7f, p.Item1 + p.Item2 }));
                    var blend = new ColorBlend()
                    {
                        Positions = absPos.Select(p => (p - head) / height).ToArray(),
                        Colors = new[] { BackgroundEdgeColor }.Concat(Enumerable.Range(0, orderedVisibleSteps.Count - 1).SelectMany(p => new[] { BackgroundMiddleColor, BackgroundMiddleColor, BackgroundEdgeColor })).ToArray()
                    };
                    brush.InterpolationColors = blend;
                    path.FillMode = FillMode.Winding;
                    dc.Graphics.FillPath(brush, path);
                }
            }
            //スライドの線
            using (var pen = new Pen(dc.ColorProfile.SlideLineColor, noteHeight * 0.4f))
            {
                if (usingBezier)
                {
                    int i = 0;
                    foreach (var step in orderedSteps)
                    {
                        foreach (var flick in flicks)
                        {
                            //Console.WriteLine("isVisible:" + visibleSteps.Contains(step.Point.Y) + "notLast:" + orderedVisibleSteps.Last() != step.Point.Y + "Tick:" + (step.Tick == flick.Tick) + "LaneIndex:" + (step.LaneIndex == flick.LaneIndex));
                            if (visibleSteps.Contains(step.Point.Y) && orderedVisibleSteps.Last() != step.Point.Y && orderedSteps.First().Point.Y != step.Point.Y && step.Tick == flick.Tick && step.LaneIndex == flick.LaneIndex)
                            {
                                step.Skippable = true;
                            }

                        }
                    }
                    var lineSteps = orderedSteps.Where(q => !q.Skippable).ToList();
                    foreach (var p in orderedSteps)
                    {


                        if (i + 2 > orderedSteps.Count) //Countは2~ 
                            continue;

                        if (flicks.Where(q => visibleSteps.Contains(p.Point.Y) && orderedVisibleSteps.Last() != p.Point.Y && orderedSteps.First().Point.Y != p.Point.Y && p.Tick == q.Tick && p.LaneIndex == q.LaneIndex).FirstOrDefault() != null)
                        {
                            p.Skippable = true; //もしフリックが重なってたらスキップ可能に

                        }

                        while (!orderedSteps[i + 1].Skippable)
                        {

                            //Console.WriteLine("i: " + i + " " + !orderedSteps[i + 1].Skippable);
                            if (!orderedSteps[i + 1].Skippable)
                            {
                                break;
                            }
                            else
                            {
                                i++;
                            }

                        }

                        //Console.WriteLine("i: " + i + " " + !orderedSteps[i + 1].Skippable + "end");




                        if (p.Skippable) { i++; continue; };

                        //Console.WriteLine("linestep: " + lineSteps.IndexOf(p));
                        var nextstep = lineSteps.OrderBy(q => q.Tick).ToList()[lineSteps.IndexOf(p) + 1];
                        /*
                        var o = 1;
                        while (!nextstep.Skippable)
                        {
                            nextstep = lineSteps[Math.Min(lineSteps.Count - 1, lineSteps.IndexOf(p) + o)];
                            Console.WriteLine("next" + o + " " + nextstep.Skippable);
                            if (!nextstep.Skippable)
                            {
                                break;
                            }
                            else
                            {
                                o++;
                            }
                        }
                        */


                        var start = new PointF(p.Point.X + p.Width / 2, p.Point.Y);
                        var end = new PointF(nextstep.Point.X + nextstep.Width / 2, nextstep.Point.Y);

                        float un = nextstep.Point.Y - p.Point.Y;
                        var step1 = new PointF(p.Point.X + p.Width / 2, p.Point.Y + un / 5 * 3);
                        var step2 = new PointF(nextstep.Point.X + nextstep.Width / 2, p.Point.Y + un / 5 * 2);
                        var step3 = new PointF(p.Point.X + p.Width / 2, p.Point.Y + un / 5 * 3);
                        var step5 = new PointF(nextstep.Point.X + nextstep.Width / 2, p.Point.Y + un / 5 * 3);
                        var step6 = new PointF(p.Point.X + p.Width / 2, nextstep.Point.Y - un / 5 * 3);


                        var linepoints = new[] { start, end };

                        switch (p.CurveType)
                        {
                            case 0:
                                dc.Graphics.DrawLines(pen, linepoints);
                                break;
                            case 1:
                                dc.Graphics.DrawBezier(pen, start, start, step1, end);
                                break;
                            case 2:
                            case 3:
                                dc.Graphics.DrawBezier(pen, start, start, step2, end);
                                break;
                            case 4:
                                dc.Graphics.DrawBezier(pen, start, step1, step2, end);
                                break;
                            case 5:
                            case 6:
                                dc.Graphics.DrawBezier(pen, start, step5, step6, end);
                                break;
                        }

                        i++;
                    }
                }
                else
                {
                    dc.Graphics.DrawLines(pen, orderedSteps.Select(p => new PointF(p.Point.X + p.Width / 2, p.Point.Y)).ToArray());
                }


            }

            dc.Graphics.SmoothingMode = prevMode;
            
            
            
        }

        public static GraphicsPath GetSlideBackgroundPath(float width1, float width2, float x1, float y1, float x2, float y2, bool useBezier, int curveType)
        {
            var path = new GraphicsPath();
                path.AddPolygon(new PointF[]
                            {
                new PointF(x1, y1),
                new PointF(x1 + width1, y1),
                new PointF(x2 + width2, y2),
                new PointF(x2, y2)
                            });
            
            
            return path;
        }


        public static void DrawGuideBegin(this DrawingContext dc, RectangleF rect, bool isch, int mode, USCGuideColor color)
        {
            dc.DrawGuideStep(rect, isch, mode, color);
            dc.Graphics.DrawTapSymbol(rect, mode);

        }

        public static void DrawGuideStep(this DrawingContext dc,  RectangleF rect, bool isch, int mode, USCGuideColor color)
        {
            var guidecolor = dc.ColorProfile.GuideColor;
            var invguidecolor = dc.ColorProfile.InvGuideColor;

            switch (color)
            {
                case USCGuideColor.neutral:
                    guidecolor = dc.ColorProfile.GuideNeutralColor;
                    invguidecolor = dc.ColorProfile.InvGuideNeutralColor;
                    break;
                case USCGuideColor.red:
                    guidecolor = dc.ColorProfile.GuideRedColor;
                    invguidecolor = dc.ColorProfile.InvGuideRedColor;
                    break;
                case USCGuideColor.green:
                    guidecolor = dc.ColorProfile.GuideColor;
                    invguidecolor = dc.ColorProfile.InvGuideColor;
                    break;
                case USCGuideColor.blue:
                    guidecolor = dc.ColorProfile.GuideBlueColor;
                    invguidecolor = dc.ColorProfile.InvGuideBlueColor;
                    break;
                case USCGuideColor.yellow:
                    guidecolor = dc.ColorProfile.GuideYellowColor;
                    invguidecolor = dc.ColorProfile.InvGuideYellowColor;
                    break;
                case USCGuideColor.purple:
                    guidecolor = dc.ColorProfile.GuidePurpleColor;
                    invguidecolor = dc.ColorProfile.InvGuidePurpleColor;
                    break;
                case USCGuideColor.cyan:
                    guidecolor = dc.ColorProfile.GuideCyanColor;
                    invguidecolor = dc.ColorProfile.InvGuideCyanColor;
                    break;
                case USCGuideColor.black:
                    guidecolor = dc.ColorProfile.GuideBlackColor;
                    invguidecolor = dc.ColorProfile.InvGuideBlackColor;
                    break;
            }

            if (isch)
            {
                dc.Graphics.DrawNote(rect, guidecolor, dc.ColorProfile.BorderColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawNote(rect, invguidecolor, dc.ColorProfile.InvBorderColor);
                        break;
                    case 2:
                        dc.Graphics.DrawNote(rect, guidecolor, dc.ColorProfile.BorderColor);
                        break;
                }

            }

        }


        /// <summary>
        /// GUIDEの背景を描画します。
        /// </summary>
        /// <param name="dc">処理対象の<see cref="DrawingContext"/></param>
        /// <param name="steps">全ての中継点位置からなるリスト</param>
        /// <param name="visibleSteps">可視中継点のY座標からなるリスト</param>
        /// <param name="noteHeight">ノート描画高さ</param>
        public static void DrawGuideBackground(this DrawingContext dc, IEnumerable<GuideStepElement> steps, IEnumerable<float> visibleSteps, float noteHeight, bool isch, int mode, USCGuideColor color, bool usingBezier, IReadOnlyCollection<Air> airs, IEnumerable<Tap> taps)
        {
            if (!isch && mode == 0) return;
            var prevMode = dc.Graphics.SmoothingMode;
            dc.Graphics.SmoothingMode = SmoothingMode.AntiAlias;



            Color BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundColor.DarkColor;
            Color BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundColor.LightColor;
            Color GuideLineColor = dc.ColorProfile.SlideLineColor;
            //mode 0:非表示, 1:半透明, 2:表示

            var orderedSteps = steps.OrderBy(p => p.Point.Y).ToList();
            var orderedVisibleSteps = visibleSteps.OrderBy(p => p).ToList();

            if (!isch && mode == 1)
            {
                GuideLineColor = dc.ColorProfile.InvSlideLineColor;
            }
            
                switch (color)
                {
                case USCGuideColor.neutral:
                    if(!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundNeutralColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundNeutralColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundNeutralColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundNeutralColor.LightColor;
                    }
                    
                    break;
                case USCGuideColor.red:
                    if (!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundRedColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundRedColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundRedColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundRedColor.LightColor;
                    }
                    break;
                case USCGuideColor.green:
                    if (!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundColor.LightColor;
                    }
                    break;
                case USCGuideColor.blue:
                    if (!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundBlueColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundBlueColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundBlueColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundBlueColor.LightColor;
                    }
                    break;
                case USCGuideColor.yellow:
                    if (!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundYellowColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundYellowColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundYellowColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundYellowColor.LightColor;
                    }
                    break;
                case USCGuideColor.purple:
                    if (!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundPurpleColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundPurpleColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundPurpleColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundPurpleColor.LightColor;
                    }
                    break;
                case USCGuideColor.cyan:
                    if (!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundCyanColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundCyanColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundCyanColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundCyanColor.LightColor;
                    }
                    break;
                case USCGuideColor.black:
                    if (!isch && mode == 1)
                    {
                        BackgroundEdgeColor = dc.ColorProfile.InvGuideBackgroundBlackColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.InvGuideBackgroundBlackColor.LightColor;
                    }
                    else
                    {
                        BackgroundEdgeColor = dc.ColorProfile.GuideBackgroundBlackColor.DarkColor;
                        BackgroundMiddleColor = dc.ColorProfile.GuideBackgroundBlackColor.LightColor;
                    }
                    break;
            }


            



            using (var path = new GraphicsPath())
            {
                var left = orderedSteps.Select(p => p.Point);
                var right = orderedSteps.Select(p => new PointF(p.Point.X + p.Width, p.Point.Y)).Reverse();

                float head = orderedVisibleSteps[0];
                float height = orderedVisibleSteps[orderedVisibleSteps.Count - 1] - head;
                var pathBounds = path.GetBounds();
                var blendBounds = new RectangleF(pathBounds.X, head, pathBounds.Width + 0.01f, height + 0.01f);

                if (usingBezier)
                {
                    int i = 0;
                    foreach (var step in orderedSteps)
                    {
                        if (i + 2 > orderedSteps.Count)
                            continue;
                        int curvetype1 = 0;
                        int curvetype2 = 0;
                        var air = airs.Where(p => p.ParentNote.Tick == step.Tick && p.ParentNote.LaneIndex == step.LaneIndex).FirstOrDefault();
                        if (orderedSteps.IndexOf(step) == 0)
                        {
                            var tap = taps.Where(p => p.Tick == step.Tick && p.LaneIndex == step.LaneIndex && p.IsStart).FirstOrDefault();
                            if (air != null)
                            {
                                if (air.VerticalDirection == VerticalAirDirection.Down)
                                {
                                    curvetype1 = (int)air.HorizontalDirection + 1;
                                    curvetype2 = (int)air.VerticalDirection;
                                    if (curvetype2 == 0) curvetype1 += 3; //DOWN  center1 left,right 2,3 UP center4 left, right5,6
                                }
                                else
                                {
                                    if (tap != null)
                                    {
                                        curvetype1 = (int)air.HorizontalDirection + 1;
                                        curvetype2 = (int)air.VerticalDirection;
                                        if (curvetype2 == 0) curvetype1 += 3; //DOWN  center1 left,right 2,3 UP center4 left, right5,6
                                    }
                                }

                            }
                        }
                        else
                        {

                            if (air != null)
                            {
                                curvetype1 = (int)air.HorizontalDirection + 1;
                                curvetype2 = (int)air.VerticalDirection;
                                if (curvetype2 == 0) curvetype1 += 3; //DOWN  center1 left,right 2,3 UP center4 left, right5,6
                            }
                        }

                        step.CurveType = curvetype1;


                        //Console.WriteLine(lineSteps.IndexOf(step) + " count:" + lineSteps.Count);
                        var nextstep = orderedSteps.OrderBy(q => q.Tick).ToList()[Math.Min(orderedSteps.IndexOf(step) + 1, orderedSteps.Count - 1)];

                        float un = nextstep.Point.Y - step.Point.Y;
                        float ln = nextstep.Point.X - step.Point.X;

                        var startpoint = step.Point;
                        var steppoint1 = new PointF(step.Point.X, step.Point.Y + un / 3);
                        var steppoint2 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var steppoint3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var endpoint = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var startpointr = new PointF(step.Point.X + step.Width, step.Point.Y);
                        var steppoint1r = new PointF(step.Point.X + step.Width, step.Point.Y + un / 3);
                        var steppoint2r = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                        var endpointr = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);

                        var start = step.Point;
                        var step1 = step.Point;
                        var step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                        var end = new PointF(step.Point.X + step.Width, step.Point.Y);
                        var step3 = step.Point;
                        var step4 = new PointF(step.Point.X + step.Width / 2, step.Point.Y + un / 3);
                        var end2 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var step5 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                        var step6 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                        var end3 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                        var step7 = new PointF(step.Point.X, step.Point.Y + un / 5 * 3);
                        var step8 = new PointF(nextstep.Point.X, nextstep.Point.Y - un / 5 * 3);
                        PointF[] points = { start, step1, step2, end };
                        PointF[] points2 = { start, step1, step2, end };

                        switch (step.CurveType)
                        {
                            case 0:
                                points = new[] { step.Point, end, end3, end2 };
                                path.AddPolygon(points);
                                path.CloseFigure();
                                break;
                            case 1:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step4 = new PointF(step.Point.X + step.Width, step.Point.Y + un / 5 * 3);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(step.Point.X, step.Point.Y + un / 5 * 3);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step6, step7, start };

                                path.AddBeziers(points);
                                path.CloseFigure();
                                break;

                            case 2:
                            case 3:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step4 = new PointF(nextstep.Point.X + nextstep.Width, step.Point.Y + un / 5 * 2);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(nextstep.Point.X, step.Point.Y + un / 5 * 2);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step6, step7, start };
                                path.AddBeziers(points);
                                path.CloseFigure();
                                break;

                            case 4:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(step.Point.X + step.Width, step.Point.Y + un / 5 * 3);
                                step4 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y - un / 5 * 3);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(step.Point.X, step.Point.Y + un / 5 * 3);
                                step8 = new PointF(nextstep.Point.X, nextstep.Point.Y - un / 5 * 3);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step8, step7, start };

                                path.AddBeziers(points);
                                path.CloseFigure();

                                break;

                            case 5:
                            case 6:

                                start = step.Point;
                                step1 = step.Point;
                                step2 = new PointF(step.Point.X + step.Width, step.Point.Y);
                                end = new PointF(step.Point.X + step.Width, step.Point.Y);
                                step3 = new PointF(nextstep.Point.X + nextstep.Width, step.Point.Y + un / 5 * 3);
                                step4 = new PointF(step.Point.X + step.Width, nextstep.Point.Y - un / 5 * 3);
                                end2 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step5 = new PointF(nextstep.Point.X + nextstep.Width, nextstep.Point.Y);
                                step6 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                end3 = new PointF(nextstep.Point.X, nextstep.Point.Y);
                                step7 = new PointF(step.Point.X, nextstep.Point.Y - un / 5 * 3);
                                step8 = new PointF(nextstep.Point.X, step.Point.Y + un / 5 * 3);

                                points = new[] { start, step1, step2, end, step3, step4, end2, step5, step6, end3, step7, step8, start };

                                path.AddBeziers(points);
                                path.CloseFigure();

                                break;

                            default:
                                //Console.WriteLine("default");
                                points = new[] { step.Point, end, end3, end2 };
                                path.AddPolygon(points);
                                path.CloseFigure();
                                break;

                        }



                        i++;
                    }

                }
                else
                {
                    path.AddPolygon(left.Concat(right).ToArray());
                }

                using (var brush = new LinearGradientBrush(blendBounds, Color.Black, Color.Black, LinearGradientMode.Vertical))
                {
                    var heights = orderedVisibleSteps.Zip(orderedVisibleSteps.Skip(1), (p, q) => Tuple.Create(p, q - p));
                    var absPos = new[] { head }.Concat(heights.SelectMany(p => new[] { p.Item1 + p.Item2 * 0.3f, p.Item1 + p.Item2 * 0.7f, p.Item1 + p.Item2 }));
                    var blend = new ColorBlend()
                    {
                        Positions = absPos.Select(p => (p - head) / height).ToArray(),
                        Colors = new[] { BackgroundEdgeColor }.Concat(Enumerable.Range(0, orderedVisibleSteps.Count - 1).SelectMany(p => new[] { BackgroundMiddleColor, BackgroundMiddleColor, BackgroundEdgeColor })).ToArray()
                    };
                    brush.InterpolationColors = blend;
                    dc.Graphics.FillPath(brush, path);
                }
            }

            using (var pen = new Pen(GuideLineColor, noteHeight * 0.4f))
            {
                if (usingBezier)
                {
                    int i = 0;
                    foreach (var p in orderedSteps)
                    {


                        if (i + 2 > orderedSteps.Count) //Countは2~ 
                            continue;

                        //Console.WriteLine("i: " + i + " " + !orderedSteps[i + 1].Skippable + "end");



                        //Console.WriteLine("linestep: " + lineSteps.IndexOf(p));
                        var nextstep = orderedSteps.OrderBy(q => q.Tick).ToList()[orderedSteps.IndexOf(p) + 1];
                        /*
                        var o = 1;
                        while (!nextstep.Skippable)
                        {
                            nextstep = lineSteps[Math.Min(lineSteps.Count - 1, lineSteps.IndexOf(p) + o)];
                            Console.WriteLine("next" + o + " " + nextstep.Skippable);
                            if (!nextstep.Skippable)
                            {
                                break;
                            }
                            else
                            {
                                o++;
                            }
                        }
                        */


                        var start = new PointF(p.Point.X + p.Width / 2, p.Point.Y);
                        var end = new PointF(nextstep.Point.X + nextstep.Width / 2, nextstep.Point.Y);

                        float un = nextstep.Point.Y - p.Point.Y;
                        var step1 = new PointF(p.Point.X + p.Width / 2, p.Point.Y + un / 5 * 3);
                        var step2 = new PointF(nextstep.Point.X + nextstep.Width / 2, p.Point.Y + un / 5 * 2);
                        var step3 = new PointF(p.Point.X + p.Width / 2, p.Point.Y + un / 5 * 3);
                        var step5 = new PointF(nextstep.Point.X + nextstep.Width / 2, p.Point.Y + un / 5 * 3);
                        var step6 = new PointF(p.Point.X + p.Width / 2, nextstep.Point.Y - un / 5 * 3);


                        var linepoints = new[] { start, end };

                        switch (p.CurveType)
                        {
                            case 0:
                                dc.Graphics.DrawLines(pen, linepoints);
                                break;
                            case 1:
                                dc.Graphics.DrawBezier(pen, start, start, step1, end);
                                break;
                            case 2:
                            case 3:
                                dc.Graphics.DrawBezier(pen, start, start, step2, end);
                                break;
                            case 4:
                                dc.Graphics.DrawBezier(pen, start, step1, step2, end);
                                break;
                            case 5:
                            case 6:
                                dc.Graphics.DrawBezier(pen, start, step5, step6, end);
                                break;
                        }

                        i++;
                    }
                }
                else
                {
                    dc.Graphics.DrawLines(pen, orderedSteps.Select(p => new PointF(p.Point.X + p.Width / 2, p.Point.Y)).ToArray());
                }
            }

            dc.Graphics.SmoothingMode = prevMode;

        }

        public static GraphicsPath GetGuideBackgroundPath(float width1, float width2, float x1, float y1, float x2, float y2)
        {
            var path = new GraphicsPath();
            path.AddPolygon(new PointF[]
            {
                new PointF(x1, y1),
                new PointF(x1 + width1, y1),
                new PointF(x2 + width2, y2),
                new PointF(x2, y2)
            });
            return path;
        }




        public static void DrawAir(this DrawingContext dc, RectangleF targetNoteRect, VerticalAirDirection verticalDirection, HorizontalAirDirection horizontalDirection, bool isch, int mode)
        {
            if (isch)
            {
                var targetRect = GetAirRect(targetNoteRect);
                // ノートを内包するRect(ノートの下部中心が原点)
                var box = new RectangleF(-targetRect.Width / 2, -targetRect.Height, targetRect.Width, targetRect.Height);
                // ノート形状の構成点(上向き)
                var points = new PointF[]
                {
                new PointF(box.Left, box.Bottom),
                new PointF(box.Left, box.Top + box.Height / 3),
                new PointF(box.Left + box.Width / 2 , box.Top),
                new PointF(box.Right, box.Top + box.Height / 3),
                new PointF(box.Right, box.Bottom),
                new PointF(box.Left + box.Width / 2, box.Bottom - box.Height / 3)
                };

                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(points);
                    var prevMatrix = dc.Graphics.Transform;
                    var matrix = prevMatrix.Clone();

                    // 描画先の下部中心を原点にもってくる
                    matrix.Translate(targetRect.Left + targetRect.Width / 2, targetRect.Top);
                    // 振り上げなら上下反転(描画座標が上下逆になってるので……)
                    if (verticalDirection == VerticalAirDirection.Up) matrix.Scale(1, -1);
                    // 左右分で傾斜をかける
                    if (horizontalDirection != HorizontalAirDirection.Center) matrix.Shear(horizontalDirection == HorizontalAirDirection.Left ? 0.5f : -0.5f, 0);
                    // 振り下げでずれた高さを補正
                    if (verticalDirection == VerticalAirDirection.Down) matrix.Translate(0, box.Height);

                    dc.Graphics.Transform = matrix;

                    using (var brush = new SolidBrush(verticalDirection == VerticalAirDirection.Down ? dc.ColorProfile.AirDownColor : dc.ColorProfile.AirUpColor))
                    {
                        dc.Graphics.FillPath(brush, path);
                    }
                    // 斜めになると太さが大きく出てしまう
                    using (var pen = new Pen(dc.ColorProfile.BorderColor.LightColor, targetRect.Height * (horizontalDirection == HorizontalAirDirection.Center ? 0.12f : 0.1f)) { LineJoin = LineJoin.Bevel })
                    {
                        dc.Graphics.DrawPath(pen, path);
                    }

                    dc.Graphics.Transform = prevMatrix;
                }
            }
            else
            {
                var targetRect = GetAirRect(targetNoteRect);
                // ノートを内包するRect(ノートの下部中心が原点)
                var box = new RectangleF(-targetRect.Width / 2, -targetRect.Height, targetRect.Width, targetRect.Height);
                // ノート形状の構成点(上向き)
                var points = new PointF[]
                {
                new PointF(box.Left, box.Bottom),
                new PointF(box.Left, box.Top + box.Height / 3),
                new PointF(box.Left + box.Width / 2 , box.Top),
                new PointF(box.Right, box.Top + box.Height / 3),
                new PointF(box.Right, box.Bottom),
                new PointF(box.Left + box.Width / 2, box.Bottom - box.Height / 3)
                };


                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(points);
                    var prevMatrix = dc.Graphics.Transform;
                    var matrix = prevMatrix.Clone();

                    // 描画先の下部中心を原点にもってくる
                    matrix.Translate(targetRect.Left + targetRect.Width / 2, targetRect.Top);
                    // 振り上げなら上下反転(描画座標が上下逆になってるので……)
                    if (verticalDirection == VerticalAirDirection.Up) matrix.Scale(1, -1);
                    // 左右分で傾斜をかける
                    if (horizontalDirection != HorizontalAirDirection.Center) matrix.Shear(horizontalDirection == HorizontalAirDirection.Left ? 0.5f : -0.5f, 0);
                    // 振り下げでずれた高さを補正
                    if (verticalDirection == VerticalAirDirection.Down) matrix.Translate(0, box.Height);

                    dc.Graphics.Transform = matrix;

                    
                    switch (mode)
                    {
                        case 0:
                            break;
                        case 1:
                            using (var brush = new SolidBrush(verticalDirection == VerticalAirDirection.Down ? dc.ColorProfile.InvAirDownColor : dc.ColorProfile.InvAirUpColor))
                            {
                                dc.Graphics.FillPath(brush, path);
                            }
                            using (var pen = new Pen(dc.ColorProfile.InvBorderColor.LightColor, targetRect.Height * (horizontalDirection == HorizontalAirDirection.Center ? 0.12f : 0.1f)) { LineJoin = LineJoin.Bevel })
                            {
                                dc.Graphics.DrawPath(pen, path);
                            }
                            break;
                        case 2:
                            using (var brush = new SolidBrush(verticalDirection == VerticalAirDirection.Down ? dc.ColorProfile.AirDownColor : dc.ColorProfile.AirUpColor))
                            {
                                dc.Graphics.FillPath(brush, path);
                            }
                            using (var pen = new Pen(dc.ColorProfile.BorderColor.LightColor, targetRect.Height * (horizontalDirection == HorizontalAirDirection.Center ? 0.12f : 0.1f)) { LineJoin = LineJoin.Bevel })
                            {
                                dc.Graphics.DrawPath(pen, path);
                            }
                            break;
                    }
                    

                    dc.Graphics.Transform = prevMatrix;
                }
            }
            
        }

        public static RectangleF GetAirRect(RectangleF targetNoteRect)
        {
            var targetSize = new SizeF(targetNoteRect.Width * 0.9f, targetNoteRect.Height * 3);
            var targetLocation = new PointF(targetNoteRect.Left + targetNoteRect.Width * 0.05f, targetNoteRect.Bottom + targetNoteRect.Height);
            return new RectangleF(targetLocation, targetSize);
        }

        public static void DrawAirAction(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                using (var brush = new LinearGradientBrush(rect, dc.ColorProfile.AirActionColor.DarkColor, dc.ColorProfile.AirActionColor.LightColor, LinearGradientMode.Vertical))
                {
                    dc.Graphics.FillRectangle(brush, rect);
                }
                using (var brush = new LinearGradientBrush(rect.Expand(rect.Height * 0.1f), dc.ColorProfile.BorderColor.DarkColor, dc.ColorProfile.BorderColor.LightColor, LinearGradientMode.Vertical))
                {
                    using (var pen = new Pen(brush, rect.Height * 0.1f))
                    {
                        dc.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    }
                }
            }
            else
            {
                using (var brush = new LinearGradientBrush(rect, dc.ColorProfile.InvAirActionColor.DarkColor, dc.ColorProfile.InvAirActionColor.LightColor, LinearGradientMode.Vertical))
                {
                    dc.Graphics.FillRectangle(brush, rect);
                }
                using (var brush = new LinearGradientBrush(rect.Expand(rect.Height * 0.1f), dc.ColorProfile.InvBorderColor.DarkColor, dc.ColorProfile.InvBorderColor.LightColor, LinearGradientMode.Vertical))
                {
                    using (var pen = new Pen(brush, rect.Height * 0.1f))
                    {
                        dc.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    }
                }
            }
            
        }

        public static void DrawAirStep(this DrawingContext dc, RectangleF rect, bool isch)
        {
            if (isch)
            {
                dc.Graphics.DrawNote(rect, dc.ColorProfile.AirStepColor, dc.ColorProfile.BorderColor);
            }
            else
            {
                dc.Graphics.DrawNote(rect, dc.ColorProfile.InvAirStepColor, dc.ColorProfile.InvBorderColor);
            }
            
        }

        public static void DrawAirHoldLine(this DrawingContext dc, float x, float y1, float y2, float noteHeight, bool isch)
        {
            using (var pen = new Pen(dc.ColorProfile.AirHoldLineColor, noteHeight / 2))
            {
                dc.Graphics.DrawLine(pen, x, y1, x, y2);
            }
        }

        public static void DrawBorder(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                dc.Graphics.DrawBorder(rect, dc.ColorProfile.BorderColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawBorder(rect, dc.ColorProfile.InvBorderColor);
                        break;
                    case 2:
                        dc.Graphics.DrawBorder(rect, dc.ColorProfile.BorderColor);
                        break;
                }
                
            }
            
        }

        public static void DrawBorder(this DrawingContext dc, RectangleF rect, bool isch, int mode, GradientColor color, GradientColor invcolor)
        {
            if (isch)
            {
                dc.Graphics.DrawBorder(rect, color, 0.4f);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawBorder(rect, invcolor);
                        break;
                    case 2:
                        dc.Graphics.DrawBorder(rect, invcolor);
                        break;
                }

            }

        }
        public static void DrawHighlight(this DrawingContext dc, RectangleF rect, bool isch, int mode, GradientColor color, GradientColor invcolor, float dx, float dy)
        {
            if (isch)
            {
                dc.Graphics.DrawBorder(rect.Expand(dx, dy), color, 0.4f);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawBorder(rect, invcolor);
                        break;
                    case 2:
                        dc.Graphics.DrawBorder(rect, invcolor);
                        break;
                }

            }

        }

        public static void DrawTapBorder(this DrawingContext dc, RectangleF rect, bool isch, int mode)
        {
            if (isch)
            {
                dc.Graphics.DrawBorder(rect, dc.ColorProfile.TapColor);
            }
            else
            {
                switch (mode)
                {
                    case 0:
                        break;
                    case 1:
                        dc.Graphics.DrawBorder(rect, dc.ColorProfile.InvTapColor);
                        break;
                    case 2:
                        dc.Graphics.DrawBorder(rect, dc.ColorProfile.TapColor);
                        break;
                }
            }
        }

        


    }

    public class SlideStepElement
    {
        public PointF Point { get; set; }
        public int Tick { get; set; }
        public float LaneIndex { get; set; }
        public float LaneWidth { get; set; }
        public float Width { get; set; }
        public int CurveType {  get; set; }
        public bool Skippable { get; set; } = false;
    }
    public class GuideStepElement
    {
        public PointF Point { get; set; }
        public int Tick { get; set; }
        public float LaneIndex { get; set; }
        public float LaneWidth { get; set; }
        public float Width { get; set; }
        public int CurveType { get; set; }
    }
}
