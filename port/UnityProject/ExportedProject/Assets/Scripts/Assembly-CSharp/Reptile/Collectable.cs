using Reptile.Phone;
using UnityEngine;

namespace Reptile
{
	public class Collectable : GameplayEvent
	{
		private Pickup.PickUpType pickUpType;

		private float timer;

		internal bool pickedUp;

		internal bool shownOpening;

		public AUnlockable unlock;

		public override void InitSceneObject()
		{
			base.InitSceneObject();
			InitBasedOnUnlock();
		}

		public override void UpdateGameplayEvent()
		{
			if (pickedUp || base.gameObject.activeSelf || !canOpen)
			{
				return;
			}
			if (openingSequence == null)
			{
				base.gameObject.SetActive(value: true);
			}
			else
			{
				if (shownOpening)
				{
					return;
				}
				timer += Core.dt;
				if (timer >= 0.5f)
				{
					base.gameObject.SetActive(value: true);
					shownOpening = true;
					if (openingSequence != null)
					{
						StartOpeningSequence();
					}
				}
			}
		}

		public virtual void PickupCollectable(Player byPlayer)
		{
			Core instance = Core.Instance;
			Pickup.ApplyPickupType(instance.SaveManager, instance.AudioManager, instance.Platform.User, byPlayer, unlock, pickUpType, base.transform.position);
			pickedUp = true;
			base.gameObject.SetActive(value: false);
			WriteToData();
		}

		internal override void SetAvailable(bool set)
		{
			if (!pickedUp && canOpen)
			{
				base.SetAvailable(set);
			}
		}

		public virtual bool CanBePickedUp()
		{
			return true;
		}

		public virtual void StartOpeningSequence()
		{
			SequenceHandler.instance.StartEnteringSequence(openingSequence, this, setHidePlayer: true, setInterruptPlayer: false);
		}

		public override void SetDefaultData(StageProgress stageProgress)
		{
			progressableData = stageProgress.CreateProgressableData(uid, ProgressableType.COLLECTABLE_PROGRESS);
			CollectableProgress obj = (CollectableProgress)progressableData;
			obj.pickedUp = pickedUp;
			obj.canOpen = canOpen;
			base.gameObject.SetActive(!pickedUp && canOpen);
		}

		public override void ReadFromData()
		{
			CollectableProgress collectableProgress = (CollectableProgress)progressableData;
			pickedUp = collectableProgress.pickedUp;
			canOpen = collectableProgress.canOpen;
			base.gameObject.SetActive(!pickedUp && canOpen);
		}

		public override void WriteToData()
		{
			CollectableProgress obj = (CollectableProgress)progressableData;
			obj.pickedUp = pickedUp;
			obj.canOpen = canOpen;
			Core.Instance.SaveManager.SaveCurrentSaveSlot();
		}

		public void InitBasedOnUnlock()
		{
			if (unlock != null)
			{
				if (unlock.GetType() == typeof(GraffitiAppEntry))
				{
					pickUpType = Pickup.PickUpType.GRAFFITI_UNLOCKABLE;
				}
				else if (unlock.GetType() == typeof(MusicTrack))
				{
					pickUpType = Pickup.PickUpType.MUSIC_UNLOCKABLE;
				}
				else if (unlock.GetType() == typeof(OutfitUnlockable))
				{
					pickUpType = Pickup.PickUpType.OUTFIT_UNLOCKABLE;
				}
				else if (unlock.GetType() == typeof(MoveStyleSkin))
				{
					pickUpType = Pickup.PickUpType.MOVESTYLE_SKIN_UNLOCKABLE;
					SetMovestyleSkinTexture(this, unlock);
				}
			}
		}

		internal static void SetMovestyleSkinTexture(Component component, AUnlockable unlock)
		{
			MoveStyleSkin moveStyleSkin = unlock as MoveStyleSkin;
			MoveStyleCollectableVisuals componentInChildren = component.GetComponentInChildren<MoveStyleCollectableVisuals>();
			if (!(componentInChildren != null))
			{
				return;
			}
			Renderer[] componentsInChildren = componentInChildren.GetComponentsInChildren<MeshRenderer>();
			Renderer[] array = componentsInChildren;
			if (array == null)
			{
				return;
			}
			int num = 2;
			Material[] array2 = new Material[num];
			Material[] array3 = new Material[num];
			for (int i = 0; i < array.Length; i++)
			{
				Material[] sharedMaterials = array[i].sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					for (int k = 0; k < num; k++)
					{
						if (array2[k] == null)
						{
							array3[k] = sharedMaterials[j];
							Material material = new Material(sharedMaterials[j]);
							material.mainTexture = moveStyleSkin.skinTexture;
							array2[k] = material;
							sharedMaterials[j] = material;
							break;
						}
						if (array3[k] == sharedMaterials[j])
						{
							sharedMaterials[j] = array2[k];
							break;
						}
					}
				}
				array[i].sharedMaterials = sharedMaterials;
			}
		}
	}
}
