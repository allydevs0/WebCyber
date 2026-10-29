using System;
using Rewired;
using UnityEngine;

namespace Reptile
{
	public static class RewiredUtility
	{
		public static Rewired.Player GetRewiredPlayerByPlayerId(int playerId)
		{
			if (!ReInput.isReady)
			{
				HandleRewiredNotReady();
				return null;
			}
			if (playerId >= ReInput.players.allPlayerCount)
			{
				return null;
			}
			return ReInput.players.GetPlayer(playerId);
		}

		public static int GetDefaultCategoryForControllerType(ControllerType controllerType)
		{
			return controllerType switch
			{
				ControllerType.Joystick => 0, 
				ControllerType.Mouse => 0, 
				_ => 0, 
			};
		}

		private static void HandleRewiredNotReady()
		{
		}

		public static void PrintAllRewiredDebugInformation()
		{
		}

		public static void PrintAllRewiredConnectedControllers()
		{
			Debug.Log("Rewired found " + ReInput.controllers.joystickCount + " joysticks attached.");
			for (int i = 0; i < ReInput.controllers.joystickCount; i++)
			{
				Joystick joystick = ReInput.controllers.Joysticks[i];
				Debug.Log("[" + i + "] Joystick: " + joystick.name + "\nHardware Name: " + joystick.hardwareName + "\nIs Recognized: " + ((joystick.hardwareTypeGuid != Guid.Empty) ? "Yes" : "No") + "\nIs Assigned: " + (ReInput.controllers.IsControllerAssigned(joystick.type, joystick) ? "Yes" : "No"));
			}
		}

		public static void PrintAllRewiredPlayersInfo()
		{
			foreach (Rewired.Player player in ReInput.players.Players)
			{
				Debug.Log("PlayerId = " + player.id + " is assigned " + player.controllers.joystickCount + " joysticks.");
				foreach (Joystick joystick in player.controllers.Joysticks)
				{
					Debug.Log("Joystick: " + joystick.name + "\nIs Recognized: " + ((joystick.hardwareTypeGuid != Guid.Empty) ? "Yes" : "No"));
					PrintAllRewiredPlayerControllerMaps(player, joystick);
				}
			}
		}

		public static void PrintAllRewiredPlayerControllerMaps(Rewired.Player player, Controller controller)
		{
			Debug.Log("Controller type: " + controller.type);
			int id = controller.id;
			Debug.Log("Controller type: " + id);
			foreach (ControllerMap map in player.controllers.maps.GetMaps(controller.type, controller.id))
			{
				Debug.Log("Controller Map:\nCategory = " + ReInput.mapping.GetMapCategory(map.categoryId).name + "\nLayout = " + ReInput.mapping.GetJoystickLayout(map.layoutId).name + "\nenabled = " + map.enabled);
				PrintAllRewiredPlayerControllerActionElementMaps(map);
			}
		}

		public static void PrintAllRewiredPlayerControllerActionElementMaps(ControllerMap controllerMap)
		{
			ActionElementMap[] elementMaps = controllerMap.GetElementMaps();
			foreach (ActionElementMap actionElementMap in elementMaps)
			{
				InputAction action = ReInput.mapping.GetAction(actionElementMap.actionId);
				if (action != null)
				{
					Debug.Log("Action \"" + action.name + "\" is bound to \"" + actionElementMap.elementIdentifierName + "\"");
				}
			}
		}
	}
}
