using System;

namespace AddAppAPI.Models
{
	public class Request
	{
		public long Id { get; set; }
		public int UserId { get; set; }
		public byte TargetTypeId { get; set; }
		public long TargetId { get; set; }
		public byte MessageTypeId { get; set; }
		public long? SavedMessageId { get; set; }
		public long? UserRelashionshipId { get; set; }
		public string Text { get; set; }
		public byte[] Picture { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime ScheduledDate { get; set; }
		public int ScheduledStartTime { get; set; }
		public int ScheduledDuration { get; set; }
		public byte StatusId { get; set; }
		public bool? CanDelayStartTime { get; set; }
		public short? MaxDelay { get; set; }
		public bool? RecurringMessage { get; set; }
		public byte? RecurringTimes { get; set; }
		public int? RecurForEvery { get; set; }
		public bool Deleted { get; set; }
	}
}

