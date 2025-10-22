using UnityEngine;
using SH.Core;
using SH.Input;

// Botones de debug para probar el stub CommandMenuInputSource.
public class UIInputTestDriver : MonoBehaviour
{
    public CommandMenuInputSource uiSource;
    public MonoBehaviour currentActorRef; // arrastra aquí el PlayerActor (implementa IActor)

    void OnGUI()
    {
        if (!uiSource || !(currentActorRef is IActor actor)) return;

        GUILayout.BeginArea(new Rect(10,10,200,140), "UI Stub", GUI.skin.window);
        if (GUILayout.Button("Attack"))
            uiSource.SubmitSelectedAction(new ActionData(ActionType.Attack, actor, null, 0));
        if (GUILayout.Button("Defend"))
            uiSource.SubmitSelectedAction(new ActionData(ActionType.Defend, actor, null, 0));
        if (GUILayout.Button("Wait"))
            uiSource.SubmitSelectedAction(new ActionData(ActionType.Wait, actor, null, 0));
        if (GUILayout.Button("ShieldSkill"))
            uiSource.SubmitSelectedAction(new ActionData(ActionType.ShieldSkill, actor, null, 1));
        GUILayout.EndArea();
    }
}