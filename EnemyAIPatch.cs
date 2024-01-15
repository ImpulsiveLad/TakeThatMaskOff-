using HarmonyLib;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using GameNetcodeStuff;


namespace TakeTheMaskOff.Patches
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemy_Patches
    {
        private static IEnumerator AnimateMaskFall(GameObject mask, Vector3 startPosition, Vector3 endPosition, float duration)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                mask.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            mask.transform.position = endPosition;
        }

        [HarmonyPatch(nameof(MaskedPlayerEnemy.KillEnemy))]
        [HarmonyPostfix]
        internal static void DropMaskOnDeath(MaskedPlayerEnemy __instance)
        {
            UnMaskTheDeadBase.Instance.mls.LogInfo("HarmonyPatchOpened");

            if (__instance.mimickingPlayer != null && __instance.mimickingPlayer.deadBody != null && __instance.mimickingPlayer.isPlayerDead)
            {
                // Disable enemy model if this was a corrupted player
                __instance.gameObject.SetActive(false);

                // Reenable player ragdoll
                var deadBody = __instance.mimickingPlayer.deadBody;
                deadBody.gameObject.SetActive(true);
                deadBody.SetBodyPartsKinematic(false);
                deadBody.SetRagdollPositionSafely(__instance.transform.position);
                deadBody.deactivated = false;

                // Hide mask on player ragdoll
                deadBody.transform.Find("spine.001/spine.002/spine.003/spine.004/HeadMask").gameObject.SetActive(false);
            }
            var maskToSpawn = __instance.maskTypes[0].activeSelf ? ComedyItem : TragedyItem;
            __instance.gameObject.transform.Find("ScavengerModel/metarig/spine/spine.001/spine.002/spine.003/spine.004/HeadMaskComedy").gameObject.SetActive(false);
            __instance.gameObject.transform.Find("ScavengerModel/metarig/spine/spine.001/spine.002/spine.003/spine.004/HeadMaskTragedy").gameObject.SetActive(false);

            if (!NetworkManager.Singleton.IsServer) return;

            var startPosition = __instance.transform.position + new Vector3(0, 2.5f, 0);
            var endPosition = startPosition + new Vector3(0, -2.5f, 0); // adjust as needed

            var spawnedMask = GameObject.Instantiate(maskToSpawn.spawnPrefab, startPosition, Quaternion.identity);
            __instance.StartCoroutine(AnimateMaskFall(spawnedMask, startPosition, endPosition, 0.5f)); // adjust duration as needed

            spawnedMask.GetComponentInChildren<GrabbableObject>().fallTime = 0f;
            spawnedMask.GetComponentInChildren<GrabbableObject>().SetScrapValue(UnMaskTheDeadBase.Instance.MaskValue.Value);
            spawnedMask.GetComponentInChildren<NetworkObject>().Spawn();
            UnMaskTheDeadBase.Instance.mls.LogInfo("Halfway there, the vars vared");
            RoundManager.Instance.SyncScrapValuesClientRpc(new NetworkObjectReference[] { spawnedMask.GetComponent<NetworkObject>() }, new int[] { spawnedMask.GetComponent<GrabbableObject>().scrapValue });
        }

        private static Item _tragedyItem;
        private static Item TragedyItem
        {
            get
            {
                if (_tragedyItem == null)
                {
                    _tragedyItem = StartOfRound.Instance.allItemsList.itemsList.First(i => i.name == "TragedyMask");
                }
                return _tragedyItem;
            }
        }

        private static Item _comedyItem;
        private static Item ComedyItem
        {
            get
            {
                if (_comedyItem == null)
                {
                    _comedyItem = StartOfRound.Instance.allItemsList.itemsList.First(i => i.name == "ComedyMask");
                }
                return _comedyItem;
            }
        }
    }
}
