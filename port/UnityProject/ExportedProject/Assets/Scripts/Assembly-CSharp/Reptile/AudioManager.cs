using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

namespace Reptile
{
	public class AudioManager : IDisposable
	{
		public const float defaultRandomPitchVariance = 0.05f;

		private SfxLibrary sfxLibrary;

		private AudioSettings audioSettings;

		private IAudioSubSystem audioSubSystem;

		private AudioSource[] audioSources;

		private AudioMixerGroup[] mixerGroups;

		private IMusicPlayer musicPlayer;

		private IMusicLibraryPlayer musicLibraryPlayer;

		private bool currentlyChangingVolume;

		private float musicVolumeBeforeLowering = -1f;

		private float temporaryVolumePercentage = 0.2f;

		private int temporaryVolumeRequests;

		internal float masterAudioMaxVolume01Clamped;

		internal float musicAudioMaxVolume01Clamped;

		internal float sfxAudioMaxVolume01Clamped;

		private Coroutine startLowerVolumeCoroutine;

		private Coroutine endLowerVolumeCoroutine;

		private MonoBehaviour startLowerVolumeRunner;

		private MonoBehaviour endLowerVolumeRunner;

		public SfxCollectionID[] characterToVoiceCollection;

		private Collider[] ambienceColliders;

		private AudioSource[] ambienceAudioSources;

		private AudioListener activeAudioListener;

		private StageAmbience stageAmbience;

		private AudioClipID currentAmbienceClip;

		private int currentAmbienceZonePriority;

		private AudioAmbienceZone currentAmbienceZone;

		private List<AudioSource> activeGameplayLoopingAudioSources;

		private const float CONST_AudioLerpTime = 0.6f;

		private Coroutine forcedVolumeCoroutine;

		private float volumeBeforeForcedVolume;

		private float normalVolumeDuringForcedVolume;

		private float loweredVolumeDuringForcedVolume;

		internal bool initialized;

		internal IMusicPlayer MusicPlayer => musicPlayer;

		internal IMusicLibraryPlayer MusicLibraryPlayer => musicLibraryPlayer;

		internal bool IsVolumeLowered => temporaryVolumeRequests > 0;

		private float MasterVolume => audioSettings.masterVolume;

		private float SfxVolume => audioSettings.sfxVolume;

		private float MusicVolume => audioSettings.musicVolume;

		internal AudioManager(SfxLibrary sfxLibrary, MusicLibrary musicLibrary, AudioSettings audioSettings, AudioSource musicAudioSource, AudioSource blendingMusicAudioSource, AudioSource gameplaySfxAudioSource, AudioSource uiSfxAudioSource, AudioSource voicesSfxAudioSource, StageAmbience stageAmbience, AudioSource[] ambienceAudioSources, IAudioSubSystem audioSubSystem)
		{
			this.sfxLibrary = sfxLibrary;
			this.audioSubSystem = audioSubSystem;
			this.stageAmbience = stageAmbience;
			this.ambienceAudioSources = ambienceAudioSources;
			audioSources = new AudioSource[7];
			audioSources[3] = gameplaySfxAudioSource;
			audioSources[2] = uiSfxAudioSource;
			audioSources[5] = voicesSfxAudioSource;
			ambienceColliders = new Collider[5];
			InitializeVoicesLookupTable(ref characterToVoiceCollection);
			GetAudioChannels();
			SetAudioSettings(audioSettings);
			InitializeSfxLibrary();
			InitializeMusicPlayer(musicAudioSource, blendingMusicAudioSource, musicLibrary);
			activeGameplayLoopingAudioSources = new List<AudioSource>();
			initialized = true;
		}

