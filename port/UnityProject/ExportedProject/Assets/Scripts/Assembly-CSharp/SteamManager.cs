using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	protected static SteamManager s_instance;

	protected static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized => false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitOnPlayMode()
	{
		s_instance = null;
	}

	public void Initialize()
	{
		if (s_instance != null)
		{
			Destroy(gameObject);
			return;
		}
		s_instance = this;
		DontDestroyOnLoad(gameObject);
	}

	private void Awake()
	{
		Initialize();
	}
}
