using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Reptile
{
	public class SaveSlotData : ISaveable
	{
		private const int PROGRESS_DATA_SAVE_VERSION = 3;

		public int saveSlotId;

		private string lastStageName = Stage.NONE.ToString();

		private string currentStageName = Stage.Prelude.ToString();

		private StageProgress[] totalStageProgress;

		private CharacterProgress[] totalCharacterProgress;

		private List<UnlockableSaveData> unlockables = new List<UnlockableSaveData>();

		private StageProgress currentStageProgress;

		public Characters currentCharacter = Characters.headMan;

		private Story.ObjectiveID currentStoryObjective;

		public bool characterSelectLocked;

		public bool boostAbilityLocked;

		public bool boostpackLocked;

		public bool phoneLocked;

		public bool spraycanLocked;

		public bool fortuneAppLocked;

		public bool cameraAppLocked;

		public bool switchToEquippedMovestyleLocked;

		public bool taxiLocked;

		public bool lastFortuneWasRare;

		public DateTime fortuneLastCrack = DateTime.MinValue;

		public int lastFortune;

		public Dictionary<string, int> emailIndexTable = new Dictionary<string, int>();

		public float maxWantedStars = 3f;

		public DateTime lastTimePlayed = DateTime.MinValue;

		public int crimeCount;

		public bool[] dancesUnlocked = new bool[6] { true, false, false, false, false, false };

		public int bombEaterHighscore = -1;

		public Stage CurrentStage
		{
			get
			{
				return Utility.SceneNameToStage(currentStageName);
			}
			set
			{
				currentStageName = value.ToString();
			}
		}

		public Stage LastStage
		{
			get
			{
				return Utility.SceneNameToStage(lastStageName);
			}
			set
			{
				lastStageName = value.ToString();
			}
		}

		public Story.ObjectiveID CurrentStoryObjective
		{
			get
			{
				return currentStoryObjective;
			}
			set
			{
				currentStoryObjective = value;
			}
		}

		public bool HasRespawnSaved(Stage stage)
		{
			if (CurrentStage != stage || stage == Stage.NONE || stage == Stage.MAX)
			{
				return false;
			}
			return GetStageProgress(stage).respawnPos != Vector3.zero;
		}

		public Vector3 GetStageRespawnPosition(Stage stage)
		{
			return GetStageProgress(stage).respawnPos;
		}

		public Vector3 GetStageRespawnRotation(Stage stage)
		{
			return GetStageProgress(stage).respawnRot;
		}

		public SaveSlotData()
		{
			totalStageProgress = new StageProgress[14];
			for (int i = 0; i < totalStageProgress.Length; i++)
			{
				Stage stageID = (Stage)i;
				totalStageProgress[i] = new StageProgress(stageID);
			}
			totalCharacterProgress = new CharacterProgress[26];
			for (int j = 0; j < totalCharacterProgress.Length; j++)
			{
				totalCharacterProgress[j] = new CharacterProgress((Characters)j);
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(3);
			writer.Write(saveSlotId);
			writer.Write(lastStageName);
			writer.Write(currentStageName);
			writer.Write(currentCharacter.ToString());
			writer.Write(currentStoryObjective.ToString());
			writer.Write(characterSelectLocked);
			writer.Write(boostAbilityLocked);
			writer.Write(boostpackLocked);
			writer.Write(phoneLocked);
			writer.Write(spraycanLocked);
			writer.Write(fortuneAppLocked);
			writer.Write(cameraAppLocked);
			writer.Write(switchToEquippedMovestyleLocked);
			writer.Write(taxiLocked);
			writer.Write(lastFortuneWasRare);
			writer.Write(fortuneLastCrack.Ticks);
			writer.Write(lastFortune);
			writer.Write(emailIndexTable.Count);
			foreach (KeyValuePair<string, int> item in emailIndexTable)
			{
				writer.Write(item.Key);
				writer.Write(item.Value);
			}
			writer.Write(totalStageProgress.Length);
			for (int i = 0; i < totalStageProgress.Length; i++)
			{
				totalStageProgress[i].Write(writer);
			}
			writer.Write(unlockables.Count);
			for (int j = 0; j < unlockables.Count; j++)
			{
				unlockables[j].Write(writer);
			}
			writer.Write(totalCharacterProgress.Length);
			for (int k = 0; k < totalCharacterProgress.Length; k++)
			{
				totalCharacterProgress[k].Write(writer);
			}
			writer.Write(maxWantedStars);
			lastTimePlayed = DateTime.Now;
			writer.Write(lastTimePlayed.Ticks);
			writer.Write(crimeCount);
			for (int l = 0; l < dancesUnlocked.Length; l++)
			{
				writer.Write(dancesUnlocked[l]);
			}
			for (int m = 0; m < totalStageProgress.Length; m++)
			{
				writer.Write(totalStageProgress[m].stageID.ToString());
				totalStageProgress[m].WriteVersionTwo(writer);
			}
			writer.Write(bombEaterHighscore);
			for (int n = 0; n < totalStageProgress.Length; n++)
			{
				writer.Write(totalStageProgress[n].stageID.ToString());
				totalStageProgress[n].WriteVersionThree(writer);
			}
		}

		public void Read(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			if (num < 0)
			{
				return;
			}
			saveSlotId = reader.ReadInt32();
			lastStageName = reader.ReadString();
			currentStageName = reader.ReadString();
			string value = reader.ReadString();
			currentCharacter = (Characters)Enum.Parse(typeof(Characters), value);
			string value2 = reader.ReadString();
			currentStoryObjective = (Story.ObjectiveID)Enum.Parse(typeof(Story.ObjectiveID), value2);
			characterSelectLocked = reader.ReadBoolean();
			boostAbilityLocked = reader.ReadBoolean();
			boostpackLocked = reader.ReadBoolean();
			phoneLocked = reader.ReadBoolean();
			spraycanLocked = reader.ReadBoolean();
			fortuneAppLocked = reader.ReadBoolean();
			cameraAppLocked = reader.ReadBoolean();
			switchToEquippedMovestyleLocked = reader.ReadBoolean();
			taxiLocked = reader.ReadBoolean();
			lastFortuneWasRare = reader.ReadBoolean();
			fortuneLastCrack = new DateTime(reader.ReadInt64());
			lastFortune = reader.ReadInt32();
			int num2 = reader.ReadInt32();
			emailIndexTable = new Dictionary<string, int>();
			for (int i = 0; i < num2; i++)
			{
				string key = reader.ReadString();
				int value3 = reader.ReadInt32();
				emailIndexTable.Add(key, value3);
			}
			int num3 = reader.ReadInt32();
			for (int j = 0; j < num3; j++)
			{
				StageProgress stageProgress = new StageProgress();
				stageProgress.Read(reader);
				for (int k = 0; k < totalStageProgress.Length; k++)
				{
					if (totalStageProgress[k].stageID == stageProgress.stageID)
					{
						totalStageProgress[k] = stageProgress;
						break;
					}
				}
			}
			int num4 = reader.ReadInt32();
			for (int l = 0; l < num4; l++)
			{
				UnlockableSaveData unlockableSaveData = new UnlockableSaveData(string.Empty);
				unlockableSaveData.Read(reader);
				unlockables.Add(unlockableSaveData);
			}
			int num5 = reader.ReadInt32();
			for (int m = 0; m < num5; m++)
			{
				CharacterProgress characterProgress = new CharacterProgress();
				characterProgress.Read(reader);
				for (int n = 0; n < totalCharacterProgress.Length; n++)
				{
					if (totalCharacterProgress[n].character == characterProgress.character)
					{
						totalCharacterProgress[n] = characterProgress;
						break;
					}
				}
			}
			maxWantedStars = reader.ReadSingle();
			lastTimePlayed = new DateTime(reader.ReadInt64());
			crimeCount = reader.ReadInt32();
			for (int num6 = 0; num6 < dancesUnlocked.Length; num6++)
			{
				dancesUnlocked[num6] = reader.ReadBoolean();
			}
			if (num >= 1)
			{
				for (int num7 = 0; num7 < num3; num7++)
				{
					StageProgress stageProgress2 = ReadStageProgress(reader);
					if (stageProgress2 != null)
					{
						stageProgress2.ReadVersionTwo(reader);
					}
					else
					{
						reader.ReadBoolean();
					}
				}
			}
			if (num >= 2)
			{
				bombEaterHighscore = reader.ReadInt32();
			}
			if (num < 3)
			{
				return;
			}
			for (int num8 = 0; num8 < num3; num8++)
			{
				StageProgress stageProgress3 = ReadStageProgress(reader);
				if (stageProgress3 != null)
				{
					stageProgress3.ReadVersionThree(reader);
				}
				else
				{
					SaveDataUtility.ConsumeStageProgressVersionThree(reader);
				}
			}
		}

		public void SetCurrentStageProgress(Stage stage)
		{
			currentStageProgress = GetStageProgress(stage);
		}

		public void InvalidateCurrentStageProgress()
		{
			currentStageProgress = null;
		}

		public StageProgress GetStageProgress(Stage stage)
		{
			StageProgress result = null;
			if (stage != Stage.NONE)
			{
				for (int i = 0; i < totalStageProgress.Length; i++)
				{
					if (totalStageProgress[i].stageID == stage)
					{
						result = totalStageProgress[i];
						break;
					}
				}
			}
			return result;
		}

		public StageProgress[] GetAllStageProgress()
		{
			return totalStageProgress;
		}

		public void SetupStoryCharactersLocked()
		{
			for (int i = 0; i < totalCharacterProgress.Length; i++)
			{
				if (totalCharacterProgress[i].character != Characters.metalHead && totalCharacterProgress[i].character != Characters.blockGuy && totalCharacterProgress[i].character != Characters.spaceGirl)
				{
					totalCharacterProgress[i].unlocked = false;
				}
			}
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void UnlockCharacter(Characters character)
		{
			for (int i = 0; i < totalCharacterProgress.Length; i++)
			{
				if (totalCharacterProgress[i].character == character)
				{
					totalCharacterProgress[i].unlocked = true;
				}
			}
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void LockCharacter(Characters character)
		{
			for (int i = 0; i < totalCharacterProgress.Length; i++)
			{
				if (totalCharacterProgress[i].character == character)
				{
					totalCharacterProgress[i].unlocked = false;
				}
			}
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void UnlockDance(Dances dance)
		{
			dancesUnlocked[(int)dance] = true;
		}

		public void UnlockDance(int num)
		{
			dancesUnlocked[num] = true;
		}

		public CharacterProgress GetCharacterProgress(Characters character)
		{
			if (character == Characters.NONE)
			{
				return null;
			}
			for (int i = 0; i < totalCharacterProgress.Length; i++)
			{
				if (totalCharacterProgress[i].character == character)
				{
					return totalCharacterProgress[i];
				}
			}
			return null;
		}

		public StageProgress GetCurrentStageProgress()
		{
			return currentStageProgress;
		}

		public UnlockableSaveData GetUnlockableDataByUid(string uid)
		{
			int num = unlockables.FindIndex((UnlockableSaveData unlockable) => unlockable.Uid.Equals(uid));
			UnlockableSaveData unlockableSaveData = null;
			if (num >= 0)
			{
				unlockableSaveData = unlockables[num];
			}
			else
			{
				unlockableSaveData = new UnlockableSaveData(uid);
				unlockables.Add(unlockableSaveData);
			}
			return unlockableSaveData;
		}

		internal void UnlockGraffitiBombedAchievementLocallyForStage(Stage stage)
		{
			totalStageProgress[(int)stage].graffitiBombedAchievementUnlocked = true;
		}

		internal bool IsGraffitiBombedAchievementLocallyUnlocked(Stage stage)
		{
			return totalStageProgress[(int)stage].graffitiBombedAchievementUnlocked;
		}

		internal bool HasAllGraffitiBombedAchievementsLocally(Stage[] stagesToCheck)
		{
			int num = 0;
			for (int i = 0; i < stagesToCheck.Length; i++)
			{
				int num2 = (int)stagesToCheck[i];
				num += Convert.ToInt32(totalStageProgress[num2].graffitiBombedAchievementUnlocked);
			}
			return num >= stagesToCheck.Length;
		}

		private StageProgress ReadStageProgress(BinaryReader reader)
		{
			Stage result = Stage.NONE;
			Enum.TryParse<Stage>(reader.ReadString(), out result);
			StageProgress result2 = null;
			for (int i = 0; i < totalStageProgress.Length; i++)
			{
				if (totalStageProgress[i].stageID == result)
				{
					result2 = totalStageProgress[i];
					break;
				}
			}
			return result2;
		}
	}
}
