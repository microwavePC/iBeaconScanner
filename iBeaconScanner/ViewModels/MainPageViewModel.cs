using System;
using System.ComponentModel;
using System.Windows.Input;
using iBeaconScanner.Models;
using Plugin.Vibrate;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace iBeaconScanner.ViewModels
{
	public class MainPageViewModel : BindableBase, INavigationAware
	{
		#region PROPERTIES

		private string _uuidStr;
		public string UuidStr
		{
			get { return _uuidStr; }
			set { SetProperty(ref _uuidStr, value); }
		}

		private string _majorStr;
		public string MajorStr
		{
			get { return _majorStr; }
			set { SetProperty(ref _majorStr, value); }
		}

		private string _minorStr;
		public string MinorStr
		{
			get { return _minorStr; }
			set { SetProperty(ref _minorStr, value); }
		}

		private short _thresholdRssi = -65;
		public short ThresholdRssi
		{
			get { return _thresholdRssi; }
			set { SetProperty(ref _thresholdRssi, value); }
		}

		private int _timeIntervalSec = 30;
		public int TimeIntervalSec
		{
			get { return _timeIntervalSec; }
			set { SetProperty(ref _timeIntervalSec, value); }
		}

		private int _eventIndex;
		public int EventIndex
		{
			get { return _eventIndex; }
			set { SetProperty(ref _eventIndex, value); }
		}

		private bool _isScanning;
		public bool IsScanning
		{
			get { return _isScanning; }
			set { SetProperty(ref _isScanning, value); }
		}

		#endregion



		#region FIELDS

		private readonly IiBeaconEventTriggerService _ibeaconEventTriggerService;
		private readonly IPageDialogService _pageDialogService;

		#endregion



		#region COMMANDS

		public ICommand AddEventCommand { get; }
		public ICommand ClearAllEventCommand { get; }
		public ICommand StartScanCommand { get; }
		public ICommand StopScanCommand { get; }

		#endregion



		#region CONSTRUCTOR

		public MainPageViewModel(IiBeaconEventTriggerService ibeaconEventTriggerService,
		                         IPageDialogService pageDialogService)
		{
			_ibeaconEventTriggerService = ibeaconEventTriggerService;
			_pageDialogService = pageDialogService;

			_ibeaconEventTriggerService.PropertyChanged += servicePropertyChanged;

			AddEventCommand      = new DelegateCommand(addEvent);
			ClearAllEventCommand = new DelegateCommand(clearAllEvent);
			StartScanCommand     = new DelegateCommand(startScan);
			StopScanCommand      = new DelegateCommand(stopScan);
		}

		#endregion



		#region PUBLIC METHODS

		public void OnNavigatedFrom(NavigationParameters parameters)
		{

		}

		public void OnNavigatedTo(NavigationParameters parameters)
		{
			
		}

		public void OnNavigatingTo(NavigationParameters parameters)
		{
			
		}

		#endregion



		#region PRIVATE METHODS

		private void addEvent()
		{
			string errorMessage = string.Empty;

			if (!isValidUuid())
			{
				errorMessage += "UUIDの入力値が不正です。" + Environment.NewLine;
			}
			if (!isValidMajor())
			{
				errorMessage += "MAJORには1〜65535の範囲の数値を入力してください。" + Environment.NewLine;
			}
			if (!isValidMinor())
			{
				errorMessage += "MINORには1〜65535の範囲の数値を入力してください。" + Environment.NewLine;
			}

			if (errorMessage != string.Empty)
			{
				_pageDialogService.DisplayAlertAsync("エラー",
				                                     errorMessage,
					                                 "OK");
				return;
			}

			Action targetEvent = null;
			switch (EventIndex)
			{
				case 0:
					targetEvent = showAlert;
					break;
				case 1:
					targetEvent = vibrate;
					break;
				default:
					return;
			}

			_ibeaconEventTriggerService.AddEvent(new Guid(UuidStr),
			                                     ushort.Parse(MajorStr),
			                                     ushort.Parse(MinorStr),
			                                     ThresholdRssi,
			                                     TimeIntervalSec * 1000,
			                                     targetEvent);

			_pageDialogService.DisplayAlertAsync("iBeaconScanner",
			                                     "イベントを追加しました。",
					                             "OK");
		}

		private void clearAllEvent()
		{
			_ibeaconEventTriggerService.ClearAllEvent();

			_pageDialogService.DisplayAlertAsync("iBeaconScanner",
			                                     "イベントを削除しました。",
					                             "OK");
		}


		private void startScan()
		{
			_ibeaconEventTriggerService.StartScan();
		}


		private void stopScan()
		{
			_ibeaconEventTriggerService.StopScan();
		}


		private bool isValidUuid()
		{
			Guid uuid;
			bool result = Guid.TryParse(UuidStr, out uuid);
			return result;
		}


		private bool isValidMajor()
		{
			ushort major;
			bool result = ushort.TryParse(MajorStr, out major);
			return result;
		}


		private bool isValidMinor()
		{
			ushort minor;
			bool result = ushort.TryParse(MinorStr, out minor);

			return result;
		}

		#endregion



		#region EVENTS TRIGGERED BY iBeaconEventTriggerService

		private void showAlert()
		{
			_pageDialogService.DisplayAlertAsync("iBeaconScanner",
				                                 "This event is triggered by service!",
				                                 "OK");
		}


		private void vibrate()
		{
			var vibrator = CrossVibrate.Current;
			vibrator.Vibration(1000);
		}


		private void servicePropertyChanged(object s, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsScanningBeacon")
			{
				IsScanning = _ibeaconEventTriggerService.IsScanningBeacon;
			}
		}

		#endregion
	}
}

