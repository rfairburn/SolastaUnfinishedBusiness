﻿using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

public static class VictoryModalPatcher
{
    [HarmonyPatch(typeof(VictoryModal), "OnBeginShow")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class OnBeginShow_Patch
    {
        public static void Prefix([NotNull] VictoryModal __instance)
        {
            //PATCH: scales down the victory modal whenever the party size is bigger than 4 (PARTYSIZE)
            var partyCount = Gui.GameCampaign.Party.CharactersList.Count;

            if (partyCount > ToolsContext.GamePartySize)
            {
                var scale = (float)Math.Pow(ToolsContext.CustomScale, partyCount - ToolsContext.GamePartySize);

                __instance.heroStatsGroup.localScale = new Vector3(scale, 1, scale);
            }
            else
            {
                __instance.heroStatsGroup.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
