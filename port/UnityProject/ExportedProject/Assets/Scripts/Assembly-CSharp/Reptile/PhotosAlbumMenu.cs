using System;
using System.Collections;
using System.Threading;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Reptile
{
	[RequireComponent(typeof(Canvas))]
	public class PhotosAlbumMenu : AMenuController
	{
		internal enum PhotosAlbumState
		{
			CLOSED = 0,
			ALBUM = 1,
			FULLSCREEN = 2,
			FULLSCREEN_WAIT_FRAME = 3,
			DELETION_CONFIRMATION = 4,
			DELETION_COMPLETE = 5,
			DELETION_CANCEL = 6
		}

		internal enum PhotoLoadStatus
		{
			NULL = 0,
			LOADING = 1,
			COMPLETE = 2
		}

		internal struct PhotoLoadInfo
		{
			internal volatile int next;

			internal volatile int loadComplete;

			internal volatile int photoIndex;

			internal volatile PhonePictureData pictureData;
		}

		public delegate void MenuDisableCallback();

		[SerializeField]
		private GameObject fullscreenPhoto;

		[SerializeField]
		private GameObject glyphButtonContainer;

		[SerializeField]
		private Image fullscreenPhotoImage;

		[SerializeField]
		private Scrollbar verticalScrollbar;

		[SerializeField]
		private UIPrompt deletionPrompt;

		[SerializeField]
		private GameObject photosContainer;

		[SerializeField]
		private GameObject fullscreenGlyph;

		[SerializeField]
		private GameObject deleteGlyph;

		[SerializeField]
		private GridLayoutGroup gridLayout;

		[SerializeField]
		private RectTransform viewRectTransform;

		[SerializeField]
		private Canvas photoAlbumCanvas;

		[SerializeField]
		private TextMeshProUGUI photoCountDisplayText;

		[Tooltip("Make sure these are in ascending order please. (1,2,3,etc.)")]
		[SerializeField]
		private PhotoButton[] photoButtons;

		private Core coreInstance;

		private PhotosManager _photosManager;

		internal bool initialized;

		private PhotosAlbumState state;

		internal int selectedButtonIndex;

		internal int horizontalButtonCount;

		internal int rowsInViewCount;

		internal int totalRowsCount;

		private MenuDisableCallback menuDisableCallback;

		private bool isShowingOpeningAnimations;

		private bool isShowingClosingAnimations;

		internal int deletionStatus;

		internal int photoIndexAwaitingDeletion;

		internal UnityAction yesActionDeletionPrompt;

		internal int photosRefreshStatus;

		internal int firstPhotoInView;

		internal int photosInViewCount;

		internal int nextFreeInfoSlot;

		internal int nextLoadingInfoSlot;

		internal PhotoLoadInfo[] photoLoadInfos;

		internal float scrollBarHandleMaxSize = 0.45f;

		private float previousScrollbarProgress;

		internal float scrollSpeed = 0.1f;

		public void Init(PhotosManager photosManager)
		{
			_photosManager = photosManager;
			coreInstance = Core.Instance;
			base.gameObject.SetActive(value: false);
			fullscreenPhoto.SetActive(value: false);
			selectedButtonIndex = -1;
			RefreshGridLayoutVariables();
			firstPhotoInView = 0;
			photosInViewCount = 0;
			deletionPrompt.Hide();
			for (int i = 0; i < photoButtons.Length; i++)
			{
				photoButtons[i].Init(i, this);
			}
			photoLoadInfos = new PhotoLoadInfo[photoButtons.Length];
			SetScrollBarValues(verticalScrollbar, 0, 0);
			verticalScrollbar.onValueChanged.AddListener(ScrollBarMoved);
		}

		internal void OnPhotoViewScroll(float scrollDelta)
		{
			float num = Mathf.Clamp01(verticalScrollbar.value - scrollSpeed * scrollDelta);
			verticalScrollbar.SetValueWithoutNotify(num);
			ScrollBarMoved(num);
		}

		internal void ScrollBarMoved(float scrollbarProgress)
		{
			previousScrollbarProgress = scrollbarProgress;
			int availablePhotosCount = _photosManager.availablePhotosCount;
			int rowFromPhotoCeil = GetRowFromPhotoCeil(availablePhotosCount, horizontalButtonCount);
			int num = 0;
			if (rowFromPhotoCeil > rowsInViewCount)
			{
				int a = horizontalButtonCount * GetRowFromPhotoFloor(availablePhotosCount, horizontalButtonCount);
				int num2 = Mathf.RoundToInt(scrollbarProgress * (float)(rowFromPhotoCeil - rowsInViewCount));
				num = Mathf.Min(a, num2 * horizontalButtonCount);
			}
			int num3 = CalculatePhotosInViewCount(photoButtons.Length, availablePhotosCount, num);
			int selectionIndex = Mathf.Clamp(selectedButtonIndex, 0, photoButtons.Length - 1);
			if (firstPhotoInView != num)
			{
				UpdateViewAndSelection(num, num3, selectionIndex, updateScrollBar: false);
			}
		}

		internal static int GetRowFromPhotoCeil(int photoIndex, int horizontalButtonCount)
		{
			return Mathf.CeilToInt((float)photoIndex / (float)horizontalButtonCount);
		}

		internal static int GetRowFromPhotoFloor(int photoIndex, int horizontalButtonCount)
		{
			return Mathf.FloorToInt((float)photoIndex / (float)horizontalButtonCount);
		}

		internal float CalculateScrollBarHandleSize(int availablePhotosCount, int rowsInViewCount, int horizontalButtonCount)
		{
			int num = rowsInViewCount * horizontalButtonCount;
			float result = 0f;
			if (availablePhotosCount != 0)
			{
				result = Mathf.Min(scrollBarHandleMaxSize, (float)num / (float)availablePhotosCount);
			}
			return result;
		}

		internal void SetScrollBarValues(Scrollbar scrollBar, int availablePhotosCount, int firstPhotoInView)
		{
			int rowFromPhotoCeil = GetRowFromPhotoCeil(availablePhotosCount, horizontalButtonCount);
			bool flag = rowFromPhotoCeil > rowsInViewCount;
			if (verticalScrollbar.enabled != flag)
			{
				verticalScrollbar.enabled = flag;
				verticalScrollbar.gameObject.SetActive(flag);
			}
			float size = CalculateScrollBarHandleSize(availablePhotosCount, rowsInViewCount, horizontalButtonCount);
			float num = GetRowFromPhotoCeil(firstPhotoInView, horizontalButtonCount);
			float valueWithoutNotify = 0f;
			if (rowFromPhotoCeil > rowsInViewCount)
			{
				valueWithoutNotify = num / (float)(rowFromPhotoCeil - rowsInViewCount);
			}
			verticalScrollbar.size = size;
			verticalScrollbar.SetValueWithoutNotify(valueWithoutNotify);
		}

		public void Terminate()
		{
			initialized = false;
			menuDisableCallback = null;
			verticalScrollbar.onValueChanged.RemoveAllListeners();
			if (WorldHandler.instance != null)
			{
				Player currentPlayer = WorldHandler.instance.GetCurrentPlayer();
				if (currentPlayer != null)
				{
					currentPlayer.userInputEnabled = true;
				}
			}
			_photosManager = null;
			coreInstance = null;
		}

		public void Enable(MenuDisableCallback disableCallback)
		{
			menuDisableCallback = disableCallback;
			Core.Instance.UIManager.PushNewMenu(this);
		}

		public override IMenuAnimationTransitionController ShowMenu()
		{
			isShowingOpeningAnimations = true;
			return base.ShowMenu();
		}

		public override IMenuAnimationTransitionController HideMenu()
		{
			isShowingClosingAnimations = true;
			CloseDeletionPrompt();
			return base.HideMenu();
		}

		protected override void OnMenuAnimationEnded()
		{
			base.OnMenuAnimationEnded();
			if (isShowingOpeningAnimations)
			{
				photosContainer.SetActive(value: true);
			}
			isShowingOpeningAnimations = false;
			isShowingClosingAnimations = false;
		}

		protected override void OnMenuAnimationStarted()
		{
			base.OnMenuAnimationStarted();
			if (isShowingClosingAnimations)
			{
				photosContainer.SetActive(value: false);
			}
		}

		public override GameObject GetFirstFocusGameObject()
		{
			GameObject result = null;
			if (_photosManager.availablePhotosCount > 0 && selectedButtonIndex >= 0)
			{
				int num = Mathf.Min(selectedButtonIndex, rowsInViewCount * horizontalButtonCount - 1);
				if (num != selectedButtonIndex)
				{
					SetSelection(num);
				}
				result = photoButtons[selectedButtonIndex].gameObject;
			}
			return result;
		}

		internal void SetButtonNavigation(int inViewCount, bool updateScrollBar)
		{
			if (updateScrollBar)
			{
				SetScrollBarValues(verticalScrollbar, _photosManager.availablePhotosCount, firstPhotoInView);
			}
			for (int i = 0; i < photoButtons.Length; i++)
			{
				bool flag = false;
				Selectable selectOnLeft = null;
				Selectable selectOnRight = null;
				Selectable selectOnUp = null;
				Selectable selectOnDown = null;
				int num = horizontalButtonCount * rowsInViewCount;
				if (i >= 0 && i < photoButtons.Length)
				{
					if (i != 0)
					{
						selectOnLeft = photoButtons[i - 1];
					}
					if (i < inViewCount)
					{
						flag = true;
						if (i < inViewCount - 1 && i < num - 1)
						{
							selectOnRight = photoButtons[i + 1];
						}
					}
					if (i >= horizontalButtonCount)
					{
						selectOnUp = photoButtons[i - horizontalButtonCount];
					}
					if (i + horizontalButtonCount < inViewCount && i + horizontalButtonCount < horizontalButtonCount * rowsInViewCount)
					{
						selectOnDown = photoButtons[i + horizontalButtonCount];
					}
				}
				Navigation navigation = photoButtons[i].navigation;
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnLeft = selectOnLeft;
				navigation.selectOnRight = selectOnRight;
				navigation.selectOnUp = selectOnUp;
				navigation.selectOnDown = selectOnDown;
				navigation.wrapAround = false;
				photoButtons[i].navigation = navigation;
				if (flag)
				{
					if (!photoButtons[i].HasPhotoData())
					{
						photoButtons[i].ShowLoadingIcon();
					}
					else
					{
						photoButtons[i].HideLoadingIcon();
					}
				}
				else
				{
					photoButtons[i].ClearPhoto();
					photoButtons[i].HideLoadingIcon();
				}
			}
		}

		internal void RefreshGridLayoutVariables()
		{
			Vector2 cellSize = gridLayout.cellSize;
			Vector2 spacing = gridLayout.spacing;
			_ = gridLayout.padding;
			Vector2 size = RectTransformUtility.PixelAdjustRect(viewRectTransform, photoAlbumCanvas).size;
			rowsInViewCount = (int)((size.y + spacing.y) / (cellSize.y + spacing.y));
			horizontalButtonCount = (int)((size.x + spacing.x) / (cellSize.x + spacing.x));
			totalRowsCount = GetRowFromPhotoCeil(photoButtons.Length, horizontalButtonCount);
		}

		internal void RefreshTotalPhotoCountDisplayText(int updatedPhotoCount)
		{
			photoCountDisplayText.text = $"{updatedPhotoCount}/{50}";
		}

		public override void Activate()
		{
			base.Activate();
			Core.Instance.BaseModule.PauseGame(PauseType.Photos);
			WorldHandler instance = WorldHandler.instance;
			instance.CurrentCamera.enabled = false;
			instance.GetCurrentPlayer().userInputEnabled = false;
			Core.Instance.GameInput.EnableControllerMap(6);
			state = PhotosAlbumState.ALBUM;
			RefreshGridLayoutVariables();
			photosContainer.SetActive(value: false);
			SetSelection(-1);
			nextFreeInfoSlot = 1;
			nextLoadingInfoSlot = 0;
			for (int i = 0; i < photoLoadInfos.Length; i++)
			{
				int num = i + 1;
				if (i == photoLoadInfos.Length - 1)
				{
					photoLoadInfos[i].next = 0;
				}
				else
				{
					photoLoadInfos[i].next = num + 1;
				}
				Interlocked.Exchange(ref photoLoadInfos[i].loadComplete, 0);
			}
			initialized = true;
			firstPhotoInView = 0;
			photosInViewCount = _photosManager.availablePhotosCount;
			SetButtonNavigation(photosInViewCount, updateScrollBar: true);
			RefreshTotalPhotoCountDisplayText(_photosManager.availablePhotosCount);
			_photosManager.OnPhotosRefresh += OnPhotosCountUpdated;
			_photosManager.RefreshPhotoCountAsync();
		}

		public override void Deactivate()
		{
			base.Deactivate();
			if (_photosManager != null)
			{
				_photosManager.OnPhotosRefresh -= OnPhotosCountUpdated;
			}
			if (Core.Instance.BaseModule != null)
			{
				Core.Instance.BaseModule.UnPauseGame(PauseType.Photos);
			}
			state = PhotosAlbumState.CLOSED;
			WorldHandler instance = WorldHandler.instance;
			if (instance != null)
			{
				instance.CurrentCamera.enabled = true;
				Player currentPlayer = instance.GetCurrentPlayer();
				if (currentPlayer != null)
				{
					currentPlayer.userInputEnabled = true;
				}
			}
			if (menuDisableCallback != null)
			{
				menuDisableCallback();
			}
			menuDisableCallback = null;
			GC.Collect();
		}

		internal void OnPhotosCountUpdated(ATransaction.TransactionStatus lastTransactionStatus)
		{
			Interlocked.Exchange(ref photosRefreshStatus, (int)lastTransactionStatus);
		}

		internal void ShowPhotoFullscreen()
		{
			if (state == PhotosAlbumState.ALBUM && selectedButtonIndex >= 0 && _photosManager.availablePhotosCount > 0 && photoButtons[selectedButtonIndex].HasPhotoData())
			{
				state = PhotosAlbumState.FULLSCREEN_WAIT_FRAME;
				fullscreenPhoto.SetActive(value: true);
				glyphButtonContainer.SetActive(value: true);
				verticalScrollbar.interactable = false;
				fullscreenPhotoImage.sprite = photoButtons[selectedButtonIndex].GetPhoto();
				fullscreenPhotoImage.enabled = true;
			}
		}

		private void OnDeletionDone()
		{
			Interlocked.Exchange(ref deletionStatus, 2);
		}

		private void DeletePhoto(int selectionIndex)
		{
			deletionPrompt.Hide();
			if (Interlocked.CompareExchange(ref deletionStatus, 1, 0) == 0)
			{
				PhonePictureData pictureData = photoButtons[selectionIndex].MarkPhotoForDeletion();
				_photosManager.DeletePhotoAsync(pictureData, OnDeletionDone);
				EventSystem.current.sendNavigationEvents = false;
			}
		}

		private void CancelPhotoDeletion()
		{
			CloseDeletionPrompt();
			GameObject firstFocusGameObject = GetFirstFocusGameObject();
			EventSystem.current.SetSelectedGameObject(firstFocusGameObject);
		}

		private void CloseDeletionPrompt()
		{
			state = PhotosAlbumState.DELETION_CANCEL;
			deletionPrompt.Hide();
		}

		protected void HidePhotoFullscreen()
		{
			fullscreenPhotoImage.enabled = false;
			fullscreenPhoto.SetActive(value: false);
			state = PhotosAlbumState.ALBUM;
			verticalScrollbar.interactable = true;
		}

		protected void ToggleFullscreenUI()
		{
			glyphButtonContainer.SetActive(!glyphButtonContainer.activeSelf);
		}

		internal void LoadPhotoResult(int loadInfoIndex, int photoIndex, PhonePictureData pictureData)
		{
			if (photoIndex == photoLoadInfos[loadInfoIndex].photoIndex && Interlocked.CompareExchange(ref photoLoadInfos[loadInfoIndex].loadComplete, 2, 1) == 1)
			{
				photoLoadInfos[loadInfoIndex].pictureData = pictureData;
			}
		}

		internal static int InsertIntoFreeList(ref int nextInFreeList, int slotToInsert)
		{
			int result = nextInFreeList;
			nextInFreeList = slotToInsert;
			return result;
		}

		internal static int CalculatePhotosInViewCount(int photoButtonsLength, int availablePhotosCount, int firstPhotoInView)
		{
			return Mathf.Min(photoButtonsLength, availablePhotosCount - firstPhotoInView);
		}

		protected void Update()
		{
			if (!base.IsEnabled || base.IsTransitioning)
			{
				return;
			}
			switch ((ATransaction.TransactionStatus)Interlocked.Exchange(ref photosRefreshStatus, 0))
			{
			case ATransaction.TransactionStatus.RETRY:
				_photosManager.RefreshPhotoCountAsync();
				break;
			case ATransaction.TransactionStatus.EXCEPTION:
			case ATransaction.TransactionStatus.FILE_NOT_FOUND:
			case ATransaction.TransactionStatus.NO_STORAGE:
			case ATransaction.TransactionStatus.CORRUPTED_DATA:
			case ATransaction.TransactionStatus.NO_ACCESS:
				photosInViewCount = 0;
				firstPhotoInView = 0;
				SetButtonNavigation(0, updateScrollBar: true);
				Core.Instance.BaseModule.ShowErrorAndReturnToIntro(null);
				break;
			case ATransaction.TransactionStatus.SUCCESS:
			{
				int availablePhotosCount = _photosManager.availablePhotosCount;
				int num = Mathf.Max(0, Mathf.Min(firstPhotoInView, availablePhotosCount - 1));
				num = Mathf.FloorToInt((float)num / (float)horizontalButtonCount) * horizontalButtonCount;
				int num2 = Mathf.Min(0, firstPhotoInView - num);
				if (num2 >= photoButtons.Length)
				{
					num2 = 0;
				}
				firstPhotoInView = num;
				photosInViewCount = 0;
				int num3 = CalculatePhotosInViewCount(photoButtons.Length, _photosManager.availablePhotosCount, firstPhotoInView);
				UpdateViewAndSelection(num, num3, num2);
				RefreshTotalPhotoCountDisplayText(_photosManager.availablePhotosCount);
				break;
			}
			}
			if (nextLoadingInfoSlot != 0)
			{
				int next = nextLoadingInfoSlot;
				int num4 = next - 1;
				while (next != 0)
				{
					int num5 = next - 1;
					if (Interlocked.CompareExchange(ref photoLoadInfos[num5].loadComplete, 0, 2) == 2)
					{
						int num6 = photoLoadInfos[num5].photoIndex - firstPhotoInView;
						if (num6 >= 0 && num6 < photoButtons.Length)
						{
							PhonePictureData pictureData = photoLoadInfos[num5].pictureData;
							photoButtons[num6].SetPhoto(pictureData);
							if (num6 == selectedButtonIndex)
							{
								photoButtons[num6].ShowSelectionImage();
							}
						}
						if (nextLoadingInfoSlot == next)
						{
							nextLoadingInfoSlot = photoLoadInfos[num5].next;
						}
						else
						{
							photoLoadInfos[num4].next = photoLoadInfos[num5].next;
						}
						photoLoadInfos[num5].next = InsertIntoFreeList(ref nextFreeInfoSlot, next);
					}
					next = photoLoadInfos[num5].next;
					num4 = num5;
				}
			}
			if (state == PhotosAlbumState.DELETION_CONFIRMATION && Interlocked.CompareExchange(ref deletionStatus, 0, 2) == 2)
			{
				state = PhotosAlbumState.DELETION_COMPLETE;
				_photosManager.RefreshPhotoCountAsync();
			}
			switch (state)
			{
			case PhotosAlbumState.ALBUM:
				if (coreInstance.GameInput.GetButtonNew(20))
				{
					Core.Instance.UIManager.PopCurrentMenu();
				}
				else if (coreInstance.GameInput.GetButtonNew(19))
				{
					ShowPhotoFullscreen();
				}
				else if (coreInstance.GameInput.GetButtonNew(59) && selectedButtonIndex >= 0 && _photosManager.availablePhotosCount > 0 && photoButtons[selectedButtonIndex].HasPhotoData())
				{
					state = PhotosAlbumState.DELETION_CONFIRMATION;
					yesActionDeletionPrompt = () =>
					{
						DeletePhoto(selectedButtonIndex);
					};
					deletionPrompt.Show("PHOTOS_MESSAGE", "CHOICEPROMPT_YES", "CHOICEPROMPT_NO", yesActionDeletionPrompt, CancelPhotoDeletion);
				}
				break;
			case PhotosAlbumState.FULLSCREEN:
				if (coreInstance.GameInput.GetButtonNew(19))
				{
					ToggleFullscreenUI();
				}
				else if (coreInstance.GameInput.GetButtonNew(20))
				{
					HidePhotoFullscreen();
				}
				break;
			case PhotosAlbumState.FULLSCREEN_WAIT_FRAME:
				state = PhotosAlbumState.FULLSCREEN;
				break;
			case PhotosAlbumState.DELETION_CANCEL:
				state = PhotosAlbumState.ALBUM;
				break;
			case PhotosAlbumState.DELETION_COMPLETE:
				CloseDeletionPrompt();
				state = PhotosAlbumState.ALBUM;
				EventSystem.current.sendNavigationEvents = true;
				break;
			case PhotosAlbumState.DELETION_CONFIRMATION:
				break;
			}
		}

		internal bool PhotoButtonsInteractable()
		{
			return state == PhotosAlbumState.ALBUM;
		}

		internal void SetSelection(int selectionIndex)
		{
			if (selectedButtonIndex >= 0)
			{
				photoButtons[selectedButtonIndex].HideSelectionImage();
			}
			if (selectionIndex >= 0)
			{
				photoButtons[selectionIndex].ShowSelectionImage();
				StartCoroutine(EndOfFrameSelect(selectionIndex));
			}
			selectedButtonIndex = selectionIndex;
		}

		internal void UpdateViewAndSelection(int firstPhotoInView, int photosInViewCount, int selectionIndex, bool updateScrollBar = true)
		{
			selectionIndex = LoadPhotosRangeIntoView(firstPhotoInView, photosInViewCount, selectionIndex, updateScrollBar);
			SetSelection(selectionIndex);
		}

		internal void OnUIMove(int index, MoveDirection moveDirection)
		{
			int num = index;
			int selectionIndex = selectedButtonIndex;
			switch (moveDirection)
			{
			case MoveDirection.Left:
				num = index - 1;
				selectionIndex = num;
				break;
			case MoveDirection.Right:
				num = index + 1;
				selectionIndex = num;
				break;
			case MoveDirection.Up:
				num = index - horizontalButtonCount;
				break;
			case MoveDirection.Down:
				num = index + horizontalButtonCount;
				break;
			}
			if (0 == 0 && (num < 0 || num >= horizontalButtonCount * rowsInViewCount))
			{
				float num2 = (float)(num - index) / (float)horizontalButtonCount;
				float num3 = Mathf.Sign(num2);
				int num4 = (int)(num3 * Mathf.Ceil(num3 * num2));
				int num5 = Mathf.Max(0, _photosManager.availablePhotosCount - rowsInViewCount * horizontalButtonCount);
				int max = horizontalButtonCount * Mathf.CeilToInt((float)num5 / (float)horizontalButtonCount);
				int num6 = Mathf.Clamp(firstPhotoInView + num4 * horizontalButtonCount, 0, max);
				int num7 = CalculatePhotosInViewCount(photoButtons.Length, _photosManager.availablePhotosCount, num6);
				if (firstPhotoInView != num6)
				{
					UpdateViewAndSelection(num6, num7, selectionIndex);
				}
			}
		}

		internal void OnUISelect(int index)
		{
			if (index != selectedButtonIndex)
			{
				SetSelection(index);
			}
		}

		private IEnumerator EndOfFrameSelect(int movedIndex)
		{
			yield return new WaitForEndOfFrame();
			EventSystem.current.SetSelectedGameObject(photoButtons[movedIndex].gameObject);
		}

		internal int LoadPhotosRangeIntoView(int photoIndexFrom, int photosToLoadCount, int selectionIndex, bool updateScrollBar)
		{
			NativeArray<int> nativeArray = new NativeArray<int>(photoButtons.Length, Allocator.Temp);
			if (firstPhotoInView > photoIndexFrom)
			{
				int num = (int)Mathf.Ceil((float)(firstPhotoInView - photoIndexFrom) / (float)horizontalButtonCount);
				int num2 = -(num * horizontalButtonCount);
				int num3 = 0;
				int num4 = photoButtons.Length;
				if (num < totalRowsCount)
				{
					for (int num5 = photoButtons.Length - 1; num5 >= num * horizontalButtonCount; num5--)
					{
						nativeArray[num5] = (photoButtons[num5 + num2].HasPhotoData() ? 2 : 0);
						if (photoButtons[num5 + num2].HasPhotoData())
						{
							photoButtons[num5].SetPhoto(photoButtons[num5 + num2]);
						}
						else
						{
							photoButtons[num5].ClearPhoto();
						}
					}
					num4 = num * horizontalButtonCount;
				}
				for (int i = num3; i < num4; i++)
				{
					nativeArray[i] = 0;
					photoButtons[i].ClearPhoto();
				}
			}
			else if (firstPhotoInView < photoIndexFrom)
			{
				int num6 = photoIndexFrom - firstPhotoInView;
				int rowFromPhotoCeil = GetRowFromPhotoCeil(num6, horizontalButtonCount);
				int num7 = num6;
				int num8 = 0;
				int num9 = photoButtons.Length;
				if (rowFromPhotoCeil < totalRowsCount)
				{
					for (int j = 0; j < (totalRowsCount - rowFromPhotoCeil) * horizontalButtonCount; j++)
					{
						int num10 = j + num7;
						nativeArray[j] = (photoButtons[num10].HasPhotoData() ? 2 : 0);
						if (num10 < photoButtons.Length && photoButtons[num10].HasPhotoData())
						{
							photoButtons[j].SetPhoto(photoButtons[num10]);
						}
						else
						{
							photoButtons[j].ClearPhoto();
						}
					}
					num8 = (totalRowsCount - rowFromPhotoCeil) * horizontalButtonCount;
					num9 = photosInViewCount;
				}
				for (int k = num8; k < num9; k++)
				{
					nativeArray[k] = 0;
					photoButtons[k].ClearPhoto();
				}
			}
			int num11 = photoIndexFrom - firstPhotoInView;
			Mathf.Abs(num11);
			firstPhotoInView = photoIndexFrom;
			photosInViewCount = photosToLoadCount;
			PhotoLoadInfo photoLoadInfo = default;
			for (int l = 0; l < photosInViewCount; l++)
			{
				if (nativeArray[l] != 0)
				{
					continue;
				}
				int num12 = firstPhotoInView + l;
				int num13 = nextFreeInfoSlot;
				if (num13 == 0)
				{
					int num14 = 0;
					int num15 = 0;
					int next = nextLoadingInfoSlot;
					int num16 = next - 1;
					while (next != 0)
					{
						int num17 = next - 1;
						int num18 = Volatile.Read(ref photoLoadInfos[num17].loadComplete);
						if (num18 == 1 && photoLoadInfos[num17].photoIndex == num12)
						{
							num14 = 0;
							num15 = 0;
							break;
						}
						if (IsLoadingPictureCompleteOrOutOfRange(num18, photoLoadInfos[num17].photoIndex, firstPhotoInView, photosInViewCount))
						{
							num14 = next;
							num15 = num16;
						}
						next = photoLoadInfos[num17].next;
						num16 = num17;
					}
					if (num14 != 0)
					{
						int num19 = num14 - 1;
						Interlocked.Exchange(ref photoLoadInfos[num19].loadComplete, 0);
						if (nextLoadingInfoSlot == num14)
						{
							nextLoadingInfoSlot = photoLoadInfos[num19].next;
						}
						else
						{
							photoLoadInfos[num15].next = photoLoadInfos[num19].next;
						}
						photoLoadInfos[num19].next = InsertIntoFreeList(ref nextFreeInfoSlot, num14);
					}
					num13 = nextFreeInfoSlot;
				}
				if (num13 != 0)
				{
					int num20 = num13 - 1;
					nextFreeInfoSlot = photoLoadInfos[num20].next;
					int next2 = InsertIntoFreeList(ref nextLoadingInfoSlot, num13);
					photoLoadInfo.next = next2;
					photoLoadInfo.loadComplete = 1;
					photoLoadInfo.photoIndex = num12;
					photoLoadInfo.pictureData = null;
					photoLoadInfos[num20] = photoLoadInfo;
					_photosManager.LoadPhotoAsync(num20, num12, LoadPhotoResult);
				}
			}
			SetButtonNavigation(photosInViewCount, updateScrollBar);
			int num21 = Mathf.Min(photosInViewCount, horizontalButtonCount * rowsInViewCount);
			int num22 = selectionIndex - num11;
			if (num22 < 0 || num22 >= num21)
			{
				num22 = selectedButtonIndex;
				if (selectedButtonIndex >= num21)
				{
					num22 = num21 - 1;
				}
			}
			return num22;
		}

		private static bool IsLoadingPictureCompleteOrOutOfRange(int loadStatus, int photoIndex, int firstPhotoInView, int totalPhotosInViewCount)
		{
			switch (loadStatus)
			{
			case 1:
				if (photoIndex >= firstPhotoInView)
				{
					return photoIndex >= firstPhotoInView + totalPhotosInViewCount;
				}
				return true;
			default:
				return false;
			case 2:
				return true;
			}
		}

		private bool IsLoadingAnyVisiblePhoto()
		{
			bool result = false;
			int num = Mathf.Min(photosInViewCount, horizontalButtonCount * rowsInViewCount);
			for (int i = 0; i < num; i++)
			{
				if (!photoButtons[selectedButtonIndex].HasPhotoData())
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
