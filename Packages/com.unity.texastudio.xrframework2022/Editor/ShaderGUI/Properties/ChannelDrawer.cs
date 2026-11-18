
using UnityEditor;
using UnityEngine;

namespace IKASAMADUS
{
     public class ChannelDrawer:MaterialPropertyDrawer
    {
        
        private static readonly GUIContent[] ChannelLabels = new GUIContent[]
        {
            new ("R"),
            new ("G"),
            new ("B"),
            new ("A"),
            new ("RGB"),
            new ("RGBA"),
            new ("RGB Average"),
            new ("RGB Luminance"),
            new ("None"),
        };
        private static readonly int[] ChannelIndex = new int[]
        {
            0,1,2,3,4,5,6,7,8
        };
        private static readonly Vector4[] ChannelVectors = new Vector4[]
        {
            new (1,0,0,0),
            new (0,1,0,0),
            new (0,0,1,0),
            new (0,0,0,1),
            new (1,1,1,0),
            new (1,1,1,1),
            new (0.3333333f,0.3333333f,0.3333333f,0),
            new (0.2126f,0.7152f,0.0722f,0),
            new (0,0,0,0),
        };

        private static int GetChannelIndex(MaterialProperty prop)
        {
            var index = -1;
            for (var i = 0; i < ChannelVectors.Length; i++)
            {
                if (prop.vectorValue == ChannelVectors[i])
                    index = i;
            }

            if (index != -1) return index;
            Debug.LogError("ChannelDrawer:"+ prop.name+" 无法识别的通道向量,请检查");
            prop.vectorValue = ChannelVectors[3];
            index = 3;
            return index;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type != MaterialProperty.PropType.Vector)
            {
                GUILayout.Label(prop.displayName + " :   Property must be of type vector");
                return;
            }

            var index = GetChannelIndex(prop);
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var num = EditorGUI.IntPopup(position, label, index, ChannelLabels, ChannelIndex);
            EditorGUI.showMixedValue = false;
            if (!EditorGUI.EndChangeCheck()) return;
            editor.RegisterPropertyChangeUndo(label.text);
            prop.vectorValue = ChannelVectors[num];

        }
    }
}


