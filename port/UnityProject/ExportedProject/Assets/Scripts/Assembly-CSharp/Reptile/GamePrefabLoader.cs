using System.Collections;
using UnityEngine;

namespace Reptile
{
	public class GamePrefabLoader : ASceneAssetLoader
	{
		public const string GAME_PREFABS_ASSETBUNDLE_NAME = "in_game_assets";

		private const string WORLD_HANDLER_ASSET_NAME = "worldhandler";

		private const string WANTED_MANAGER_ASSET_NAME = "wantedmanager";

		private const string GAMEPLAY_UI_ASSET_NAME = "gameplayui";

		private const string MAP_CONTROLLER_ASSET_NAME = "mapcontroller";

		private GameObject worldHandlerPrefab;

		private GameObject wantedManagerPrefab;

		private GameObject gameplayUIPrefab;

		private GameObject mapControllerPrefab;

		public GameObject WorldHandlerPrefab => worldHandlerPrefab;

		public GameObject WantedManagerPrefab => wantedManagerPrefab;

		public GameObject GameplayUIPrefab => gameplayUIPrefab;

		public GameObject MapControllerPrefab => mapControllerPrefab;

		public GamePrefabLoader(Assets assets)
			: base(assets)
		{
		}

		public void Dispose()
		{
			mapControllerPrefab = null;
			gameplayUIPrefab = null;
			wantedManagerPrefab = null;
			worldHandlerPrefab = null;
		}

		public override void LoadAssets(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadGameplayPrefabs)
			{
				LoadPrefabs();
			}
		}

		public override IEnumerator LoadAssetsASync(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadGameplayPrefabs)
			{
				yield return LoadPrefabsASync();
			}
		}

		public override void LoadAssetsEditor(AssetsToLoadData assetsToLoadData)
		{
			if (assetsToLoadData.loadGameplayPrefabs)
			{
				LoadPrefabsForEditor();
			}
		}

		public override void UnloadAssets(AssetsToLoadData assetsToLoadData)
		{
			worldHandlerPrefab = null;
			wantedManagerPrefab = null;
			gameplayUIPrefab = null;
		}

		private void LoadPrefabs()
		{
			worldHandlerPrefab = assets.LoadAssetFromBundle<GameObject>("in_game_assets", "worldhandler");
			wantedManagerPrefab = assets.LoadAssetFromBundle<GameObject>("in_game_assets", "wantedmanager");
			gameplayUIPrefab = assets.LoadAssetFromBundle<GameObject>("in_game_assets", "gameplayui");
			mapControllerPrefab = assets.LoadAssetFromBundle<GameObject>("in_game_assets", "mapcontroller");
		}

		private IEnumerator LoadPrefabsASync()
		{
			yield return LoadWorldHandlerPrefab();
			yield return LoadWantedManagerPrefab();
			yield return LoadGameplayUIPrefab();
			yield return LoadMapControllerPrefab();
		}

		private IEnumerator LoadWorldHandlerPrefab()
		{
			AssetBundleRequest worldHandlerPrefabRequest = assets.LoadAssetFromBundleASync<GameObject>("in_game_assets", "worldhandler");
			yield return worldHandlerPrefabRequest;
			worldHandlerPrefab = worldHandlerPrefabRequest.asset as GameObject;
		}

		private IEnumerator LoadWantedManagerPrefab()
		{
			AssetBundleRequest wantedManagerPrefabRequest = assets.LoadAssetFromBundleASync<GameObject>("in_game_assets", "wantedmanager");
			yield return wantedManagerPrefabRequest;
			wantedManagerPrefab = wantedManagerPrefabRequest.asset as GameObject;
		}

		private IEnumerator LoadGameplayUIPrefab()
		{
			AssetBundleRequest gameplayUIPrefabRequest = assets.LoadAssetFromBundleASync<GameObject>("in_game_assets", "gameplayui");
			yield return gameplayUIPrefabRequest;
			gameplayUIPrefab = gameplayUIPrefabRequest.asset as GameObject;
		}

		private IEnumerator LoadMapControllerPrefab()
		{
			AssetBundleRequest mapControllerPrefabRequest = assets.LoadAssetFromBundleASync<GameObject>("in_game_assets", "mapcontroller");
			yield return mapControllerPrefabRequest;
			mapControllerPrefab = mapControllerPrefabRequest.asset as GameObject;
		}

		private void LoadPrefabsForEditor()
		{
		}
	}
}
