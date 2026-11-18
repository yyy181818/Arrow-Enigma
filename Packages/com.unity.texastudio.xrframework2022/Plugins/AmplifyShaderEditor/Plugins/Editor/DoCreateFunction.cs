// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEditor;
using UnityEditor.ProjectWindowCallback;
namespace AmplifyShaderEditor
{
	public class DoCreateFunction : AssetCreationEndAction
	{
		public override void Action( UnityEngine.EntityId instanceId, string pathName, string resourceFile )
		{
		#if UNITY_6000_3_OR_NEWER
			UnityEngine.Object obj = EditorUtility.EntityIdToObject( instanceId );
		#else
			UnityEngine.Object obj = EditorUtility.InstanceIDToObject( instanceId );
		#endif
			AssetDatabase.CreateAsset( obj, AssetDatabase.GenerateUniqueAssetPath( pathName ) );
			AmplifyShaderEditorWindow.LoadShaderFunctionToASE( (AmplifyShaderFunction)obj, false );
		}
	}
}
