using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI;
using UnityEngine;
using System.Collections;


//very special thanks to Sabakan for allowing me to study and work from his source code!
namespace KK_FixationalEyeMovementVR
{

    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess("KoikatuVR")]
    [BepInProcess("Koikatsu Party VR")]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class KK_FixationalEyeMovementVR : BaseUnityPlugin
    {
        public const string PluginName = "KK_FixationalEyeMovementVR";
        public const string GUID = "koikdaisy.kkfixationaleyemovementvr";
        public const string Version = "1.0.0";

        internal static new ManualLogSource Logger;

        private ConfigEntry<bool> _enabled;

        //Sabakan's variables
        private static ConfigEntry<bool> hiliteShakeEnabled;
        private static ConfigEntry<float> Range;
        private static ConfigEntry<float> ChangeTime;
        //end Sabakan's variables

        private void Awake()
        {

            Logger = base.Logger;

            //Sabakan's config entries
            hiliteShakeEnabled = Config.Bind<bool>("Settings", "Enable Highlight Shaking (requires scene restart)", true, "");
            Range = Config.Bind<float>("Options", "Eye Move Distance", 0.5f, new ConfigDescription("How far the eyes move around.", new AcceptableValueRange<float>(0f, 1), new object[0]));
            ChangeTime = Config.Bind<float>("Options", "Change Time", 1f, new ConfigDescription("The smaller this value, the faster the eyes move.", new AcceptableValueRange<float>(1f, 2f), new object[0]));
            //end Sabakan's config entries

            _enabled = Config.Bind("General", "Enable this plugin", true, "If false, this plugin will do nothing (requires restart).");

            if (_enabled.Value)
            {
                Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
            }
        }


        private static class Hooks
        {
            [HarmonyPostfix, HarmonyPatch(typeof(VRHScene), "MapSameObjectDisable")]
            private static void Post_VRHScene(VRHScene __instance)
            {
                ChaControl[] charas = FindObjectsOfType<ChaControl>();

                foreach (ChaControl chara in charas)
                {
                    if (chara.sex == 1)
                    {
                        FixationalEyeMovementVRController controller = chara.gameObject.AddComponent<FixationalEyeMovementVRController>();
                        controller.InitializePlugin(__instance);
                    }
                }
            }
        }

        private static readonly string leftEyePath = "/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz/cf_J_Eye_txdam_L/cf_J_Eye_tx_L/cf_J_Eye_rz_L/cf_J_hitomi_tx_L";
        private static readonly string rightEyePath = "/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz/cf_J_Eye_txdam_R/cf_J_Eye_tx_R/cf_J_Eye_rz_R/cf_J_hitomi_tx_R";
        private static readonly string leftHilightPath = "/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/ct_head/N_tonn_face/N_cf_haed/n_eyebase/n_hitomi/cf_Ohitomi_L02";
        private static readonly string rightHilightPath = "/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/ct_head/N_tonn_face/N_cf_haed/n_eyebase/n_hitomi/cf_Ohitomi_R02";

        private class FixationalEyeMovementVRController : MonoBehaviour
        {
            private GameObject leftEye;
            private GameObject rightEye;
            private GameObject leftHilite;
            private GameObject rightHilite;
            private Vector3 deltaEyeRotation;
            private Quaternion defaultLeftRotation;
            private Quaternion defaultRightRotation;

            public void InitializePlugin(VRHScene __instance)
            {
                leftEye = GameObject.Find(gameObject.name + leftEyePath);
                if (leftEye) defaultLeftRotation = leftEye.transform.localRotation;

                rightEye = GameObject.Find(gameObject.name + rightEyePath);
                if (rightEye) defaultRightRotation = rightEye.transform.localRotation;

                leftHilite = GameObject.Find(gameObject.name + leftHilightPath);
                rightHilite = GameObject.Find(gameObject.name + rightHilightPath);

                if (leftEye && rightEye && leftHilite && rightHilite) __instance.StartCoroutine(EyeShake());
            }

            private IEnumerator EyeShake()
            {
                while (true)
                {
                    yield return new WaitForSecondsRealtime(Random.Range(ChangeTime.Value * 0.1f, ChangeTime.Value * 0.8f));
                    
                    deltaEyeRotation.x = Random.Range(Range.Value * -1f, Range.Value * +1f);
                    deltaEyeRotation.y = Random.Range(Range.Value * -3.5f, Range.Value * +3.5f);

                    leftEye.transform.localRotation = defaultLeftRotation * Quaternion.Euler(deltaEyeRotation);
                    rightEye.transform.localRotation = defaultRightRotation * Quaternion.Euler(deltaEyeRotation);

                    if (hiliteShakeEnabled.Value)
                    {
                        leftHilite.GetComponent<EyeLookMaterialControll>().ChangeShaking(true);
                        rightHilite.GetComponent<EyeLookMaterialControll>().ChangeShaking(true);
                    }
                }
            }
        }
    }
}
