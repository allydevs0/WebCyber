using System.Collections.Generic;
using Rewired;

namespace Reptile
{
	public class RewiredInputUIActionBinder
	{
		private RewiredMappingHandler rewiredMappingHandler;

		private RewiredRemappingHandler rewiredRemappingHandler;

		private RewiredControllerHandler rewiredControllerHandler;

		private bool isMappingAction;

		private bool foundConflict;

		private int rewiredPlayerId;

		private List<RewiredRemappingHandler.ConflictInfo> conflicts;

		private int[] bindableCategoryIds;

		public bool IsMappingAction => isMappingAction;

		public bool HasConflict => foundConflict;

		public RewiredInputUIActionBinder(RewiredMappingHandler rewiredMappingHandler, RewiredRemappingHandler rewiredRemappingHandler, RewiredControllerHandler rewiredControllerHandler)
		{
			this.rewiredMappingHandler = rewiredMappingHandler;
			this.rewiredRemappingHandler = rewiredRemappingHandler;
			this.rewiredControllerHandler = rewiredControllerHandler;
			conflicts = new List<RewiredRemappingHandler.ConflictInfo>();
		}

		public List<RewiredRemappingHandler.ConflictInfo> GetConflictingActionIds()
		{
			return conflicts;
		}

		public void StartAttemptMappingAction(int rewiredPlayerId, int[] bindableCategoryIds)
		{
			this.rewiredPlayerId = rewiredPlayerId;
			this.bindableCategoryIds = bindableCategoryIds;
			foundConflict = false;
			isMappingAction = true;
			conflicts.Clear();
		}

		public void StopAttemptMappingAction()
		{
			isMappingAction = false;
		}

		public void DetectConflictingMappings(Controller controller)
		{
			conflicts.Clear();
			rewiredRemappingHandler.RetrieveConflictingActionIds(controller, bindableCategoryIds, conflicts, rewiredPlayerId);
			foundConflict = conflicts.Count > 0;
		}

		public void UpdateAttemptMappingSingleController(Controller controller, InputSelectionRowController inputSelectionRowController, int controllerMapCategoryId)
		{
			if (rewiredRemappingHandler.DetectPollForButton(controller, out var info))
			{
				UpdateAttemptMappingForController(controller, inputSelectionRowController, info, controllerMapCategoryId);
			}
		}

		public void UpdateAttemptMappingDoubleController(Controller forController, InputSelectionRowController inputSelectionRowController, ControllerType otherType, int controllerMapCategoryId)
		{
			if (rewiredRemappingHandler.DetectPollForButton(forController, out var info))
			{
				UpdateAttemptMappingForController(forController, inputSelectionRowController, info, controllerMapCategoryId);
				return;
			}
			Controller playerCurrentControllerByType = rewiredControllerHandler.GetPlayerCurrentControllerByType(otherType);
			if (rewiredRemappingHandler.DetectPollForButton(playerCurrentControllerByType, out info))
			{
				UpdateAttemptMappingForController(playerCurrentControllerByType, inputSelectionRowController, info, controllerMapCategoryId);
			}
		}

		private void UpdateAttemptMappingForController(Controller controller, InputSelectionRowController inputSelectionRowController, ControllerPollingInfo controllerPollingInfo, int controllerMapCategoryId)
		{
			ActionElementMap actionElementMapToUpdate = GetActionElementMapToUpdate(inputSelectionRowController, controller);
			if (actionElementMapToUpdate == null)
			{
				UpdateAttemptCreateNewActionMapping(controller, inputSelectionRowController, controllerPollingInfo, controllerMapCategoryId);
			}
			else
			{
				UpdateAttemptRemappingAction(controller, actionElementMapToUpdate, inputSelectionRowController, controllerPollingInfo, controllerMapCategoryId);
			}
		}

