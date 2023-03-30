using Adobe.Substance;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Adobe.SubstanceEditor
{
    internal static class SubstanceFileExtensions
    {
        internal static List<SubstanceGraphSO> GetGraphs(this SubstanceFileSO fileSO)
        {
            var result = new List<SubstanceGraphSO>();

            var path = AssetDatabase.GetAssetPath(fileSO);

            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SubstanceGraphSO)));

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                SubstanceGraphSO graph = AssetDatabase.LoadAssetAtPath<SubstanceGraphSO>(assetPath);

                if (graph != null)
                {
                    var filePath = AssetDatabase.GetAssetPath(graph.RawData);

                    if (filePath.Equals(path, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(graph);
                    }
                }
            }

            return result;
        }
    }
}