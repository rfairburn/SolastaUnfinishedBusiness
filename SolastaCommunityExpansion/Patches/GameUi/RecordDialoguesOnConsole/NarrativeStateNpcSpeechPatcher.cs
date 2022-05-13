﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using I2.Loc;
using ModKit.Utility;

namespace SolastaCommunityExpansion.Patches.GameUi.RecordDialoguesOnConsole
{
    [HarmonyPatch(typeof(NarrativeStateNpcSpeech), "RecordSpeechLine")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class NarrativeStateNpcSpeech_RecordSpeechLine_Getter
    {
        internal static void Postfix(string speakerName, string textLine)
        {
            if (!Main.Settings.EnableLogDialoguesToConsole || LocalizationManager.CurrentLanguageCode == "de")
            {
                return;
            }

            var screen = Gui.GuiService.GetScreen<GuiConsoleScreen>();

            screen.Game.GameConsole.LogSimpleLine($"{speakerName.White().Bold()}: {textLine}");
        }
    }
}
