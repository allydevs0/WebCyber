using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reptile
{
	public class AnimationInstancingManager
	{
		private const int MAX_INSTANCES = 2000;

		private CullingGroup cullingGroup;

		private BoundingSphere[] boundingSpheres;

		private AnimationInstancing[] animationInstances;

		private int instancesCount;

		private AnimationTexture[] animationTextures;

		private WorldHandler worldHandler;

		private const int maxInstancingCountPerPackage = 200;

		private int instancingPackageMaxSize = 200;

		private Dictionary<int, VertexCache> vertexCachePool;

		public void Init(WorldHandler worldHandler)
		{
			instancesCount = 0;
			boundingSpheres = new BoundingSphere[2000];
			animationInstances = new AnimationInstancing[2000];
			this.worldHandler = worldHandler;
			Camera targetCamera = null;
			if (worldHandler.CurrentCamera != null)
			{
				targetCamera = worldHandler.CurrentCamera;
			}
			cullingGroup = new CullingGroup();
			cullingGroup.targetCamera = targetCamera;
			cullingGroup.onStateChanged = CullingStateChanged;
			cullingGroup.SetBoundingSpheres(boundingSpheres);
			cullingGroup.SetBoundingSphereCount(instancesCount);
			animationTextures = AnimationAssetManager.GetInstance().animationTextures;
			vertexCachePool = new Dictionary<int, VertexCache>();
			Core.OnAlwaysUpdate += Update;
		}

		~AnimationInstancingManager()
		{
			Dispose();
		}

		public void Dispose()
		{
			Core.OnAlwaysUpdate -= Update;
			if (cullingGroup != null)
			{
				cullingGroup.Dispose();
				cullingGroup = null;
			}
			if (vertexCachePool != null)
			{
				vertexCachePool.Clear();
				vertexCachePool = null;
				animationInstances = null;
				animationTextures = null;
			}
		}

		private int GetPackageCount(VertexCache vertexCache)
		{
			int result = 1;
			if (vertexCache.boneTextureIndex >= 0)
			{
				result = animationTextures[vertexCache.boneTextureIndex].boneTexture.Length;
			}
			return result;
		}

		private InstanceData[] CreateInstanceData(int packageCount)
		{
			InstanceData[] array = new InstanceData[packageCount];
			for (int i = 0; i < packageCount; i++)
			{
				InstanceData obj = (array[i] = new InstanceData());
				obj.worldMatrix = new List<Matrix4x4[]>();
				obj.frameIndex = new List<float[]>();
				obj.preFrameIndex = new List<float[]>();
				obj.transitionProgress = new List<float[]>();
			}
			return array;
		}

		public InstancingPackage CreatePackage(InstanceData data, Mesh mesh, Material[] originalMaterial)
		{
			InstancingPackage instancingPackage = new InstancingPackage();
			instancingPackage.material = new Material[mesh.subMeshCount];
			instancingPackage.subMeshCount = mesh.subMeshCount;
			instancingPackage.size = 1;
			for (int i = 0; i != mesh.subMeshCount; i++)
			{
				instancingPackage.material[i] = new Material(originalMaterial[i]);
				instancingPackage.material[i].enableInstancing = true;
				instancingPackage.material[i].EnableKeyword("INSTANCING_ON");
				instancingPackage.propertyBlock = new MaterialPropertyBlock();
				instancingPackage.material[i].EnableKeyword("USE_CONSTANT_BUFFER");
				instancingPackage.material[i].DisableKeyword("USE_COMPUTE_BUFFER");
			}
			Matrix4x4[] item = new Matrix4x4[instancingPackageMaxSize];
			float[] item2 = new float[instancingPackageMaxSize];
			float[] item3 = new float[instancingPackageMaxSize];
			float[] item4 = new float[instancingPackageMaxSize];
			data.worldMatrix.Add(item);
			data.frameIndex.Add(item2);
			data.preFrameIndex.Add(item3);
			data.transitionProgress.Add(item4);
			return instancingPackage;
		}

		private MaterialBlock CreateBlock(VertexCache cache, Material[] materials)
		{
			MaterialBlock materialBlock = new MaterialBlock();
			int packageCount = GetPackageCount(cache);
			materialBlock.instanceData = CreateInstanceData(packageCount);
			materialBlock.packageList = new List<InstancingPackage>[packageCount];
			for (int i = 0; i < materialBlock.packageList.Length; i++)
			{
				materialBlock.packageList[i] = new List<InstancingPackage>();
				InstancingPackage instancingPackage = CreatePackage(materialBlock.instanceData[i], cache.mesh, materials);
				materialBlock.packageList[i].Add(instancingPackage);
				instancingPackage.instancingCount = 1;
			}
			materialBlock.runtimePackageIndex = new int[packageCount];
			return materialBlock;
		}

		private VertexCache CreateVertexCache(AnimationInstanceType type, int renderName, int alias, Mesh mesh)
		{
			VertexCache vertexCache = new VertexCache();
			int key = (vertexCache.nameCode = renderName + alias);
			vertexCache.mesh = mesh;
			vertexCache.boneTextureIndex = (int)type;
			vertexCache.weight = new Vector4[mesh.vertexCount];
			vertexCache.boneIndex = new Vector4[mesh.vertexCount];
			vertexCache.instanceBlockList = new Dictionary<int, MaterialBlock>();
			vertexCachePool[key] = vertexCache;
			return vertexCache;
		}

		public void SetupAdditionalData(VertexCache vertexCache)
		{
			Color[] array = new Color[vertexCache.weight.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = vertexCache.weight[i];
			}
			vertexCache.mesh.colors = array;
			List<Vector4> list = new List<Vector4>(vertexCache.boneIndex.Length);
			for (int j = 0; j != vertexCache.boneIndex.Length; j++)
			{
				list.Add(vertexCache.boneIndex[j]);
			}
			vertexCache.mesh.SetUVs(2, list);
			vertexCache.mesh.UploadMeshData(markNoLongerReadable: false);
		}

		private void SetBoneIndex(Transform[] boneTransforms, int[] boneIndices, int boneIndex, int nameHash)
		{
			for (int i = 0; i < boneTransforms.Length; i++)
			{
				if (nameHash == boneTransforms[i].name.GetHashCode())
				{
					boneIndices[boneIndex] = i;
					break;
				}
			}
		}

		private void SetupVertexCacheForSkinnedRenderer(VertexCache vertexCache, MaterialBlock block, SkinnedMeshRenderer renderer, Transform[] boneTransforms, int bonePerVertex, Matrix4x4[] bindPosesBase)
		{
			int[] array = null;
			if (renderer.bones.Length != boneTransforms.Length)
			{
				if (renderer.bones.Length == 0)
				{
					array = new int[1];
					int hashCode = renderer.transform.parent.name.GetHashCode();
					SetBoneIndex(boneTransforms, array, 0, hashCode);
				}
				else
				{
					array = new int[renderer.bones.Length];
					for (int i = 0; i < renderer.bones.Length; i++)
					{
						array[i] = -1;
						int hashCode2 = renderer.bones[i].name.GetHashCode();
						SetBoneIndex(boneTransforms, array, i, hashCode2);
					}
					if (array.Length == 0)
					{
						array = null;
					}
				}
			}
			SkinnedMeshRenderer componentInChildren = GameObject.Find("pedBusiness").GetComponentInChildren<SkinnedMeshRenderer>(includeInactive: true);
			Transform[] bones = componentInChildren.bones;
			vertexCache.boneOffset = new Vector4[bindPosesBase.Length];
			for (int j = 0; j < renderer.bones.Length; j++)
			{
				if (array != null)
				{
					j = array[j];
				}
				vertexCache.boneOffset[j] = renderer.bones[j].position - renderer.transform.parent.position - (bones[j].position - componentInChildren.transform.parent.position);
			}
			Mesh sharedMesh = renderer.sharedMesh;
			BoneWeight[] boneWeights = sharedMesh.boneWeights;
			for (int k = 0; k != sharedMesh.vertexCount; k++)
			{
				vertexCache.weight[k].x = boneWeights[k].weight0;
				vertexCache.weight[k].y = boneWeights[k].weight1;
				vertexCache.weight[k].z = boneWeights[k].weight2;
				vertexCache.weight[k].w = boneWeights[k].weight3;
				if (array == null)
				{
					vertexCache.boneIndex[k].x = boneWeights[k].boneIndex0;
					vertexCache.boneIndex[k].y = boneWeights[k].boneIndex1;
					vertexCache.boneIndex[k].z = boneWeights[k].boneIndex2;
					vertexCache.boneIndex[k].w = boneWeights[k].boneIndex3;
				}
				else
				{
					vertexCache.boneIndex[k].x = array[boneWeights[k].boneIndex0];
					vertexCache.boneIndex[k].y = array[boneWeights[k].boneIndex1];
					vertexCache.boneIndex[k].z = array[boneWeights[k].boneIndex2];
					vertexCache.boneIndex[k].w = array[boneWeights[k].boneIndex3];
				}
				switch (bonePerVertex)
				{
				case 3:
				{
					float num2 = 1f / (vertexCache.weight[k].x + vertexCache.weight[k].y + vertexCache.weight[k].z);
					vertexCache.weight[k].x = vertexCache.weight[k].x * num2;
					vertexCache.weight[k].y = vertexCache.weight[k].y * num2;
					vertexCache.weight[k].z = vertexCache.weight[k].z * num2;
					vertexCache.weight[k].w = -0.1f;
					break;
				}
				case 2:
				{
					float num = 1f / (vertexCache.weight[k].x + vertexCache.weight[k].y);
					vertexCache.weight[k].x = vertexCache.weight[k].x * num;
					vertexCache.weight[k].y = vertexCache.weight[k].y * num;
					vertexCache.weight[k].z = -0.1f;
					vertexCache.weight[k].w = -0.1f;
					break;
				}
				case 1:
					vertexCache.weight[k].x = 1f;
					vertexCache.weight[k].y = -0.1f;
					vertexCache.weight[k].z = -0.1f;
					vertexCache.weight[k].w = -0.1f;
					break;
				}
			}
			if (vertexCache.materials == null)
			{
				vertexCache.materials = renderer.sharedMaterials;
			}
			SetupAdditionalData(vertexCache);
			for (int l = 0; l < block.packageList.Length; l++)
			{
				InstancingPackage instancingPackage = CreatePackage(block.instanceData[l], vertexCache.mesh, renderer.sharedMaterials);
				block.packageList[l].Add(instancingPackage);
				PreparePackageMaterial(instancingPackage, vertexCache, l);
			}
		}

		public void BindAttachment(VertexCache parentCache, VertexCache attachmentCache, Mesh sharedMesh, int boneIndex)
		{
			attachmentCache.mesh = Object.Instantiate(sharedMesh);
			Matrix4x4 inverse = parentCache.bindPose[boneIndex].inverse;
			Vector3 vector = inverse.GetColumn(3);
			Quaternion quaternion = RuntimeHelper.QuaternionFromMatrix(inverse);
			Vector3[] vertices = attachmentCache.mesh.vertices;
			for (int i = 0; i < attachmentCache.mesh.vertexCount; i++)
			{
				vertices[i] = quaternion * vertices[i];
				vertices[i] += vector;
			}
			attachmentCache.mesh.vertices = vertices;
			for (int j = 0; j < attachmentCache.mesh.vertexCount; j++)
			{
				attachmentCache.weight[j].x = 1f;
				attachmentCache.weight[j].y = -0.1f;
				attachmentCache.weight[j].z = -0.1f;
				attachmentCache.weight[j].w = -0.1f;
				attachmentCache.boneIndex[j].x = boneIndex;
			}
		}

		private void SetupVertexCacheForMeshRenderer(VertexCache vertexCache, MaterialBlock block, MeshRenderer renderer, Mesh mesh, Transform[] boneTransform, int bonePerVertex)
		{
			int num = -1;
			if (boneTransform != null)
			{
				int hashCode = renderer.transform.parent.name.GetHashCode();
				for (int i = 0; i < boneTransform.Length; i++)
				{
					if (hashCode == boneTransform[i].name.GetHashCode())
					{
						num = i;
						break;
					}
				}
			}
			if (num >= 0)
			{
				BindAttachment(vertexCache, vertexCache, vertexCache.mesh, num);
			}
			if (vertexCache.materials == null)
			{
				vertexCache.materials = renderer.sharedMaterials;
			}
			SetupAdditionalData(vertexCache);
			for (int j = 0; j < block.packageList.Length; j++)
			{
				InstancingPackage instancingPackage = CreatePackage(block.instanceData[j], vertexCache.mesh, renderer.sharedMaterials);
				block.packageList[j].Add(instancingPackage);
				PreparePackageMaterial(instancingPackage, vertexCache, j);
			}
		}

		private int GetIdentify(Material[] material)
		{
			int num = 0;
			for (int i = 0; i < material.Length; i++)
			{
				num += material[i].name.GetHashCode();
			}
			return num;
		}

		public void AddMeshVertex(AnimationInstanceType type, LodInfo[] lodInfo, Transform[] bones, List<Matrix4x4> bindPose, int bonePerVertex, ShadowCastingMode shadowCastingMode, bool receiveShadow, int layer, Matrix4x4[] bindPosesBase, string alias = null)
		{
			foreach (LodInfo lodInfo2 in lodInfo)
			{
				for (int j = 0; j != lodInfo2.skinnedMeshRenderer.Length; j++)
				{
					SkinnedMeshRenderer skinnedMeshRenderer = lodInfo2.skinnedMeshRenderer[j];
					Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
					if (!(sharedMesh != null))
					{
						continue;
					}
					int hashCode = skinnedMeshRenderer.name.GetHashCode();
					Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
					int identify = GetIdentify(sharedMaterials);
					MaterialBlock value2;
					if (vertexCachePool.TryGetValue(hashCode, out var value))
					{
						if (!value.instanceBlockList.TryGetValue(identify, out value2))
						{
							value2 = CreateBlock(value, sharedMaterials);
							value.instanceBlockList.Add(identify, value2);
						}
					}
					else
					{
						value = CreateVertexCache(type, hashCode, 0, sharedMesh);
						value.bindPose = bindPose.ToArray();
						value2 = CreateBlock(value, sharedMaterials);
						value.instanceBlockList.Add(identify, value2);
						SetupVertexCacheForSkinnedRenderer(value, value2, skinnedMeshRenderer, bones, bonePerVertex, bindPosesBase);
					}
					value.shadowcastingMode = shadowCastingMode;
					value.receiveShadow = receiveShadow;
					value.layer = layer;
					lodInfo2.vertexCacheList[j] = value;
					lodInfo2.materialBlockList[j] = value2;
				}
				for (int k = 0; k != lodInfo2.meshRenderer.Length; k++)
				{
					Mesh sharedMesh2 = lodInfo2.meshFilter[k].sharedMesh;
					if (!(sharedMesh2 != null))
					{
						continue;
					}
					int hashCode2 = lodInfo2.meshRenderer[k].name.GetHashCode();
					int num = alias?.GetHashCode() ?? 0;
					int identify2 = GetIdentify(lodInfo2.meshRenderer[k].sharedMaterials);
					MaterialBlock value4;
					if (vertexCachePool.TryGetValue(hashCode2 + num, out var value3))
					{
						if (!value3.instanceBlockList.TryGetValue(identify2, out value4))
						{
							value4 = CreateBlock(value3, lodInfo2.meshRenderer[k].sharedMaterials);
							value3.instanceBlockList.Add(identify2, value4);
						}
					}
					else
					{
						value3 = CreateVertexCache(type, hashCode2, num, sharedMesh2);
						if (bindPose != null)
						{
							value3.bindPose = bindPose.ToArray();
						}
						value4 = CreateBlock(value3, lodInfo2.meshRenderer[k].sharedMaterials);
						value3.instanceBlockList.Add(identify2, value4);
						SetupVertexCacheForMeshRenderer(value3, value4, lodInfo2.meshRenderer[k], sharedMesh2, bones, bonePerVertex);
					}
					int num2 = lodInfo2.skinnedMeshRenderer.Length + k;
					lodInfo2.vertexCacheList[num2] = value3;
					lodInfo2.materialBlockList[num2] = value4;
				}
			}
		}

		public int AddInstance(AnimationInstancing instance, LodInfo info)
		{
			int num = instancesCount++;
			CalculateAndAddBoundingSphere(instance, info, num);
			animationInstances[num] = instance;
			return num;
		}

		public void RemoveInstance(int instanceIndex)
		{
			if (instancesCount > 0)
			{
				int num = instancesCount - 1;
				animationInstances[instanceIndex] = animationInstances[num];
				animationInstances[num] = null;
				instancesCount--;
				boundingSpheres[instanceIndex] = boundingSpheres[num];
				cullingGroup.SetBoundingSphereCount(instancesCount);
			}
		}

		public void SetCullingCamera(Camera camera)
		{
			cullingGroup.targetCamera = camera;
		}

		private void CullingStateChanged(CullingGroupEvent cullingEvent)
		{
			if (cullingEvent.hasBecomeVisible)
			{
				if (animationInstances[cullingEvent.index].isActiveAndEnabled)
				{
					animationInstances[cullingEvent.index].visible = true;
				}
			}
			else if (cullingEvent.hasBecomeInvisible)
			{
				animationInstances[cullingEvent.index].visible = false;
			}
		}

		private void CalculateAndAddBoundingSphere(AnimationInstancing instance, LodInfo info, int instanceIndex)
		{
			Bounds bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));
			for (int i = 0; i != info.meshRenderer.Length; i++)
			{
				MeshRenderer meshRenderer = info.meshRenderer[i];
				bounds.Encapsulate(meshRenderer.bounds);
			}
			for (int j = 0; j != info.skinnedMeshRenderer.Length; j++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = info.skinnedMeshRenderer[j];
				bounds.Encapsulate(skinnedMeshRenderer.bounds);
			}
			float rad = Mathf.Max(bounds.size.z, Mathf.Max(bounds.size.x, bounds.size.y));
			BoundingSphere boundingSphere = new BoundingSphere(new Vector3(0f, 0f, 0f), rad);
			boundingSpheres[instanceIndex] = boundingSphere;
			cullingGroup.SetBoundingSphereCount(instanceIndex + 1);
			instance.visible = true;
		}

		internal void PreparePackageMaterial(InstancingPackage package, VertexCache vertexCache, int aniTextureIndex)
		{
			if (vertexCache.boneTextureIndex >= 0)
			{
				for (int i = 0; i != package.subMeshCount; i++)
				{
					AnimationTexture animationTexture = animationTextures[vertexCache.boneTextureIndex];
					package.material[i].SetTexture("_boneTexture", animationTexture.boneTexture[aniTextureIndex]);
					package.material[i].SetInt("_boneTextureWidth", animationTexture.boneTexture[aniTextureIndex].width);
					package.material[i].SetInt("_boneTextureHeight", animationTexture.boneTexture[aniTextureIndex].height);
					package.material[i].SetInt("_boneTextureBlockWidth", animationTexture.blockWidth);
					package.material[i].SetInt("_boneTextureBlockHeight", animationTexture.blockHeight);
					package.material[i].SetVectorArray("boneOffset", vertexCache.boneOffset);
				}
			}
		}

		private void Render()
		{
			foreach (KeyValuePair<int, VertexCache> item in vertexCachePool)
			{
				VertexCache value = item.Value;
				foreach (KeyValuePair<int, MaterialBlock> instanceBlock in value.instanceBlockList)
				{
					List<InstancingPackage>[] packageList = instanceBlock.Value.packageList;
					for (int i = 0; i != packageList.Length; i++)
					{
						for (int j = 0; j != packageList[i].Count; j++)
						{
							InstancingPackage instancingPackage = packageList[i][j];
							if (instancingPackage.instancingCount != 0)
							{
								for (int k = 0; k != instancingPackage.subMeshCount; k++)
								{
									InstanceData instanceData = instanceBlock.Value.instanceData[i];
									instancingPackage.propertyBlock.SetFloatArray("frameIndex", instanceData.frameIndex[j]);
									instancingPackage.propertyBlock.SetFloatArray("preFrameIndex", instanceData.preFrameIndex[j]);
									instancingPackage.propertyBlock.SetFloatArray("transitionProgress", instanceData.transitionProgress[j]);
									Graphics.DrawMeshInstanced(value.mesh, k, instancingPackage.material[k], instanceData.worldMatrix[j], instancingPackage.instancingCount, instancingPackage.propertyBlock, value.shadowcastingMode, value.receiveShadow, value.layer);
								}
								instancingPackage.instancingCount = 0;
							}
						}
						instanceBlock.Value.runtimePackageIndex[i] = 0;
					}
				}
			}
		}

		private void ApplyRootMotion(AnimationInstancing instance)
		{
			AnimationInstanceInfo currentAnimationInfo = instance.GetCurrentAnimationInfo();
			if (currentAnimationInfo != null && currentAnimationInfo.rootMotion)
			{
				int num = (int)instance.curFrame;
				int num2 = (int)(instance.curFrame + 1f);
				if (num2 < currentAnimationInfo.totalFrame)
				{
					float t = instance.curFrame - (float)num;
					Vector3 a = currentAnimationInfo.velocity[num];
					Vector3 b = currentAnimationInfo.velocity[num2];
					Vector3 vector = Vector3.Lerp(a, b, t);
					Vector3 a2 = currentAnimationInfo.angularVelocity[num];
					Vector3 b2 = currentAnimationInfo.angularVelocity[num2];
					Quaternion quaternion = Quaternion.Euler(Vector3.Lerp(a2, b2, t) * Time.deltaTime);
					Quaternion localRotation = instance.worldTransform.localRotation;
					localRotation *= quaternion;
					Vector3 vector2 = vector * Time.deltaTime;
					Vector3 localPosition = instance.worldTransform.localPosition;
					localPosition += localRotation * vector2;
					instance.worldTransform.SetPositionAndRotation(localPosition, localRotation);
				}
			}
		}

		private static bool IsPlaying(AnimationInstancing instance)
		{
			if (instance.aniIndex < 0)
			{
				return instance.isMeshRender;
			}
			return true;
		}

		private void ApplyBoneMatrix()
		{
			Vector3 position = worldHandler.CurrentCameraTransform.position;
			for (int i = 0; i < instancesCount; i++)
			{
				AnimationInstancing animationInstancing = animationInstances[i];
				if (!IsPlaying(animationInstancing) || (animationInstancing.aniIndex < 0 && !(animationInstancing.parentInstance != null)))
				{
					continue;
				}
				if (animationInstancing.applyRootMotion)
				{
					ApplyRootMotion(animationInstancing);
				}
				animationInstancing.UpdateAnimation();
				boundingSpheres[i].position = animationInstancing.transform.position;
				if (!animationInstancing.visible)
				{
					continue;
				}
				animationInstancing.UpdateLod(position);
				LodInfo lodInfo = animationInstancing.lodInfo[animationInstancing.lodLevel];
				int aniTextureIndex = animationInstancing.aniTextureIndex;
				if (animationInstancing.parentInstance != null)
				{
					aniTextureIndex = animationInstancing.parentInstance.aniTextureIndex;
				}
				for (int j = 0; j < lodInfo.vertexCacheList.Length; j++)
				{
					VertexCache vertexCache = lodInfo.vertexCacheList[j];
					MaterialBlock materialBlock = lodInfo.materialBlockList[j];
					int num = materialBlock.runtimePackageIndex[aniTextureIndex];
					InstancingPackage instancingPackage = materialBlock.packageList[aniTextureIndex][num];
					if (instancingPackage.instancingCount >= instancingPackageMaxSize)
					{
						num = ++materialBlock.runtimePackageIndex[aniTextureIndex];
						if (num >= materialBlock.packageList[aniTextureIndex].Count)
						{
							InstancingPackage instancingPackage2 = CreatePackage(materialBlock.instanceData[aniTextureIndex], vertexCache.mesh, vertexCache.materials);
							materialBlock.packageList[aniTextureIndex].Add(instancingPackage2);
							PreparePackageMaterial(instancingPackage2, vertexCache, aniTextureIndex);
							instancingPackage2.instancingCount = 1;
							instancingPackage = instancingPackage2;
						}
					}
					else
					{
						instancingPackage.instancingCount++;
					}
					InstanceData instanceData = materialBlock.instanceData[aniTextureIndex];
					int num2 = instancingPackage.instancingCount - 1;
					if (num2 < 0)
					{
						continue;
					}
					Matrix4x4 localToWorldMatrix = animationInstancing.worldTransform.localToWorldMatrix;
					instanceData.worldMatrix[num][num2] = localToWorldMatrix;
					float num3 = 0f;
					float num4 = -1f;
					float num5 = 0f;
					AnimationInstancing parentInstance = animationInstancing.parentInstance;
					if (animationInstancing.parentInstance != null)
					{
						num3 = (float)parentInstance.animationInfo[parentInstance.aniIndex].animationIndex + parentInstance.curFrame;
						if (parentInstance.preAniIndex >= 0)
						{
							num4 = (float)parentInstance.animationInfo[parentInstance.preAniIndex].animationIndex + parentInstance.preAniFrame;
						}
						num5 = parentInstance.transitionProgress;
					}
					else
					{
						num3 = (float)animationInstancing.animationInfo[animationInstancing.aniIndex].animationIndex + animationInstancing.curFrame;
						if (animationInstancing.preAniIndex >= 0)
						{
							num4 = (float)animationInstancing.animationInfo[animationInstancing.preAniIndex].animationIndex + animationInstancing.preAniFrame;
						}
						num5 = animationInstancing.transitionProgress;
					}
					instanceData.frameIndex[num][num2] = num3;
					instanceData.preFrameIndex[num][num2] = num4;
					instanceData.transitionProgress[num][num2] = num5;
				}
			}
		}

		private void Update()
		{
			ApplyBoneMatrix();
			Render();
		}

		private void OnApplicationFocus(bool focus)
		{
			if (focus)
			{
				RefreshMaterial();
			}
		}

		private void RefreshMaterial()
		{
			if (vertexCachePool == null)
			{
				return;
			}
			foreach (KeyValuePair<int, VertexCache> item in vertexCachePool)
			{
				VertexCache value = item.Value;
				foreach (KeyValuePair<int, MaterialBlock> instanceBlock in value.instanceBlockList)
				{
					for (int i = 0; i != instanceBlock.Value.packageList.Length; i++)
					{
						for (int j = 0; j != instanceBlock.Value.packageList[i].Count; j++)
						{
							InstancingPackage package = instanceBlock.Value.packageList[i][j];
							PreparePackageMaterial(package, value, i);
						}
					}
				}
			}
		}
	}
}
