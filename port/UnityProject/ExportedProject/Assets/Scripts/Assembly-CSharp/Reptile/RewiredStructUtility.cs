using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace Reptile
{
	public static class RewiredStructUtility
	{
		public static ElementAssignment CreateElementReassignmentFromPollingInfo(ControllerType controllerType, ControllerPollingInfo info, int actionId, int elementMapId, Pole axisContribution)
		{
			ElementAssignment result = default;
			if (controllerType == ControllerType.Joystick)
			{
				result = CreateJoystickElementReassignment(info.elementType, actionId, info.elementIdentifierId, info.axisPole, elementMapId);
			}
			if (controllerType == ControllerType.Keyboard)
			{
				result = CreateKeyboardElementReassignment(info.keyboardKey, ModifierKeyFlags.None, actionId, axisContribution, elementMapId);
			}
			if (controllerType == ControllerType.Mouse)
			{
				result = CreateMouseElementReassignment(actionId, info.elementIdentifierId, axisContribution, elementMapId);
			}
			return result;
		}

		public static ElementAssignment CreateJoystickElementReassignment(ControllerElementType elementType, int actionId, int elementIdentifierId, Pole axisPole, int elementMapId)
		{
			ElementAssignment result = default;
			if (elementType == ControllerElementType.Button)
			{
				result = ElementAssignment.ButtonAssignment(elementIdentifierId, actionId, axisPole, elementMapId);
			}
			if (elementType == ControllerElementType.Axis)
			{
				result = ElementAssignment.FullAxisAssignment(elementIdentifierId, actionId, invert: false, elementMapId);
			}
			return result;
		}

		public static ElementAssignment CreateKeyboardElementReassignment(KeyCode keyCode, ModifierKeyFlags modifierKeyFlags, int actionId, Pole axisPole, int elementMapId)
		{
			return ElementAssignment.KeyboardKeyAssignment(keyCode, modifierKeyFlags, actionId, axisPole, elementMapId);
		}

		public static ElementAssignment CreateMouseElementReassignment(int actionId, int elementIdentifierId, Pole axisPole, int elementMapId)
		{
			return ElementAssignment.ButtonAssignment(elementIdentifierId, actionId, axisPole, elementMapId);
		}

		public static ElementAssignment CreateNewElementAssignmentFromPollingInfo(ControllerType controllerType, ControllerPollingInfo info, int actionId, Pole axisContribution)
		{
			ElementAssignment result = default;
			if (controllerType == ControllerType.Joystick)
			{
				result = CreateNewJoystickElementAssignment(info.elementType, actionId, info.elementIdentifierId, info.axisPole);
			}
			if (controllerType == ControllerType.Keyboard)
			{
				result = CreateNewKeyboardElementAssignment(info.keyboardKey, ModifierKeyFlags.None, actionId, axisContribution);
			}
			if (controllerType == ControllerType.Mouse)
			{
				result = CreateNewMouseElementAssignment(actionId, info.elementIdentifierId, axisContribution);
			}
			return result;
		}

		public static ElementAssignment CreateNewJoystickElementAssignment(ControllerElementType elementType, int actionId, int elementIdentifierId, Pole axisPole)
		{
			ElementAssignment result = default;
			if (elementType == ControllerElementType.Button)
			{
				result = ElementAssignment.ButtonAssignment(elementIdentifierId, actionId, axisPole);
			}
			if (elementType == ControllerElementType.Axis)
			{
				result = ElementAssignment.FullAxisAssignment(elementIdentifierId, actionId, invert: false);
			}
			return result;
		}

		public static ElementAssignment CreateNewKeyboardElementAssignment(KeyCode keyCode, ModifierKeyFlags modifierKeyFlags, int actionId, Pole axisPole)
		{
			return ElementAssignment.KeyboardKeyAssignment(keyCode, modifierKeyFlags, actionId, axisPole);
		}

		public static ElementAssignment CreateNewMouseElementAssignment(int actionId, int elementIdentifierId, Pole axisPole)
		{
			return ElementAssignment.ButtonAssignment(elementIdentifierId, actionId, axisPole);
		}

		public static ElementAssignmentConflictCheck CreateElementAssignmentConflictCheck(Controller controller, ActionElementMap actionElementMap, int playerId = 0)
		{
			ElementAssignmentConflictCheck result = default;
			if (controller.type == ControllerType.Joystick)
			{
				result = CreateJoystickElementAssignmentConflictCheck(controller, actionElementMap, playerId);
			}
			if (controller.type == ControllerType.Keyboard)
			{
				result = CreateKeyboardElementAssignmentConflictCheck(controller, actionElementMap, playerId);
			}
			if (controller.type == ControllerType.Mouse)
			{
				result = CreateMouseElementAssignmentConflictCheck(controller, actionElementMap, playerId);
			}
			return result;
		}

		public static ElementAssignmentConflictCheck CreateJoystickElementAssignmentConflictCheck(Controller controller, ActionElementMap actionElementMap, int playerId = 0)
		{
			ElementAssignmentConflictCheck result = default;
			if (actionElementMap.elementType == ControllerElementType.Button)
			{
				result = new ElementAssignmentConflictCheck(playerId, controller.type, controller.id, actionElementMap.controllerMap.id, actionElementMap.elementIdentifierId, actionElementMap.actionId, actionElementMap.axisContribution);
			}
			if (actionElementMap.elementType == ControllerElementType.Axis)
			{
				result = new ElementAssignmentConflictCheck(playerId, controller.type, controller.id, actionElementMap.controllerMap.id, actionElementMap.elementIdentifierId, actionElementMap.actionId, false);
			}
			return result;
		}

		public static ElementAssignmentConflictCheck CreateKeyboardElementAssignmentConflictCheck(Controller controller, ActionElementMap actionElementMap, int playerId = 0)
		{
			return new ElementAssignmentConflictCheck(playerId, controller.type, controller.id, actionElementMap.controllerMap.id, actionElementMap.keyCode, actionElementMap.modifierKeyFlags, actionElementMap.actionId, actionElementMap.axisContribution);
		}

		public static ElementAssignmentConflictCheck CreateMouseElementAssignmentConflictCheck(Controller controller, ActionElementMap actionElementMap, int playerId = 0)
		{
			return new ElementAssignmentConflictCheck(playerId, controller.type, controller.id, actionElementMap.controllerMap.id, actionElementMap.elementIdentifierId, actionElementMap.actionId, actionElementMap.axisContribution);
		}

		public static ElementAssignment CreateExistingBindingElementAssignment(GameInput.ControllerActionElementSaveMap controllerActionElementSaveMap, ControllerType controllerType, int elementMapId)
		{
			return new ElementAssignment(controllerType, controllerActionElementSaveMap.elementType, controllerActionElementSaveMap.elementIdentifierId, AxisRange.Full, controllerActionElementSaveMap.keyCode, ModifierKeyFlags.None, controllerActionElementSaveMap.actionId, controllerActionElementSaveMap.axisContribution, false, elementMapId);
		}

		public static ElementAssignment CreateNewBindingElementAssignment(GameInput.ControllerActionElementSaveMap controllerActionElementSaveMap, ControllerType controllerType)
		{
			return new ElementAssignment(controllerType, controllerActionElementSaveMap.elementType, controllerActionElementSaveMap.elementIdentifierId, AxisRange.Full, controllerActionElementSaveMap.keyCode, ModifierKeyFlags.None, controllerActionElementSaveMap.actionId, controllerActionElementSaveMap.axisContribution, false);
		}

		public static GameInput.ControllerActionElementSaveMap CreateControllerActionElementSaveMap(ActionElementMap actionElementMap)
		{
			GameInput.ControllerActionElementSaveMap result = default;
			result.actionId = actionElementMap.actionId;
			result.axisContribution = actionElementMap.axisContribution;
			result.elementIdentifierId = actionElementMap.elementIdentifierId;
			result.elementType = actionElementMap.elementType;
			result.keyCode = actionElementMap.keyCode;
			return result;
		}

		public static List<GameInput.ControllerActionElementSaveMap> CreateControllerActionElementSaveMapList(IList<ActionElementMap> actionElementMaps)
		{
			List<GameInput.ControllerActionElementSaveMap> list = new List<GameInput.ControllerActionElementSaveMap>();
			foreach (ActionElementMap actionElementMap in actionElementMaps)
			{
				GameInput.ControllerActionElementSaveMap item = CreateControllerActionElementSaveMap(actionElementMap);
				list.Add(item);
			}
			return list;
		}
	}
}
