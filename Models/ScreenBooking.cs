using System;

namespace AddAppAPI.Models
{
	public class ScreenBooking
	{
		public long Id { get; set; }
		public long ScreenId { get; set; }
		public long RequestId { get; set; }
		public DateTime ScheduledDate { get; set; }
		public int ScheduledStartTime { get; set; }
		public int ScheduledDuration { get; set; }
		public DateTime DisplayedDate { get; set; }
		public int DisplayedStartTime { get; set; }
		public int DisplayedDuration { get; set; }
		public decimal RequestCharges { get; set; }
		public bool Displayed { get; set; }
		public string ReasonForDelay { get; set; }
		public bool Deleted { get; set; }
	}
}

