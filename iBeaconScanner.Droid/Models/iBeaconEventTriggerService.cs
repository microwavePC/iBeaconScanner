using System;
using System.Collections.Generic;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using iBeaconScanner.Models;
using Prism.Mvvm;

namespace iBeaconScanner.Droid.Models
{
	public class iBeaconEventTriggerService : BindableBase, IiBeaconEventTriggerService
	{
		#region PRERTIES

		private bool _isScanningBeacon;
		public bool IsScanningBeacon
		{
			get { return _isScanningBeacon; }
			set { SetProperty(ref _isScanningBeacon, value); }
		}

		#endregion



		#region FIELDS

		private BluetoothManager _btManager;
		private BluetoothAdapter _btAdapter;
		private BluetoothLeScanner _bleScanner;
		private BleScanCallback _scanCallback;

		#endregion



		#region CONSTRUCTOR

		public iBeaconEventTriggerService()
		{
			_btManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
			_btAdapter = _btManager.Adapter;
			_bleScanner = _btAdapter.BluetoothLeScanner;
			_scanCallback = new BleScanCallback();
		}

		#endregion



		#region PUBLIC METHODS

		public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action func)
		{
			//TODO: 非同期メソッドや引数ありのメソッドもセットできるようにしたい
			iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

			if (!_scanCallback.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
			{
				_scanCallback.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
			}
			_scanCallback.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, func);
		}


		public void ClearAllEvent()
		{
			_scanCallback.BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
		}


		public void StartScan()
		{
			if (IsScanningBeacon)
			{
				return;
			}

			_bleScanner.StartScan(_scanCallback);
			IsScanningBeacon = true;
		}


		public void StopScan()
		{
			if (!IsScanningBeacon)
			{
				return;
			}

			_bleScanner.StopScan(_scanCallback);
			IsScanningBeacon = false;
		}

		#endregion
	}
}