		internal void ResetTemporaryVolumes()
		{
			if (endLowerVolumeCoroutine != null)
			{
				if (endLowerVolumeRunner != null)
				{
					endLowerVolumeRunner.StopCoroutine(endLowerVolumeCoroutine);
					endLowerVolumeRunner = null;
				}
				if (currentlyChangingVolume)
				{
					audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, musicVolumeBeforeLowering, musicAudioMaxVolume01Clamped);
					currentlyChangingVolume = false;
				}
			}
			if (startLowerVolumeCoroutine != null)
			{
				if (startLowerVolumeRunner != null)
				{
					startLowerVolumeRunner.StopCoroutine(startLowerVolumeCoroutine);
					startLowerVolumeRunner = null;
				}
				if (currentlyChangingVolume)
				{
					audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, musicVolumeBeforeLowering, musicAudioMaxVolume01Clamped);
					currentlyChangingVolume = false;
				}
			}
			endLowerVolumeCoroutine = null;
			startLowerVolumeCoroutine = null;
		}

		private void InitializeSfxLibrary()
		{
			sfxLibrary.Init();
		}

		private void InitializeMusicPlayer(AudioSource musicAudioSource, AudioSource blendingMusicAudioSource, MusicLibrary musicLibrary)
		{
			musicLibrary.Init();
			MusicTrackQueue musicTrackQueue = new MusicTrackQueue(new DefaultIndexQueue(-1), new MusicPlayerBuffer(), 1u, 1u);
			musicPlayer = new MusicPlayer(musicAudioSource, musicTrackQueue);
			musicLibraryPlayer = new MusicLibraryPlayer(musicLibrary, musicPlayer);
			musicAudioSource.outputAudioMixerGroup = mixerGroups[4];
			blendingMusicAudioSource.outputAudioMixerGroup = mixerGroups[4];
		}

		internal AudioSource GetAudioSource(AudioSourceID audioSourceID)
		{
			return audioSources[(int)audioSourceID];
		}

		internal void SetStageAudio(Stage newStage)
		{
			ambienceAudioSources[0].time = 0f;
			ambienceAudioSources[1].time = 0f;
			if (newStage == Stage.NONE)
			{
				currentAmbienceClip = AudioClipID.NONE;
				ambienceAudioSources[0].volume = 0f;
				ambienceAudioSources[1].volume = 0f;
			}
			else
			{
				currentAmbienceClip = stageAmbience.GetStageAmbience(newStage);
				if (currentAmbienceClip == AudioClipID.NONE)
				{
					ambienceAudioSources[0].volume = 0f;
					ambienceAudioSources[1].volume = 0f;
				}
				else
				{
					AudioClip audioClipFromCollection = sfxLibrary.GetAudioClipFromCollection(SfxCollectionID.AmbienceSfx, currentAmbienceClip);
					SetAmbienceOnAudioSource(ambienceAudioSources[0], audioClipFromCollection, mixerGroups[6], 1f);
					SetAmbienceOnAudioSource(ambienceAudioSources[1], audioClipFromCollection, mixerGroups[6], 0f);
				}
			}
			if (currentlyChangingVolume)
			{
				audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, musicVolumeBeforeLowering, musicAudioMaxVolume01Clamped);
				currentlyChangingVolume = false;
				temporaryVolumeRequests = 0;
			}
		}

		public void Dispose()
		{
			initialized = false;
			startLowerVolumeCoroutine = null;
			startLowerVolumeRunner = null;
			endLowerVolumeCoroutine = null;
			endLowerVolumeRunner = null;
			musicPlayer.Dispose();
			musicPlayer = null;
			musicLibraryPlayer.Dispose();
			musicLibraryPlayer = null;
			audioSubSystem = null;
			audioSettings = null;
			sfxLibrary = null;
			activeGameplayLoopingAudioSources.Clear();
			audioSources = null;
			DisposeAudioChannels();
		}

		private void DisposeAudioChannels()
		{
			mixerGroups = null;
		}

		private void GetAudioChannels()
		{
			mixerGroups = new AudioMixerGroup[7];
			mixerGroups[0] = audioSubSystem.GetAudioChannel(AudioChannelID.Master);
			mixerGroups[4] = audioSubSystem.GetAudioChannel(AudioChannelID.Music);
			mixerGroups[1] = audioSubSystem.GetAudioChannel(AudioChannelID.Sfx);
			mixerGroups[2] = audioSubSystem.GetAudioChannel(AudioChannelID.UI);
			mixerGroups[3] = audioSubSystem.GetAudioChannel(AudioChannelID.Gameplay);
			mixerGroups[5] = audioSubSystem.GetAudioChannel(AudioChannelID.Voices);
			mixerGroups[6] = audioSubSystem.GetAudioChannel(AudioChannelID.Ambience);
			masterAudioMaxVolume01Clamped = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Master, 1f);
			sfxAudioMaxVolume01Clamped = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Sfx, 1f);
			musicAudioMaxVolume01Clamped = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Music, 1f);
		}

		internal AudioMixerGroup GetAudioChannel(AudioChannelID channelID)
		{
			return audioSubSystem.GetAudioChannel(channelID);
		}

		internal void SetAudioSettings(AudioSettings audioSettings)
		{
			this.audioSettings = audioSettings;
			SetMasterVolume(MasterVolume);
			SetSfxVolume(SfxVolume);
			SetMusicVolume(MusicVolume);
		}

		internal void SetMasterVolume(float audioChannelVolumePercentage)
		{
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Master, audioChannelVolumePercentage, masterAudioMaxVolume01Clamped);
		}

		internal void SetSfxVolume(float audioChannelVolumePercentage)
		{
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Sfx, audioChannelVolumePercentage, sfxAudioMaxVolume01Clamped);
		}

		internal void SetMusicVolume(float audioChannelVolumePercentage)
		{
			musicVolumeBeforeLowering = audioChannelVolumePercentage;
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, audioChannelVolumePercentage, musicAudioMaxVolume01Clamped);
		}

		internal void StartTemporarilyLowerVolume(MonoBehaviour behaviourToRunOn)
		{
			temporaryVolumeRequests++;
			if (temporaryVolumeRequests == 1 && startLowerVolumeCoroutine == null)
			{
				if (endLowerVolumeCoroutine != null && endLowerVolumeRunner != null)
				{
					endLowerVolumeRunner.StopCoroutine(endLowerVolumeCoroutine);
					endLowerVolumeCoroutine = null;
					endLowerVolumeRunner = null;
				}
				startLowerVolumeRunner = behaviourToRunOn;
				startLowerVolumeCoroutine = startLowerVolumeRunner.StartCoroutine(TemporarilyLowerVolumeBegin(startLowerVolumeRunner));
			}
		}

		internal void StopTemporarilyLowerVolume(MonoBehaviour behaviourToRunOn)
		{
			temporaryVolumeRequests--;
			if (temporaryVolumeRequests == 0 && endLowerVolumeCoroutine == null)
			{
				if (startLowerVolumeCoroutine != null && startLowerVolumeRunner != null)
				{
					startLowerVolumeRunner.StopCoroutine(startLowerVolumeCoroutine);
					startLowerVolumeCoroutine = null;
					startLowerVolumeRunner = null;
				}
				endLowerVolumeRunner = behaviourToRunOn;
				endLowerVolumeCoroutine = endLowerVolumeRunner.StartCoroutine(TemporarilyLowerVolumeEnd(endLowerVolumeRunner));
			}
		}

		private IEnumerator TemporarilyLowerVolumeBegin(MonoBehaviour behaviour)
		{
			if (!currentlyChangingVolume)
			{
				musicVolumeBeforeLowering = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Music, musicAudioMaxVolume01Clamped);
			}
			currentlyChangingVolume = true;
			float temporaryVolume = musicVolumeBeforeLowering * temporaryVolumePercentage;
			float timePassed = 0f;
			float totalTimeForLerp = 0.6f;
			do
			{
				float volumeScale = Mathf.Lerp(musicVolumeBeforeLowering, temporaryVolume, timePassed / totalTimeForLerp);
				audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, volumeScale, musicAudioMaxVolume01Clamped);
				yield return null;
				timePassed += Core.dt;
			}
			while (timePassed < totalTimeForLerp);
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, temporaryVolume, musicAudioMaxVolume01Clamped);
			currentlyChangingVolume = false;
			startLowerVolumeCoroutine = null;
			startLowerVolumeRunner = null;
		}

		private IEnumerator TemporarilyLowerVolumeEnd(MonoBehaviour behaviour)
		{
			currentlyChangingVolume = true;
			float temporaryVolume = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Music, musicAudioMaxVolume01Clamped);
			float timePassed = 0f;
			float totalTimeForLerp = 0.6f;
			do
			{
				float volumeScale = Mathf.Lerp(temporaryVolume, musicVolumeBeforeLowering, timePassed / totalTimeForLerp);
				audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, volumeScale, musicAudioMaxVolume01Clamped);
				yield return null;
				timePassed += Core.dt;
				if (temporaryVolumeRequests != 0)
				{
					yield break;
				}
			}
			while (timePassed < totalTimeForLerp && temporaryVolumeRequests == 0);
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, musicVolumeBeforeLowering, musicAudioMaxVolume01Clamped);
			currentlyChangingVolume = false;
			endLowerVolumeCoroutine = null;
			endLowerVolumeRunner = null;
		}

		internal void DirectTemporaryLowerVolume()
		{
			temporaryVolumeRequests++;
			if (temporaryVolumeRequests == 1)
			{
				if (!currentlyChangingVolume)
				{
					musicVolumeBeforeLowering = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Music, musicAudioMaxVolume01Clamped);
				}
				float volumeScale = musicVolumeBeforeLowering * temporaryVolumePercentage;
				audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, volumeScale, musicAudioMaxVolume01Clamped);
				currentlyChangingVolume = false;
			}
		}

		internal void DirectTemporaryRecoverVolume()
		{
			temporaryVolumeRequests--;
			if (temporaryVolumeRequests == 0)
			{
				audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, musicVolumeBeforeLowering, musicAudioMaxVolume01Clamped);
				currentlyChangingVolume = false;
			}
		}

		internal void RefreshTemporaryVolumeValues()
		{
			normalVolumeDuringForcedVolume = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Music, musicAudioMaxVolume01Clamped);
			volumeBeforeForcedVolume = normalVolumeDuringForcedVolume;
			loweredVolumeDuringForcedVolume = normalVolumeDuringForcedVolume * temporaryVolumePercentage;
			if (temporaryVolumeRequests != 0)
			{
				volumeBeforeForcedVolume = normalVolumeDuringForcedVolume * temporaryVolumePercentage;
				musicVolumeBeforeLowering = normalVolumeDuringForcedVolume;
			}
		}

		protected IEnumerator LerpForcedVolume(float targetVolume, float targetTime)
		{
			float startVolume = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Music, musicAudioMaxVolume01Clamped);
			for (float time = 0f; time < targetTime; time += Time.deltaTime)
			{
				float volumeScale = startVolume + (targetVolume - startVolume) * (time / targetTime);
				audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, volumeScale, musicAudioMaxVolume01Clamped);
				yield return null;
			}
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, targetVolume, musicAudioMaxVolume01Clamped);
			forcedVolumeCoroutine = null;
		}

		protected void StartLerpForcedVolume(float targetVolume, float targetTime)
		{
			if (forcedVolumeCoroutine != null)
			{
				Core.Instance.StopCoroutine(forcedVolumeCoroutine);
			}
			forcedVolumeCoroutine = Core.Instance.StartCoroutine(LerpForcedVolume(targetVolume, targetTime));
		}

		internal void BeginForcedVolume()
		{
			if (normalVolumeDuringForcedVolume == 0f)
			{
				if (forcedVolumeCoroutine == null)
				{
					volumeBeforeForcedVolume = audioSubSystem.GetChannelVolumeScale(AudioChannelID.Music, musicAudioMaxVolume01Clamped);
				}
				normalVolumeDuringForcedVolume = volumeBeforeForcedVolume;
				loweredVolumeDuringForcedVolume = volumeBeforeForcedVolume * temporaryVolumePercentage;
				if (temporaryVolumeRequests != 0)
				{
					loweredVolumeDuringForcedVolume = volumeBeforeForcedVolume;
					normalVolumeDuringForcedVolume = musicVolumeBeforeLowering;
				}
				SetForcedVolumeToLower();
			}
		}

		internal void SetForcedVolumeToNormal()
		{
			if (normalVolumeDuringForcedVolume != 0f)
			{
				StartLerpForcedVolume(normalVolumeDuringForcedVolume, 0.6f);
			}
		}

		internal void SetForcedVolumeToLower()
		{
			if (normalVolumeDuringForcedVolume != 0f)
			{
				StartLerpForcedVolume(loweredVolumeDuringForcedVolume, 0.6f);
			}
		}

		internal void EndForcedVolume()
		{
			if (normalVolumeDuringForcedVolume != 0f)
			{
				StartLerpForcedVolume(volumeBeforeForcedVolume, 0.6f);
				normalVolumeDuringForcedVolume = 0f;
			}
		}

		internal void PlaySfxGameplayLooping(SfxClip sfxClip, AudioSource audioSource, float randomPitchVariance = 0f, float delayedSeconds = 0f)
		{
			PlaySfxGameplayLooping(sfxClip.collectionId, sfxClip.audioClipId, audioSource, randomPitchVariance);
			AddNewActiveLoopingAudioSource(audioSource);
		}

		internal void PlaySfxGameplayLooping(SfxCollectionID collectionId, AudioClipID audioClipId, AudioSource audioSource, float randomPitchVariance = 0f, float delayedSeconds = 0f)
		{
			AudioClip audioClipFromCollection = sfxLibrary.GetAudioClipFromCollection(collectionId, audioClipId);
			PlayLoopingSfx(mixerGroups[3], audioClipFromCollection, audioSource, randomPitchVariance, delayedSeconds);
			AddNewActiveLoopingAudioSource(audioSource);
		}

		internal void PlaySfxGameplayLooping(MoveStyle moveStyle, AudioClipID audioClipId, AudioSource audioSource, float randomPitchVariance = 0f, float delayedSeconds = 0f)
		{
			AudioClip audioClipFromCollection = sfxLibrary.GetAudioClipFromCollection((SfxCollectionID)(8 + moveStyle), audioClipId);
			PlayLoopingSfx(mixerGroups[3], audioClipFromCollection, audioSource, randomPitchVariance, delayedSeconds);
			AddNewActiveLoopingAudioSource(audioSource);
		}

		internal void PlaySfxUILooping(SfxCollectionID collectionId, AudioClipID audioClipId, AudioSource audioSource, float randomPitchVariance = 0f, float delayedSeconds = 0f)
		{
			AudioClip audioClipFromCollection = sfxLibrary.GetAudioClipFromCollection(collectionId, audioClipId);
			PlayLoopingSfx(mixerGroups[2], audioClipFromCollection, audioSource, randomPitchVariance, delayedSeconds);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PlayLoopingSfx(AudioMixerGroup audioMixerGroup, AudioClip audioClip, AudioSource audioSource, float randomPitchVariance, float delayedSeconds)
		{
			if (!(audioClip == null))
			{
				if (randomPitchVariance > 0f)
				{
					randomPitchVariance = UnityEngine.Random.Range(0f - randomPitchVariance, randomPitchVariance);
				}
				audioSource.clip = audioClip;
				audioSource.loop = true;
				audioSource.outputAudioMixerGroup = audioMixerGroup;
				audioSource.pitch = 1f - randomPitchVariance;
				if (delayedSeconds != 0f)
				{
					audioSource.PlayDelayed(delayedSeconds);
				}
				else
				{
					audioSource.Play();
				}
			}
		}

		internal void PlaySfxGameplay(SfxCollectionID collectionId, AudioClipID audioClipId, AudioSource audioSource, float randomPitchVariance = 0f)
		{
			AudioClip randomAudioClipFromCollection = sfxLibrary.GetRandomAudioClipFromCollection(collectionId, audioClipId);
			PlayOneShotSfx(mixerGroups[3], randomAudioClipFromCollection, audioSource, randomPitchVariance);
		}

		internal void PlaySfxGameplayPitched(SfxCollectionID collectionId, AudioClipID audioClipId, AudioSource audioSource, float pitch = 1f)
		{
			AudioClip randomAudioClipFromCollection = sfxLibrary.GetRandomAudioClipFromCollection(collectionId, audioClipId);
			PlayOneShotSfxPitched(mixerGroups[3], randomAudioClipFromCollection, audioSource, pitch);
		}

		internal void PlaySfxGameplay(MoveStyle moveStyle, AudioClipID audioClipId, AudioSource audioSource, float randomPitchVariance = 0f)
		{
			AudioClip randomAudioClipFromCollection = sfxLibrary.GetRandomAudioClipFromCollection((SfxCollectionID)(8 + moveStyle), audioClipId);
			PlayOneShotSfx(mixerGroups[3], randomAudioClipFromCollection, audioSource, randomPitchVariance);
		}

		internal void PlaySfxGameplay(SfxCollectionID collectionId, AudioClipID audioClipId, float randomPitchVariance = 0f)
		{
			PlaySfxGameplay(collectionId, audioClipId, audioSources[3], randomPitchVariance);
		}

		internal void PlaySfxGameplayByindex(SfxCollectionID collectionId, AudioClipID audioClipId, AudioSource audioSource, int index, float randomPitchVariance = 0f)
		{
			AudioClip audioClipByIndexFromCollection = sfxLibrary.GetAudioClipByIndexFromCollection(collectionId, audioClipId, index);
			PlayOneShotSfx(mixerGroups[3], audioClipByIndexFromCollection, audioSource, randomPitchVariance);
		}

		internal void PlaySfxGameplayByindex(MoveStyle moveStyle, AudioClipID audioClipId, AudioSource audioSource, int index, float randomPitchVariance = 0f)
		{
			AudioClip audioClipByIndexFromCollection = sfxLibrary.GetAudioClipByIndexFromCollection((SfxCollectionID)(8 + moveStyle), audioClipId, index);
			PlayOneShotSfx(mixerGroups[3], audioClipByIndexFromCollection, audioSource, randomPitchVariance);
		}

		internal void PlaySfxGameplayByindex(SfxCollectionID collectionId, AudioClipID audioClipId, int index, float randomPitchVariance = 0f)
		{
			PlaySfxGameplayByindex(collectionId, audioClipId, audioSources[3], index, randomPitchVariance);
		}

		internal static AudioClip GetAudioClip(SfxClip sfxClip, bool randomAudioClip, int indexOfAudioClip)
		{
			if (randomAudioClip)
			{
				return Core.Instance.AudioManager.GetRandomAudioClipFromID(sfxClip);
			}
			if (indexOfAudioClip >= 0)
			{
				return Core.Instance.AudioManager.GetAudioClipByIndexFromID(sfxClip, indexOfAudioClip);
			}
			return Core.Instance.AudioManager.GetAudioClipFromID(sfxClip);
		}

		internal void PlaySfx(SfxClip sfxClip, bool randomAudioClip, int indexOfAudioClip, AudioSourceID source)
		{
			AudioClip audioClip = GetAudioClip(sfxClip, randomAudioClip, indexOfAudioClip);
			PlayOneShotSfx(mixerGroups[(int)source], audioClip, audioSources[(int)source], 0f);
		}

		internal void PlaySfx(SfxClip sfxClip, bool randomAudioClip, int indexOfAudioClip, AudioSource audioSource, AudioSourceID channel)
		{
			AudioClip audioClip = GetAudioClip(sfxClip, randomAudioClip, indexOfAudioClip);
			PlayOneShotSfx(mixerGroups[(int)channel], audioClip, audioSource, 0f);
		}

		internal void PlaySfxUI(SfxCollectionID collectionId, AudioClipID audioClipId, float randomPitchVariance = 0f)
		{
			AudioClip randomAudioClipFromCollection = sfxLibrary.GetRandomAudioClipFromCollection(collectionId, audioClipId);
			PlayOneShotSfx(mixerGroups[2], randomAudioClipFromCollection, audioSources[2], randomPitchVariance);
		}

		internal void PlaySfxUIByindex(SfxCollectionID collectionId, AudioClipID audioClipId, int index, float randomPitchVariance = 0f)
		{
			AudioClip audioClipByIndexFromCollection = sfxLibrary.GetAudioClipByIndexFromCollection(collectionId, audioClipId, index);
			PlayOneShotSfx(mixerGroups[2], audioClipByIndexFromCollection, audioSources[2], randomPitchVariance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void PlaySfxGameplay(SfxClip clip, AudioSource audioSource, float randomPitchVariance = 0f)
		{
			PlaySfxGameplay(clip.collectionId, clip.audioClipId, audioSource, randomPitchVariance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void PlaySfxGameplay(SfxClip clip, float randomPitchVariance = 0f)
		{
			PlaySfxGameplay(clip.collectionId, clip.audioClipId, randomPitchVariance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void PlaySfxGameplayByindex(SfxClip clip, AudioSource audioSource, int index, float randomPitchVariance = 0f)
		{
			PlaySfxGameplayByindex(clip.collectionId, clip.audioClipId, audioSource, index, randomPitchVariance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void PlaySfxGameplayByindex(SfxClip clip, int index, float randomPitchVariance = 0f)
		{
			PlaySfxGameplayByindex(clip.collectionId, clip.audioClipId, index, randomPitchVariance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void PlaySfxUI(SfxClip clip, float randomPitchVariance = 0f)
		{
			PlaySfxUI(clip.collectionId, clip.audioClipId, randomPitchVariance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void PlaySfxUIByindex(SfxClip clip, int index, float randomPitchVariance = 0f)
		{
			PlaySfxUIByindex(clip.collectionId, clip.audioClipId, index, randomPitchVariance);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void PlayOneShotSfx(AudioMixerGroup audioMixerGroup, AudioClip audioClip, AudioSource audioSource, float randomPitchVariance)
		{
			if (!(audioClip == null))
			{
				if (randomPitchVariance != 0f)
				{
					randomPitchVariance = UnityEngine.Random.Range(0f - randomPitchVariance, randomPitchVariance);
				}
				audioSource.outputAudioMixerGroup = audioMixerGroup;
				audioSource.pitch = 1f - randomPitchVariance;
				audioSource.PlayOneShot(audioClip);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PlayOneShotSfxPitched(AudioMixerGroup audioMixerGroup, AudioClip audioClip, AudioSource audioSource, float pitch)
		{
			if (!(audioClip == null))
			{
				audioSource.outputAudioMixerGroup = audioMixerGroup;
				audioSource.pitch = pitch;
				audioSource.PlayOneShot(audioClip);
			}
		}

		internal SfxCollectionID GetCharacterVoiceSfxCollection(Characters character)
		{
			return characterToVoiceCollection[(int)character];
		}

		internal VoicePriority PlayVoice(SfxCollectionID sfxCollectionID, AudioClipID audioClipID)
		{
			AudioClip randomAudioClipFromCollection = sfxLibrary.GetRandomAudioClipFromCollection(sfxCollectionID, audioClipID);
			PlayNonloopingSfx(audioSources[5], randomAudioClipFromCollection, mixerGroups[5], 0f);
			return VoicePriority.MOVEMENT;
		}

		internal VoicePriority PlayVoice(Characters character, AudioClipID audioClipID)
		{
			SfxCollectionID sfxCollectionId = characterToVoiceCollection[(int)character];
			AudioClip randomAudioClipFromCollection = sfxLibrary.GetRandomAudioClipFromCollection(sfxCollectionId, audioClipID);
			PlayNonloopingSfx(audioSources[5], randomAudioClipFromCollection, mixerGroups[5], 0f);
			return VoicePriority.MOVEMENT;
		}

		internal void PlayVoice(ref VoicePriority currentPriority, Characters character, AudioClipID audioClipID, AudioSource audioSource, VoicePriority playbackPriority = VoicePriority.MOVEMENT)
		{
			if (playbackPriority > currentPriority || !audioSource.isPlaying)
			{
				SfxCollectionID sfxCollectionId = characterToVoiceCollection[(int)character];
				AudioClip randomAudioClipFromCollection = sfxLibrary.GetRandomAudioClipFromCollection(sfxCollectionId, audioClipID);
				PlayNonloopingSfx(audioSource, randomAudioClipFromCollection, mixerGroups[5], 0f);
				currentPriority = playbackPriority;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PlayNonloopingSfx(AudioSource audioSource, AudioClip audioClip, AudioMixerGroup audioMixerGroup, float randomPitchVariance)
		{
			if (!(audioClip == null))
			{
				if (randomPitchVariance > 0f)
				{
					randomPitchVariance = UnityEngine.Random.Range(0f - randomPitchVariance, randomPitchVariance);
				}
				audioSource.clip = audioClip;
				audioSource.loop = false;
				audioSource.outputAudioMixerGroup = audioMixerGroup;
				audioSource.pitch = 1f - randomPitchVariance;
				audioSource.Play();
			}
		}

		internal static bool ClipIsFinished(AudioSource audioSource)
		{
			if (!audioSource.isPlaying)
			{
				return audioSource.time == 0f;
			}
			return false;
		}

		internal void StopLoopingSfx(AudioSource audioSource)
		{
			audioSource.Stop();
			audioSource.clip = null;
			RemoveActiveLoopingAudioSource(audioSource);
		}

		internal void PauseAllGameplayLoopingSfx()
		{
			bool flag = false;
			for (int i = 0; i < activeGameplayLoopingAudioSources.Count; i++)
			{
				if (activeGameplayLoopingAudioSources[i] == null)
				{
					flag = true;
				}
				else
				{
					activeGameplayLoopingAudioSources[i].Pause();
				}
			}
			if (flag)
			{
				activeGameplayLoopingAudioSources.RemoveAll((AudioSource loopingAudioSource) => loopingAudioSource == null);
			}
		}

		internal void ResumeAllGameplayLoopingSfx()
		{
			bool flag = false;
			for (int i = 0; i < activeGameplayLoopingAudioSources.Count; i++)
			{
				if (activeGameplayLoopingAudioSources[i] == null)
				{
					flag = true;
				}
				else
				{
					activeGameplayLoopingAudioSources[i].Play();
				}
			}
			if (flag)
			{
				activeGameplayLoopingAudioSources.RemoveAll((AudioSource loopingAudioSource) => loopingAudioSource == null);
			}
		}

		internal void ClearAllGameplayLoopingSfx()
		{
			activeGameplayLoopingAudioSources.Clear();
		}

		internal AudioClip GetAudioClipFromID(SfxClip sfxClip)
		{
			return GetAudioClipFromID(sfxClip.collectionId, sfxClip.audioClipId);
		}

		internal AudioClip GetRandomAudioClipFromID(SfxClip sfxClip)
		{
			return GetRandomAudioClipFromID(sfxClip.collectionId, sfxClip.audioClipId);
		}

		internal AudioClip GetAudioClipByIndexFromID(SfxClip sfxClip, int index)
		{
			return GetAudioClipByIndexFromID(sfxClip.collectionId, sfxClip.audioClipId, index);
		}

		internal AudioClip GetAudioClipFromID(SfxCollectionID sfxCollectionID, AudioClipID id)
		{
			return sfxLibrary.GetAudioClipFromCollection(sfxCollectionID, id);
		}

		internal AudioClip GetRandomAudioClipFromID(SfxCollectionID sfxCollectionID, AudioClipID id)
		{
			return sfxLibrary.GetRandomAudioClipFromCollection(sfxCollectionID, id);
		}

		internal AudioClip GetAudioClipByIndexFromID(SfxCollectionID sfxCollectionID, AudioClipID id, int index)
		{
			return sfxLibrary.GetAudioClipByIndexFromCollection(sfxCollectionID, id, index);
		}

		internal void ToggleMute()
		{
			if (IsChannelMuted(AudioChannelID.Master))
			{
				UnMute();
			}
			else
			{
				Mute();
			}
		}

		private bool IsChannelMuted(AudioChannelID audioChannelID)
		{
			return audioSubSystem.GetChannelVolumeScale(audioChannelID, 1f) == 0f;
		}

		internal void UnMute()
		{
			SetMasterVolume(MasterVolume);
		}

		internal void Mute()
		{
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Master, 0f, masterAudioMaxVolume01Clamped);
		}

		internal void ToggleMuteSfx()
		{
			if (IsChannelMuted(AudioChannelID.Sfx))
			{
				UnMuteSfx();
			}
			else
			{
				MuteSfx();
			}
		}

		internal void UnMuteSfx()
		{
			SetSfxVolume(SfxVolume);
		}

		internal void MuteSfx()
		{
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Sfx, 0f, sfxAudioMaxVolume01Clamped);
		}

		internal void ToggleMuteMusic()
		{
			if (IsChannelMuted(AudioChannelID.Music))
			{
				UnMuteMusic();
			}
			else
			{
				MuteMusic();
			}
		}

		internal void UnMuteMusic()
		{
			SetMusicVolume(MusicVolume);
		}

		internal void MuteMusic()
		{
			musicVolumeBeforeLowering = 0f;
			audioSubSystem.SetChannelVolumeScale(AudioChannelID.Music, 0f, musicAudioMaxVolume01Clamped);
		}

		internal static void SetActiveAudioListener(AudioManager audioManager, AudioListener audioListener)
		{
			if (audioManager.activeAudioListener != null)
			{
				audioManager.activeAudioListener.enabled = false;
			}
			audioManager.activeAudioListener = audioListener;
			audioManager.activeAudioListener.enabled = true;
		}

		private static void SetAmbienceOnAudioSource(AudioSource audioSource, AudioClip clip, AudioMixerGroup audioMixerGroup, float volume)
		{
			float time = audioSource.time;
			audioSource.clip = clip;
			audioSource.volume = volume;
			if (!audioSource.isPlaying)
			{
				audioSource.outputAudioMixerGroup = audioMixerGroup;
				audioSource.time = (time - 0.001f + clip.length) % clip.length;
				audioSource.Play();
			}
		}

		internal static void FadeAmbience(AudioManager audioManager, float percentage, SfxCollectionID collectionId, AudioClipID fromAudioClipId, AudioClipID toAudioClipId, bool noneIsSilence = false)
		{
			if (fromAudioClipId >= AudioClipID.MAX)
			{
				fromAudioClipId = AudioClipID.NONE;
			}
			if (toAudioClipId >= AudioClipID.MAX)
			{
				toAudioClipId = AudioClipID.NONE;
			}
			if (fromAudioClipId == AudioClipID.NONE && !noneIsSilence)
			{
				fromAudioClipId = audioManager.stageAmbience.GetStageAmbience(Core.Instance.BaseModule.CurrentStage);
			}
			audioManager.currentAmbienceClip = fromAudioClipId;
			if (toAudioClipId == AudioClipID.NONE)
			{
				audioManager.ambienceAudioSources[1].volume = 0f;
			}
			else
			{
				AudioClip audioClipFromCollection = audioManager.sfxLibrary.GetAudioClipFromCollection(collectionId, toAudioClipId);
				SetAmbienceOnAudioSource(audioManager.ambienceAudioSources[1], audioClipFromCollection, audioManager.mixerGroups[6], percentage);
			}
			if (fromAudioClipId == AudioClipID.NONE)
			{
				audioManager.ambienceAudioSources[0].volume = 0f;
				return;
			}
			AudioClip audioClipFromCollection2 = audioManager.sfxLibrary.GetAudioClipFromCollection(collectionId, fromAudioClipId);
			SetAmbienceOnAudioSource(audioManager.ambienceAudioSources[0], audioClipFromCollection2, audioManager.mixerGroups[6], 1f - percentage);
		}

		private static void SetFullAmbience(AudioManager audioManager, AudioSource audioSource, SfxCollectionID sfxCollectionID, AudioClipID clipToSet, bool noneIsSilence = false)
		{
			if (audioManager.currentAmbienceClip != clipToSet || audioSource.volume != 1f)
			{
				FadeAmbience(audioManager, 0f, sfxCollectionID, clipToSet, clipToSet, noneIsSilence);
			}
		}

		private void AddNewActiveLoopingAudioSource(AudioSource audioSource)
		{
			activeGameplayLoopingAudioSources.Add(audioSource);
		}

		private void RemoveActiveLoopingAudioSource(AudioSource audioSource)
		{
			activeGameplayLoopingAudioSources.Remove(audioSource);
		}

		internal void AmbienceUpdate()
		{
			if (activeAudioListener == null)
			{
				return;
			}
			AudioAmbienceTrigger audioAmbienceTrigger = null;
			AudioAmbienceZone audioAmbienceZone = null;
			int num = 0;
			float num2 = 0.001f;
			int num3 = Physics.OverlapSphereNonAlloc(activeAudioListener.transform.position, num2, ambienceColliders, 4096);
			for (int i = 0; i < num3; i++)
			{
				AudioAmbienceTrigger component = ambienceColliders[i].GetComponent<AudioAmbienceTrigger>();
				if (component != null && audioAmbienceTrigger == null)
				{
					audioAmbienceTrigger = component;
					num = i;
					continue;
				}
				AudioAmbienceZone component2 = ambienceColliders[i].GetComponent<AudioAmbienceZone>();
				if ((bool)component2)
				{
					if ((component2 == currentAmbienceZone && currentAmbienceZonePriority == component2.priority) || currentAmbienceZonePriority < component2.priority)
					{
						currentAmbienceZonePriority = component2.priority;
						audioAmbienceZone = component2;
						num = i;
					}
				}
				else
				{
					AudioRectangularReverb component3 = ambienceColliders[i].GetComponent<AudioRectangularReverb>();
					if ((bool)component3)
					{
						component3.SetZonePosition(activeAudioListener.transform.position);
					}
				}
			}
			if (audioAmbienceTrigger != null)
			{
				Vector3 forward = audioAmbienceTrigger.transform.forward;
				BoxCollider boxCollider = ambienceColliders[num] as BoxCollider;
				Vector3 vector = 0.25f * Vector3.Scale(boxCollider.size, audioAmbienceTrigger.transform.localScale);
				vector.x += num2;
				vector.y += num2;
				vector.z += num2;
				Vector3 vector2 = audioAmbienceTrigger.transform.rotation * vector;
				Vector3 position = audioAmbienceTrigger.transform.position;
				float num4 = Vector3.Dot(activeAudioListener.transform.position, forward);
				float num5 = Vector3.Dot(position - vector2, forward);
				float num6 = Vector3.Dot(position + vector2, forward);
				float percentage = (num4 - num5) / (num6 - num5);
				FadeAmbience(this, percentage, SfxCollectionID.AmbienceSfx, audioAmbienceTrigger.from, audioAmbienceTrigger.to);
			}
			else if (audioAmbienceZone != null)
			{
				currentAmbienceZone = audioAmbienceZone;
				SetFullAmbience(this, ambienceAudioSources[0], SfxCollectionID.AmbienceSfx, currentAmbienceZone.ambienceClip, currentAmbienceZone.noneIsSilence);
			}
			else
			{
				currentAmbienceZonePriority = 0;
				SetFullAmbience(this, ambienceAudioSources[0], SfxCollectionID.AmbienceSfx, stageAmbience.GetStageAmbience(Core.Instance.BaseModule.CurrentStage));
			}
		}

		internal void StartCustomTrack(MusicTrack trackToPlay, ref MusicTrack musicTrackInstance)
		{
			if (!(trackToPlay != null))
			{
				return;
			}
			musicTrackInstance = UnityEngine.Object.Instantiate(trackToPlay);
			musicTrackInstance.isRepeatable = true;
			if (!musicPlayer.IsPlaying)
			{
				musicLibraryPlayer.AddTrack(musicTrackInstance);
				musicLibraryPlayer.StartTrack(musicTrackInstance);
				return;
			}
			musicPlayer.AddAndBufferAsCurrentTrack(musicTrackInstance);
			musicPlayer.ClearExceptCurrentTrack();
			if (!musicPlayer.IsCurrentPlayingTrack(trackToPlay) && (!(Core.Instance.musicBlendingAudioSource.clip == trackToPlay.AudioClip) || !Core.Instance.musicBlendingAudioSource.isPlaying))
			{
				musicPlayer.AttemptStartCurrentTrack();
			}
		}

		private static void InitializeVoicesLookupTable(ref SfxCollectionID[] characterToVoiceCollection)
		{
			characterToVoiceCollection = new SfxCollectionID[26];
			characterToVoiceCollection[6] = SfxCollectionID.VoiceAngel;
			characterToVoiceCollection[4] = SfxCollectionID.VoiceBlockGuy;
			characterToVoiceCollection[11] = SfxCollectionID.VoiceBoarder;
			characterToVoiceCollection[22] = SfxCollectionID.VoiceBunGirl;
			characterToVoiceCollection[9] = SfxCollectionID.VoiceDJ;
			characterToVoiceCollection[8] = SfxCollectionID.VoiceDummy;
			characterToVoiceCollection[7] = SfxCollectionID.VoiceEightBall;
			characterToVoiceCollection[24] = SfxCollectionID.VoiceEightBallBoss;
			characterToVoiceCollection[1] = SfxCollectionID.VoiceFrank;
			characterToVoiceCollection[20] = SfxCollectionID.VoiceFutureGirl;
			characterToVoiceCollection[0] = SfxCollectionID.VoiceGirl1;
			characterToVoiceCollection[14] = SfxCollectionID.VoiceJetpackBoss;
			characterToVoiceCollection[15] = SfxCollectionID.VoiceLegendFace;
			characterToVoiceCollection[10] = SfxCollectionID.VoiceMedusa;
			characterToVoiceCollection[3] = SfxCollectionID.VoiceMetalHead;
			characterToVoiceCollection[13] = SfxCollectionID.VoicePrince;
			characterToVoiceCollection[21] = SfxCollectionID.VoicePufferGirl;
			characterToVoiceCollection[2] = SfxCollectionID.VoiceRingDude;
			characterToVoiceCollection[5] = SfxCollectionID.VoiceSpaceGirl;
			characterToVoiceCollection[19] = SfxCollectionID.VoiceWideKid;
			characterToVoiceCollection[12] = SfxCollectionID.VoiceHeadman;
			characterToVoiceCollection[23] = SfxCollectionID.VoiceHeadmanNoJetpack;
			characterToVoiceCollection[25] = SfxCollectionID.VoiceMetalHead;
			characterToVoiceCollection[18] = SfxCollectionID.VoiceSkate;
			characterToVoiceCollection[16] = SfxCollectionID.VoiceOldhead;
			characterToVoiceCollection[17] = SfxCollectionID.VoiceRobot;
		}
	}
}
