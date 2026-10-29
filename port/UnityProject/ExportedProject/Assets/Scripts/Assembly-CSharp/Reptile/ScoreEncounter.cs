using System.Globalization;
using UnityEngine;

namespace Reptile
{
	public class ScoreEncounter : Encounter
	{
		private float timeLimitTimer;

		private bool hasOpponents;

		private float AIScoreIncreaseInterval = 0.6f;

		private float AITeammatesScoreTimer;

		private float AIOpponentsScoreTimer;

		private float AITeammatesScoreIncrease;

		private float AIOpponentScoreIncrease;

		private int teammatesScore;

		public float timeLimit = 40f;

		public int targetScore = 10000;

		private int opponentsScore;

		internal float ScoreGot;

		public string scoreName = "Bomb Rush Crew";

		public string targetScoreName = "Opponent";

		public float teammateScoreFactor = 0.33f;

		public bool forcePlayerAsBombRushCrew;

		public Racer[] contestants;

		private bool hasContestants;

		private CultureInfo cultureInfo;

		private const string SCORE_LABEL_FORMAT = "{0}:";

		public override void InitSceneObject()
		{
			hasContestants = contestants != null && contestants.Length != 0;
			cultureInfo = CultureInfo.CurrentCulture;
			base.InitSceneObject();
		}

		public override void StartMainEvent()
		{
			player.score = 0f;
			ScoreGot = 0f;
			timeLimitTimer = timeLimit;
			hasOpponents = false;
			if (hasContestants)
			{
				SetupContestantScore();
			}
			if (hasOpponents)
			{
				opponentsScore = 0;
			}
			else
			{
				opponentsScore = targetScore;
			}
			base.StartMainEvent();
		}

		public override void UpdateMainEvent()
		{
			if (player.IsBusyWithSequence())
			{
				return;
			}
			timeLimitTimer -= Core.dt;
			if (timeLimitTimer < 0f)
			{
				timeLimitTimer = 0f;
			}
			ScoreGot = player.score + player.baseScore * player.scoreMultiplier;
			if (hasContestants)
			{
				UpdateContestants();
			}
			SetScoreUI();
			if (ScoreGot >= (float)opponentsScore && !hasOpponents)
			{
				SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
			}
			else if (timeLimitTimer <= 0f)
			{
				if (ScoreGot >= (float)opponentsScore)
				{
					SetEncounterState(EncounterState.MAIN_EVENT_SUCCES_DECAY);
				}
				else
				{
					SetEncounterState(EncounterState.MAIN_EVENT_FAILED_DECAY);
				}
			}
			else
			{
				base.UpdateMainEvent();
			}
		}

		private string NiceTimerString(float timer)
		{
			string text = timer.ToString();
			int num = ((int)timer).ToString().Length + 3;
			if (text.Length > num)
			{
				text = text.Remove(num);
			}
			if (timer == 0f)
			{
				text = "0.00";
			}
			return text;
		}

		public override void TriggerDetectPlayer(Collider trigger, Player player)
		{
			if (state == EncounterState.OPEN)
			{
				ActivateEncounter();
			}
		}

		public override void EnterEncounterState(EncounterState setState)
		{
			if (setState == EncounterState.CLOSED || setState == EncounterState.OPEN || setState == EncounterState.OPEN_STARTUP)
			{
				TurnOffScoreUI();
				if (setState == EncounterState.CLOSED && hasContestants)
				{
					DestroyContestants();
				}
			}
			else
			{
				SetScoreUI();
			}
			if (setState == EncounterState.INTRO)
			{
				if (forcePlayerAsBombRushCrew && player.character != Characters.blockGuy && player.character != Characters.spaceGirl && player.character != Characters.metalHead)
				{
					SetPlayerAsMetalhead();
				}
				player.SwitchToEquippedMovestyle(set: false, doAirTrick: false, changeProp: true, showEffect: false);
				player.ActivateAbility(player.switchMoveStyleAbility);
				if (hasContestants)
				{
					EnterEncounterStateContestants();
				}
			}
			if (setState == EncounterState.MAIN_EVENT && hasContestants)
			{
				MainStateContestants();
			}
			if (setState == EncounterState.OPEN)
			{
				ActivateTriggers(set: true);
			}
			else
			{
				ActivateTriggers(set: false);
			}
			base.EnterEncounterState(setState);
		}

		public void ActivateTriggers(bool set)
		{
			foreach (Transform item in base.transform)
			{
				if (item.gameObject.layer == 19)
				{
					item.gameObject.SetActive(set);
				}
			}
		}

		public void DestroyContestants()
		{
			for (int i = 0; i < contestants.Length; i++)
			{
				if (contestants[i].player != null)
				{
					WorldHandler.instance.RemovePlayer(contestants[i].player);
					Object.Destroy(contestants[i].player.gameObject);
				}
			}
		}

