using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reptile
{
	[AddComponentMenu("AnimationInstancing")]
	public class AnimationInstancing : MonoBehaviour
	{
		[NonSerialized]
		public Transform worldTransform;

		[NonSerialized]
		public LodInfo[] lodInfo;

		[NonSerialized]
		public int lodLevel;

		[NonSerialized]
		public int layer;

		[NonSerialized]
		public float curFrame;

		[NonSerialized]
		public float preAniFrame;

		[NonSerialized]
		public int aniIndex = -1;

		[NonSerialized]
		public int preAniIndex = -1;

		[NonSerialized]
		public int aniTextureIndex = -1;

		[NonSerialized]
		public float transitionProgress;

		[NonSerialized]
		private List<AnimationInstancing> listAttachment;

		public AnimationInstanceType animationType;

		public float playSpeed = 1f;

		public ShadowCastingMode shadowCastingMode;

		public bool receiveShadow;

		public bool applyRootMotion;

		[Range(1f, 4f)]
		public int bonePerVertex = 4;

		internal AnimationInstanceInfo[] animationInfo;

		internal AnimationInstancing parentInstance;

		internal bool isMeshRender;

		internal bool visible;

		private Animator animator;

		private ComparerHash comparer;

		private AnimationInstanceInfo searchInfo;

		private AnimationInstanceEvent aniEvent;

		private Transform[] allTransforms;

		private float lodCalculateFrequency = 0.5f;

		private float lodFrequencyCount;

		private float speedParameter = 1f;

		private float cacheParameter = 1f;

		private float transitionDuration;

		private float transitionTimer;

		private bool isInTransition;

		private int eventIndex = -1;

		private int instanceIndex;

		private float initialNormalizedTime;

		private string initialAnimationName;

		private void Start()
		{
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		private void OnStageInitialized()
		{
			StageManager.OnStageInitialized -= OnStageInitialized;
			worldTransform = GetComponent<Transform>();
			animator = GetComponent<Animator>();
			listAttachment = new List<AnimationInstancing>();
			layer = base.gameObject.layer;
			switch (QualitySettings.skinWeights)
			{
			case SkinWeights.TwoBones:
				bonePerVertex = ((bonePerVertex > 2) ? 2 : bonePerVertex);
				break;
			case SkinWeights.OneBone:
				bonePerVertex = 1;
				break;
			}
			LODGroup component = GetComponent<LODGroup>();
			if (component != null)
			{
				this.lodInfo = new LodInfo[component.lodCount];
				LOD[] lODs = component.GetLODs();
				for (int i = 0; i < lODs.Length; i++)
				{
					if (lODs[i].renderers == null)
					{
						continue;
					}
					LodInfo lodInfo = new LodInfo();
					lodInfo.lodLevel = i;
					lodInfo.vertexCacheList = new VertexCache[lODs[i].renderers.Length];
					lodInfo.materialBlockList = new MaterialBlock[lodInfo.vertexCacheList.Length];
					List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
					List<MeshRenderer> list2 = new List<MeshRenderer>();
					Renderer[] renderers = lODs[i].renderers;
					foreach (Renderer renderer in renderers)
					{
						if (renderer is SkinnedMeshRenderer)
						{
							list.Add((SkinnedMeshRenderer)renderer);
						}
						if (renderer is MeshRenderer)
						{
							list2.Add((MeshRenderer)renderer);
						}
					}
					lodInfo.skinnedMeshRenderer = list.ToArray();
					lodInfo.meshRenderer = list2.ToArray();
					lodInfo.meshFilter = null;
					for (int k = 0; k != lODs[i].renderers.Length; k++)
					{
						lODs[i].renderers[k].enabled = false;
					}
					this.lodInfo[i] = lodInfo;
				}
			}
			else
			{
				this.lodInfo = new LodInfo[1];
				LodInfo lodInfo2 = new LodInfo();
				lodInfo2.lodLevel = 0;
				lodInfo2.skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
				lodInfo2.meshRenderer = GetComponentsInChildren<MeshRenderer>();
				lodInfo2.meshFilter = GetComponentsInChildren<MeshFilter>();
				lodInfo2.vertexCacheList = new VertexCache[lodInfo2.skinnedMeshRenderer.Length + lodInfo2.meshRenderer.Length];
				lodInfo2.materialBlockList = new MaterialBlock[lodInfo2.vertexCacheList.Length];
				this.lodInfo[0] = lodInfo2;
				for (int l = 0; l != lodInfo2.meshRenderer.Length; l++)
				{
					lodInfo2.meshRenderer[l].enabled = false;
				}
				for (int m = 0; m != lodInfo2.skinnedMeshRenderer.Length; m++)
				{
					lodInfo2.skinnedMeshRenderer[m].enabled = false;
				}
			}
			if (animator != null)
			{
				animator.enabled = false;
			}
			visible = true;
		}

		private void OnDestroy()
		{
			_ = Core.Instance.BaseModule.StageManager;
			if (parentInstance != null)
			{
				parentInstance.Deattach(this);
				parentInstance = null;
			}
		}

		private void OnEnable()
		{
			playSpeed = 1f;
			visible = true;
			if (listAttachment != null)
			{
				for (int i = 0; i != listAttachment.Count; i++)
				{
					listAttachment[i].gameObject.SetActive(value: true);
				}
			}
		}

		private void OnDisable()
		{
			playSpeed = 0f;
			visible = false;
			if (listAttachment != null)
			{
				for (int i = 0; i != listAttachment.Count; i++)
				{
					listAttachment[i].gameObject.SetActive(value: false);
				}
			}
		}

		public void SetInitialAnimation(string animationName, float normalizedTime = 0f)
		{
			initialAnimationName = animationName;
			initialNormalizedTime = normalizedTime;
		}

		public bool InitializeAnimation(AnimationInstancingManager instancingManager)
		{
			bool result = false;
			if (animationType != AnimationInstanceType.NULL)
			{
				isMeshRender = false;
				InstanceAnimationInstanceInfo instanceAnimationInstanceInfo = AnimationAssetManager.GetInstance().GetAnimationInfo(animationType);
				if (instanceAnimationInstanceInfo != null)
				{
					if (lodInfo[0].skinnedMeshRenderer.Length != 0)
					{
						animationInfo = instanceAnimationInstanceInfo.listAniInfo;
						ExtraBoneInfo extraBoneInfo = instanceAnimationInstanceInfo.extraBoneInfo;
						List<Matrix4x4> list = new List<Matrix4x4>(150);
						Transform[] collection = (allTransforms = RuntimeHelper.MergeBone(lodInfo[0].skinnedMeshRenderer, list));
						if (extraBoneInfo != null)
						{
							List<Transform> list2 = new List<Transform>();
							list2.AddRange(collection);
							Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
							for (int i = 0; i != extraBoneInfo.extraBone.Length; i++)
							{
								for (int j = 0; j != componentsInChildren.Length; j++)
								{
									if (extraBoneInfo.extraBone[i] == componentsInChildren[j].name)
									{
										list2.Add(componentsInChildren[j]);
									}
								}
								list.Add(extraBoneInfo.extraBindPose[i]);
							}
							allTransforms = list2.ToArray();
						}
						instancingManager.AddMeshVertex(animationType, lodInfo, allTransforms, list, bonePerVertex, shadowCastingMode, receiveShadow, layer, instanceAnimationInstanceInfo.baseBindPoses);
						UnityEngine.Object.Destroy(animator);
						searchInfo = new AnimationInstanceInfo();
						comparer = new ComparerHash();
						if (initialAnimationName != null)
						{
							PlayAnimation(initialAnimationName, initialNormalizedTime);
						}
						else
						{
							PlayAnimation(0, initialNormalizedTime);
						}
						result = true;
					}
					else
					{
						isMeshRender = true;
						instancingManager.AddMeshVertex(animationType, lodInfo, null, null, bonePerVertex, shadowCastingMode, receiveShadow, layer, instanceAnimationInstanceInfo.baseBindPoses);
						result = true;
					}
				}
			}
			else
			{
				Debug.LogError("The AnimationType is NULL. Please select a valid AnimationType.");
			}
			return result;
		}

		public bool IsPlaying()
		{
			return speedParameter != 0f;
		}

		public float GetNormalizedTime()
		{
			float result = 0f;
			if (speedParameter != 0f && playSpeed != 0f && aniIndex > 0)
			{
				result = Mathf.Clamp01(curFrame / (float)animationInfo[aniIndex].totalFrame);
			}
			return result;
		}

		public void PlayAnimation(string name, float normalizedTime)
		{
			int num = (int)normalizedTime;
			float normalizedTime2 = normalizedTime - (float)num;
			int hashCode = name.GetHashCode();
			int animationIndex = FindAnimationInfo(hashCode);
			PlayAnimation(animationIndex, normalizedTime2);
		}

		public void PlayAnimation(string name)
		{
			int hashCode = name.GetHashCode();
			int animationIndex = FindAnimationInfo(hashCode);
			PlayAnimation(animationIndex);
		}

		public void PlayAnimation(int animationIndex, float normalizedTime = 0f)
		{
			if (animationInfo != null)
			{
				transitionDuration = 0f;
				transitionProgress = 1f;
				isInTransition = false;
				if (animationIndex >= 0 && animationIndex < animationInfo.Length)
				{
					preAniIndex = aniIndex;
					aniIndex = animationIndex;
					preAniFrame = (int)(curFrame + 0.5f);
					curFrame = normalizedTime * (float)animationInfo[aniIndex].totalFrame;
					eventIndex = -1;
					aniTextureIndex = animationInfo[aniIndex].textureIndex;
					speedParameter = 1f;
					RefreshAttachmentAnimation(aniTextureIndex);
				}
				else
				{
					Debug.LogWarning("The requested animation index is out of bounds");
				}
			}
		}

		public void CrossFade(string animationName, float duration)
		{
			int hashCode = animationName.GetHashCode();
			int animationIndex = FindAnimationInfo(hashCode);
			CrossFade(animationIndex, duration);
		}

		public void CrossFade(int animationIndex, float duration)
		{
			PlayAnimation(animationIndex);
			if (duration > 0f)
			{
				isInTransition = true;
				transitionTimer = 0f;
				transitionProgress = 0f;
			}
			else
			{
				transitionProgress = 1f;
			}
			transitionDuration = duration;
		}

		public void Pause()
		{
			cacheParameter = speedParameter;
			speedParameter = 0f;
		}

		public void Resume()
		{
			speedParameter = cacheParameter;
		}

		public void Stop()
		{
			aniIndex = -1;
			preAniIndex = -1;
			eventIndex = -1;
			curFrame = 0f;
		}

		public AnimationInstanceInfo GetCurrentAnimationInfo()
		{
			AnimationInstanceInfo result = null;
			if (animationInfo != null && 0 <= aniIndex && aniIndex < animationInfo.Length)
			{
				result = animationInfo[aniIndex];
			}
			return result;
		}

		public AnimationInstanceInfo GetPreAnimationInfo()
		{
			AnimationInstanceInfo result = null;
			if (animationInfo != null && 0 <= preAniIndex && preAniIndex < animationInfo.Length)
			{
				result = animationInfo[preAniIndex];
			}
			return result;
		}

		public void UpdateAnimation()
		{
			if (animationInfo == null || speedParameter == 0f)
			{
				return;
			}
			if (isInTransition)
			{
				transitionTimer += Time.deltaTime;
				float a = transitionTimer / transitionDuration;
				transitionProgress = Mathf.Min(a, 1f);
				if (transitionProgress >= 1f)
				{
					isInTransition = false;
					preAniIndex = -1;
					preAniFrame = -1f;
				}
			}
			float num = playSpeed * speedParameter;
			curFrame += num * Time.deltaTime * (float)animationInfo[aniIndex].fps;
			int totalFrame = animationInfo[aniIndex].totalFrame;
			switch (animationInfo[aniIndex].wrapMode)
			{
			case WrapMode.Loop:
				if (curFrame < 0f)
				{
					curFrame += totalFrame - 1;
				}
				else if (curFrame > (float)(totalFrame - 1))
				{
					curFrame -= totalFrame - 1;
				}
				break;
			case WrapMode.PingPong:
				if (curFrame < 0f)
				{
					speedParameter = Mathf.Abs(speedParameter);
					curFrame = Mathf.Abs(curFrame);
				}
				else if (curFrame > (float)(totalFrame - 1))
				{
					speedParameter = 0f - Mathf.Abs(speedParameter);
					curFrame = (float)(2 * (totalFrame - 1)) - curFrame;
				}
				break;
			case WrapMode.Default:
			case WrapMode.Once:
				if (curFrame < 0f || curFrame > (float)totalFrame - 1f)
				{
					Pause();
				}
				break;
			}
			curFrame = Mathf.Clamp(curFrame, 0f, totalFrame - 1);
			for (int i = 0; i != listAttachment.Count; i++)
			{
				AnimationInstancing animationInstancing = listAttachment[i];
				animationInstancing.transform.position = base.transform.position;
				animationInstancing.transform.rotation = base.transform.rotation;
			}
			UpdateAnimationEvent();
		}

		public void UpdateLod(Vector3 cameraPosition)
		{
			lodFrequencyCount += Time.deltaTime;
			if (lodFrequencyCount > lodCalculateFrequency)
			{
				float sqrMagnitude = (cameraPosition - worldTransform.position).sqrMagnitude;
				if (sqrMagnitude < 50f)
				{
					lodLevel = 0;
				}
				else if (sqrMagnitude < 500f)
				{
					lodLevel = 1;
				}
				else
				{
					lodLevel = 2;
				}
				lodFrequencyCount = 0f;
				lodLevel = Mathf.Clamp(lodLevel, 0, lodInfo.Length - 1);
			}
		}

		private void UpdateAnimationEvent()
		{
			AnimationInstanceInfo currentAnimationInfo = GetCurrentAnimationInfo();
			if (currentAnimationInfo == null || currentAnimationInfo.eventList.Count <= 0)
			{
				return;
			}
			float num = curFrame / (float)currentAnimationInfo.fps;
			if (aniEvent == null)
			{
				for (int i = ((eventIndex >= 0) ? eventIndex : 0); i < currentAnimationInfo.eventList.Count; i++)
				{
					if (currentAnimationInfo.eventList[i].time > num)
					{
						aniEvent = currentAnimationInfo.eventList[i];
						eventIndex = i;
						break;
					}
				}
			}
			else if (aniEvent.time <= num)
			{
				base.gameObject.SendMessage(aniEvent.function, aniEvent);
				aniEvent = null;
			}
		}

		private int FindAnimationInfo(int hash)
		{
			int result = -1;
			if (animationInfo != null)
			{
				searchInfo.animationNameHash = hash;
				result = Array.BinarySearch(animationInfo, searchInfo, comparer);
			}
			return result;
		}

		public void Deattach(AnimationInstancing attachment)
		{
			attachment.visible = false;
			attachment.parentInstance = null;
			RefreshAttachmentAnimation(-1);
			listAttachment.Remove(attachment);
		}

		private void RefreshAttachmentAnimation(int index)
		{
			for (int i = 0; i < listAttachment.Count; i++)
			{
				AnimationInstancing animationInstancing = listAttachment[i];
				for (int j = 0; j < animationInstancing.lodInfo.Length; j++)
				{
					LodInfo lodInfo = animationInstancing.lodInfo[j];
					for (int k = 0; k < lodInfo.meshRenderer.Length; k++)
					{
						lodInfo.vertexCacheList[lodInfo.skinnedMeshRenderer.Length + k].boneTextureIndex = index;
					}
				}
			}
		}
	}
}
