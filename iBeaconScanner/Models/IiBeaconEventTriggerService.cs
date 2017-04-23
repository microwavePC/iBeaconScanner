using System;
using System.ComponentModel;

namespace iBeaconScanner.Models
{
	public interface IiBeaconEventTriggerService : INotifyPropertyChanged
	{
		bool IsScanningBeacon { get; }

		void AddEvent(Guid uuid,
					  ushort major,
					  ushort minor,
					  short thresholdRssi,
					  int intervalMilliSec,
					  Action function);

		void ClearAllEvent();
		
		void StartScan();

		void StopScan();
	}
}
