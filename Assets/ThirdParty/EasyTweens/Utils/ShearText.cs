using TMPro;

using UnityEngine;

namespace EasyTweens
{
    public class ShearText : TextMeshProUGUI
    {
        public Vector2 shear = Vector2.zero;
        public Vector2 shearPivot = Vector2.zero;

        public Vector2 TextSize()
        {
            
            Vector2 size = Vector2.zero;

            if (m_textInfo.characterInfo != null && m_textInfo.characterCount > 0)
            {
                var tmpCharacterInfo = m_textInfo.characterInfo[0];

                size.x = tmpCharacterInfo.vertex_TR.position.x - tmpCharacterInfo.vertex_TL.position.x;
                size.y = tmpCharacterInfo.vertex_BL.position.y - tmpCharacterInfo.vertex_TL.position.y;
            }

            return size;
        }
#if UNITY_2023_1_OR_NEWER
        protected override void FillCharacterVertexBuffers(int i)
        {
            base.FillCharacterVertexBuffers(i);
            
            int materialIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
            int index_X4 = m_textInfo.meshInfo[materialIndex].vertexCount - 4;
            
        #else
        
        protected override void FillCharacterVertexBuffers(int i, int index_X4)
        {
            base.FillCharacterVertexBuffers(i, index_X4);
            
            int materialIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
            index_X4 = m_textInfo.meshInfo[materialIndex].vertexCount - 4;
#endif
            TMP_CharacterInfo[] characterInfoArray = m_textInfo.characterInfo;
            
            float height = characterInfoArray[i].vertex_TL.position.y - characterInfoArray[i].vertex_BL.position.y;
            float width = characterInfoArray[i].vertex_TR.position.x - characterInfoArray[i].vertex_TL.position.x;
            
            Vector2 absoluteShearPivot = new Vector2(characterInfoArray[i].vertex_BL.position.x + shearPivot.x * width, characterInfoArray[i].vertex_BL.position.y + shearPivot.y * height);
            
            // Setup Vertices for Characters
            m_textInfo.meshInfo[materialIndex].vertices[0 + index_X4] =
                characterInfoArray[i].vertex_BL.position + 
                new Vector3(shear.x * (characterInfoArray[i].vertex_BL.position.y - absoluteShearPivot.y)/height, 
                    shear.y * (characterInfoArray[i].vertex_BL.position.x - absoluteShearPivot.x) / width, 
                    0);
            
            m_textInfo.meshInfo[materialIndex].vertices[1 + index_X4] = 
                characterInfoArray[i].vertex_TL.position + 
                new Vector3(shear.x * (characterInfoArray[i].vertex_TL.position.y - absoluteShearPivot.y)/height,
                    shear.y * (characterInfoArray[i].vertex_TL.position.x - absoluteShearPivot.x) / width,
                    0);
            
            m_textInfo.meshInfo[materialIndex].vertices[2 + index_X4] =
                characterInfoArray[i].vertex_TR.position + 
                new Vector3(shear.x * (characterInfoArray[i].vertex_TR.position.y - absoluteShearPivot.y)/height,
                    shear.y * (characterInfoArray[i].vertex_TR.position.x - absoluteShearPivot.x) / width,
                    0);
            
            m_textInfo.meshInfo[materialIndex].vertices[3 + index_X4] = 
                characterInfoArray[i].vertex_BR.position + 
                new Vector3(shear.x * (characterInfoArray[i].vertex_BR.position.y - absoluteShearPivot.y)/height,
                    shear.y * (characterInfoArray[i].vertex_BR.position.x - absoluteShearPivot.x) / width, 
                    0);
        }
    }
}