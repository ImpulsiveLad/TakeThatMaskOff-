using HarmonyLib;
using JetBrains.Annotations;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace TakeTheMaskOff.Patches
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemy_Patches
    {
        [HarmonyPatch(nameof(MaskedPlayerEnemy.KillEnemy))]
        [HarmonyPostfix]
        internal static void DropMaskOnDeath(MaskedPlayerEnemy __instance)
        {
            UnMaskTheDeadBase.Instance.mls.LogInfo("HarmonyPatchOpened");
            if (!NetworkManager.Singleton.IsServer) return;

            var maskToSpawn = Random.value >= 0.5f ? GetTragedyItem() : GetComedyItem();

            var spawnedMask = GameObject.Instantiate(maskToSpawn.spawnPrefab, __instance.transform.position + new Vector3(0, 2.5f, 0), Quaternion.identity);
            spawnedMask.GetComponentInChildren<GrabbableObject>().fallTime = 0f;
            spawnedMask.GetComponentInChildren<GrabbableObject>().SetScrapValue(UnMaskTheDeadBase.Instance.MaskValue.Value);
            spawnedMask.GetComponentInChildren<NetworkObject>().Spawn();
            UnMaskTheDeadBase.Instance.mls.LogInfo("Halfway there, the vars vared");
            RoundManager.Instance.SyncScrapValuesClientRpc(new NetworkObjectReference[] { spawnedMask.GetComponent<NetworkObject>() }, new int[] { spawnedMask.GetComponent<GrabbableObject>().scrapValue });
        }

        private static Item _tragedyItem;
        private static Item GetTragedyItem()
        {
            if (_tragedyItem == null)
            {
                _tragedyItem = StartOfRound.Instance.allItemsList.itemsList.First(i => i.name == "TragedyMask");
            }
            UnMaskTheDeadBase.Instance.mls.LogInfo("poggies");
            return _tragedyItem;
        }

        private static Item _comedyItem;
        private static Item GetComedyItem()
        {
            if (_comedyItem == null)
            {
                _comedyItem = StartOfRound.Instance.allItemsList.itemsList.First(i => i.name == "ComedyMask");
            }
            UnMaskTheDeadBase.Instance.mls.LogInfo("Check Check ALLLLLL Check");
            return _comedyItem;
        }
    }
}
