using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class CharacterSelect : MonoBehaviour
	{
		public enum CharSelectState
		{
			CHARACTERS_JOINING = 0,
			MAIN_STATE = 1,
			CHARACTER_PICKED = 2,
			CYPHERING = 3
		}

		private const float CHARACTER_SELECT_BLACK_BAR_HEIGHT = 130f;

		private const float BLACK_BAR_ANIMATION_DURATION = 0.25f;

		private const float CHARACTER_SELECT_MOVEMENT_THRESHOLD = 1f;

		private const float CHARACTER_SELECT_MOVEMENT_RESET_THRESHOLD = 0f;

		[SerializeField]
		private Vector3 localPosOnCharacter = new Vector3(-1.65f, 1.2f, 0.75f);

		[SerializeField]
		private GameObject swapSequence;

		[SerializeField]
		private PlayableDirector selectSequence1;

		[SerializeField]
		private PlayableDirector selectSequence2;

		[SerializeField]
		private PlayableDirector selectSequence3;

		[SerializeField]
		private PlayableDirector selectSequence4;

		[SerializeField]
		private PlayableDirector selectSequence5;

		[SerializeField]
		private PlayableDirector selectSequence6;

		[SerializeField]
		private PlayableDirector selectSequence7;

		[SerializeField]
		private PlayableDirector selectSequence8;

		[SerializeField]
		private PlayableDirector selectSequence9;

		[SerializeField]
		private PlayableDirector selectSequence10;

		[SerializeField]
		private PlayableDirector selectSequence11;

		[SerializeField]
		private PlayableDirector selectSequence12;

		[SerializeField]
		private PlayableDirector selectSequence13;

		[SerializeField]
		private PlayableDirector selectSequence14;

		[SerializeField]
		private PlayableDirector selectSequence15;

		[SerializeField]
		private PlayableDirector selectSequence16;

		[SerializeField]
		private PlayableDirector selectSequence17;

		[SerializeField]
		private PlayableDirector selectSequence18;

		[SerializeField]
		private PlayableDirector selectSequence19;

		[SerializeField]
		private GameObject charCollision;

		[SerializeField]
		private GameObject charTrigger;

		[SerializeField]
		private Camera characterSelectCamera;

		[SerializeField]
		private DownloadableContentItem charactersDownloadableContent;

		[SerializeField]
		private float cameraMovementSpeed = 10.67f;

		[SerializeField]
		private float minimalCameraMovementSpeed = 1f;

		[SerializeField]
		private float cameraRotationSpeed = 265f;

		[SerializeField]
		private float minimalRotationSpeed = 10f;

		private float initialDistance;

		private float initialAngleDifference;

		public const int MAX_CHARACTERS_IN_CIRCLE = 5;

		private const int CHARACTERS_VISIBLE_TO_THE_LEFT = 1;

		private const float QUARTER_CIRCLE_ANGLE = 90f;

		private int charactersToPlaceInCircleCount;

		private Transform tf;

		private List<Vector3> characterPositions = new List<Vector3>();

		private List<Vector3> characterDirections = new List<Vector3>();

		private List<Characters> selectableCharacters = new List<Characters>();

		private Transform characterSelectCameraTf;

		private Player player;

		private Vector3 camTargetPos;

		private Quaternion camTargetRot;

		private CharacterSelectUI characterSelectUI;

		private CharSelectState state;

		private float stateTimer;

		private EffectsUI effectsUI;

		private CharacterSelectCharacter[] charactersInCircle;

		private int charactersDoingIntroCount;

		private int selection;

		private bool acceptMoveSelection = true;

		private CharacterSelectSpot spot;

		private GameInput gameInput;

		private bool hasCharacterDownloadableContent;

		private int dance2Hash;

		private int characterNumGoingIn;

		private int selectionInCircle;

		public CharacterSelectCharacter[] CharactersInCircle => charactersInCircle;

		private static float AnglePerDude(int charactersInCircle)
		{
			return Mathf.Min(360 / charactersInCircle, 120);
		}

		public void Init(Player p, CharacterSelectSpot setSpot)
		{
			spot = setSpot;
			gameInput = Core.Instance.GameInput;
			characterSelectUI = Core.Instance.UIManager.characterSelectUI;
			effectsUI = Core.Instance.UIManager.effects;
			hasCharacterDownloadableContent = Core.Instance.Platform.DownloadableContent.IsDownloadableContentUnlocked(charactersDownloadableContent);
			tf = base.transform;
			tf.position = spot.transform.position;
			tf.rotation = spot.transform.rotation;
			player = p;
			p.SetPosAndRotHard(tf.position, tf.rotation);
			spot.GetComponentInChildren<NPC>(includeInactive: true).SetAvailable(set: false);
			characterSelectCamera.cullingMask = WorldHandler.GetGameplayCameraCullingMask();
			characterSelectCameraTf = characterSelectCamera.transform;
			dance2Hash = Animator.StringToHash("dance2");
			PopulateListOfSelectableCharacters(p);
			DefineLocations();
			charactersInCircle = new CharacterSelectCharacter[charactersToPlaceInCircleCount];
			for (int i = 0; i < charactersToPlaceInCircleCount; i++)
			{
				CreateCharacterSelectCharacter(selectableCharacters[i], i, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
			}
			charactersDoingIntroCount = Mathf.Min(charactersToPlaceInCircleCount, 3);
			int num = Random.Range(0, 5);
			for (int j = 0; j < charactersDoingIntroCount; j++)
			{
				charactersInCircle[j].startOffset = 1f * (float)j;
				charactersInCircle[j].startSequence = (num + j) % 5;
				charactersInCircle[j].SetState(CharacterSelectCharacter.CharSelectCharState.JOINING);
			}
			characterSelectUI.SetAvailableCharacterCount(selectableCharacters.Count);
			characterSelectUI.SetSelectedCharacterIndexReversed(selection);
			characterSelectUI.SetCharacterInformation(selectableCharacters[selection]);
			EnableUIControllerMaps();
			Core.Instance.UIManager.gameplay.gameObject.SetActive(value: false);
			WantedManager.OnWanted += OnWanted;
			AudioSource audioSource = Core.Instance.AudioManager.GetAudioSource(AudioSourceID.Voices);
			SetupAudioSourceBindings(selectSequence1, audioSource);
			SetupAudioSourceBindings(selectSequence2, audioSource);
			SetupAudioSourceBindings(selectSequence3, audioSource);
			SetupAudioSourceBindings(selectSequence4, audioSource);
			SetupAudioSourceBindings(selectSequence5, audioSource);
			SetupAudioSourceBindings(selectSequence6, audioSource);
			SetupAudioSourceBindings(selectSequence7, audioSource);
			SetupAudioSourceBindings(selectSequence8, audioSource);
			SetupAudioSourceBindings(selectSequence9, audioSource);
			SetupAudioSourceBindings(selectSequence10, audioSource);
			SetupAudioSourceBindings(selectSequence11, audioSource);
			SetupAudioSourceBindings(selectSequence12, audioSource);
			SetupAudioSourceBindings(selectSequence13, audioSource);
			SetupAudioSourceBindings(selectSequence14, audioSource);
			SetupAudioSourceBindings(selectSequence15, audioSource);
			SetupAudioSourceBindings(selectSequence16, audioSource);
			SetupAudioSourceBindings(selectSequence17, audioSource);
			SetupAudioSourceBindings(selectSequence18, audioSource);
			SetupAudioSourceBindings(selectSequence19, audioSource);
			SetState(CharSelectState.CHARACTERS_JOINING);
		}

		private void SetupAudioSourceBindings(PlayableDirector director, AudioSource source)
		{
			foreach (PlayableBinding output in director.playableAsset.outputs)
			{
				if (output.outputTargetType != null && output.outputTargetType.Equals(typeof(AudioSource)))
				{
					director.SetGenericBinding(output.sourceObject, source);
				}
			}
		}

		private void EnableUIControllerMaps()
		{
			gameInput.DisableAllControllerMaps();
			gameInput.EnableControllerMap(1);
		}

		private void PopulateListOfSelectableCharacters(Player player)
		{
			selectableCharacters.Clear();
			for (int i = 0; i < 26; i++)
			{
				Characters characters = (Characters)i;
				if (characters != Characters.headManNoJetpack && characters != Characters.headMan && player.character != characters && IsCharacterUnlocked(characters))
				{
					selectableCharacters.Add(characters);
				}
			}
			Shuffle(selectableCharacters);
		}

		private bool IsCharacterUnlocked(Characters character)
		{
			if (character == Characters.robot || character == Characters.skate)
			{
				return hasCharacterDownloadableContent;
			}
			return Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).unlocked;
		}

		private void Shuffle(List<Characters> list)
		{
			int num = list.Count;
			while (num > 1)
			{
				num--;
				int index = Random.Range(0, num + 1);
				Characters value = list[index];
				list[index] = list[num];
				list[num] = value;
			}
		}

		private void DefineLocations()
		{
			characterPositions.Clear();
			characterDirections.Clear();
			charactersToPlaceInCircleCount = Mathf.Min(5, selectableCharacters.Count);
			float num = 2.1f;
			int layerMask = 1;
			for (int i = 0; i < charactersToPlaceInCircleCount; i++)
			{
				Vector3 vector = Quaternion.Euler(0f, (0f - AnglePerDude(charactersToPlaceInCircleCount)) * (float)i, 0f) * Vector3.forward;
				CapsuleCollider capsule = player.motor.GetCapsule();
				if (Physics.Raycast(tf.position + Vector3.up * (capsule.height / 2f) + vector * num, maxDistance: capsule.height / 2f + capsule.radius, direction: Vector3.down, hitInfo: out var hitInfo, layerMask: layerMask))
				{
					characterPositions.Add(hitInfo.point);
					characterDirections.Add(vector);
				}
			}
		}

		private void CreateCharacterSelectCharacter(Characters character, int numInCircle, CharacterSelectCharacter.CharSelectCharState startState)
		{
			CharacterVisual characterVisual = player.CharacterConstructor.CreateNewCharacterVisual(character, player.animatorController, IK: true, player.motor.groundDetection.groundLimit);
			int outfit = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).outfit;
			Material sharedMaterial = player.CharacterConstructor.CreateCharacterMaterial(character, outfit);
			characterVisual.mainRenderer.sharedMaterial = sharedMaterial;
			charactersInCircle[numInCircle] = characterVisual.gameObject.AddComponent<CharacterSelectCharacter>();
			charactersInCircle[numInCircle].transform.position = characterPositions[numInCircle];
			charactersInCircle[numInCircle].transform.rotation = Quaternion.LookRotation(characterDirections[numInCircle] * -1f);
			charactersInCircle[numInCircle].transform.parent = tf;
			charactersInCircle[numInCircle].Init(this, character, charCollision, charTrigger, tf.position, Object.Instantiate(swapSequence, charactersInCircle[numInCircle].transform).GetComponent<PlayableDirector>());
			charactersInCircle[numInCircle].SetState(startState);
		}

		public void SetState(CharSelectState setState)
		{
			state = setState;
			stateTimer = 0f;
			if (state == CharSelectState.CHARACTERS_JOINING)
			{
				effectsUI.ShowBars(0.25f, 130f);
			}
			else if (state == CharSelectState.MAIN_STATE)
			{
				Core.Instance.UIManager.PushNewMenu(characterSelectUI);
				effectsUI.HideBars(0.25f);
				SetCameraValues();
				characterSelectCameraTf.position = camTargetPos;
				characterSelectCameraTf.rotation = camTargetRot;
			}
			else if (state == CharSelectState.CHARACTER_PICKED)
			{
				Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.confirm);
			}
		}

		private void SetCameraValues()
		{
			camTargetPos = charactersInCircle[selectionInCircle].StartPos + charactersInCircle[selectionInCircle].transform.rotation * localPosOnCharacter;
			camTargetRot = Quaternion.LookRotation(charactersInCircle[selectionInCircle].StartPos + charactersInCircle[selectionInCircle].transform.forward + Vector3.up * localPosOnCharacter.y - camTargetPos);
		}

		public void CharSelectUpdate()
		{
			stateTimer += Core.dt;
			if (state == CharSelectState.CHARACTERS_JOINING)
			{
				if (stateTimer < 1f)
				{
					Vector3 normalized = (characterPositions[0] - tf.position).normalized;
					camTargetPos = tf.position + Vector3.up * 0.4f + normalized + charactersInCircle[0].transform.right;
					camTargetRot = Quaternion.LookRotation(-(camTargetPos - (characterPositions[0] + Vector3.up * 0.4f)).normalized);
				}
				if (stateTimer >= 0.85f && stateTimer <= 1f && !effectsUI.IsShowingFade)
				{
					effectsUI.FadeInAndOutBlack(0.15f, 0f, 0.15f);
				}
				if (stateTimer >= 1f && stateTimer < 2f)
				{
					Vector3 normalized2 = (characterPositions[1] - tf.position).normalized;
					camTargetPos = tf.position + Vector3.up * 0.4f + normalized2 - charactersInCircle[1].transform.right;
					camTargetRot = Quaternion.LookRotation(-(camTargetPos - (characterPositions[1] + Vector3.up * 0.4f)).normalized);
				}
				if (stateTimer >= 1.85f && stateTimer < 2f && !effectsUI.IsShowingFade)
				{
					effectsUI.FadeInAndOutBlack(0.15f, 0f, 0.15f);
				}
				if (charactersToPlaceInCircleCount >= 3)
				{
					if (stateTimer >= 2f && stateTimer < 3f)
					{
						Vector3 normalized3 = (characterPositions[2] - tf.position).normalized;
						camTargetPos = tf.position + Vector3.up * 0.4f + normalized3 + charactersInCircle[2].transform.right;
						camTargetRot = Quaternion.LookRotation(-(camTargetPos - (characterPositions[2] + Vector3.up * 0.4f)).normalized);
					}
					if (stateTimer >= 2.85f)
					{
						effectsUI.FadeInAndOutBlack(0.15f, 0f, 0.15f);
					}
					if (stateTimer >= 3f)
					{
						SetState(CharSelectState.MAIN_STATE);
					}
				}
				else if (stateTimer >= 2f)
				{
					SetState(CharSelectState.MAIN_STATE);
				}
				UpdateCharacterSelectCharacters();
				characterSelectCameraTf.position = camTargetPos;
				characterSelectCameraTf.rotation = camTargetRot;
			}
			else if (state == CharSelectState.MAIN_STATE)
			{
				float distance = Vector3.Distance(camTargetPos, characterSelectCameraTf.position);
				float angleDifference = Quaternion.Angle(camTargetRot, characterSelectCameraTf.rotation);
				float axis = gameInput.GetAxis(0);
				if (axis >= -0f && axis <= 0f)
				{
					acceptMoveSelection = true;
				}
				if (Mathf.Abs(axis) >= 1f && acceptMoveSelection)
				{
					MoveSelection(axis);
					acceptMoveSelection = false;
					SetCameraValues();
					initialDistance = Vector3.Distance(camTargetPos, characterSelectCameraTf.position);
					initialAngleDifference = Quaternion.Angle(camTargetRot, characterSelectCameraTf.rotation);
				}
				float num = CalculateCameraMovementSpeed(initialDistance, distance);
				float num2 = CalculateCameraRotationSpeed(initialAngleDifference, angleDifference);
				characterSelectCameraTf.position = Vector3.MoveTowards(characterSelectCameraTf.position, camTargetPos, num * Time.deltaTime);
				characterSelectCameraTf.rotation = Quaternion.RotateTowards(characterSelectCameraTf.rotation, camTargetRot, num2 * Time.deltaTime);
				if (gameInput.GetButtonNew(2))
				{
					SetState(CharSelectState.CHARACTER_PICKED);
				}
				else if (gameInput.GetButtonNew(3))
				{
					StopCharacterSelect();
				}
				UpdateCharacterSelectCharacters();
			}
			else if (state == CharSelectState.CHARACTER_PICKED)
			{
				if (stateTimer >= 0.3f && !effectsUI.IsShowingFade)
				{
					effectsUI.FadeInAndOutBlack(0.5f, 0f, 0.5f);
				}
				if (stateTimer > 0.8f)
				{
					Characters character = charactersInCircle[selectionInCircle].character;
					SetPlayerToCharacter(selectionInCircle);
					FinishCharacterSelect();
					DoSelectSequence(character);
				}
			}
			else
			{
				if (state != CharSelectState.CYPHERING)
				{
					return;
				}
				for (int i = 0; i < charactersInCircle.Length; i++)
				{
					if (WorldHandler.instance.CharacterIsAlreadyNPCInScene(charactersInCircle[i].character))
					{
						charactersInCircle[i].SetAvailable(set: false);
					}
				}
				if (stateTimer > 1f && NobodyIsInTheCypher())
				{
					int num3 = characterNumGoingIn + 1;
					if (num3 > charactersToPlaceInCircleCount - 1)
					{
						num3 = 0;
					}
					characterNumGoingIn = num3;
					if (charactersInCircle[num3].state != CharacterSelectCharacter.CharSelectCharState.TALKING)
					{
						charactersInCircle[num3].SetState(CharacterSelectCharacter.CharSelectCharState.GOING_IN);
					}
				}
				UpdateCharacterSelectCharacters();
			}
		}

		private float CalculateCameraMovementSpeed(float initialDistance, float distance)
		{
			if (initialDistance > 0f)
			{
				return Mathf.Lerp(minimalCameraMovementSpeed, cameraMovementSpeed, distance / initialDistance);
			}
			return cameraMovementSpeed;
		}

		private float CalculateCameraRotationSpeed(float initalAngleDifference, float angleDifference)
		{
			if (angleDifference <= 67.5f)
			{
				return Mathf.Lerp(minimalRotationSpeed, cameraRotationSpeed, angleDifference / initalAngleDifference);
			}
			return cameraRotationSpeed;
		}

		private void UpdateCharacterSelectCharacters()
		{
			for (int i = 0; i < charactersToPlaceInCircleCount; i++)
			{
				charactersInCircle[i].UpdateCharacter();
			}
		}

		private bool NobodyIsInTheCypher()
		{
			for (int i = 0; i < charactersToPlaceInCircleCount; i++)
			{
				if (charactersInCircle[i].available && (charactersInCircle[i].state == CharacterSelectCharacter.CharSelectCharState.DOING_RUN || charactersInCircle[i].state == CharacterSelectCharacter.CharSelectCharState.GOING_IN))
				{
					return false;
				}
			}
			if (Vector3.Distance(player.tf.position, tf.position) < 1f)
			{
				return false;
			}
			return true;
		}

		public void MoveSelection(float move)
		{
			int num = 0;
			if (move >= 1f)
			{
				num = 1;
			}
			else if (move <= -1f)
			{
				num = -1;
			}
			selectionInCircle -= num;
			if (selectionInCircle >= charactersToPlaceInCircleCount)
			{
				selectionInCircle = 0;
			}
			if (selectionInCircle < 0)
			{
				selectionInCircle = charactersToPlaceInCircleCount - 1;
			}
			int num2 = selection - num;
			if (num2 >= selectableCharacters.Count)
			{
				num2 = 0;
			}
			if (num2 < 0)
			{
				num2 = selectableCharacters.Count - 1;
			}
			selection = num2;
			characterSelectUI.SetSelectedCharacterIndexReversed(selection);
			characterSelectUI.SetCharacterInformation(selectableCharacters[selection]);
			Core.Instance.AudioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.selectionMove);
			UpdateCharactersInCircle(move < 0f);
		}

		public void UpdateCharactersInCircle(bool forward)
		{
			if (forward)
			{
				int num = selectionInCircle + 1;
				if (num >= charactersToPlaceInCircleCount)
				{
					num = 0;
				}
				if (num < 0)
				{
					num = charactersToPlaceInCircleCount + num;
				}
				int num2 = selection + 1;
				if (num2 >= selectableCharacters.Count)
				{
					num2 = 0;
				}
				if (num2 < 0)
				{
					num2 = selectableCharacters.Count + num2;
				}
				if (charactersInCircle[num] == null || charactersInCircle[num].character != selectableCharacters[num2])
				{
					if (charactersInCircle[num] != null)
					{
						Object.Destroy(charactersInCircle[num].gameObject);
					}
					CreateCharacterSelectCharacter(selectableCharacters[num2], num, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
				}
			}
			else if ((charactersInCircle[selectionInCircle] == null || charactersInCircle[selectionInCircle].character != selectableCharacters[selection]) && charactersInCircle[selectionInCircle].character != selectableCharacters[selection])
			{
				if (charactersInCircle[selectionInCircle] != null)
				{
					Object.Destroy(charactersInCircle[selectionInCircle].gameObject);
				}
				CreateCharacterSelectCharacter(selectableCharacters[selection], selectionInCircle, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
			}
		}

		private void StopCharacterSelect()
		{
			Object.Destroy(characterSelectCameraTf.gameObject);
			player.StopCurrentAbility();
			SetState(CharSelectState.CYPHERING);
			EnableInGameControllerMaps();
		}

		private void FinishCharacterSelect()
		{
			StopCharacterSelect();
		}

		private void EnableInGameControllerMaps()
		{
			gameInput.DisableControllerMap(1);
			gameInput.EnableControllerMaps(BaseModule.IN_GAME_INPUT_MAPS);
		}

		private static PlayableDirector ChooseRandomSelectSequence(PlayableDirector a, PlayableDirector b, PlayableDirector c)
		{
			PlayableDirector result = c;
			if (Random.value < 0.33f)
			{
				result = a;
			}
			else if (Random.value > 0.5f)
			{
				result = b;
			}
			return result;
		}

		private PlayableDirector GetSelectSequenceAnim(Characters c)
		{
			PlayableDirector result = selectSequence1;
			switch (c)
			{
			case Characters.frank:
				result = ChooseRandomSelectSequence(selectSequence4, selectSequence11, selectSequence5);
				break;
			case Characters.girl1:
				result = ChooseRandomSelectSequence(selectSequence4, selectSequence6, selectSequence14);
				break;
			case Characters.ringdude:
				result = ChooseRandomSelectSequence(selectSequence4, selectSequence3, selectSequence12);
				break;
			case Characters.metalHead:
			case Characters.headMan:
			case Characters.headManNoJetpack:
				result = ChooseRandomSelectSequence(selectSequence2, selectSequence19, selectSequence14);
				break;
			case Characters.blockGuy:
				result = ChooseRandomSelectSequence(selectSequence1, selectSequence18, selectSequence10);
				break;
			case Characters.spaceGirl:
				result = ChooseRandomSelectSequence(selectSequence17, selectSequence6, selectSequence12);
				break;
			case Characters.angel:
				result = ChooseRandomSelectSequence(selectSequence1, selectSequence5, selectSequence14);
				break;
			case Characters.dj:
				result = ChooseRandomSelectSequence(selectSequence3, selectSequence10, selectSequence19);
				break;
			case Characters.dummy:
				result = ChooseRandomSelectSequence(selectSequence6, selectSequence15, selectSequence17);
				break;
			case Characters.eightBall:
			case Characters.eightBallBoss:
				result = ChooseRandomSelectSequence(selectSequence2, selectSequence10, selectSequence11);
				break;
			case Characters.medusa:
				result = ChooseRandomSelectSequence(selectSequence7, selectSequence10, selectSequence17);
				break;
			case Characters.boarder:
				result = ChooseRandomSelectSequence(selectSequence1, selectSequence6, selectSequence8);
				break;
			case Characters.prince:
				result = ChooseRandomSelectSequence(selectSequence7, selectSequence8, selectSequence9);
				break;
			case Characters.legendFace:
			case Characters.legendMetalHead:
				result = ChooseRandomSelectSequence(selectSequence8, selectSequence16, selectSequence18);
				break;
			case Characters.oldheadPlayer:
				result = ChooseRandomSelectSequence(selectSequence18, selectSequence14, selectSequence10);
				break;
			case Characters.robot:
				result = ChooseRandomSelectSequence(selectSequence18, selectSequence9, selectSequence16);
				break;
			case Characters.skate:
				result = ChooseRandomSelectSequence(selectSequence13, selectSequence17, selectSequence15);
				break;
			case Characters.wideKid:
				result = ChooseRandomSelectSequence(selectSequence18, selectSequence14, selectSequence10);
				break;
			case Characters.futureGirl:
				result = ChooseRandomSelectSequence(selectSequence2, selectSequence13, selectSequence14);
				break;
			case Characters.pufferGirl:
				result = ChooseRandomSelectSequence(selectSequence3, selectSequence11, selectSequence19);
				break;
			case Characters.bunGirl:
				result = ChooseRandomSelectSequence(selectSequence7, selectSequence12, selectSequence14);
				break;
			case Characters.jetpackBossPlayer:
				result = ChooseRandomSelectSequence(selectSequence9, selectSequence15, selectSequence12);
				break;
			}
			return result;
		}

		private void DoSelectSequence(Characters c)
		{
			PlayableDirector selectSequenceAnim = GetSelectSequenceAnim(c);
			selectSequenceAnim.transform.forward = player.tf.forward;
			SequenceHandler.instance.StartEnteringSequence(selectSequenceAnim, null, setHidePlayer: true, setInterruptPlayer: false, instantly: true);
		}

		public void SwapWithCharacter(GameObject swapTarget)
		{
			int num = 0;
			for (int i = 0; i < charactersInCircle.Length; i++)
			{
				if (charactersInCircle[i].gameObject == swapTarget)
				{
					num = i;
				}
			}
			player.tf.position = tf.position;
			Characters character = charactersInCircle[num].character;
			SetPlayerToCharacter(num);
			for (int j = 0; j < charactersInCircle.Length; j++)
			{
				charactersInCircle[j].SetState(CharacterSelectCharacter.CharSelectCharState.BOUNCING);
				charactersInCircle[j].transform.position = charactersInCircle[j].StartPos;
			}
			player.SetRotHard((tf.position - charactersInCircle[num].StartPos).normalized);
			stateTimer = 0f;
			DoSelectSequence(character);
		}

		private void SetPlayerToCharacter(int index)
		{
			CharacterSelectCharacter characterSelectCharacter = charactersInCircle[index];
			Characters character = player.character;
			Characters character2 = characterSelectCharacter.character;
			player.SetCharacter(character2);
			player.InitVisual();
			player.PlayAnim(player.characterVisual.bounceAnimHash);
			player.SetCurrentMoveStyleEquipped(Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character2).moveStyle);
			player.SetRotHard(characterSelectCharacter.visual.tf.forward);
			int outfit = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(characterSelectCharacter.character).outfit;
			Material material = player.CharacterConstructor.CreateCharacterMaterial(characterSelectCharacter.character, outfit);
			player.characterVisual.mainRenderer.material = material;
			Object.Destroy(characterSelectCharacter.gameObject);
			CreateCharacterSelectCharacter(character, index, CharacterSelectCharacter.CharSelectCharState.BOUNCING);
		}

		public void OnWanted(bool wanted)
		{
			if (wanted)
			{
				WorldHandler.instance.DestroyCharacterSelect();
			}
		}

		private void OnDestroy()
		{
			if (spot != null)
			{
				spot.GetComponentInChildren<NPC>(includeInactive: true).SetAvailable(set: true);
			}
		}
	}
}
