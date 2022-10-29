﻿using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Models;

internal static class Global
{
    // true if in a multiplayer game
    internal static bool IsMultiplayer => ServiceRepository.GetService<INetworkingService>().IsMultiplayerGame;

    // level up hero
    [CanBeNull]
    internal static RulesetCharacterHero LevelUpHero =>
        ServiceRepository.GetService<ICharacterBuildingService>()?.CurrentLocalHeroCharacter;

    // last level up hero name
    internal static string LastLevelUpHeroName { get; set; }

    // inspected hero on both location and pool
    [CanBeNull] internal static RulesetCharacterHero InspectedHero { get; set; }

    // active player character
    internal static GameLocationCharacter ActionCharacter { get; private set; }

    private static GameLocationCharacter ControlledLocationCharacter
    {
        get
        {
            var exploration = Gui.GuiService.GetScreen<GameLocationScreenExploration>();

            if (exploration.Visible)
            {
                return exploration.CharacterControlPanel.GuiCharacter?.GameLocationCharacter;
            }

            var battle = Gui.GuiService.GetScreen<GameLocationScreenBattle>();

            return battle.Visible
                ? battle.CharacterControlPanel.GuiCharacter?.GameLocationCharacter
                : null;
        }
    }

    internal static RulesetCharacter CurrentCharacter => InspectedHero
                                                         ?? LevelUpHero
                                                         ?? ControlledLocationCharacter?.RulesetCharacter
                                                         ?? ActionCharacter?.RulesetCharacter;

    // current action from any character on the map
    internal static CharacterAction CurrentAction { get; private set; }

    // casted spell
    internal static SpellDefinition CastedSpell { get; private set; }

    // last attack was a critical hit
    internal static bool CriticalHit { get; set; }

    // conditions that should display on char panel even if set to silent
    internal static HashSet<ConditionDefinition> CharacterLabelEnabledConditions { get; } = new();

    // special case for our powers that add a turn off stop provider
    internal static HashSet<FeatureDefinitionPower> PowersThatIgnoreInterruptions { get; } = new();

    // restate globals on every new action
    internal static void ActionStarted([NotNull] CharacterAction characterAction)
    {
        CurrentAction = characterAction;
        ActionCharacter = characterAction.ActingCharacter;
        CastedSpell = null;

        Main.Logger.Log($"{ActionCharacter?.Name} -> {CurrentAction.ActionDefinition.Name}");

        switch (characterAction)
        {
            case CharacterActionCastSpell actionCastSpell:
                CastedSpell = actionCastSpell.ActiveSpell.SpellDefinition;

                // Hold the state of the SHIFT key on BOOL PARAM 5. Used to determine which slot to use on MC Warlock
                var isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                characterAction.actionParams.BoolParameter5 = isShiftPressed;
                break;

            case CharacterActionReady actionReady:
                CustomReactionsContext.ReadReadyActionPreferredCantrip(actionReady.actionParams);
                break;

            case CharacterActionSpendPower spendPower:
                PowersBundleContext.SpendBundledPowerIfNeeded(spendPower);
                break;
        }
    }

    // internal static bool ActiveLevelUpHeroHasCantrip(SpellDefinition spellDefinition)
    // {
    //     var hero = ActiveLevelUpHero;
    //
    //     if (hero == null)
    //     {
    //         return true;
    //     }
    //
    //     return hero.SpellRepertoires.Any(x => x.KnownCantrips.Contains(spellDefinition))
    //            || hero.GetHeroBuildingData().AcquiredCantrips.Any(e => e.Value.Contains(spellDefinition));
    // }

    // internal static bool ActiveLevelUpHeroHasSubclass(string subclass)
    // {
    //     var hero = ActiveLevelUpHero;
    //
    //     return hero == null || hero.ClassesAndSubclasses.Any(e => e.Value.Name == subclass);
    // }

    // internal static bool ActiveLevelUpHeroHasFeature(FeatureDefinition feature, bool recursive = true)
    // {
    //     var hero = ActiveLevelUpHero;
    //
    //     if (feature is FeatureDefinitionFeatureSet set)
    //     {
    //         return hero != null && hero.HasAllFeatures(set.FeatureSet);
    //     }
    //
    //     return hero == null || recursive
    //         ? hero.HasAnyFeature(feature)
    //         : hero.ActiveFeatures
    //               .SelectMany(x => x.Value)
    //               .Any(x => x == feature)
    //           || hero.GetHeroBuildingData().AllActiveFeatures.Contains(feature);
    // }
}
