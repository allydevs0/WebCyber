using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class StyleSwitchResetTexture : MonoBehaviour
	{
		public MoveStyle moveStyle;

		private void Start()
		{
			StageManager.OnStageInitialized += StageManager_OnStageInitialized;
		}

		private void OnDestroy()
		{
			StageManager.OnStageInitialized -= StageManager_OnStageInitialized;
		}

		private void StageManager_OnStageInitialized()
		{
			Texture moveStyleSkinTexture = WorldHandler.instance.GetCurrentPlayer().CharacterConstructor.GetMoveStyleSkinTexture(moveStyle, 0);
			Transform parent = base.transform.parent;
			if (parent.GetComponent<NPC>() == null)
			{
				parent = parent.parent;
			}
			int num = 0;
			Material[] array = new Material[2];
			Renderer[] componentsInChildren = parent.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				List<Material> list = new List<Material>(2);
				componentsInChildren[i].GetSharedMaterials(list);
				for (int j = 0; j < list.Count; j++)
				{
					bool flag = false;
					for (int k = 0; k < num; k++)
					{
						flag |= array[k].shader == list[j].shader;
					}
					if (!flag)
					{
						Material material = new Material(list[j]);
						material.mainTexture = moveStyleSkinTexture;
						array[num++] = material;
					}
				}
				for (int l = 0; l < list.Count; l++)
				{
					list[l] = array[l];
				}
				componentsInChildren[i].sharedMaterials = list.ToArray();
			}
		}
	}
}
