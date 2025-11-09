using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterAIController))]
public class FovEditor : Editor
{
    private void OnSceneGUI()
    {
        var monster = (MonsterAIController)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(monster.transform.position, Vector3.up, Vector3.forward, 360, monster.VisionRadius);

        var viewAngleLeft = DirFromAngle(monster.transform.eulerAngles.y, -monster.VisionAngle / 2);
        var viewAngleRight = DirFromAngle(monster.transform.eulerAngles.y, monster.VisionAngle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(monster.transform.position, monster.transform.position + viewAngleLeft * monster.VisionRadius);
        Handles.DrawLine(monster.transform.position, monster.transform.position + viewAngleRight * monster.VisionRadius);

        if (monster.bCanSeeTarget)
        {
            Handles.color = Color.green;
            Handles.DrawLine(monster.transform.position, monster.transform.position + viewAngleRight * monster.VisionRadius);
        }
    }

    private Vector3 DirFromAngle(float eulerY, float angleInDeg)
    {
        angleInDeg += eulerY;

        return new Vector3(Mathf.Sin(angleInDeg * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDeg * Mathf.Deg2Rad));
    }
}
