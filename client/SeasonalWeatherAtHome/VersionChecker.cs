﻿using System;
using System.Diagnostics;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;

namespace SeasonalWeatherAtHome;

[AttributeUsage(AttributeTargets.Assembly)]
public abstract class VersionChecker : Attribute
{
    /// <summary>
    /// Check the currently running program's version against the plugin assembly VersionChecker attribute, and
    /// return false if they do not match. 
    /// Optionally add a fake setting to the F12 menu if Config is passed in
    /// </summary>
    /// <param name="config">The plugin's ConfigFile, if provided, a custom message will be added to the F12 menu</param>
    /// <returns></returns>
    public static bool CheckEftVersion(ConfigFile? config = null)
    {
        var currentVersion = FileVersionInfo.GetVersionInfo(BepInEx.Paths.ExecutablePath).FilePrivatePart;
        const int buildVersion = Plugin.TarkovVersion;
        if (currentVersion == buildVersion) return true;
        var errorMessage = $"ERROR: This version of Use Items Anywhere was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.";
        Plugin.Log?.LogError(errorMessage);
        Chainloader.DependencyErrors.Add(errorMessage);

        // TypeofThis results in a bogus config entry in the BepInEx config file for the plugin, but it shouldn't hurt anything
        // We leave the "section" parameter empty so there's no section header drawn
        config?.Bind("", "TarkovVersion", "", new ConfigDescription(
            errorMessage, null, new ConfigurationManagerAttributes
            {
                CustomDrawer = ErrorLabelDrawer,
                ReadOnly = true,
                HideDefaultButton = true,
                HideSettingName = true,
                Category = null
            }
        ));

        return false;

    }

    private static void ErrorLabelDrawer(ConfigEntryBase entry)
    {
        var styleNormal = new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            stretchWidth = true
        };

        var styleError = new GUIStyle(GUI.skin.label)
        {
            stretchWidth = true,
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.red
            },
            fontStyle = FontStyle.Bold
        };

        // General notice that we're the wrong version
        GUILayout.BeginVertical();
        GUILayout.Label(entry.Description.Description, styleNormal, [GUILayout.ExpandWidth(true)]);

        // Centered red disabled text
        GUILayout.Label("Plugin has been disabled!", styleError, [GUILayout.ExpandWidth(true)]);
        GUILayout.EndVertical();
    }

#pragma warning disable 0169, 0414, 0649
    internal sealed class ConfigurationManagerAttributes
    {
        public bool? ShowRangeAsPercent;
        public Action<ConfigEntryBase>? CustomDrawer;
        public CustomHotkeyDrawerFunc? CustomHotkeyDrawer;
        public delegate void CustomHotkeyDrawerFunc(ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);
        public bool? Browsable;
        public string? Category;
        public object? DefaultValue;
        public bool? HideDefaultButton;
        public bool? HideSettingName;
        public string? Description;
        public string? DispName;
        public int? Order;
        public bool? ReadOnly;
        public bool? IsAdvanced;
        public Func<object, string>? ObjToStr;
        public Func<string, object>? StrToObj;
    }
}