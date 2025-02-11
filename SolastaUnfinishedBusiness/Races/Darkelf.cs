﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using TA;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterRaceDefinitions;

namespace SolastaUnfinishedBusiness.Races;

internal static class DarkelfSubraceBuilder
{
    internal static readonly FeatureDefinitionPower PowerDarkelfFaerieFire = FeatureDefinitionPowerBuilder
        .Create("PowerDarkelfFaerieFire")
        .SetGuiPresentation(Category.Feature, SpellDefinitions.FaerieFire)
        .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
        .SetEffectDescription(EffectDescriptionBuilder
            .Create(SpellDefinitions.FaerieFire.EffectDescription)
            .SetSavingThrowData(
                false,
                AttributeDefinitions.Dexterity,
                false,
                EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                AttributeDefinitions.Charisma,
                8)
            .Build())
        .AddToDB();

    internal static readonly FeatureDefinitionPower PowerDarkelfDarkness = FeatureDefinitionPowerBuilder
        .Create("PowerDarkelfDarkness")
        .SetGuiPresentation(Category.Feature, SpellDefinitions.Darkness)
        .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
        .SetEffectDescription(SpellDefinitions.Darkness.EffectDescription)
        .AddToDB();

    internal static CharacterRaceDefinition SubraceDarkelf { get; } = BuildDarkelf();

    internal static FeatureDefinitionCastSpell CastSpellDarkelfMagic { get; private set; }

    [NotNull]
    private static CharacterRaceDefinition BuildDarkelf()
    {
        CastSpellDarkelfMagic = FeatureDefinitionCastSpellBuilder
            .Create(FeatureDefinitionCastSpells.CastSpellElfHigh, "CastSpellDarkelfMagic")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetSpellList(SpellListDefinitionBuilder
                .Create(SpellListDefinitions.SpellListWizard, "SpellListDarkelf")
                .SetGuiPresentationNoContent()
                .SetSpellsAtLevel(0, SpellDefinitions.DancingLights)
                .FinalizeSpells()
                .AddToDB())
            .SetSpellCastingAbility(AttributeDefinitions.Charisma)
            .AddToDB();

        var darkelfSpriteReference = Sprites.GetSprite("Darkelf", Resources.Darkelf, 1024, 512);

        var attributeModifierDarkelfCharismaAbilityScoreIncrease = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierDarkelfCharismaAbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Charisma, 1)
            .AddToDB();

        var lightAffinityDarkelfLightSensitivity = FeatureDefinitionLightAffinityBuilder
            .Create("LightAffinityDarkelfLightSensitivity")
            .SetGuiPresentation(CustomConditionsContext.LightSensitivity.Name, Category.Condition)
            .AddLightingEffectAndCondition(
                new FeatureDefinitionLightAffinity.LightingEffectAndCondition
                {
                    lightingState = LocationDefinitions.LightingState.Bright,
                    condition = CustomConditionsContext.LightSensitivity
                })
            .AddToDB();

        var proficiencyDarkelfWeaponTraining = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyDarkelfWeaponTraining")
            .SetGuiPresentation(Category.Feature)
            .SetProficiencies(
                ProficiencyType.Weapon,
                CustomWeaponsContext.HandXbowWeaponType.Name,
                WeaponTypeDefinitions.RapierType.Name,
                WeaponTypeDefinitions.ShortswordType.Name)
            .AddToDB();

        var darkelfRacePresentation = Elf.RacePresentation.DeepCopy();

        darkelfRacePresentation.femaleNameOptions = ElfHigh.RacePresentation.FemaleNameOptions;
        darkelfRacePresentation.maleNameOptions = ElfHigh.RacePresentation.MaleNameOptions;
        darkelfRacePresentation.preferedSkinColors = new RangedInt(48, 53);
        darkelfRacePresentation.preferedHairColors = new RangedInt(48, 53);

        darkelfRacePresentation.surNameOptions = new List<string>();

        for (var i = 1; i <= 5; i++)
        {
            darkelfRacePresentation.surNameOptions.Add($"Race/&DarkelfSurName{i}Title");
        }

        var raceDarkelf = CharacterRaceDefinitionBuilder
            .Create(ElfHigh, "RaceDarkelf")
            .SetGuiPresentation(Category.Race, darkelfSpriteReference)
            .SetRacePresentation(darkelfRacePresentation)
            .SetFeaturesAtLevel(1,
                FeatureDefinitionMoveModes.MoveModeMove6,
                FeatureDefinitionSenses.SenseSuperiorDarkvision,
                FeatureDefinitionFeatureSets.FeatureSetElfHighLanguages,
                attributeModifierDarkelfCharismaAbilityScoreIncrease,
                proficiencyDarkelfWeaponTraining,
                CastSpellDarkelfMagic,
                lightAffinityDarkelfLightSensitivity)
            .AddFeaturesAtLevel(3,
                PowerDarkelfFaerieFire)
            .AddFeaturesAtLevel(5,
                PowerDarkelfDarkness)
            .AddToDB();

        raceDarkelf.subRaces.Clear();
        Elf.SubRaces.Add(raceDarkelf);

        return raceDarkelf;
    }
}
