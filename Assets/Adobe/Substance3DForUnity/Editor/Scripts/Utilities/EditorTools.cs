using Adobe.Substance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    internal static class EditorTools
    {
        /// <summary>
        /// Makes an object editable. (Usefull for object managed by Importers)
        /// </summary>
        /// <param name="pObject"></param>
        public static void OverrideReadOnlyFlag(UnityEngine.Object unityObject)
        {
            unityObject.hideFlags &= ~HideFlags.NotEditable;
        }

        public static SubstanceGraphSO CreateSubstanceInstance(string assetPath, SubstanceFileRawData fileData, string name, int index, string guid, bool isRoot = false, SubstanceGraphSO copy = null)
        {
            var instanceAsset = ScriptableObject.CreateInstance<SubstanceGraphSO>();
            instanceAsset.AssetPath = assetPath;
            instanceAsset.RawData = fileData;
            instanceAsset.Name = name;
            instanceAsset.IsRoot = isRoot;
            instanceAsset.GUID = guid;
            instanceAsset.OutputPath = CreateGraphFolder(assetPath, name);
            instanceAsset.SetNativeID(index);
            instanceAsset.GenerateAllMipmaps = true;
            var instancePath = MakeRootGraphAssetPath(instanceAsset);
            SubstanceEditorEngine.instance.InitializeInstance(instanceAsset, instancePath, out SubstanceGraphSO _);
            SubstanceEditorEngine.instance.CreateGraphObject(instanceAsset, copy);
            AssetDatabase.CreateAsset(instanceAsset, instancePath);
            return instanceAsset;
        }

        public static void UpdateSubstanceInstance(SubstanceGraphSO instanceAsset, SubstanceFileRawData newFileData)
        {
            instanceAsset.RawData = newFileData;
            var inputState = SubstanceEditorEngine.instance.SerializeCurrentState(instanceAsset);
            SubstanceEditorEngine.instance.ReleaseInstance(instanceAsset);
            SubstanceEditorEngine.instance.InitializeInstance(instanceAsset, null, out SubstanceGraphSO _);
            SubstanceEditorEngine.instance.SetStateFromSerializedData(instanceAsset, inputState);
            SubstanceEditorEngine.instance.CreateGraphObject(instanceAsset, null);
        }

        public class SubstanceInstanceInfo
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public string GUID { get; set; }
            public bool IsRoot { get; set; }

            public SubstanceInstanceInfo()
            {
            }
        }

        public static void CreateSubstanceInstanceAsync(string assetPath, SubstanceFileRawData fileData, IEnumerable<SubstanceInstanceInfo> infos)
        {
            var instances = new List<Tuple<SubstanceGraphSO, string>>();

            foreach (var item in infos)
            {
                var instanceAsset = ScriptableObject.CreateInstance<SubstanceGraphSO>();
                instanceAsset.AssetPath = assetPath;
                instanceAsset.RawData = fileData;
                instanceAsset.Name = item.Name;
                instanceAsset.IsRoot = item.IsRoot;
                instanceAsset.GUID = item.GUID;
                instanceAsset.OutputPath = CreateGraphFolder(assetPath, item.Name);
                instanceAsset.SetNativeID(item.Index);
                instanceAsset.GenerateAllMipmaps = true;
                var instancePath = MakeRootGraphAssetPath(instanceAsset);
                SubstanceEditorEngine.instance.InitializeInstance(instanceAsset, instancePath, out SubstanceGraphSO matchingInstance);

                SubstanceEditorEngine.instance.CreateGraphObject(instanceAsset, matchingInstance);
                instances.Add(new Tuple<SubstanceGraphSO, string>(instanceAsset, instancePath));
            }

            foreach (var instance in instances)
            {
                SubstanceEditorEngine.instance.DelayAssetCreation(instance.Item1, instance.Item2);
            }
        }

        public static void Rename(this SubstanceGraphSO substanceMaterial, string name)
        {
            var oldFolder = substanceMaterial.OutputPath;

            if (substanceMaterial.Name == name)
                return;

            substanceMaterial.Name = name;

            var dir = Path.GetDirectoryName(substanceMaterial.AssetPath);
            var assetName = Path.GetFileNameWithoutExtension(substanceMaterial.AssetPath);
            var newFolder = Path.Combine(dir, $"{assetName}_{name}");
            substanceMaterial.OutputPath = newFolder;

            FileUtil.MoveFileOrDirectory(oldFolder, substanceMaterial.OutputPath);
            File.Delete($"{oldFolder}.meta");

            EditorUtility.SetDirty(substanceMaterial);
            AssetDatabase.Refresh();

            var oldPath = AssetDatabase.GetAssetPath(substanceMaterial);
            var error = AssetDatabase.RenameAsset(oldPath, $"{name}.asset");

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);

            var materialOldName = AssetDatabase.GetAssetPath(substanceMaterial.OutputMaterial);
            var materialNewName = Path.GetFileName(substanceMaterial.GetAssociatedAssetPath($"{name}_material", "mat"));
            error = AssetDatabase.RenameAsset(materialOldName, materialNewName);
            EditorUtility.SetDirty(substanceMaterial.OutputMaterial);

            if (!string.IsNullOrEmpty(error))
                Debug.LogError(error);

            AssetDatabase.Refresh();
        }

        public static void Move(this SubstanceGraphSO substanceMaterial, string to)
        {
            substanceMaterial.OutputPath = Path.GetDirectoryName(to);

            var oldMaterialPath = AssetDatabase.GetAssetPath(substanceMaterial.OutputMaterial);
            AssetDatabase.MoveAsset(oldMaterialPath, Path.Combine(substanceMaterial.OutputPath, Path.GetFileName(oldMaterialPath)));

            foreach (var output in substanceMaterial.Output)
            {
                var textureAssetPath = AssetDatabase.GetAssetPath(output.OutputTexture);
                var textureFileName = Path.GetFileName(textureAssetPath);
                var newTexturePath = Path.Combine(substanceMaterial.OutputPath, textureFileName);
                AssetDatabase.MoveAsset(textureAssetPath, newTexturePath);
            }

            EditorUtility.SetDirty(substanceMaterial);
            AssetDatabase.Refresh();
        }

        private static string CreateGraphFolder(string assetPath, string graphName)
        {
            var dir = Path.GetDirectoryName(assetPath);
            var assetName = Path.GetFileNameWithoutExtension(assetPath);

            var newFolder = Path.Combine(dir, $"{assetName}_{graphName}");

            if (Directory.Exists(newFolder))
                return newFolder;

            string guid = AssetDatabase.CreateFolder(dir, $"{assetName}_{graphName}");
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        private static string MakeRootGraphAssetPath(SubstanceGraphSO substanceMaterial)
        {
            return Path.Combine(substanceMaterial.OutputPath, $"{substanceMaterial.Name}.asset");
        }
    }

    public static class SubstanceEditorTools
    {
        public static void SetGraphFloatInput(SubstanceGraphSO graph, int inputId, float value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.floatValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphFloat2Input(SubstanceGraphSO graph, int inputId, Vector2 value)
        {
            var so = new SerializedObject(graph);

            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector2Value = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphFloat3Input(SubstanceGraphSO graph, int inputId, Vector3 value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector3Value = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphFloat4Input(SubstanceGraphSO graph, int inputId, Vector3 value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector4Value = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphIntInput(SubstanceGraphSO graph, int inputId, int value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.intValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphInt2Input(SubstanceGraphSO graph, int inputId, Vector2Int value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector2IntValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphInt3Input(SubstanceGraphSO graph, int inputId, Vector3Int value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.vector3IntValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphInt4Input(SubstanceGraphSO graph, int inputId, int value0, int value1, int value2, int value3)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp0 = graphInput.FindPropertyRelative("Data0");
            var dataProp1 = graphInput.FindPropertyRelative("Data1");
            var dataProp2 = graphInput.FindPropertyRelative("Data2");
            var dataProp3 = graphInput.FindPropertyRelative("Data3");
            dataProp0.intValue = value0;
            dataProp1.intValue = value1;
            dataProp2.intValue = value2;
            dataProp3.intValue = value3;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphInputString(SubstanceGraphSO graph, int inputId, string value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.stringValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        public static void SetGraphInputTexture(SubstanceGraphSO graph, int inputId, Texture2D value)
        {
            var so = new SerializedObject(graph);
            var graphInputs = so.FindProperty("Input");
            var graphInput = graphInputs.GetArrayElementAtIndex(inputId);
            var dataProp = graphInput.FindPropertyRelative("Data");
            dataProp.objectReferenceValue = value;

            so.ApplyModifiedProperties();

            UpdateNativeInput(graph, inputId);
        }

        private static void UpdateNativeInput(SubstanceGraphSO graph, int inputId)
        {
            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out SubstanceNativeGraph _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return;

                SubstanceEditorEngine.instance.InitializeInstance(graph, null, out SubstanceGraphSO _);
            }

            if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out _nativeGraph))
                graph.RuntimeInitialize(_nativeGraph, graph.IsRuntimeOnly);

            graph.Input[inputId].UpdateNativeHandle(_nativeGraph);
        }

        public static void RenderGraph(SubstanceGraphSO graph)
        {
            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out SubstanceNativeGraph _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return;

                SubstanceEditorEngine.instance.InitializeInstance(graph, null, out SubstanceGraphSO _);
            }

            if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out _nativeGraph))
            {
                SubstanceEditorEngine.instance.SubmitAsyncRenderWork(_nativeGraph, graph);
            }
        }

        public static string CreatePresetFromCurrentState(SubstanceGraphSO graph)
        {
            if (!SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out SubstanceNativeGraph _nativeGraph))
            {
                if (!SubstanceEditorEngine.instance.IsInitialized)
                    return string.Empty;

                SubstanceEditorEngine.instance.InitializeInstance(graph, null, out SubstanceGraphSO _);
            }

            if (SubstanceEditorEngine.instance.TryGetHandlerFromInstance(graph, out _nativeGraph))
                graph.RuntimeInitialize(_nativeGraph, graph.IsRuntimeOnly);

            return _nativeGraph.CreatePresetFromCurrentState();
        }
    }
}