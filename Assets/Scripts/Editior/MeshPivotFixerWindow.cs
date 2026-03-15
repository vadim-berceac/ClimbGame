using UnityEditor;
using UnityEngine;

public class MeshPivotFixerWindow : EditorWindow
{
    private GameObject targetObject;
    private Vector3 customOffset = Vector3.zero;
    private bool useCustomOffset = false;
    private bool keepOriginalScale = true;
    private string containerSuffix = "_Mesh";
    
    [MenuItem("Tools/Mesh Pivot Fixer")]
    public static void ShowWindow()
    {
        GetWindow<MeshPivotFixerWindow>("Mesh Pivot Fixer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Mesh Pivot Fixer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Создаёт контейнер и смещает меш так, чтобы pivot и center совпадали визуально.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        targetObject = EditorGUILayout.ObjectField(
            "Target Object",
            targetObject,
            typeof(GameObject),
            true) as GameObject;
        
        if (targetObject != null)
        {
            var hasRenderer = targetObject.GetComponent<Renderer>();
            if (hasRenderer == null)
            {
                EditorGUILayout.HelpBox(
                    "Выбранный объект не имеет SkinnedMeshRenderer или MeshRenderer!",
                    MessageType.Warning);
            }
        }
        
        EditorGUILayout.Space(10);
        
        useCustomOffset = EditorGUILayout.Toggle("Use Custom Offset", useCustomOffset);
        if (useCustomOffset)
        {
            customOffset = EditorGUILayout.Vector3Field("Custom Offset", customOffset);
        }
        
        keepOriginalScale = EditorGUILayout.Toggle("Keep Original Scale", keepOriginalScale);
        containerSuffix = EditorGUILayout.TextField("Container Name Suffix", containerSuffix);
        
        EditorGUILayout.Space(10);
        
        EditorGUI.BeginDisabledGroup(targetObject == null);
        if (GUILayout.Button("Fix Pivot", GUILayout.Height(40)))
        {
            FixMeshPivot(targetObject);
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Fix Selected Objects", GUILayout.Height(35)))
        {
            FixSelectedObjects();
        }
    }
    
    private void FixMeshPivot(GameObject target)
    {
        if (target == null)
        {
            EditorUtility.DisplayDialog("Error", "No target selected!", "OK");
            return;
        }
        
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            EditorUtility.DisplayDialog("Error", "Target has no Renderer component!", "OK");
            return;
        }
        
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        
        try
        {
            // Рассчитываем смещение
            Vector3 offset = CalculateOffset(renderer);
            if (useCustomOffset)
            {
                offset = customOffset;
            }
            
            Debug.Log($"Calculated offset: {offset}", target);
            
            // Создаём контейнер
            var container = new GameObject(target.name + containerSuffix);
            Undo.RegisterCreatedObjectUndo(container, "Create Pivot Container");
            
            // Копируем трансформ родителя
            if (target.transform.parent != null)
            {
                container.transform.SetParent(target.transform.parent);
            }
            container.transform.position = target.transform.position;
            container.transform.rotation = target.transform.rotation;
            
            if (keepOriginalScale)
            {
                container.transform.localScale = target.transform.localScale;
            }
            
            // Сохраняем локальные данные перед переносом
            var originalLocalPos = target.transform.localPosition;
            var originalLocalRot = target.transform.localRotation;
            var originalLocalScale = target.transform.localScale;
            
            // Перемещаем рендерер в контейнер
            Undo.SetTransformParent(target.transform, container.transform, "Reparent to Container");
            
            // Применяем смещение
            target.transform.localPosition = -offset;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = originalLocalScale;
            
            // Копируем компоненты на контейнер если нужно
            CopyComponentsToContainer(target, container);
            
            Undo.CollapseUndoOperations(undoGroup);
            
            EditorUtility.DisplayDialog(
                "Success",
                $"Pivot fixed!\nOffset applied: {offset}",
                "OK");
            
            Debug.Log($"[MeshPivotFixer] Successfully fixed {target.name}", container);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MeshPivotFixer] Error: {e.Message}", target);
            Undo.RevertAllDownToGroup(undoGroup);
        }
    }
    
    private void FixSelectedObjects()
    {
        var selected = Selection.gameObjects;
        
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No objects selected!", "OK");
            return;
        }
        
        int successCount = 0;
        foreach (var obj in selected)
        {
            if (obj.GetComponent<Renderer>() != null)
            {
                FixMeshPivot(obj);
                successCount++;
            }
        }
        
        Debug.Log($"[MeshPivotFixer] Fixed {successCount}/{selected.Length} objects");
    }
    
    private Vector3 CalculateOffset(Renderer renderer)
    {
        Mesh mesh = null;
        
        if (renderer is SkinnedMeshRenderer skinnedMesh)
        {
            mesh = new Mesh();
            skinnedMesh.BakeMesh(mesh, false);
        }
        else if (renderer is MeshRenderer meshRenderer)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogWarning("MeshRenderer has no MeshFilter!", renderer.gameObject);
                return Vector3.zero;
            }
            mesh = meshFilter.sharedMesh;
        }
        
        if (mesh == null)
        {
            Debug.LogWarning("Could not get mesh from renderer!", renderer.gameObject);
            return Vector3.zero;
        }
        
        return mesh.bounds.center;
    }
    
    private void CopyComponentsToContainer(GameObject source, GameObject container)
    {
        // Копируем Collider'ы на контейнер
        var colliders = source.GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            var newCollider = container.AddComponent(collider.GetType()) as Collider;
            EditorUtility.CopySerialized(collider, newCollider);
            
            // Для SkinnedMeshCollider нужна специальная обработка
            if (collider is CapsuleCollider capsule)
            {
                var newCapsule = newCollider as CapsuleCollider;
                newCapsule.center = Vector3.zero;
                newCapsule.direction = capsule.direction;
            }
        }
    }
}

