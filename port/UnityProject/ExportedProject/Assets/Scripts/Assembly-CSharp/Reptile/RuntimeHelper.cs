using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class RuntimeHelper
	{
		public static Transform[] MergeBone(SkinnedMeshRenderer[] skinnedRenderers, List<Matrix4x4> bindPose)
		{
			List<Transform> list = new List<Transform>(150);
			for (int i = 0; i != skinnedRenderers.Length; i++)
			{
				Transform[] bones = skinnedRenderers[i].bones;
				Matrix4x4[] bindposes = skinnedRenderers[i].sharedMesh.bindposes;
				int boneIndex = 0;
				while (boneIndex != bones.Length)
				{
					int num = list.FindIndex((Transform x) => x == bones[boneIndex]);
					if (num < 0)
					{
						list.Add(bones[boneIndex]);
						bindPose?.Add(bindposes[boneIndex]);
					}
					else
					{
						bindPose[num] = bindposes[boneIndex];
					}
					int num2 = boneIndex + 1;
					boneIndex = num2;
				}
				skinnedRenderers[i].enabled = false;
			}
			return list.ToArray();
		}

		public static Quaternion QuaternionFromMatrix(Matrix4x4 mat)
		{
			Vector3 forward = default;
			forward.x = mat.m02;
			forward.y = mat.m12;
			forward.z = mat.m22;
			Vector3 upwards = default;
			upwards.x = mat.m01;
			upwards.y = mat.m11;
			upwards.z = mat.m21;
			return Quaternion.LookRotation(forward, upwards);
		}
	}
}
