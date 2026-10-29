using System;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	[CreateAssetMenu(fileName = "DownloadContentItem", menuName = "Reptile/DLC/Downloadable content item")]
	public class DownloadableContentItem : ScriptableObject
	{
		[SerializeField]
		private string dlcName = string.Empty;

		[SerializeField]
		private uint steamAppID;

		[SerializeField]
		private int switchAocIndex = -1;

		[SerializeField]
		private string playstationEntitlementKey = string.Empty;

		[SerializeField]
		private string playstationContentLabel = string.Empty;

		[SerializeField]
		private string xboxDLCID = string.Empty;

		[SerializeField]
		private ulong gogGalaxyProductID;

		public uint SteamAppID => steamAppID;

		public int SwitchAocIndex => switchAocIndex;

		public string XboxDLC => xboxDLCID;

		public string PlaystationEntitlementKey => playstationEntitlementKey;

		public string PlaystationContentLabel => playstationContentLabel;

		public string DlcName => dlcName;

		public ulong GogGalaxyProductID => gogGalaxyProductID;
	}
}
