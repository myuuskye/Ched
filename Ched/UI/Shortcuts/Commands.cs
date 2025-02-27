﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ched.UI.Shortcuts
{
    public static class Commands
    {
        // MenuStrip
        public static string NewFile => "files.new";
        public static string OpenFile => "files.open";
        public static string Save => "files.save";
        public static string SaveAs => "files.saveAs";
        public static string ReExport => "files.reExport";
        public static string SUSExport => "files.SUSExport";
        public static string USCExport => "files.USCExport";
        public static string ShowScoreBookProperties => "editor.action.showScoreBookProperties";
        public static string ShowShortcutSettings => "application.showShortcutSettings";

        public static string Undo => "editor.action.undo";
        public static string Redo => "editor.action.redo";

        public static string Cut => "editor.action.clipboardCut";
        public static string Copy => "editor.action.clipboardCopy";
        public static string Paste => "editor.action.clipboardPaste";
        public static string PasteFlip => "editor.action.clipboardPasteFlip";

        public static string SelectAll => "editor.action.selectAll";
        public static string SelectToBegin => "editor.action.selectToBegin";
        public static string SelectToEnd => "editor.action.selectToEnd";

        public static string FlipSelectedNotes => "editor.action.flipSelectedNotes";
        public static string RemoveSelectedNotes => "editor.action.removeSelectedNotes";
        public static string ChangeChannelSelectedNotes => "editor.action.chgchSelectedNotes";

        public static string CopyEvents => "editor.action.clipboardCopyEvents";
        public static string CutEvents => "editor.action.clipboardCutEvents";
        public static string PasteEvents => "editor.action.clipboardPasteEvents";
        public static string PasteChEvents => "editor.action.clipboardPasteChEvents";
        public static string RemoveSelectedEvents => "editor.action.removeSelectedEvents";

        public static string SwitchScorePreviewMode => "editor.view.switchScorePreviewMode";

        public static string WidenLaneWidth => "editor.view.widenLaneWidth";
        public static string NarrowLaneWidth => "editor.view.narrowLaneWidth";

        public static string InsertBpmChange => "editor.action.insertBpmChange";
        public static string InsertTimeSignatureChange => "editor.action.insertTimeSignatureChange";
        public static string InsertHighSpeedChange => "editor.action.insertHighSpeedChange";
        public static string InsertComment => "editor.action.insertComment";
        public static string InsertMarker => "editor.action.insertMarker";

        public static string PlayPreview => "editor.view.playPreview";

        public static string ShowHelp => "application.showHelp";

        public static string NoteCollection => "editor.action.noteCollection";


        // ToolStrip
        public static string SelectPen => "editor.selectPen";
        public static string SelectSelection => "editor.selectSelection";
        public static string SelectEraser => "editor.selectEraser";
        public static string SelectPaint => "editor.selectPaint";
        public static string SelectProperty => "editor.selectProperty";
        public static string SelectMarker => "editor.selectMarker";
        public static string SelectStepEditor => "editor.selectStepEditor";

        public static string ZoomIn => "editor.view.zoomIn";
        public static string ZoomOut => "editor.view.zoomOut";

        public static string SelectTap => "editor.selectTap";
        public static string SelectExTap => "editor.selectExTap";
        public static string SelectHold => "editor.selectHold";
        public static string SelectSlide => "editor.selectSlide";
        public static string SelectSlideStep => "editor.selectSlideStep";
        public static string SelectAir => "editor.selectAir";
        public static string SelectAirUp => "editor.selectAirUp";
        public static string SelectAirDown => "editor.selectAirDown";
        public static string SelectAirAction => "editor.selectAirAction";
        public static string SelectFlick => "editor.selectFlick";
        public static string SelectDamage => "editor.selectDamage";
        public static string SelectStepNoteTap => "editor.selectStepNoteTap";
        public static string SelectGuide => "editor.selectGuide";
        public static string SelectGuideStep => "editor.selectGuideStep";
        public static string SelectTap2 => "editor.selectTap2";
        public static string SelectExTap2 => "editor.selectExTap2";
        public static string SelectFlick2 => "editor.selectFlick2";
        public static string SelectDamage2 => "editor.selectDamage2";
    }
}
