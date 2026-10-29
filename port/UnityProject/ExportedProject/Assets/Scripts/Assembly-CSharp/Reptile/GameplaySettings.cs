using System;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class GameplaySettings : ISaveable
	{
		private const int GAMEPLAY_SETTINGS_VERSION = 1;

		public const bool DEFAULT_RUMBLE_SETTING = true;

		public const float DEFAULT_AIM_SENSITIVITY = 0.5f;

		public const bool DEFAULT_INVERT_Y_AXIS_SETTING = false;

		public const bool DEFAULT_FRAME_RATE_LOCK_SETTING = false;

		public SystemLanguage currentLanguage = SystemLanguage.English;

		public bool invertY;

		public float aimSensitivity = 0.5f;

		public bool rumble = true;

		public bool frameRateLock;

		public void Write(BinaryWriter writer)
		{
			writer.Write(1);
			writer.Write(currentLanguage.ToString());
			writer.Write(invertY);
			writer.Write(aimSensitivity);
			writer.Write(rumble);
		}

		public void Read(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			if (num >= 0)
			{
				Enum.TryParse<SystemLanguage>(reader.ReadString(), out currentLanguage);
			}
			if (num >= 1)
			{
				invertY = reader.ReadBoolean();
				aimSensitivity = reader.ReadSingle();
				rumble = reader.ReadBoolean();
			}
		}
	}
}
