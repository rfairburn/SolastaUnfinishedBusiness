using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

public static class GameManagerPatcher
{
    private static IEnumerable<WorldLocationCharacter> WorldLocationCharacters => Gui.GameCampaign.Party.CharactersList
        .Select(x => GameLocationCharacter.GetFromActor(x.RulesetCharacter))
        .Select(x => ServiceRepository.GetService<IWorldLocationEntityFactoryService>()
            .TryFindWorldCharacter(x, out var worldLocationCharacter)
            ? worldLocationCharacter
            : null);

    [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class BindPostDatabase_Patch
    {
        public static void Postfix()
        {
            //PATCH: loads all mod contexts
            BootContext.Startup();
        }
    }

    [HarmonyPatch(typeof(GameManager), "BindServiceSettings")]
    internal static class BindServiceSettings_Patch
    {
        public static void Prefix(GameManager __instance)
        {
            //PATCH: add unofficial languages before game tries to load the game settings xml
            var languageByCode = __instance.languageByCode;

            if (languageByCode == null)
            {
                return;
            }

            foreach (var language in TranslatorContext.Languages
                         .Where(language => !languageByCode.ContainsKey(language.Code)))
            {
                languageByCode.Add(language.Code, language.Text);
            }
        }
    }

    [HarmonyPatch(typeof(NarrativeStateTeleportCharacterBase), "Begin")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class Begin_Patch
    {
        public static bool Prefix(NarrativeStateTeleportCharacterBase __instance, params object[] parameters)
        {
            var roles = new[] { "GpPlayer5", "GpPlayer6" };
            var i = 0;
            var stackedMember = false;
            var flag = false;

            foreach (var teleportDescription in __instance.NarrativeStateDescription.CharacterTeleportDescriptions)
            {
                if (teleportDescription.PlayerMarkerIndex >= 0)
                {
                    if (__instance.PlayerRolesMap.ContainsKey(teleportDescription.Role) &&
                        !__instance.NarrativeSequence.PlayerPlacementMarkers.Empty() &&
                        teleportDescription.PlayerMarkerIndex <
                        __instance.NarrativeSequence.PlayerPlacementMarkers.Length)
                    {
                        __instance.TeleportCharacterImpl(__instance.PlayerRolesMap[teleportDescription.Role],
                            __instance.NarrativeSequence.PlayerPlacementMarkers[teleportDescription.PlayerMarkerIndex],
                            !flag, out stackedMember);
                        flag |= stackedMember;
                    }
                }
                else if (teleportDescription.NpcMarkerIndex >= 0 &&
                         __instance.NpcRolesMap.ContainsKey(teleportDescription.Role) &&
                         !__instance.NarrativeSequence.NpcPlacementMarkers.Empty() &&
                         teleportDescription.NpcMarkerIndex < __instance.NarrativeSequence.NpcPlacementMarkers.Length)
                {
                    __instance.TeleportCharacterImpl(__instance.NpcRolesMap[teleportDescription.Role],
                        __instance.NarrativeSequence.NpcPlacementMarkers[teleportDescription.NpcMarkerIndex], !flag,
                        out stackedMember);
                    flag |= stackedMember;
                }
            }

            var rulesetCharacters =
                __instance.PlayerRolesMap.Values.Select(x => x.GameLocationCharacter.RulesetCharacter);

            foreach (var rulesetCharacter in Gui.GameCampaign.Party.CharactersList
                         .Where(x => !rulesetCharacters.Contains(x.RulesetCharacter))
                         .Select(x => x.RulesetCharacter))
            {
                var gameLocationCharacter = GameLocationCharacter.GetFromActor(rulesetCharacter);

                if (ServiceRepository.GetService<IWorldLocationEntityFactoryService>()
                    .TryFindWorldCharacter(gameLocationCharacter, out var worldLocationCharacter))
                {
                    var role = roles[i++ % 2];

                    __instance.PlayerRolesMap.Add(role, worldLocationCharacter);
                    __instance.TeleportCharacterImpl(__instance.PlayerRolesMap[role],
                        __instance.NarrativeSequence.PlayerPlacementMarkers[0], !flag, out stackedMember);
                }

                flag |= stackedMember;
            }

            if (flag)
            {
                ServiceRepository.GetService<IGameLocationCharacterService>().UnstackPartyMembers();
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(NarrativeSequence), "PlayerActors", MethodType.Getter)]
    internal static class PlayerActors_Getter_Patch
    {
        public static void Postfix(List<WorldLocationCharacter> __result)
        {
            if (__result.Count != 4)
            {
                return;
            }

            var worldLocationCharacters = WorldLocationCharacters.Except(__result);

            __result.AddRange(worldLocationCharacters);
        }
    }

    [HarmonyPatch(typeof(NarrativeSequence), "PlayerPositionsPostSequence", MethodType.Getter)]
    internal static class PlayerPositionsPostSequence_Getter_Patch
    {
        public static void Postfix(List<WorldNode> __result)
        {
            if (__result.Count != 4)
            {
                return;
            }

            var i = 2;
            var partyCount = Gui.GameCampaign.Party.CharactersList.Count;

            while (__result.Count < partyCount)
            {
                __result.Add(__result[i++ % 4]);
            }
        }
    }

    [HarmonyPatch(typeof(NarrativeSequence), "PlayerPositionsPostSequenceB", MethodType.Getter)]
    internal static class PlayerPositionsPostSequenceB_Getter_Patch
    {
        public static void Postfix(List<WorldNode> __result)
        {
            if (__result.Count != 4)
            {
                return;
            }

            var i = 2;
            var partyCount = Gui.GameCampaign.Party.CharactersList.Count;

            while (__result.Count < partyCount)
            {
                __result.Add(__result[i++ % 4]);
            }
        }
    }

    [HarmonyPatch(typeof(NarrativeSequence), "PlayerPositionsPostSequenceC", MethodType.Getter)]
    internal static class PlayerPositionsPostSequenceC_Getter_Patch
    {
        public static void Postfix(List<WorldNode> __result)
        {
            if (__result.Count != 4)
            {
                return;
            }

            var i = 2;
            var partyCount = Gui.GameCampaign.Party.CharactersList.Count;

            while (__result.Count < partyCount)
            {
                __result.Add(__result[i++ % 4]);
            }
        }
    }

    [HarmonyPatch(typeof(NarrativeSequence), "PlayerPlacementMarkers", MethodType.Getter)]
    internal static class PlayerPlacementMarkers_Getter_Patch
    {
        public static void Postfix(ref Transform[] __result)
        {
            if (__result.Length != 4)
            {
                return;
            }

            var partyCount = Gui.GameCampaign.Party.CharactersList.Count;
            var result = new Transform[partyCount];

            Array.Copy(__result, result, 4);

            result[4] = __result[2];
            result[5] = __result[3];

            __result = result;
        }
    }

    [HarmonyPatch(typeof(NarrativeSequence), "PlayerMovementMarkers", MethodType.Getter)]
    internal static class PlayerMovementMarkers_Getter_Patch
    {
        public static void Postfix(ref Transform[] __result)
        {
            if (__result.Length != 4)
            {
                return;
            }

            var partyCount = Gui.GameCampaign.Party.CharactersList.Count;
            var result = new Transform[partyCount];

            Array.Copy(__result, result, 4);

            result[4] = __result[2];
            result[5] = __result[3];

            __result = result;
        }
    }

    [HarmonyPatch(typeof(NarrativeSequence), "PlayerRolesMap", MethodType.Getter)]
    internal static class PlayerRolesMap_Getter_Patch
    {
        public static void Postfix(Dictionary<string, WorldLocationCharacter> __result)
        {
            if (__result.Count != 4)
            {
                return;
            }

            var roles = new[] { "GpPlayer5", "GpPlayer6" };
            var i = 0;
            var missingCharacters = WorldLocationCharacters.Except(__result.Values);

            foreach (var missingCharacter in missingCharacters)
            {
                __result.Add(roles[i++ % 2], missingCharacter);
            }
        }
    }
}
