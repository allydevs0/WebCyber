using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Reptile
{
	public class FixedFramerateSequence : MonoBehaviour
	{
		public class OnPlay : UnityEvent
		{
		}

		public class OnPause : UnityEvent
		{
		}

		public class OnResume : UnityEvent
		{
		}

		public class OnStop : UnityEvent
		{
		}

		public OnPlay onPlay;

		public OnPause onPause;

		public OnResume onResume;

		public OnStop onStop;

		[SerializeField]
		private PlayableDirector director;

		[SerializeField]
		private float targetFPS = 60f;

		private bool paused = true;

		private bool playing;

		private float playTime;

		private float gameTime;

		private float fps;

		private void Awake()
		{
			director.timeUpdateMode = DirectorUpdateMode.Manual;
			Core.OnLateUpdate += LateUpdateDirector;
			fps = 1f / targetFPS;
			if (onPlay == null)
			{
				onPlay = new OnPlay();
			}
			if (onPause == null)
			{
				onPause = new OnPause();
			}
			if (onResume == null)
			{
				onResume = new OnResume();
			}
			if (onStop == null)
			{
				onStop = new OnStop();
			}
			if (director.playOnAwake)
			{
				Play();
			}
		}

		private void OnDestroy()
		{
			Core.OnLateUpdate -= LateUpdateDirector;
		}

		public void Pause()
		{
			if (playing)
			{
				paused = true;
				if (onPause != null)
				{
					onPause.Invoke();
				}
			}
		}

		public void Play()
		{
			gameTime = 0f;
			playTime = 0f;
			paused = false;
			playing = true;
			if (onPlay != null)
			{
				onPlay.Invoke();
			}
		}

		public void Resume()
		{
			if (playing)
			{
				if (onResume != null)
				{
					onResume.Invoke();
				}
				paused = false;
			}
		}

		public void Stop()
		{
			playing = false;
			paused = true;
			if (onStop != null)
			{
				onStop.Invoke();
			}
		}

		private void LateUpdateDirector()
		{
			if (!paused && playing)
			{
				gameTime += Core.dt;
				if (gameTime - playTime >= fps)
				{
					int num = Mathf.FloorToInt((gameTime - playTime) / fps);
					playTime += fps * (float)num;
					director.time += fps * (float)num;
				}
				director.Evaluate();
			}
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus)
			{
				director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
			}
			else
			{
				director.timeUpdateMode = DirectorUpdateMode.Manual;
			}
		}
	}
}