/// <summary>
/// Версия для использования из скрипта без GUI
/// </summary>
public static class MeshPivotFixer
{
    public class Options
    {
        public Vector3? CustomOffset;
        public bool KeepOriginalScale = true;
        public string ContainerSuffix = "_Mesh";
    }
    
    public static GameObject FixPivot(GameObject target, Options options = null)
    {
        options ??= new Options();
        
        if (target == null)
            throw new System.ArgumentNullException(nameof(target));
        
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null)
            throw new System.InvalidOperationException("Target has no Renderer");
        
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        
        try
        {
            // Рассчитываем смещение
            Vector3 offset = options.CustomOffset ?? CalculateOffset(renderer);
            
            // Создаём контейнер
            var container = new GameObject(target.name + options.ContainerSuffix);
            Undo.RegisterCreatedObjectUndo(container, "Create Pivot Container");
            
            // Копируем трансформ
            if (target.transform.parent != null)
                container.transform.SetParent(target.transform.parent);
            
            container.transform.position = target.transform.position;
            container.transform.rotation = target.transform.rotation;
            
            if (options.KeepOriginalScale)
                container.transform.localScale = target.transform.localScale;
            
            // Перемещаем рендерер
            Undo.SetTransformParent(target.transform, container.transform, "Reparent");
            target.transform.localPosition = -offset;
            target.transform.localRotation = Quaternion.identity;
            
            Undo.CollapseUndoOperations(undoGroup);
            
            Debug.Log($"[MeshPivotFixer] Fixed {target.name}, offset: {offset}", container);
            return container;
        }
        catch
        {
            Undo.RevertAllDownToGroup(undoGroup);
            throw;
        }
    }
    
    private static Vector3 CalculateOffset(Renderer renderer)
    {
        Mesh mesh = null;
        
        if (renderer is SkinnedMeshRenderer skinnedMesh)
        {
            mesh = new Mesh();
            skinnedMesh.BakeMesh(mesh, false);
        }
        else if (renderer is MeshRenderer meshRenderer)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            mesh = meshFilter?.sharedMesh;
        }
        
        return mesh?.bounds.center ?? Vector3.zero;
    }
}