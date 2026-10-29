using UnityEngine;

namespace Reptile
{
	public class CharacterConstructor
	{
		private const string CHARACTER_VISUALS_AFFIX = "Visuals";

		public const int MAX_OUTFITS = 4;

		public const int MAX_MOVESTYLE_SKINS = 10;

		private GameObject[] characterVisuals = new GameObject[26];

		private CharacterLoader characterLoader;

		private MoveStyleLoader moveStyleLoader;

		public CharacterConstructor(CharacterLoader characterLoader, MoveStyleLoader moveStyleLoader)
		{
			characterVisuals = new GameObject[26];
			this.characterLoader = characterLoader;
			this.moveStyleLoader = moveStyleLoader;
		}

		public void Dispose()
		{
			DestroyCharacterVisuals();
			characterVisuals = null;
			characterLoader = null;
			moveStyleLoader = null;
		}

		public Material[,] GetCharacterMaterials()
		{
			return characterLoader.LoadedCharacterMaterials;
		}

		public GameObject GetCharacterVisual(Characters character)
		{
			return characterVisuals[(int)character];
		}

		public Texture GetMoveStyleSkinTexture(MoveStyle moveStyle, int skinIndex)
		{
			return moveStyleLoader.GetSkinTexture(moveStyle, skinIndex);
		}

		public void SetMoveStyleSkinsForCharacter(Player player, Characters character)
		{
			if (player.isAI)
			{
				return;
			}
			int moveStyleSkin = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character).moveStyleSkin;
			for (MoveStyle moveStyle = MoveStyle.BMX; moveStyle < MoveStyle.SPECIAL_SKATEBOARD; moveStyle++)
			{
				Texture moveStyleSkinTexture = GetMoveStyleSkinTexture(moveStyle, moveStyleSkin);
				Material[] moveStyleMaterials = MoveStyleLoader.GetMoveStyleMaterials(player, moveStyle);
				for (int i = 0; i < moveStyleMaterials.Length; i++)
				{
					moveStyleMaterials[i].mainTexture = moveStyleSkinTexture;
				}
			}
		}

		public Material CreateCharacterMaterial(Characters character, int outfit)
		{
			return Object.Instantiate(characterLoader.GetCharacterMaterial(character, outfit));
		}

		public GameObject CreateCharacterFbx(Characters character)
		{
			return Object.Instantiate(characterLoader.GetCharacterFbx(character));
		}

		public CharacterVisual CreateNewCharacterVisual(Characters character, RuntimeAnimatorController controller, bool IK = false, float setGroundAngleLimit = 0f)
		{
			CharacterVisual characterVisual = Object.Instantiate(GetCharacterVisual(character)).AddComponent<CharacterVisual>();
			characterVisual.Init(character, controller, IK, setGroundAngleLimit);
			characterVisual.gameObject.SetActive(value: true);
			return characterVisual;
		}

		public void ConstructAllCharacterVisuals()
		{
			for (int i = 0; i < 26; i++)
			{
				ConstructCharacterVisual((Characters)i);
			}
		}

		public void ConstructCharacterVisual(Characters characterToConstruct)
		{
			GameObject gameObject = new GameObject($"{characterToConstruct}" + "Visuals");
			GameObject characterModel = CreateCharacterFbx(characterToConstruct);
			InitCharacterModel(characterModel, gameObject);
			InitSkinnedMeshRendererForModel(characterToConstruct, characterModel);
			InitAnimatorForModel(characterModel);
			InitCharacterVisuals(gameObject);
			characterVisuals[(int)characterToConstruct] = gameObject;
		}

		private void InitCharacterModel(GameObject characterModel, GameObject characterVisualsObject)
		{
			characterModel.transform.SetParent(characterVisualsObject.transform, worldPositionStays: false);
		}

		private void InitSkinnedMeshRendererForModel(Characters characterToConstruct, GameObject characterModel)
		{
			SkinnedMeshRenderer componentInChildren = characterModel.GetComponentInChildren<SkinnedMeshRenderer>();
			componentInChildren.sharedMaterial = CreateCharacterMaterial(characterToConstruct, 0);
			componentInChildren.receiveShadows = false;
		}

		private void InitAnimatorForModel(GameObject characterModel)
		{
			characterModel.GetComponentInChildren<Animator>().applyRootMotion = false;
		}

		private void InitCharacterVisuals(GameObject characterVisualsObject)
		{
			characterVisualsObject.SetActive(value: false);
		}

		private void DestroyCharacterVisuals()
		{
			for (int i = 0; i < characterVisuals.Length; i++)
			{
				if (!(characterVisuals[i] == null))
				{
					DestroyCharacterVisualMaterial(characterVisuals[i]);
					Object.Destroy(characterVisuals[i]);
				}
			}
		}

		private void DestroyCharacterVisualMaterial(GameObject characterVisualGameObject)
		{
			SkinnedMeshRenderer componentInChildren = characterVisualGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			if (!(componentInChildren == null))
			{
				Object.Destroy(componentInChildren.sharedMaterial);
			}
		}
	}
}
