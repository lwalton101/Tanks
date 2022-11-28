using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UITweener))]
public class UITweenerEditor : Editor
{
   public override void OnInspectorGUI()
   {
      base.OnInspectorGUI();
      UITweener tweener = (UITweener) target;

      if (GUILayout.Button("Tween"))
      {
         tweener.HandleTween();
      }
      if (GUILayout.Button("Reverse Tween"))
      {
         tweener.HandleReverseTween();
      }
      
      
   }
}
