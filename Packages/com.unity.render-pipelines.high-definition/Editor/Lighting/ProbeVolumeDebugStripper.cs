using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UnityEditor.Rendering.HighDefinition
{
    // Strip PV debug information from build if runtime debug display is disabled in global settings.
    class ProbeVolumeDebugStripper : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        private bool m_StripDebugVariants = false;

        public ProbeVolumeDebugStripper()
        {
            var globalSettings = HDRenderPipelineGlobalSettings.Ensure();

            if (globalSettings == null)
            {
                Debug.LogWarning($"Probe Volume data will not be stripped because {nameof(HDRenderPipelineGlobalSettings)} are missing");
                return;
            }

            m_StripDebugVariants = globalSettings.stripDebugVariants;
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null || !m_StripDebugVariants)
            {
                // Don't want to strip anything when entering editor playmode or if stripping is disabled
                return;
            }

            Debug.Log($"Stripping debug data from Probe Volume in scene '{scene.name}' because Strip Debug Variants is enabled in the Global Settings");

            var probeVolumePerSceneDatas = scene
                .GetRootGameObjects()
                .Select(go => go.GetComponent<ProbeVolumePerSceneData>())
                .Where(c => c);

            foreach (var probeVolumePerSceneData in probeVolumePerSceneDatas)
            {
                // TODO: Arguably we should just destroy the entire component if 'supportProbeVolumes' is disabled. Otherwise previously baked data
                //       will still be bundled with players and loaded with the scene even though the entire feature is disabled. Discuss this
                //       before enabling it, though.
#if false
                if (!settings.supportProbeVolumes)
                {
                    Debug.Log($"Stripping embedded Probe Volume data from scene '{scene.name}'.");
                    Object.DestroyImmediate(probeVolumePerSceneData);
                }
                else
#endif
                probeVolumePerSceneData.StripSupportData();
            }
        }
    }
}
