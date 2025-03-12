namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Net.Messages;

	public static class ColumnMappings
	{
		public static readonly Dictionary<string, Func<SchedulerTask, object>> columnNameToTask = new Dictionary<string, Func<SchedulerTask, object>>
		{
			{ "Start",task => task.StartTime },
			{ "End", task => task.EndTime },
			{ "Name", task => task.TaskName},
			{ "Description", task => task.Description },
			{ "Type", task => task.RepeatType },
			{ "DataMiner", task => task.HandlingDMA},

		};
	}
}
