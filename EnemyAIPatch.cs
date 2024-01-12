using HarmonyLib;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System.Collections;


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
            if (!NetworkManager.Singleton.IsServer) return;

            __instance.gameObject.transform.Find("ScavengerModel/metarig/spine/spine.001/spine.002/spine.003/spine.004/HeadMaskComedy").gameObject.SetActive(false);
            __instance.gameObject.transform.Find("ScavengerModel/metarig/spine/spine.001/spine.002/spine.003/spine.004/HeadMaskTragedy").gameObject.SetActive(false);

            var maskToSpawn = Random.value >= 0.5f ? GetTragedyItem() : GetComedyItem();
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
