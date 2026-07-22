using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace EasyTweens
{

    [Serializable, TweenCategoryOverride("TMP")]
    public class CharactersSequentialScaleTween : FloatTween<TMP_Text>
    {
        [ExposeInEditor]
        public float perCharacterAnimationOverlap = 0.5f;
        protected override float Property { get; set; }

        public override void UpdateTween(float time, float deltaTime)
        {
            if (time <= TotalDelay && time - deltaTime >= TotalDelay)
            {
                SetFactor(0);
            }

            if (time >= TotalDelay + duration && time - deltaTime <= TotalDelay + duration)
            {
                SetFactor(1);
            }
            else if (time > TotalDelay && time < TotalDelay + duration)
            {
                if (duration == 0)
                {
                    SetFactor(deltaTime >= 0 ? 1 : 0);
                }
                else
                {
                    SetFactor((time - TotalDelay) / duration);
                }
            }
        }

        public override void SetFactor(float factor)
        {
            Property = factor;

            if (target.textInfo == null)
            {
                return;
            }
            int textInfoCharacterCount = target.textInfo.characterCount;

            var perCharacterFactor = 1f / textInfoCharacterCount;
            float minBound = -perCharacterFactor * perCharacterAnimationOverlap;
            float maxBound = 1;


            target.ForceMeshUpdate();
            // target.maxVisibleCharacters = Mathf.RoundToInt(Property * textInfoCharacterCount);
            for (var i = 0; i < target.textInfo.characterInfo.Length; i++)
            {
                float zeroPos = i * perCharacterFactor - perCharacterFactor * perCharacterAnimationOverlap;
                float onePos = (i + 1) * perCharacterFactor;
                var remapProperty = Mathf.Lerp(minBound, maxBound, Property);
                float currentCharFactor = Mathf.InverseLerp(zeroPos, onePos, remapProperty);

                var info = target.textInfo.characterInfo[i];
                var vertexIndex = info.vertexIndex;
                int materialIndex = target.textInfo.characterInfo[i].materialReferenceIndex;
                Vector3[] vertices = target.textInfo.meshInfo[materialIndex].vertices;

                Vector3 offsetToMidBaseline = new Vector3(
                    (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, (vertices[vertexIndex + 0].y + vertices[vertexIndex + 2].y) / 2, 0);

                // Apply offset to adjust our pivot point.
                vertices[vertexIndex + 0] += -offsetToMidBaseline;
                vertices[vertexIndex + 1] += -offsetToMidBaseline;
                vertices[vertexIndex + 2] += -offsetToMidBaseline;
                vertices[vertexIndex + 3] += -offsetToMidBaseline;

                vertices[vertexIndex + 0] *= EvaluateCurve(currentCharFactor);
                vertices[vertexIndex + 1] *= EvaluateCurve(currentCharFactor);
                vertices[vertexIndex + 2] *= EvaluateCurve(currentCharFactor);
                vertices[vertexIndex + 3] *= EvaluateCurve(currentCharFactor);

                vertices[vertexIndex + 0] += offsetToMidBaseline;
                vertices[vertexIndex + 1] += offsetToMidBaseline;
                vertices[vertexIndex + 2] += offsetToMidBaseline;
                vertices[vertexIndex + 3] += offsetToMidBaseline;
            }

            target.UpdateVertexData();
#if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);
            }
#endif
        }
    }
}