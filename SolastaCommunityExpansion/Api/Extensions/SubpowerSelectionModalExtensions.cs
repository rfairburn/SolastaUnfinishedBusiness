﻿using System.Collections.Generic;
using SolastaCommunityExpansion.Models;
using UnityEngine;
using UnityEngine.UI;

namespace SolastaCommunityExpansion.Api.Extensions;

internal static class SubpowerSelectionModalExtensions
{
    //Re-implements native method, but uses lust of powers instead of feature set
    public static void Bind(this SubpowerSelectionModal instance,
        List<FeatureDefinitionPower> powers,
        RulesetCharacter caster,
        SubpowerSelectionModal.SubpowerEngagedHandler subpowerEngaged,
        RectTransform attachment)
    {
        var wasActive = instance.gameObject.activeSelf;
        var mainPanel = instance.mainPanel;
        instance.gameObject.SetActive(true);
        mainPanel.gameObject.SetActive(true);
        instance.caster = caster;
        instance.containerFeature = null; //Can lead to potential crash if TA suddenly starts using this
        instance.subpowerEngaged = subpowerEngaged;
        instance.subpowerCanceled = null;
        instance.powerDefinitions.Clear();
        instance.powerDefinitions.AddRange(powers);

        while (instance.subpowersTable.childCount < instance.powerDefinitions.Count)
        {
            Gui.GetPrefabFromPool(instance.subpowerItemPrefab, instance.subpowersTable);
        }

        for (var i = 0; i < instance.subpowersTable.childCount; ++i)
        {
            var child = instance.subpowersTable.GetChild(i);
            var component = child.GetComponent<SubpowerItem>();
            if (i < powers.Count)
            {
                child.gameObject.SetActive(true);
                var i1 = i;
                component.Bind(caster, instance.powerDefinitions[i], i, index =>
                {
                    if (instance.subpowerEngaged != null)
                    {
                        var usablePower = UsablePowersProvider.Get(instance.powerDefinitions[index], instance.caster);
                        instance.subpowerEngaged(usablePower, i1);
                    }

                    instance.Hide();
                });
            }
            else
            {
                child.gameObject.SetActive(false);
                component.Unbind();
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(mainPanel.RectTransform);
        var fourCornersArray = new Vector3[4];
        attachment.GetWorldCorners(fourCornersArray);
        mainPanel.RectTransform.position =
            (0.5f * (fourCornersArray[1] + fourCornersArray[2])) + new Vector3(0.0f, 4f, 0.0f);
        instance.gameObject.SetActive(wasActive);
        mainPanel.gameObject.SetActive(wasActive);
    }
}
