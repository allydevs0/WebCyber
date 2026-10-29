using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class AnimationAssetManager : ASceneAssetLoader
	{
		public const string ANIMATION_BUNDLE_NAME = "characters";

		private InstanceAnimationInstanceInfo[] animationInfos;

		internal AnimationTexture[] animationTextures;

		public AnimationAssetManager(Assets assets)
			: base(assets)
		{
		}

		public static AnimationAssetManager GetInstance()
		{
			return Core.Instance.Assets.GetAssetsLoader<AnimationAssetManager>();
		}

		public InstanceAnimationInstanceInfo GetAnimationInfo(AnimationInstanceType type)
		{
			InstanceAnimationInstanceInfo result = null;
			if (type != AnimationInstanceType.NULL)
			{
				result = animationInfos[(int)type];
			}
			return result;
		}

		private AnimationInstanceInfo[] ReadAnimationInfo(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			AnimationInstanceInfo[] array = new AnimationInstanceInfo[num];
			for (int i = 0; i < num; i++)
			{
				AnimationInstanceInfo animationInstanceInfo = new AnimationInstanceInfo();
				animationInstanceInfo.animationName = reader.ReadString();
				animationInstanceInfo.animationNameHash = animationInstanceInfo.animationName.GetHashCode();
				animationInstanceInfo.animationIndex = reader.ReadInt32();
				animationInstanceInfo.textureIndex = reader.ReadInt32();
				animationInstanceInfo.totalFrame = reader.ReadInt32();
				animationInstanceInfo.fps = reader.ReadInt32();
				animationInstanceInfo.rootMotion = reader.ReadBoolean();
				animationInstanceInfo.wrapMode = (WrapMode)reader.ReadInt32();
				if (animationInstanceInfo.rootMotion)
				{
					animationInstanceInfo.velocity = new Vector3[animationInstanceInfo.totalFrame];
					animationInstanceInfo.angularVelocity = new Vector3[animationInstanceInfo.totalFrame];
					for (int j = 0; j < animationInstanceInfo.totalFrame; j++)
					{
						animationInstanceInfo.velocity[j].x = reader.ReadSingle();
						animationInstanceInfo.velocity[j].y = reader.ReadSingle();
						animationInstanceInfo.velocity[j].z = reader.ReadSingle();
						animationInstanceInfo.angularVelocity[j].x = reader.ReadSingle();
						animationInstanceInfo.angularVelocity[j].y = reader.ReadSingle();
						animationInstanceInfo.angularVelocity[j].z = reader.ReadSingle();
					}
				}
				int num2 = reader.ReadInt32();
				animationInstanceInfo.eventList = new List<AnimationInstanceEvent>();
				for (int k = 0; k < num2; k++)
				{
					AnimationInstanceEvent animationInstanceEvent = new AnimationInstanceEvent();
					animationInstanceEvent.function = reader.ReadString();
					animationInstanceEvent.floatParameter = reader.ReadSingle();
					animationInstanceEvent.intParameter = reader.ReadInt32();
					animationInstanceEvent.stringParameter = reader.ReadString();
					animationInstanceEvent.time = reader.ReadSingle();
					animationInstanceEvent.objectParameter = reader.ReadString();
					animationInstanceInfo.eventList.Add(animationInstanceEvent);
				}
				array[i] = animationInstanceInfo;
			}
			Array.Sort(array, new ComparerHash());
			return array;
		}

		private ExtraBoneInfo ReadExtraBoneInfo(BinaryReader reader)
		{
			ExtraBoneInfo extraBoneInfo = null;
			if (reader.ReadBoolean())
			{
				int num = reader.ReadInt32();
				extraBoneInfo = new ExtraBoneInfo();
				extraBoneInfo.extraBone = new string[num];
				extraBoneInfo.extraBindPose = new Matrix4x4[num];
				for (int i = 0; i < num; i++)
				{
					extraBoneInfo.extraBone[i] = reader.ReadString();
				}
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < 16; k++)
					{
						extraBoneInfo.extraBindPose[j][k] = reader.ReadSingle();
					}
				}
			}
			return extraBoneInfo;
		}

		private void ReadAnimationTextures(BinaryReader reader, int typeIndex)
		{
			TextureFormat textureFormat = TextureFormat.RGBAHalf;
			int num = reader.ReadInt32();
			int blockWidth = reader.ReadInt32();
			int blockHeight = reader.ReadInt32();
			AnimationTexture animationTexture = new AnimationTexture();
			animationTexture.boneTexture = new Texture2D[num];
			animationTexture.type = (AnimationInstanceType)typeIndex;
			animationTexture.blockWidth = blockWidth;
			animationTexture.blockHeight = blockHeight;
			animationTextures[typeIndex] = animationTexture;
			for (int i = 0; i != num; i++)
			{
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();
				int num2 = reader.ReadInt32();
				byte[] array = new byte[num2];
				array = reader.ReadBytes(num2);
				Texture2D texture2D = new Texture2D(width, height, textureFormat, mipChain: false);
				texture2D.LoadRawTextureData(array);
				texture2D.filterMode = FilterMode.Point;
				texture2D.Apply();
				animationTexture.boneTexture[i] = texture2D;
			}
		}

		private InstanceAnimationInstanceInfo ReadOutAnimationInfo(TextAsset textAsset, int typeIndex)
		{
			InstanceAnimationInstanceInfo instanceAnimationInstanceInfo = null;
			if (textAsset != null)
			{
				instanceAnimationInstanceInfo = new InstanceAnimationInstanceInfo();
				BinaryReader binaryReader = new BinaryReader(new MemoryStream(textAsset.bytes));
				instanceAnimationInstanceInfo.listAniInfo = ReadAnimationInfo(binaryReader);
				instanceAnimationInstanceInfo.extraBoneInfo = ReadExtraBoneInfo(binaryReader);
				int num = binaryReader.ReadInt32();
				instanceAnimationInstanceInfo.baseBindPoses = new Matrix4x4[num];
				for (int i = 0; i < num; i++)
				{
					for (int j = 0; j < 16; j++)
					{
						instanceAnimationInstanceInfo.baseBindPoses[i][j] = binaryReader.ReadSingle();
					}
				}
				ReadAnimationTextures(binaryReader, typeIndex);
				binaryReader.Close();
			}
			else
			{
				Debug.Log($"[AnimationAssetManager] Can't find animation info file for {(AnimationInstanceType)typeIndex}.\n" + "Make sure you added the animation info files to the correct asset bundle!");
			}
			return instanceAnimationInstanceInfo;
		}

		private void AllocateAnimationAssets()
		{
			animationInfos = new InstanceAnimationInstanceInfo[5];
			animationInfos[0] = null;
			animationTextures = new AnimationTexture[5];
			animationTextures[0] = null;
		}

		public override void LoadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				AllocateAnimationAssets();
				for (int i = 1; i < 5; i++)
				{
					AnimationInstanceType animationInstanceType = (AnimationInstanceType)i;
					string assetName = animationInstanceType.ToString();
					TextAsset textAsset = assets.LoadAssetFromBundle<TextAsset>("characters", assetName);
					animationInfos[i] = ReadOutAnimationInfo(textAsset, i);
				}
			}
		}

		public override IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				AllocateAnimationAssets();
				for (int typeIndex = 1; typeIndex < 5; typeIndex++)
				{
					AnimationInstanceType animationInstanceType = (AnimationInstanceType)typeIndex;
					string assetName = animationInstanceType.ToString();
					AssetBundleRequest textAssetRequest = assets.LoadAssetFromBundleASync<TextAsset>("characters", assetName);
					yield return textAssetRequest;
					TextAsset textAsset = textAssetRequest.asset as TextAsset;
					animationInfos[typeIndex] = ReadOutAnimationInfo(textAsset, typeIndex);
				}
			}
		}

		public override void LoadAssetsEditor(AssetsToLoadData assetsToLoadData)
		{
		}

		public override void UnloadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadCharacters)
			{
				animationInfos = null;
			}
		}
	}
}
