using UnityEditor;
using UnityEngine;
using TMPro.EditorUtilities;

// Custom material inspector for the "TextMeshPro/Distance Field Noise" shader.
// Reuses the stock TMP SDF GUI and appends the extra outline-noise sliders that the
// built-in TMP_SDFShaderGUI has no knowledge of, so they become tweakable in the inspector.
public class TMP_SDFOutlineNoiseShaderGUI : TMP_SDFShaderGUI
{
    protected override void DoGUI()
    {
        base.DoGUI();

        if (!m_Material.HasProperty("_OutlineNoiseAmount") && !m_Material.HasProperty("_OutlineNoiseScale"))
            return;

        s_OutlineNoise = BeginPanel("Outline Noise", s_OutlineNoise);
        if (s_OutlineNoise)
        {
            EditorGUI.indentLevel += 1;

            if (m_Material.HasProperty("_OutlineNoiseAmount"))
                DoSlider("_OutlineNoiseAmount", "Amount");

            if (m_Material.HasProperty("_OutlineNoiseScale"))
                DoSlider("_OutlineNoiseScale", "Scale");

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        EndPanel();
    }

    static bool s_OutlineNoise = true;
}
