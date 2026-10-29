using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "InputControllerGuidData", menuName = "Reptile/Input/Input Controller Guid Data", order = 0)]
	public class InputControllerGuidData : ScriptableObject
	{
		public string dualShock4Guid = string.Empty;

		public string dualSenseGuid = string.Empty;

		public string xbox360Guid = string.Empty;

		public string xboxOneGuid = string.Empty;

		public string steamControllerGuid = string.Empty;

		public string switchProControllerGuid = string.Empty;

		private Dictionary<ESteamInputType, Guid> steamInputTypeGuids;

		public void GenerateSteamInputTypesDictionary()
		{
			steamInputTypeGuids = new Dictionary<ESteamInputType, Guid>();
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_PS3Controller, new Guid(dualShock4Guid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_PS4Controller, new Guid(dualShock4Guid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_PS5Controller, new Guid(dualSenseGuid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_GenericGamepad, new Guid(xbox360Guid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_XBox360Controller, new Guid(xbox360Guid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_XBoxOneController, new Guid(xboxOneGuid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_SteamController, new Guid(steamControllerGuid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_SwitchProController, new Guid(switchProControllerGuid));
			steamInputTypeGuids.Add(ESteamInputType.k_ESteamInputType_SteamDeckController, new Guid(steamControllerGuid));
		}

		public Guid GetGuidForSteamInputType(ESteamInputType steamInputType)
		{
			Guid value = Guid.Empty;
			steamInputTypeGuids.TryGetValue(steamInputType, out value);
			return value;
		}
	}
}
