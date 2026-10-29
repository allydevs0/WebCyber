using UnityEngine;

namespace Reptile
{
	public class DynamicPickup : Pickup
	{
		private bool disappear;

		private GameObject visual;

		public override void Awake()
		{
			base.Awake();
			SkinnedMeshRenderer componentInChildren = GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				visual = componentInChildren.gameObject;
			}
			else
			{
				visual = GetComponentInChildren<MeshRenderer>().gameObject;
			}
			disappear = unlock == null;
			if (WorldHandler.instance != null && WorldHandler.instance.SceneObjectsRegister != null)
			{
				AddPickupToSceneObjects();
			}
			if (pickupType == PickUpType.MOVESTYLE_SKIN_UNLOCKABLE)
			{
				Collectable.SetMovestyleSkinTexture(this, unlock);
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			sceneObjectsRegister.RemoveDynamicPickup(this);
		}

		public override void UpdatePickup()
		{
			timer += Core.dt;
			if (disappear)
			{
				if (timer > 10f)
				{
					visual.SetActive((int)(timer * 16f) % 2 == 0);
				}
				if (timer > 13f)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}

		public override bool CanBePickedUpBy(Player p)
		{
			return timer > 0.6f;
		}

		public override void PickupPickup(Player p)
		{
			PickedUpVisualization(p);
			ApplyPickup(p);
			Object.Destroy(base.gameObject);
		}

		protected void AddPickupToSceneObjects()
		{
			OnRegisterToSceneObjectsRegister(WorldHandler.instance.SceneObjectsRegister);
		}
	}
}
