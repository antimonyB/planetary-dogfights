using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OriginController))]
public class OriginEditor : Editor
{
    OriginController originController;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
        }

        if (GUILayout.Button("unifyPositions"))
        {
            originController.UnifyPositions();
        }

        if (GUILayout.Button("Calculate Origin"))
        {
            originController.CalculateOrigin();
        }
        if (GUILayout.Button("Reset Origin"))
        {
            originController.ResetOrigin();
        }

    }

    private void OnEnable()
    {
        originController = (OriginController)target;
    }


}