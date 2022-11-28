using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Block))]
public class BlockEditor : Editor
{
    public override void OnInspectorGUI(){
		if (GUILayout.Button("Place Block to the left"))
		{
			InstantiateBlock(BlockType.LightWood, Vector3.left);
		}
    }

	public void InstantiateBlock(BlockType blockType, Vector3 offset)
	{
		Block targetBlock = (Block)target; 

		GameObject block = Resources.Load<GameObject>("Models/Block");
		Instantiate(block, position: targetBlock.transform.position, Quaternion.identity);
	}
}
