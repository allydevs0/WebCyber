using UnityEngine;

namespace Reptile
{
	public class GameVersion : ScriptableObject
	{
		public int major;

		public int minor;

		public int repositoryRevision;

		public const string GAME_VERSION_ASSET_PATH = "Assets/AutomatedBuilds/Assets";

		public const string GAME_VERSION_ASSET_NAME = "GameVersion.asset";

		public override string ToString()
		{
			return major + "." + minor + "." + repositoryRevision;
		}
	}
}
