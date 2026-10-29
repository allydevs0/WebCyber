using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Reptile
{
	public class MusicAssetsLoader
	{
		public IEnumerator LoadMusicAssetFromDiskRoutine(MusicPlayerData data, AudioType audioType)
		{
			using (UnityWebRequest unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(data.Path, audioType))
			{
				yield return unityWebRequest.SendWebRequest();
				HandleLoadMusicAssetFromDiskRequest(data, unityWebRequest);
			}
			yield return null;
		}

		private void HandleLoadMusicAssetFromDiskRequest(MusicPlayerData data, UnityWebRequest unityWebRequest)
		{
			if (unityWebRequest.isDone)
			{
				try
				{
					data.FinishedLoading(DownloadHandlerAudioClip.GetContent(unityWebRequest));
				}
				catch (Exception ex)
				{
					data.FinishedWithError(ex.ToString());
				}
			}
		}

		public void LoadAudioClipAsset(AudioClip audioClip)
		{
			if (audioClip != null)
			{
				audioClip.LoadAudioData();
			}
		}

		public IEnumerator LoadAudioClipAssetASync(AudioClip audioClip)
		{
			audioClip.LoadAudioData();
			while (audioClip.loadState == AudioDataLoadState.Loading)
			{
				yield return null;
			}
		}

		public IEnumerator LoadMusicAssetFromClipASync(MusicPlayerData data)
		{
			yield return LoadAudioClipAssetASync(data.AudioClip);
			if (data.AudioClip.loadState == AudioDataLoadState.Loaded)
			{
				data.FinishedLoading();
			}
			else if (data.AudioClip.loadState == AudioDataLoadState.Failed)
			{
				data.FinishedWithError("Could not load audio clip from asset file");
			}
		}
	}
}
