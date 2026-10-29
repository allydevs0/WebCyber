using System;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class VideoSettings : ISaveable
	{
		public struct ShadowsQualityPreset
		{
			public float shadowDistance;

			public ShadowResolution shadowResolution;

			public ShadowQuality shadowQuality;

			public ShadowmaskMode shadowmaskMode;

			public int shadowCascades;
		}

		private const int VIDEO_SETTINGS_VERSION = 0;

		public FullScreenMode fullScreenMode;

		private Resolution resolution;

		public int vSyncCount = 1;

		public int qualityPreset;

		public int antiAliasing = 4;

		public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Enable;

		public int shadows = 3;

		public float shadowDistance = 500f;

		public float lodDistance = 2f;

		public ShadowResolution shadowResolution = ShadowResolution.VeryHigh;

		public ShadowQuality shadowQuality = ShadowQuality.All;

		public ShadowmaskMode shadowmaskMode = ShadowmaskMode.DistanceShadowmask;

		public int shadowCascades = 4;

		public int ResolutionHeight => GetResolution().height;

		public int ResolutionWidth => GetResolution().width;

		public Resolution GetResolution()
		{
			Resolution currentResolution = resolution;
			if (currentResolution.height == 0 || currentResolution.width == 0)
			{
				currentResolution = Screen.currentResolution;
			}
			return currentResolution;
		}

		public void SetResolution(Resolution resolution)
		{
			this.resolution = resolution;
		}

		public void SetResolutionDimensions(int width, int height)
		{
			resolution.width = width;
			resolution.height = height;
		}

		public ShadowsQualityPreset GetShadowsQualityPreset()
		{
			return new ShadowsQualityPreset
			{
				shadowDistance = shadowDistance,
				shadowResolution = shadowResolution,
				shadowQuality = shadowQuality,
				shadowmaskMode = shadowmaskMode,
				shadowCascades = shadowCascades
			};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(fullScreenMode.ToString());
			writer.Write(resolution.height);
			writer.Write(resolution.width);
			writer.Write(resolution.refreshRate);
			writer.Write(vSyncCount);
			writer.Write(qualityPreset);
			writer.Write(antiAliasing);
			writer.Write(anisotropicFiltering.ToString());
			writer.Write(shadows);
			writer.Write(shadowDistance);
			writer.Write(lodDistance);
			writer.Write((int)shadowResolution);
			writer.Write((int)shadowQuality);
			writer.Write((int)shadowmaskMode);
			writer.Write(shadowCascades);
		}

		public void Read(BinaryReader reader)
		{
			if (reader.ReadInt32() >= 0)
			{
				fullScreenMode = (FullScreenMode)Enum.Parse(typeof(FullScreenMode), reader.ReadString());
				resolution = default;
				resolution.height = reader.ReadInt32();
				resolution.width = reader.ReadInt32();
				resolution.refreshRate = reader.ReadInt32();
				vSyncCount = reader.ReadInt32();
				qualityPreset = reader.ReadInt32();
				antiAliasing = reader.ReadInt32();
				anisotropicFiltering = (AnisotropicFiltering)Enum.Parse(typeof(AnisotropicFiltering), reader.ReadString());
				shadows = reader.ReadInt32();
				shadowDistance = reader.ReadSingle();
				lodDistance = reader.ReadSingle();
				shadowResolution = (ShadowResolution)reader.ReadInt32();
				shadowQuality = (ShadowQuality)reader.ReadInt32();
				shadowmaskMode = (ShadowmaskMode)reader.ReadInt32();
				shadowCascades = reader.ReadInt32();
			}
		}
	}
}
