﻿using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace KarlsonMPclient
{
    class HarmonyHooks
    {
        public static bool Enemy_LateUpdate(Enemy __instance)
        {
            if (__instance.IsDead())
                return true;
            return __instance.GetComponent<NavMeshAgent>().enabled; // LateUpdate cancels animations
        }
    }
}