		private ActionElementMap GetActionElementMapToUpdate(InputSelectionRowController inputSelectionRowController, Controller controller)
		{
			ActionElementMap result = null;
			if (controller.type == ControllerType.Joystick)
			{
				result = inputSelectionRowController.GetGamepadActionElementMap();
			}
			else if (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
			{
				result = GetKeyboardMouseElementMap(inputSelectionRowController, controller);
			}
			return result;
		}

		private ActionElementMap GetKeyboardMouseElementMap(InputSelectionRowController inputSelectionRowController, Controller controller)
		{
			ActionElementMap result = null;
			if (controller.type == ControllerType.Keyboard)
			{
				result = inputSelectionRowController.GetKeyboardActionElementMap();
			}
			else if (controller.type == ControllerType.Mouse)
			{
				result = inputSelectionRowController.GetMouseActionElementMap();
			}
			return result;
		}

		private void UpdateAttemptCreateNewActionMapping(Controller controller, InputSelectionRowController inputSelectionRowController, ControllerPollingInfo controllerPollingInfo, int controllerMapCategoryId)
		{
			if (rewiredRemappingHandler.AttemptNewElementAssignment(controller, controllerPollingInfo, inputSelectionRowController.InputActionId, controllerMapCategoryId, inputSelectionRowController.AxisContribution, out var newActionElementMap))
			{
				FinishRemappingAction(controller, inputSelectionRowController, newActionElementMap, controllerMapCategoryId);
			}
		}

		private void UpdateAttemptRemappingAction(Controller controller, ActionElementMap actionElementMapToUpdate, InputSelectionRowController inputSelectionRowController, ControllerPollingInfo controllerPollingInfo, int controllerMapCategoryId)
		{
			if (rewiredRemappingHandler.AttemptElementReassignment(controller, controllerPollingInfo, inputSelectionRowController.InputActionId, controllerMapCategoryId, inputSelectionRowController.AxisContribution, actionElementMapToUpdate, out var updatedActionElementMap))
			{
				FinishRemappingAction(controller, inputSelectionRowController, updatedActionElementMap, controllerMapCategoryId);
			}
		}

		private void FinishRemappingAction(Controller controller, InputSelectionRowController rowController, ActionElementMap updatedActionElementMap, int controllerMapCategoryId)
		{
			if (controller.type == ControllerType.Joystick)
			{
				rowController.UpdateGamepadBinding(updatedActionElementMap);
			}
			else if (controller.type == ControllerType.Keyboard)
			{
				FinishMappingKeyboardAction(rowController, updatedActionElementMap, controllerMapCategoryId);
			}
			else if (controller.type == ControllerType.Mouse)
			{
				FinishMappingMouseAction(rowController, updatedActionElementMap, controllerMapCategoryId);
			}
			DetectConflictingMappings(controller);
			StopAttemptMappingAction();
		}

		private void FinishMappingKeyboardAction(InputSelectionRowController rowController, ActionElementMap updatedActionElementMap, int controllerMapCategoryId)
		{
			if (rowController.HasMouseBinding)
			{
				DeleteMouseBinding(rowController, controllerMapCategoryId);
			}
			rowController.UpdateKeyboardBinding(updatedActionElementMap);
		}

		private void DeleteMouseBinding(InputSelectionRowController rowController, int controllerMapCategoryId)
		{
			ActionElementMap mouseActionElementMap = rowController.GetMouseActionElementMap();
			rewiredMappingHandler.DeleteActionElementMap(ControllerType.Mouse, mouseActionElementMap.id, controllerMapCategoryId);
			rowController.RemoveMouseBinding();
		}

		private void FinishMappingMouseAction(InputSelectionRowController rowController, ActionElementMap updatedActionElementMap, int controllerMapCategoryId)
		{
			if (rowController.HasKeyboardBinding)
			{
				DeleteKeyboardBinding(rowController, controllerMapCategoryId);
			}
			rowController.UpdateMouseBinding(updatedActionElementMap);
		}

		private void DeleteKeyboardBinding(InputSelectionRowController rowController, int controllerMapCategoryId)
		{
			ActionElementMap keyboardActionElementMap = rowController.GetKeyboardActionElementMap();
			rewiredMappingHandler.DeleteActionElementMap(ControllerType.Keyboard, keyboardActionElementMap.id, controllerMapCategoryId);
			rowController.RemoveKeyboardBinding();
		}
	}
}
