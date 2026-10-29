using System;
using System.Collections.Generic;
using System.Reflection;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reptile
{
	public class RewiredControllerHandler : IDisposable
	{
		public delegate void GameInputLastActiveControllerChangedDelegate(int playerid, ControllerType controllerType);

		private const int NO_PLAYER_ID = -1;

		private const int LEFT_VIBRATION_MOTOR_INDEX = 0;

		private const int RIGHT_VIBRATION_MOTOR_INDEX = 1;

		private List<int> listeningForPlayers;

		private Rewired.Player firstRewiredPlayer;

		private IRewiredControllerGuidHandler rewiredControllerGuidHandler;

		private APlatform platform;

		private event GameInputLastActiveControllerChangedDelegate OnGameInputLastActiveControllerChanged;

		public RewiredControllerHandler(Rewired.Player firstRewiredPlayer, IRewiredControllerGuidHandler rewiredControllerGuidHandler, APlatform platform)
		{
			listeningForPlayers = new List<int>();
			this.firstRewiredPlayer = firstRewiredPlayer;
			this.rewiredControllerGuidHandler = rewiredControllerGuidHandler;
			this.platform = platform;
		}

		public void Dispose()
		{
			DisposeControllerListeners();
			listeningForPlayers.Clear();
			listeningForPlayers = null;
			firstRewiredPlayer = null;
		}

		public bool IsAnyJoystickConnected()
		{
			if (!ReInput.isReady)
			{
				return false;
			}
			return ReInput.controllers.joystickCount > 0;
		}

		public Guid GetPlayerCurrentControllerTypeGuid(ControllerType controllerType, int playerId = 0)
		{
			if (GetRewiredPlayer(playerId) == null)
			{
				return Guid.Empty;
			}
			Controller playerCurrentControllerByType = GetPlayerCurrentControllerByType(controllerType, playerId);
			if (playerCurrentControllerByType == null)
			{
				return rewiredControllerGuidHandler.GetDefaultControllerGuid();
			}
			return rewiredControllerGuidHandler.GetControllerGuid(playerCurrentControllerByType);
		}

		public Controller GetPlayerCurrentController(int playerId = 0)
		{
			Controller controller = GetPlayerLastActiveController(playerId);
			if (controller == null)
			{
				controller = GetPlayerFirstAssignedController(playerId);
			}
			return controller;
		}

		public int GetPlayerCurrentControllerId(ControllerType controllerType, int playerId = 0)
		{
			return GetPlayerCurrentControllerByType(controllerType, playerId)?.id ?? (-1);
		}

		public Controller GetPlayerCurrentControllerByType(ControllerType controllerType, int playerId = 0)
		{
			Controller controller = GetPlayerLastActiveControllerByType(controllerType, playerId);
			if (controller == null)
			{
				controller = GetPlayerFirstAssignedControllerByType(controllerType, playerId);
			}
			return controller;
		}

		public Controller GetPlayerLastActiveController(int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.controllers.GetLastActiveController();
		}

		public Controller GetPlayerFirstAssignedController(int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return null;
			}
			if (platform.PlatformDefaultControllerType == ControllerType.Joystick)
			{
				return GetPlayerFirstJoystickController(rewiredPlayer);
			}
			return GetPlayerKeyboardController(rewiredPlayer);
		}

		public Controller GetPlayerLastActiveControllerByType(ControllerType controllerType, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.controllers.GetLastActiveController(controllerType);
		}

		public Controller GetPlayerFirstAssignedControllerByType(ControllerType controllerType, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return null;
			}
			return controllerType switch
			{
				ControllerType.Joystick => GetPlayerFirstJoystickController(rewiredPlayer), 
				ControllerType.Keyboard => GetPlayerKeyboardController(rewiredPlayer), 
				ControllerType.Mouse => GetPlayerMouseController(rewiredPlayer), 
				_ => null, 
			};
		}

		public Joystick GetPlayerCurrentJoystick(int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer == null)
			{
				return null;
			}
			if (rewiredPlayer.controllers.joystickCount <= 0)
			{
				return null;
			}
			Controller playerCurrentControllerByType = GetPlayerCurrentControllerByType(ControllerType.Joystick, playerId);
			if (playerCurrentControllerByType == null)
			{
				return null;
			}
			foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
			{
				if (joystick.id == playerCurrentControllerByType.id)
				{
					return joystick;
				}
			}
			return null;
		}

		public void AddListenerForLastActiveControllerChanged(GameInputLastActiveControllerChangedDelegate playerActiveControllerChangedDelegate, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer != null)
			{
				AddLastActiveControllerChangedListener(playerActiveControllerChangedDelegate, rewiredPlayer);
			}
		}

		public void RemoveListenerForLastActiveControllerChanged(GameInputLastActiveControllerChangedDelegate playerActiveControllerChangedDelegate, int playerId = 0)
		{
			Rewired.Player rewiredPlayer = GetRewiredPlayer(playerId);
			if (rewiredPlayer != null)
			{
				RemoveLastActiveControllerChangedListener(playerActiveControllerChangedDelegate, rewiredPlayer);
			}
		}

		public IEnumerable<ControllerMap> GetAllPlayerControllerMaps(int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.controllers.maps.GetAllMaps();
		}

		public ControllerMap GetControllerMap(int controllerMapCategoryId, Controller controller, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.controllers.maps.GetFirstMapInCategory(controller, controllerMapCategoryId);
		}

		public ControllerMap GetControllerMapByType(int controllerMapCategoryId, Controller controller, ControllerType controllerType, int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.controllers.maps.GetFirstMapInCategory(controllerType, controller.id, controllerMapCategoryId);
		}

		public Rewired.Player.ControllerHelper GetPlayerControllerHelper(int playerId = 0)
		{
			return GetRewiredPlayer(playerId)?.controllers;
		}

		public int GetAssignedPlayerIDForController(Controller controller)
		{
			int allPlayerCount = ReInput.players.allPlayerCount;
			IList<Rewired.Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayerCount; i++)
			{
				if (allPlayers[i].controllers.ContainsController(controller))
				{
					return allPlayers[i].id;
				}
			}
			return -1;
		}

		public void SetVibrationOnJoystick(Joystick joystick, float leftMotor, float rightMotor, float duration)
		{
			if (joystick != null && joystick.supportsVibration)
			{
				if (joystick.vibrationMotorCount > 0)
				{
					joystick.SetVibration(0, leftMotor, duration);
				}
				if (joystick.vibrationMotorCount > 1)
				{
					joystick.SetVibration(1, rightMotor, duration);
				}
			}
		}

		public void SetVibrationOnCurrentJoystick(float leftMotor, float rightMotor, float duration, int playerId = 0)
		{
			Joystick playerCurrentJoystick = GetPlayerCurrentJoystick(playerId);
			if (playerCurrentJoystick != null)
			{
				SetVibrationOnJoystick(playerCurrentJoystick, leftMotor, rightMotor, duration);
			}
		}

		public void StopVibrationOnCurrentJoystick(int playerId = 0)
		{
			Joystick playerCurrentJoystick = GetPlayerCurrentJoystick(playerId);
			if (playerCurrentJoystick != null)
			{
				StopVibrationOnJoystick(playerCurrentJoystick);
			}
		}

		public void StopVibrationOnJoystick(Joystick joystick)
		{
			if (joystick != null && joystick.supportsVibration)
			{
				joystick.StopVibration();
			}
		}

		private void DisposeControllerListeners()
		{
			for (int i = 0; i < listeningForPlayers.Count; i++)
			{
				DisposeControllerListenerForPlayer(listeningForPlayers[i]);
			}
			if (OnGameInputLastActiveControllerChanged != null)
			{
				Delegate[] invocationList = OnGameInputLastActiveControllerChanged.GetInvocationList();
				for (int j = 0; j < invocationList.Length; j++)
				{
					OnGameInputLastActiveControllerChanged -= invocationList[j] as GameInputLastActiveControllerChangedDelegate;
				}
			}
		}

		private void DisposeControllerListenerForPlayer(int playerId)
		{
			GetRewiredPlayer(playerId)?.controllers.RemoveLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
		}

		private Controller GetPlayerFirstJoystickController(Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			if (rewiredPlayer.controllers.joystickCount > 0)
			{
				return rewiredPlayer.controllers.Joysticks[0];
			}
			return null;
		}

		private Controller GetPlayerKeyboardController(Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			if (!ReInput.configuration.nativeKeyboardSupport)
			{
				return null;
			}
			return rewiredPlayer.controllers.Keyboard;
		}

		private Controller GetPlayerMouseController(Rewired.Player rewiredPlayer)
		{
			if (rewiredPlayer == null)
			{
				return null;
			}
			if (!ReInput.configuration.nativeKeyboardSupport)
			{
				return null;
			}
			return rewiredPlayer.controllers.Mouse;
		}

		private GameObject FindSelectableInPointerEnterGameObject(PlayerPointerEventData pointerData)
		{
			GameObject result = null;
			if (pointerData != null)
			{
				if (pointerData.pointerEnter != null)
				{
					if ((bool)pointerData.pointerEnter.GetComponentInChildren<Selectable>())
					{
						result = pointerData.pointerEnter;
					}
					else
					{
						Transform parent = pointerData.pointerEnter.transform.parent;
						if (parent != null)
						{
							Selectable component = parent.GetComponent<Selectable>();
							if (component != null)
							{
								result = component.gameObject;
							}
						}
					}
				}
				else
				{
					result = pointerData.pointerEnter;
				}
			}
			return result;
		}

		private void OnLastActiveControllerChanged(Rewired.Player rewiredPlayer, Controller controller)
		{
			if (rewiredPlayer != null && controller != null)
			{
				if (OnGameInputLastActiveControllerChanged != null)
				{
					OnGameInputLastActiveControllerChanged(rewiredPlayer.id, controller.type);
				}
				EventSystem current = EventSystem.current;
				if (controller.type == ControllerType.Mouse && current != null && current.currentSelectedGameObject != null)
				{
					PlayerPointerEventData playerPointerEventData = null;
					MethodInfo method = typeof(RewiredPointerInputModule).GetMethod("GetPointerData", BindingFlags.Instance | BindingFlags.NonPublic);
					object[] array = new object[6]
					{
						9999999,
						0,
						-1,
						playerPointerEventData,
						false,
						PointerEventType.Mouse
					};
					method.Invoke(current.currentInputModule, array);
					playerPointerEventData = (PlayerPointerEventData)array[3];
					GameObject selected = FindSelectableInPointerEnterGameObject(playerPointerEventData);
					current.SetSelectedGameObject(selected, playerPointerEventData);
				}
			}
		}

		private void AddLastActiveControllerChangedListener(GameInputLastActiveControllerChangedDelegate playerActiveControllerChangedDelegate, Rewired.Player rewiredPlayer)
		{
			if (playerActiveControllerChangedDelegate != null && rewiredPlayer != null)
			{
				OnGameInputLastActiveControllerChanged += playerActiveControllerChangedDelegate;
				if (!listeningForPlayers.Contains(rewiredPlayer.id))
				{
					rewiredPlayer.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
					listeningForPlayers.Add(rewiredPlayer.id);
				}
			}
		}

		private void RemoveLastActiveControllerChangedListener(GameInputLastActiveControllerChangedDelegate playerActiveControllerChangedDelegate, Rewired.Player rewiredPlayer)
		{
			if (playerActiveControllerChangedDelegate != null && rewiredPlayer != null)
			{
				OnGameInputLastActiveControllerChanged -= playerActiveControllerChangedDelegate;
				if (listeningForPlayers.Contains(rewiredPlayer.id) && OnGameInputLastActiveControllerChanged == null)
				{
					rewiredPlayer.controllers.RemoveLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
					listeningForPlayers.Remove(rewiredPlayer.id);
				}
			}
		}

		private Rewired.Player GetRewiredPlayer(int playerId = 0)
		{
			if (playerId == 0)
			{
				return firstRewiredPlayer;
			}
			return RewiredUtility.GetRewiredPlayerByPlayerId(playerId);
		}
	}
}
