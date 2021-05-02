﻿using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace KarlsonMP
{
    [HarmonyPatch(typeof(Debug), "Start")]
    class Debug_Start
    {
        public static bool Prefix(Debug __instance)
        {
            __instance.GetType().GetMethod("OpenCloseFps", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, new object[] { 1 });
            __instance.GetType().GetMethod("OpenCloseSpeed", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, new object[] { 1 });
            __instance.GetType().GetMethod("FpsLimit", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, new object[] { 0 });
            return false;
        }
    }

    [HarmonyPatch(typeof(Debug), "Update")]
    class Debug_Update
    {
        public static bool Prefix(Debug __instance)
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                if (__instance.console.isActiveAndEnabled)
                    __instance.GetType().GetMethod("CloseConsole", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, Array.Empty<object>());
                else
                    __instance.GetType().GetMethod("OpenConsole", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, Array.Empty<object>());
            }
            __instance.GetType().GetMethod("Fps", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, Array.Empty<object>());
            return false;
        }
    }

    [HarmonyPatch(typeof(Debug), "Fps")]
    class Debug_Fps
    {
        public static void Postfix(Debug __instance)
        {
            if (!Main.isChatEnabled)
                return;
            string[] textSplit = __instance.fps.text.Split('\n');
            __instance.fps.text = string.Join(" | ", textSplit);
        }
    }

    [HarmonyPatch(typeof(Enemy), "LateUpdate")]
    class Enemy_LateUpdate
    {
        public static bool Prefix(Enemy __instance)
        {
            if (__instance.IsDead())
                return true;
            return __instance.GetComponent<NavMeshAgent>().enabled;
        }
    }

    [HarmonyPatch(typeof(Grappler), "Use")]
    class Grappler_Use
    {
        public static void Prefix(bool ___grappling, out bool __state)
        {
            __state = ___grappling;
        }
        public static void Postfix(bool ___grappling, Vector3 ___grapplePoint, bool __state)
        {
            if(___grappling == true && __state == false)
                ClientSend.Grapple(true, ___grapplePoint);
        }
    }
    [HarmonyPatch(typeof(Grappler), "StopUse")]
    class Grappler_StopUse
    {
        public static void Postfix()
        {
            ClientSend.Grapple(false);
        }
    }

    [HarmonyPatch(typeof(DetectWeapons), "Pickup")]
    class DetectWeapons_Pickup
    {
        public static void Postfix()
        {
            ClientSend.ChangeGun();
        }
    }

    [HarmonyPatch(typeof(DetectWeapons), "Throw")]
    class DetectWeapons_Throw
    {
        public static void Postfix()
        {
            ClientSend.ChangeGun(true);
        }
    }

}
