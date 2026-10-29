using UnityEngine;

namespace Reptile
{
	public abstract class AProgressable : MonoBehaviour
	{
		[SerializeField]
		protected string uid = string.Empty;

		protected ProgressableData progressableData;

		protected ProgressableType progressableType;

		private bool hasSaveData;

		public string Uid => uid;

		public virtual void Awake()
		{
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister += OnRegisterToSceneObjectsRegister;
		}

		public virtual void OnDestroy()
		{
			progressableData = null;
			SceneObjectsRegister.OnRegisterToSceneObjectsRegister -= OnRegisterToSceneObjectsRegister;
		}

		public void InitProgressable(StageProgress stageProgress)
		{
			hasSaveData = stageProgress.HasProgressableData(uid);
			if (hasSaveData)
			{
				progressableData = stageProgress.GetProgressableData(uid);
				ReadFromData();
			}
			else
			{
				SetDefaultData(stageProgress);
			}
		}

		protected virtual void OnRegisterToSceneObjectsRegister(SceneObjectsRegister sceneObjectsRegister)
		{
			sceneObjectsRegister.RegisterProgressableObject(this);
		}

		public abstract void ReadFromData();

		public abstract void SetDefaultData(StageProgress stageProgress);

		public abstract void WriteToData();
	}
}