		private void SetupContestantScore()
		{
			AITeammatesScoreTimer = Random.Range(3, 6);
			AIOpponentsScoreTimer = Random.Range(3, 6);
			AIOpponentScoreIncrease = (float)targetScore / (timeLimit / AIScoreIncreaseInterval);
			AITeammatesScoreIncrease = ((teammateScoreFactor == 0f) ? 0f : (AIOpponentScoreIncrease * teammateScoreFactor));
			teammatesScore = 0;
			opponentsScore = 0;
			for (int i = 0; i < contestants.Length; i++)
			{
				contestants[i].player.score = 0f;
				if (contestants[i].crew != Crew.PLAYERS && contestants[i].crew != Crew.NONE)
				{
					hasOpponents = true;
				}
			}
		}

		private bool CheckCharacterTeammateDuplicate(int num)
		{
			if (contestants[num].character == player.character)
			{
				return true;
			}
			for (int i = 0; i < contestants.Length; i++)
			{
				if (contestants[i].crew == Crew.PLAYERS && i != num && contestants[num].character == contestants[i].character)
				{
					return true;
				}
			}
			return false;
		}

		private Characters CycleThroughTeammateCharacters(Characters fromChar)
		{
			return fromChar switch
			{
				Characters.blockGuy => Characters.metalHead, 
				Characters.spaceGirl => Characters.blockGuy, 
				_ => Characters.spaceGirl, 
			};
		}

		private void UpdateContestants()
		{
			AITeammatesScoreTimer -= Core.dt;
			if (AITeammatesScoreTimer < 0f)
			{
				teammatesScore += (int)AITeammatesScoreIncrease;
				teammatesScore += ((teammateScoreFactor != 0f) ? (Random.Range(-3, 3) * 10) : 0);
				AITeammatesScoreTimer = Random.Range(AIScoreIncreaseInterval - 0.1f, AIScoreIncreaseInterval + 0.1f);
			}
			ScoreGot += teammatesScore;
			AIOpponentsScoreTimer -= Core.dt;
			if (AIOpponentsScoreTimer < 0f)
			{
				opponentsScore += (int)AIOpponentScoreIncrease;
				opponentsScore += Random.Range(-3, 3) * 10;
				AIOpponentsScoreTimer = Random.Range(AIScoreIncreaseInterval - 0.1f, AIScoreIncreaseInterval + 0.1f);
			}
		}

		private void EnterEncounterStateContestants()
		{
			for (int i = 0; i < contestants.Length; i++)
			{
				if (contestants[i].crew == Crew.PLAYERS)
				{
					while (CheckCharacterTeammateDuplicate(i))
					{
						contestants[i].character = CycleThroughTeammateCharacters(contestants[i].character);
					}
				}
				CharacterProgress characterProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(contestants[i].character);
				contestants[i].player = WorldHandler.instance.SetupAIPlayerAt(contestants[i].spawner, contestants[i].character, PlayerType.RACER_AI, (characterProgress != null && contestants[i].crew == Crew.PLAYERS) ? characterProgress.outfit : contestants[i].outfit, characterProgress?.moveStyle ?? contestants[i].moveStyle, contestants[i].crew);
				PlayerAIPath pathFromPathID = WorldHandler.instance.GetPathFromPathID(contestants[i].pathID);
				if (pathFromPathID != null)
				{
					contestants[i].player.AI.SetPath(pathFromPathID);
				}
				else
				{
					contestants[i].player.AI.SetPath(contestants[i].path);
				}
				contestants[i].player.userInputEnabled = false;
				contestants[i].player.SwitchToEquippedMovestyle(set: false, doAirTrick: false, changeProp: true, showEffect: false);
				contestants[i].player.ActivateAbility(contestants[i].player.switchMoveStyleAbility);
			}
		}

		private void MainStateContestants()
		{
			for (int i = 0; i < contestants.Length; i++)
			{
				contestants[i].player.AI.StartFirstPath();
				contestants[i].player.userInputEnabled = true;
			}
		}

		public void SetScoreUI()
		{
			string text = NiceTimerString(timeLimitTimer);
			GameplayUI gameplay = Core.Instance.UIManager.gameplay;
			gameplay.challengeGroup.SetActive(value: true);
			gameplay.timeLimitLabel.text = text;
			gameplay.targetScoreLabel.text = FormattingUtility.FormatPlayerScore(cultureInfo, opponentsScore);
			gameplay.totalScoreLabel.text = FormattingUtility.FormatPlayerScore(cultureInfo, ScoreGot);
			gameplay.targetScoreTitleLabel.text = $"{targetScoreName}:";
			gameplay.totalScoreTitleLabel.text = $"{scoreName}:";
		}

		public void TurnOffScoreUI()
		{
			GameplayUI gameplay = Core.Instance.UIManager.gameplay;
			gameplay.challengeGroup.SetActive(value: false);
			gameplay.timeLimitLabel.text = "";
			gameplay.targetScoreLabel.text = "";
			gameplay.totalScoreLabel.text = "";
			gameplay.targetScoreTitleLabel.text = "";
			gameplay.totalScoreTitleLabel.text = "";
		}

		public override void ActivateEncounter()
		{
			if (hasContestants)
			{
				DestroyContestants();
			}
			base.ActivateEncounter();
		}
	}
}
