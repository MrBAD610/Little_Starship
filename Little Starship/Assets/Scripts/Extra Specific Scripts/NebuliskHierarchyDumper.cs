// Assets/Editor/NebuliskHierarchyDumper.cs
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Linq;

public static class NebuliskHierarchyDumper
{
    [MenuItem("Tools/Nebulisk/Copy Selected Hierarchy")]
    public static void CopySelectedHierarchy()
    {
        var go = Selection.activeGameObject;
        if (!go) { Debug.LogWarning("Select a root (e.g., Nebulisk_Alpha) first."); return; }

        var sb = new StringBuilder();
        Dump(go.transform, 0, sb);
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log($"[Nebulisk] Hierarchy copied to clipboard ({go.name}). Paste it here.");
    }

    static void Dump(Transform t, int depth, StringBuilder sb)
    {
        string indent = new string(' ', depth * 2);
        var comps = t.GetComponents<Component>()
                     .Where(c => c && !(c is Transform))
                     .Select(c => ShortName(c.GetType()));
        sb.AppendLine($"{indent}- {t.name}  [{string.Join(", ", comps)}]");

        for (int i = 0; i < t.childCount; i++)
            Dump(t.GetChild(i), depth + 1, sb);
    }

    static string ShortName(System.Type ty)
    {
        var n = ty.Name;
        // Compact some common ones
        if (n == "Rigidbody") return "RB";
        if (n == "CapsuleCollider") return "Capsule";
        if (n == "BoxCollider") return "Box";
        if (n == "SphereCollider") return "Sphere";
        if (n == "ConfigurableJoint") return "CJ";
        if (n == "HingeJoint") return "Hinge";
        if (n == "Animator") return "Animator";
        if (n.Contains("Leg") && n.Contains("Animator")) return "FIM LegAnim";
        if (n.Contains("Spine") && n.Contains("Animator")) return "FIM SpineAnim";
        return n;
    }
}
