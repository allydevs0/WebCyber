using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class MusicTimelineMixer : PlayableBehaviour
	{
		internal MusicTrack currentMainTrack;

		internal MusicTimelineClip clipToUseAsstageMusicAtEnd;

		internal int stagePlaybackSamples;

		internal static void PlayAsStageMusic(MusicTrack currentPlayingMusicTrack, MusicTrack musicTrack)
		{
			if (currentPlayingMusicTrack != musicTrack)
			{
				Core.Instance.AudioManager.MusicPlayer.AddAndBufferAsCurrentTrack(musicTrack);
				Core.Instance.AudioManager.MusicPlayer.ClearExceptCurrentTrack();
			}
		}

		internal static void SetMainMusicAudioSource(AudioSource source, MusicTrack musicTrack, bool playAsStageMusic, ref MusicTrack currentMainTrack, ref int stagePlaybackSamples)
		{
			if (stagePlaybackSamples == 0)
			{
				stagePlaybackSamples = Core.Instance.musicAudioSource.timeSamples;
			}
			if (Core.Instance.musicAudioSource != source)
			{
				AudioSource musicAudioSource = Core.Instance.musicAudioSource;
				Core.Instance.musicAudioSource = source;
				Core.Instance.musicBlendingAudioSource = musicAudioSource;
				if (musicAudioSource.isPlaying)
				{
					musicAudioSource.Stop();
					musicAudioSource.clip = null;
				}
			}
			MusicPlayer musicPlayer = (MusicPlayer)Core.Instance.AudioManager.MusicPlayer;
			musicPlayer.SetAudioSource(source);
			source.volume = 1f;
			currentMainTrack = musicTrack;
			if (playAsStageMusic)
			{
				PlayAsStageMusic(musicPlayer.GetMusicTrack(musicPlayer.CurrentTrackIndex), musicTrack);
			}
		}

		internal static bool IsPlayingOnSource(AudioSource source, AudioClip clip)
		{
			if (source.clip == clip)
			{
				if (!source.isPlaying)
				{
					return !Application.isFocused;
				}
				return true;
			}
			return false;
		}

		internal static void PlayClipOnAudioSource(AudioSource source, AudioClip audioClip, int playbackSamples)
		{
			source.clip = audioClip;
			source.timeSamples = ((playbackSamples < audioClip.samples) ? playbackSamples : 0);
			source.Play();
		}

		internal static void BlendClipOnAvailableSource(AudioSource source0, AudioSource source1, float volume, MusicTrack musicTrack, MusicTrack currentMainTrack, int playbackSamples = 0)
		{
			AudioSource audioSource = null;
			bool flag = false;
			if (IsPlayingOnSource(source0, musicTrack.AudioClip))
			{
				audioSource = source0;
				flag = !audioSource.isPlaying;
			}
			else if (IsPlayingOnSource(source1, musicTrack.AudioClip))
			{
				audioSource = source1;
				flag = !audioSource.isPlaying;
			}
			else
			{
				audioSource = ((source0.isPlaying && source0.clip == currentMainTrack.AudioClip) ? source1 : source0);
				flag = true;
			}
			if (flag)
			{
				PlayClipOnAudioSource(audioSource, musicTrack.AudioClip, playbackSamples);
			}
			audioSource.volume = volume;
		}

		internal static void PlayAndSetMainMusic(AudioSource source0, AudioSource source1, MusicTrack musicTrack, ref MusicTrack currentMainTrack, int playbackSamples, bool playAsStageMusic, ref int stagePlaybackSamples)
		{
			AudioSource source2 = source0;
			if (!IsPlayingOnSource(source2, musicTrack.AudioClip))
			{
				source2 = source1;
				if (!IsPlayingOnSource(source2, musicTrack.AudioClip))
				{
					if (!source0.isPlaying)
					{
						source2 = source0;
						PlayClipOnAudioSource(source2, musicTrack.AudioClip, playbackSamples);
					}
					else
					{
						PlayClipOnAudioSource(source2, musicTrack.AudioClip, playbackSamples);
					}
				}
			}
			SetMainMusicAudioSource(source2, musicTrack, playAsStageMusic, ref currentMainTrack, ref stagePlaybackSamples);
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			int inputCount = playable.GetInputCount();
			IMusicPlayer musicPlayer = Core.Instance.AudioManager.MusicPlayer;
			MusicTrack musicTrack = musicPlayer.GetMusicTrack(musicPlayer.CurrentTrackIndex);
			float num = 0f;
			for (int i = 0; i < inputCount; i++)
			{
				float inputWeight = playable.GetInputWeight(i);
				MusicTimelineBehaviour behaviour = ((ScriptPlayable<MusicTimelineBehaviour>)playable.GetInput(i)).GetBehaviour();
				if (inputWeight >= 1f)
				{
					PlayAndSetMainMusic(Core.Instance.musicAudioSource, Core.Instance.musicBlendingAudioSource, behaviour.clip.musicTrack, ref currentMainTrack, behaviour.clip.playbackSamples, behaviour.clip.playAsStageMusic, ref stagePlaybackSamples);
				}
				else if (inputWeight > 0f)
				{
					if (musicTrack == behaviour.clip.musicTrack)
					{
						num -= inputWeight;
					}
					BlendClipOnAvailableSource(Core.Instance.musicAudioSource, Core.Instance.musicBlendingAudioSource, inputWeight, behaviour.clip.musicTrack, currentMainTrack, behaviour.clip.playbackSamples);
				}
				num += inputWeight;
			}
			musicTrack = musicPlayer.GetMusicTrack(musicPlayer.CurrentTrackIndex);
			if (num == 0f)
			{
				RevertToCurrentStageMusic(musicTrack, ref currentMainTrack, ref stagePlaybackSamples);
			}
			else if (num < 1f)
			{
				float volume = 1f - num;
				AudioSource musicAudioSource = Core.Instance.musicAudioSource;
				if (!musicAudioSource.isPlaying || !IsPlayingOnSource(musicAudioSource, musicTrack.AudioClip))
				{
					BlendClipOnAvailableSource(musicAudioSource, Core.Instance.musicBlendingAudioSource, volume, musicTrack, currentMainTrack, stagePlaybackSamples);
				}
			}
		}

		internal static void RevertToCurrentStageMusic(MusicTrack currentStageTrack, ref MusicTrack currentMainTrack, ref int playbackSamples)
		{
			PlayAndSetMainMusic(Core.Instance.musicAudioSource, Core.Instance.musicBlendingAudioSource, currentStageTrack, ref currentMainTrack, playbackSamples, playAsStageMusic: true, ref playbackSamples);
			Core.Instance.AudioManager.MusicPlayer.ClearExceptCurrentTrack();
			playbackSamples = 0;
		}

		public override void OnGraphStop(Playable playable)
		{
			base.OnGraphStop(playable);
			IMusicPlayer musicPlayer = Core.Instance.AudioManager.MusicPlayer;
			MusicTrack musicTrack = musicPlayer.GetMusicTrack(musicPlayer.CurrentTrackIndex);
			bool flag = false;
			if (musicTrack != null)
			{
				if (clipToUseAsstageMusicAtEnd != null)
				{
					musicTrack = clipToUseAsstageMusicAtEnd.musicTrack;
					flag = true;
					if (currentMainTrack != musicTrack)
					{
						stagePlaybackSamples = clipToUseAsstageMusicAtEnd.playbackSamples;
					}
				}
				RevertToCurrentStageMusic(musicTrack, ref currentMainTrack, ref stagePlaybackSamples);
			}
			if (!flag || (flag && clipToUseAsstageMusicAtEnd.refreshMusicQueue))
			{
				musicPlayer.RefreshMusicQueueForStage(Core.Instance.chapterMusic, musicTrack, Core.Instance.BaseModule.CurrentStage);
			}
		}
	}
}
