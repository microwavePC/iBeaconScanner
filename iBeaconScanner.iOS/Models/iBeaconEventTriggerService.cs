using System;
using System.Collections.Generic;
using CoreLocation;
using Foundation;
using iBeaconScanner.iOS.Models;
using iBeaconScanner.Models;
using Prism.Mvvm;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(iBeaconEventTriggerService))]
namespace iBeaconScanner.iOS.Models
{
	public class iBeaconEventTriggerService : BindableBase, IiBeaconEventTriggerService
	{
		#region PROPERTIES

		private bool _isScanningBeacon;
		public bool IsScanningBeacon
		{
			get { return _isScanningBeacon; }
			set { SetProperty(ref _isScanningBeacon, value); }
		}

		#endregion



		#region FIELDS

		private Dictionary<string, iBeaconEventHolder> _beaconEventHolderDict;
		private CLLocationManager _locationManager;

		#endregion



		#region CONSTRUCTOR

		public iBeaconEventTriggerService()
		{
			_beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
			_locationManager = new CLLocationManager();

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				_locationManager.RequestWhenInUseAuthorization();
			}
		}

		#endregion



		#region PUBLIC METHODS

		public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action func)
		{
			//TODO: 非同期メソッドや引数ありのメソッドもセットできるようにしたい
			iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

			if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
			{
				_beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
			}

			_beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, func);

			if (IsScanningBeacon)
			{
				var nsUuid = new NSUuid(uuid.ToString());
				var beaconRegion = new CLBeaconRegion(nsUuid, major, minor, eventHolder.BeaconIdentifyStr);
				_locationManager.StartRangingBeacons(beaconRegion);
			}
		}


		public void ClearAllEvent()
		{
			_beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
		}


		public void StartScan()
		{
			if (IsScanningBeacon)
			{
				return;
			}

			_locationManager.DidRangeBeacons += didRangeBeacons;

			foreach (var eventHolder in _beaconEventHolderDict)
			{
				var uuid = new NSUuid(eventHolder.Value.Uuid.ToString());
				var beaconRegion = new CLBeaconRegion(uuid,
													  eventHolder.Value.Major,
													  eventHolder.Value.Minor,
													  eventHolder.Value.BeaconIdentifyStr);
				
				_locationManager.StartRangingBeacons(beaconRegion);
			}

			IsScanningBeacon = true;
		}


		public void StopScan()
		{
			if (!IsScanningBeacon)
			{
				return;
			}

			foreach (var eventHolder in _beaconEventHolderDict)
			{
				var uuid = new NSUuid(eventHolder.Value.Uuid.ToString());
				var beaconRegion = new CLBeaconRegion(uuid,
				                                      eventHolder.Value.Major,
				                                      eventHolder.Value.Minor,
				                                      eventHolder.Value.BeaconIdentifyStr);
				
				_locationManager.StopRangingBeacons(beaconRegion);
			}

			IsScanningBeacon = false;
		}

		#endregion



		#region PRIVATE METHODS

		private void didRangeBeacons(object s, CLRegionBeaconsRangedEventArgs e)
		{
			foreach (var detectedBeacon in e.Beacons)
			{
				string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(
					new Guid(detectedBeacon.ProximityUuid.ToString()),
					detectedBeacon.Major.UInt16Value,
					detectedBeacon.Minor.UInt16Value);

				if (!_beaconEventHolderDict.ContainsKey(beaconIdentifier))
				{
					return;
				}

				iBeaconEventHolder eventHolder = _beaconEventHolderDict[beaconIdentifier];
				foreach (iBeaconEventDetail eventDetail in eventHolder.EventList)
				{
					if (eventDetail.ThresholdRssi < detectedBeacon.Rssi &&
						eventDetail.LastTriggeredDateTime < DateTime.Now.AddMilliseconds(-1 * eventDetail.EventTriggerIntervalMilliSec))
					{
						eventDetail.LastTriggeredDateTime = DateTime.Now;
						eventDetail.Function();
					}
				}
			}
		}

		#endregion
	}
}
