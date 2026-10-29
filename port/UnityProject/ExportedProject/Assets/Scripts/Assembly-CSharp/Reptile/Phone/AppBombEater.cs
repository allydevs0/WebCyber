using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class AppBombEater : App
	{
		private class Moveables
		{
			public RectTransform tf;

			public GameObject obj;

			public Animator anim;

			public MoveableType type;
		}

		private enum MoveableType
		{
			Bomb = 0,
			Coin = 1
		}

		private enum GameState
		{
			StartScreen = 0,
			Playing = 1,
			Decay = 2,
			GameOver = 3
		}

		private Sequence openAppSequence;

		private Sequence closeAppSequence;

		private int selectedButton;

		private GameState state;

		private float gameTime;

		private float curCoinToBombRatio = 0.9f;

		private float baseCoinToBombRatio = 0.95f;

		private float maxCoinToBombRatio = 0.7f;

		private float gameProgressStepInterval = 1f;

		private float lastIncreaseGameProgressStepsTime;

		private int gameProgressStep;

		private int score;

		private int highScore = -1;

		private List<Moveables> onlyBombs = new List<Moveables>();

		private List<Moveables> activeMoveables = new List<Moveables>();

		private List<Moveables> backupMoveables = new List<Moveables>();

		private RectTransform playerRect;

		private RectTransform bombRect;

		private RectTransform coinRect;

		private RectTransform explosionRect;

		private RectTransform pickupRect;

		private Animator playerAnimator;

		private Animator explosionAnimator;

		private Animator pickupAnimator;

		private float playerHeight;

		private bool spawnOnlyBombs;

		private bool spawnCoinRow;

		private bool onlyBombsActive;

		private float speedFactor = 1f;

		private float lastScore;

		[SerializeField]
		private GameObject startScreen;

		[SerializeField]
		private GameObject gameScreen;

		[SerializeField]
		private GameObject gameoverScreen;

		[SerializeField]
		private RectTransform gameScreenContent;

		[SerializeField]
		private RectTransform playerSpawn;

		[SerializeField]
		private RectTransform bombAndCoinSpawn;

		[SerializeField]
		private GameObject playerObject;

		[SerializeField]
		private GameObject bombSourceObject;

		[SerializeField]
		private GameObject coinSourceObject;

		[SerializeField]
		private GameObject explosionObject;

		[SerializeField]
		private GameObject pickupObject;

		[SerializeField]
		private TextMeshProUGUI currentScoreField;

		[SerializeField]
		private GameObject highScoreLabelPlaying;

		[SerializeField]
		private TextMeshProUGUI highScoreFieldGameOver;

		[SerializeField]
		private TextMeshProUGUI highScoreFieldStartScreen;

		[SerializeField]
		private Image decayBackgroundImage;

		[SerializeField]
		private CanvasGroup boostBackground;

		[SerializeField]
		private CanvasGroup onlyBombsBackground;

		private bool boost;

		private float playerOldHeight;

		private float moveProgress;

		protected override void OnAppInit()
		{
			SetGameReferences();
			SetGameState(GameState.StartScreen);
			openAppSequence = DOTween.Sequence();
			openAppSequence.SetAutoKill(autoKillOnCompletion: false);
			openAppSequence.Pause();
			openAppSequence.AppendCallback(() =>
			{
				selectedButton = 0;
				_ = Core.Instance.SaveManager.CurrentSaveSlot;
			});
			openAppSequence.Append(base.transform.DOScaleY(1f, 0.15f));
			closeAppSequence = DOTween.Sequence();
			closeAppSequence.SetAutoKill(autoKillOnCompletion: false);
			closeAppSequence.Pause();
			closeAppSequence.Append(base.transform.DOScaleY(0f, 0.15f));
			closeAppSequence.AppendCallback(() =>
			{
				base.MyPhone.CloseCurrentApp();
			});
		}

		protected override void OnAppTerminate()
		{
			openAppSequence.Kill();
			closeAppSequence.Kill();
		}

		private void SetGameReferences()
		{
			playerRect = playerObject.GetComponent<RectTransform>();
			bombRect = bombSourceObject.GetComponent<RectTransform>();
			coinRect = coinSourceObject.GetComponent<RectTransform>();
			explosionRect = explosionObject.GetComponent<RectTransform>();
			explosionAnimator = explosionObject.GetComponent<Animator>();
			pickupRect = pickupObject.GetComponent<RectTransform>();
			pickupAnimator = pickupObject.GetComponent<Animator>();
			playerAnimator = playerObject.GetComponent<Animator>();
		}

		public override void OnAppEnable()
		{
			openAppSequence.Restart();
			if (state == GameState.GameOver || state == GameState.Decay)
			{
				SetGameState(GameState.StartScreen);
			}
			_ = state;
			_ = 1;
		}

		public override void OnAppDisable()
		{
		}

		private void SetScreen()
		{
			switch (state)
			{
			case GameState.StartScreen:
				highScoreFieldStartScreen.text = ((Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore != -1) ? Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore.ToString().PadLeft(6, '0') : "000000");
				startScreen.SetActive(value: true);
				gameScreen.SetActive(value: false);
				gameoverScreen.SetActive(value: false);
				break;
			case GameState.Playing:
				startScreen.SetActive(value: false);
				gameScreen.SetActive(value: true);
				gameoverScreen.SetActive(value: false);
				break;
			case GameState.GameOver:
				highScoreFieldGameOver.text = ((Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore != -1) ? Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore.ToString().PadLeft(6, '0') : "000000");
				startScreen.SetActive(value: false);
				gameScreen.SetActive(value: false);
				gameoverScreen.SetActive(value: true);
				break;
			case GameState.Decay:
				break;
			}
		}

		public override void OnPressUp()
		{
			if (state == GameState.Playing)
			{
				ButtonUpDown(1);
			}
		}

		public override void OnPressDown()
		{
			if (state == GameState.Playing)
			{
				ButtonUpDown(-1);
			}
		}

		public override void OnPressRight()
		{
			if (state == GameState.GameOver || state == GameState.StartScreen)
			{
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.FlipPhone_Confirm);
				SetGameState(GameState.Playing);
			}
		}

		public override void OnHoldRight()
		{
			base.OnHoldRight();
			if (state == GameState.Playing)
			{
				if (!boost)
				{
					Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.BombEater_RushModeActivated_Final);
				}
				boost = true;
			}
		}

		public override void OnReleaseRight()
		{
			base.OnReleaseRight();
			if (state == GameState.Playing)
			{
				boost = false;
			}
		}

		public override void OnReleaseLeft()
		{
			closeAppSequence.Restart();
		}

		private void ButtonUpDown(int direction = 0)
		{
			if ((playerHeight < 280f && direction > 0) || (playerHeight > -488f && direction < 0))
			{
				playerOldHeight = playerHeight;
				playerHeight += direction * 384;
				playerAnimator.Play((direction < 0) ? "poloMoveDown" : "poloMoveUp", 0, 0f);
				moveProgress = 0f;
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.BombEater_Movement_Final);
			}
		}

		private void MovePlayer()
		{
			if (playerRect.anchoredPosition.y != playerHeight)
			{
				moveProgress += Core.dt * 20f * speedFactor;
				playerRect.anchoredPosition = new Vector2(playerRect.anchoredPosition.x, Mathf.Lerp(playerOldHeight, playerHeight, moveProgress));
			}
		}

		private void SetGameState(GameState newState)
		{
			state = newState;
			switch (state)
			{
			case GameState.StartScreen:
				ResetScore();
				SetScreen();
				break;
			case GameState.Playing:
				SetScreen();
				playerObject.GetComponent<Image>().enabled = true;
				bombSourceObject.SetActive(value: false);
				coinSourceObject.SetActive(value: false);
				playerObject.gameObject.SetActive(value: true);
				ClearBombsAndCoins();
				gameTime = 0f;
				gameProgressStep = 0;
				lastIncreaseGameProgressStepsTime = 0f;
				explosionObject.SetActive(value: false);
				pickupObject.SetActive(value: false);
				highScoreLabelPlaying.SetActive(value: false);
				playerRect.anchoredPosition = playerSpawn.anchoredPosition;
				playerHeight = playerRect.anchoredPosition.y;
				playerAnimator.Play("poloIdle", 0, 0f);
				spawnOnlyBombs = (spawnCoinRow = (onlyBombsActive = false));
				boostBackground.alpha = 0f;
				onlyBombsBackground.alpha = 0f;
				decayBackgroundImage.color = new Vector4(1f, 0f, 0f, 0f);
				break;
			case GameState.Decay:
				UpdateHighScore();
				boost = false;
				onlyBombsActive = false;
				gameTime = 0f;
				explosionObject.SetActive(value: true);
				playerAnimator.Play("poloDie", 0, 0f);
				boostBackground.alpha = 0f;
				onlyBombsBackground.alpha = 0f;
				break;
			case GameState.GameOver:
				ResetScore();
				ClearBombsAndCoins();
				SetScreen();
				break;
			}
		}

		public override void OnAppUpdate()
		{
			base.OnAppUpdate();
			gameTime += Core.dt;
			UpdateGameState();
		}

		private void UpdateGameState()
		{
			switch (state)
			{
			case GameState.Playing:
			{
				if (score >= Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore && Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore != -1 && !highScoreLabelPlaying.activeSelf)
				{
					highScoreLabelPlaying.SetActive(value: true);
				}
				curCoinToBombRatio = Mathf.Lerp(baseCoinToBombRatio, maxCoinToBombRatio, Mathf.Clamp((float)gameProgressStep / 50f, 0f, 1f));
				currentScoreField.text = score.ToString().PadLeft(6, '0');
				UpdateSpecialStages();
				UpdateSpeedFactor();
				MovePlayer();
				MoveBombsAndCoins();
				playerAnimator.SetFloat("speed", speedFactor);
				float a = -1000f;
				float b = -1000f;
				bool flag = false;
				bool flag2 = false;
				for (int num = activeMoveables.Count - 1; num > -1; num--)
				{
					if (!flag && activeMoveables[num].type == MoveableType.Bomb)
					{
						a = activeMoveables[num].tf.anchoredPosition.x;
						flag = true;
					}
					if (!flag2 && activeMoveables[num].type == MoveableType.Coin)
					{
						b = activeMoveables[num].tf.anchoredPosition.x - (float)(spawnCoinRow ? 384 : 0);
						flag2 = true;
					}
				}
				if (bombRect.anchoredPosition.x - Mathf.Max(a, b) > 512f)
				{
					SpawnBombAndCoins();
				}
				CheckCollisions();
				if (gameTime > lastIncreaseGameProgressStepsTime + gameProgressStepInterval)
				{
					IncreaseSteps();
				}
				onlyBombsBackground.alpha += (onlyBombsActive ? (Core.dt * 2f) : ((0f - Core.dt) * 2f));
				boostBackground.alpha += (boost ? Core.dt : (0f - Core.dt));
				break;
			}
			case GameState.Decay:
				decayBackgroundImage.color = Color.Lerp(Color.red, Color.black, gameTime / 2f);
				playerObject.GetComponent<Image>().enabled = gameTime % 0.25f > 0.125f;
				if (gameTime > 2f)
				{
					SetGameState(GameState.GameOver);
				}
				break;
			case GameState.StartScreen:
			case GameState.GameOver:
				break;
			}
		}

		private void SpawnBombAndCoins()
		{
			bool flag = Random.value < (spawnOnlyBombs ? 0f : (spawnCoinRow ? 1f : curCoinToBombRatio));
			int num = (spawnCoinRow ? 1 : Random.Range(0, 3));
			float y = bombRect.anchoredPosition.y - (float)(num * 384);
			Moveables moveable = GetMoveable(flag ? MoveableType.Coin : MoveableType.Bomb);
			moveable.tf.anchoredPosition = new Vector2(coinRect.anchoredPosition.x, y);
			activeMoveables.Add(moveable);
			if (moveable.type == MoveableType.Bomb && spawnOnlyBombs)
			{
				onlyBombs.Add(moveable);
			}
		}

		private Moveables GetMoveable(MoveableType type)
		{
			if (backupMoveables.Count > 0)
			{
				for (int num = backupMoveables.Count - 1; num > -1; num--)
				{
					if (backupMoveables[num].type == type)
					{
						Moveables moveables = backupMoveables[num];
						moveables.obj.SetActive(value: true);
						backupMoveables.Remove(moveables);
						return moveables;
					}
				}
			}
			Moveables moveables2 = new Moveables();
			moveables2.obj = Object.Instantiate((type == MoveableType.Coin) ? coinSourceObject : bombSourceObject, gameScreenContent, worldPositionStays: false);
			moveables2.obj.SetActive(value: true);
			moveables2.tf = moveables2.obj.GetComponent<RectTransform>();
			moveables2.anim = moveables2.obj.GetComponent<Animator>();
			moveables2.type = type;
			return moveables2;
		}

		private void MoveBombsAndCoins()
		{
			float x = 512f * Core.dt * speedFactor;
			if (activeMoveables.Count <= 0)
			{
				return;
			}
			for (int num = activeMoveables.Count - 1; num > -1; num--)
			{
				activeMoveables[num].tf.anchoredPosition -= new Vector2(x, 0f);
				if (activeMoveables[num].tf.anchoredPosition.x < -640f)
				{
					SendToBackup(activeMoveables[num]);
				}
			}
		}

		private void SendToBackup(Moveables moveable)
		{
			backupMoveables.Add(moveable);
			if (moveable.type == MoveableType.Bomb)
			{
				onlyBombs.Remove(moveable);
			}
			moveable.obj.SetActive(value: false);
			activeMoveables.Remove(moveable);
		}

		private void CheckCollisions()
		{
			for (int i = 0; i < activeMoveables.Count; i++)
			{
				if (Vector2.Distance(activeMoveables[i].tf.anchoredPosition, playerRect.anchoredPosition) < 128f)
				{
					if (activeMoveables[i].type == MoveableType.Bomb)
					{
						explosionRect.anchoredPosition = activeMoveables[i].tf.anchoredPosition;
						explosionAnimator.Play("explosion", 0, 0f);
						Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.BombEater_GameOver_Final);
						SetGameState(GameState.Decay);
					}
					if (activeMoveables[i].type == MoveableType.Coin)
					{
						pickupRect.anchoredPosition = activeMoveables[i].tf.anchoredPosition;
						pickupObject.SetActive(value: true);
						ScoreCoin();
						pickupAnimator.Play("pickup", 0, 0f);
						Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.BombEater_CoinSFX_Final);
					}
					SendToBackup(activeMoveables[i]);
				}
			}
		}

		private void ClearBombsAndCoins()
		{
			for (int num = activeMoveables.Count - 1; num > -1; num--)
			{
				SendToBackup(activeMoveables[num]);
			}
			onlyBombs.Clear();
		}

		private void ScoreCoin()
		{
			score += 10;
		}

		private void ResetScore()
		{
			score = 0;
		}

		private void IncreaseSteps()
		{
			gameProgressStep++;
			lastIncreaseGameProgressStepsTime = gameTime;
		}

		private void UpdateSpeedFactor()
		{
			speedFactor = Mathf.Lerp(1f, 5f, (float)gameProgressStep / 200f * (onlyBombsActive ? 1.25f : 1f)) * (boost ? 3f : 1f);
		}

		private void UpdateSpecialStages()
		{
			spawnOnlyBombs = gameProgressStep % 8 >= 6 && gameProgressStep > 25;
			spawnCoinRow = gameProgressStep % 8 == 0 && (gameProgressStep == 0 || gameProgressStep > 25);
			bool flag = onlyBombsActive;
			onlyBombsActive = onlyBombs.Count > 0;
			if (onlyBombsActive && onlyBombsActive != flag)
			{
				Core.Instance.AudioManager.PlaySfxGameplay(SfxCollectionID.PhoneSfx, AudioClipID.BombEater_Alarm_Final);
			}
		}

		private void UpdateHighScore()
		{
			if (score > Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore)
			{
				Core.Instance.SaveManager.CurrentSaveSlot.bombEaterHighscore = score;
				Core.Instance.SaveManager.SaveCurrentSaveSlot();
			}
		}
	}
}
