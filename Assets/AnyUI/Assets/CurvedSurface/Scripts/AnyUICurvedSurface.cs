using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyUI.Library
{
    [ExecuteInEditMode]
    public class AnyUICurvedSurface : MonoBehaviour
    {
      
        public MeshFilter DisplayObject;
        public SkinnedMeshRenderer Curve;
        [Header("Edit Shape")]
        public bool EditShape;
        [Range(0.0f, 100.0f)]
        public float CurveHorizontal;
        [Range(0.0f, 100.0f)]
        public float CurveVertical;
        [Range(0.0f, 100.0f)]
        public float Width;

        //--------------------------------------------------------------------------------------
        void Awake()
        {
            Curve.BakeMesh(DisplayObject.sharedMesh);
        }
        //--------------------------------------------------------------------------------------
        void OnValidate()
        {
            if (!EditShape) return;
            Curve.SetBlendShapeWeight(0, CurveHorizontal);
            Curve.SetBlendShapeWeight(1, CurveVertical);
            Curve.SetBlendShapeWeight(2, Width);
            Curve.BakeMesh(DisplayObject.sharedMesh);
        }
        //--------------------------------------------------------------------------------------

    }
}