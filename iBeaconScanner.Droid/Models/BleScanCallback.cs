using System;
using System.Collections.Generic;
using Android.Bluetooth.LE;
using iBeaconScanner.Droid.Utils;
using iBeaconScanner.Models;

namespace iBeaconScanner.Droid.Models
{
	public class BleScanCallback : ScanCallback
	{
		#region PROPERTIES

		public Dictionary<string, iBeaconEventHolder> BeaconEventHolderDict { get; set; }

		#endregion



		#region CONSTRUCTOR

		public BleScanCallback()
		{
			BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
		}

		#endregion



		public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
		{
			base.OnScanResult(callbackType, result);
		
			if (iBeaconUtility.IsIBeacon(result.ScanRecord))
			{
				Guid uuid = iBeaconUtility.GetUuidFromRecord(result.ScanRecord);
				ushort major = iBeaconUtility.GetMajorFromRecord(result.ScanRecord);
				ushort minor = iBeaconUtility.GetMinorFromRecord(result.ScanRecord);

				string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, major, minor);

				if (!BeaconEventHolderDict.ContainsKey(beaconIdentifier))
				{
					return;
				}

				var eventHolder = BeaconEventHolderDict[beaconIdentifier];
				foreach (var eventDetail in eventHolder.EventList)
				{
					if (eventDetail.ThresholdRssi < result.Rssi &&
					    eventDetail.LastTriggeredDateTime < DateTime.Now.AddMilliseconds(-1 * eventDetail.EventTriggerIntervalMilliSec))
					{
						eventDetail.LastTriggeredDateTime = DateTime.Now;
						eventDetail.Function();
					}
				}
			}
		}
	}
}
