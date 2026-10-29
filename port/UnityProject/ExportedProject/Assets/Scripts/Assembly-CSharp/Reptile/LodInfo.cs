using UnityEngine;

namespace Reptile
{
	public class LodInfo
	{
		public int lodLevel;

		public SkinnedMeshRenderer[] skinnedMeshRenderer;

		public MeshRenderer[] meshRenderer;

		public MeshFilter[] meshFilter;

		public VertexCache[] vertexCacheList;

		public MaterialBlock[] materialBlockList;
	}
}
