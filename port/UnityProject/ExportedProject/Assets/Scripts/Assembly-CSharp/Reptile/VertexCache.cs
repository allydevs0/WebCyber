using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reptile
{
	public class VertexCache
	{
		public int nameCode;

		public Mesh mesh;

		public Dictionary<int, MaterialBlock> instanceBlockList;

		public Vector4[] weight;

		public Vector4[] boneIndex;

		public Material[] materials;

		public Matrix4x4[] bindPose;

		public Transform[] bonePose;

		public Vector4[] boneOffset;

		public int boneTextureIndex = -1;

		public ShadowCastingMode shadowcastingMode;

		public bool receiveShadow;

		public int layer;
	}
}
