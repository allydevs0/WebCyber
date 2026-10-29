using System.Collections.Generic;
using System.IO;
using Rewired;
using UnityEngine;

namespace Reptile
{
	public class InputSettings : ISaveable
	{
		private int INPUT_SETTINGS_SAVE_VERSION = 1;

		private int saveFileVersion = -1;

		public List<GameInput.ControllerSaveMap> savedMaps = new List<GameInput.ControllerSaveMap>();

		public void Write(BinaryWriter writer)
		{
			writer.Write(INPUT_SETTINGS_SAVE_VERSION);
			PlayerSaveData inputSaveData = Core.Instance.GameInput.GetInputSaveData(userAssignableMapsOnly: true);
			savedMaps.Clear();
			int customControllerMapCount = inputSaveData.customControllerMapCount;
			ControllerMapSaveData[] customControllerMapSaveData = inputSaveData.customControllerMapSaveData;
			WriteMap(writer, customControllerMapCount, customControllerMapSaveData);
			int joystickMapCount = inputSaveData.joystickMapCount;
			customControllerMapSaveData = inputSaveData.joystickMapSaveData;
			WriteMap(writer, joystickMapCount, customControllerMapSaveData);
			int keyboardMapCount = inputSaveData.keyboardMapCount;
			customControllerMapSaveData = inputSaveData.keyboardMapSaveData;
			WriteMap(writer, keyboardMapCount, customControllerMapSaveData);
			int mouseMapCount = inputSaveData.mouseMapCount;
			customControllerMapSaveData = inputSaveData.mouseMapSaveData;
			WriteMap(writer, mouseMapCount, customControllerMapSaveData);
		}

		public void Read(BinaryReader reader)
		{
			saveFileVersion = reader.ReadInt32();
			if (saveFileVersion >= 0)
			{
				ReadMap(reader, ControllerType.Custom);
				ReadMap(reader, ControllerType.Joystick);
				ReadMap(reader, ControllerType.Keyboard);
				ReadMap(reader, ControllerType.Mouse);
			}
		}

		public void WriteVersionOne(BinaryWriter writer)
		{
			PlayerSaveData inputSaveData = Core.Instance.GameInput.GetInputSaveData(userAssignableMapsOnly: true);
			int joystickMapCount = inputSaveData.joystickMapCount;
			ControllerMapSaveData[] joystickMapSaveData = inputSaveData.joystickMapSaveData;
			WriteMapExtension(writer, joystickMapCount, joystickMapSaveData);
		}

		public void ReadVersionOne(BinaryReader reader)
		{
			if (saveFileVersion >= 1)
			{
				ReadMapExtension(reader);
			}
		}

		public GameInput.ControllerSaveMap GetSavedControllerMap(ControllerType controllerType, int controllerId, int playerId, int mapCategoryId)
		{
			foreach (GameInput.ControllerSaveMap savedMap in savedMaps)
			{
				if (savedMap.controllerType == controllerType && savedMap.controllerId == controllerId && savedMap.playerId == playerId && savedMap.mapCategoryId == mapCategoryId)
				{
					return savedMap;
				}
			}
			return GameInput.ControllerSaveMap.INVALID_CONTROLLERMAP;
		}

		private void WriteMap(BinaryWriter writer, int count, ControllerMapSaveData[] mapSaveData)
		{
			writer.Write(count);
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				writer.Write(mapSaveData[i].controller.id);
				writer.Write(mapSaveData[i].categoryId);
				writer.Write(mapSaveData[i].map.playerId);
				List<GameInput.ControllerActionElementSaveMap> controllerActionElementSaveMaps = RewiredStructUtility.CreateControllerActionElementSaveMapList(mapSaveData[i].map.AllMaps);
				WriteActionElementMaps(writer, controllerActionElementSaveMaps);
				GameInput.ControllerSaveMap item = new GameInput.ControllerSaveMap(mapSaveData[i].controllerType, mapSaveData[i].controller.id, mapSaveData[i].map.playerId, controllerActionElementSaveMaps, mapSaveData[i].categoryId, mapSaveData[i].controller.hardwareTypeGuid.ToString());
				savedMaps.Add(item);
			}
		}

		private void WriteActionElementMaps(BinaryWriter writer, List<GameInput.ControllerActionElementSaveMap> controllerActionElementSaveMaps)
		{
			writer.Write(controllerActionElementSaveMaps.Count);
			foreach (GameInput.ControllerActionElementSaveMap controllerActionElementSaveMap in controllerActionElementSaveMaps)
			{
				WriteActionElementMap(writer, controllerActionElementSaveMap);
			}
		}

		private void WriteActionElementMap(BinaryWriter writer, GameInput.ControllerActionElementSaveMap actionElementSaveMap)
		{
			writer.Write(actionElementSaveMap.actionId);
			writer.Write((int)actionElementSaveMap.keyCode);
			writer.Write(actionElementSaveMap.elementIdentifierId);
			writer.Write((int)actionElementSaveMap.elementType);
			writer.Write((int)actionElementSaveMap.axisContribution);
		}

		private void WriteMapExtension(BinaryWriter writer, int count, ControllerMapSaveData[] mapSaveData)
		{
			writer.Write(count);
			for (int i = 0; i < count; i++)
			{
				writer.Write(mapSaveData[i].controller.hardwareTypeGuid.ToString());
			}
		}

		private void ReadMap(BinaryReader reader, ControllerType controllerType)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int controllerId = reader.ReadInt32();
				int mapCategoryId = reader.ReadInt32();
				int playerId = reader.ReadInt32();
				List<GameInput.ControllerActionElementSaveMap> controllerActionElementSaveMaps = ReadControllerActionElementMaps(reader);
				GameInput.ControllerSaveMap item = new GameInput.ControllerSaveMap(controllerType, controllerId, playerId, controllerActionElementSaveMaps, mapCategoryId, string.Empty);
				savedMaps.Add(item);
			}
		}

		private List<GameInput.ControllerActionElementSaveMap> ReadControllerActionElementMaps(BinaryReader reader)
		{
			List<GameInput.ControllerActionElementSaveMap> list = new List<GameInput.ControllerActionElementSaveMap>();
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				GameInput.ControllerActionElementSaveMap item = ReadActionElementSaveMap(reader);
				list.Add(item);
			}
			return list;
		}

		private GameInput.ControllerActionElementSaveMap ReadActionElementSaveMap(BinaryReader reader)
		{
			int actionId = reader.ReadInt32();
			int keyCode = reader.ReadInt32();
			int elementIdentifierId = reader.ReadInt32();
			int elementType = reader.ReadInt32();
			int axisContribution = reader.ReadInt32();
			return new GameInput.ControllerActionElementSaveMap(actionId, (KeyCode)keyCode, elementIdentifierId, (ControllerElementType)elementType, (Pole)axisContribution);
		}

		private void ReadMapExtension(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string hardwareTypeGuid = reader.ReadString();
				GameInput.ControllerSaveMap item = savedMaps[i];
				item.hardwareTypeGuid = hardwareTypeGuid;
				savedMaps.RemoveAt(i);
				savedMaps.Insert(i, item);
			}
		}
	}
}
