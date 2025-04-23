namespace SchedulerTasksGetter
{
	using System;
	using System.Collections.Generic;
	using SchedulerTask = Skyline.DataMiner.Net.Messages.SchedulerTask;

	public class IntervalBuilder
	{

		private string BuildIntervalString(SchedulerTask task)
		{
			var days = GetDaysFromRepeatInterval(task.RepeatInterval, task.StartTime);
			var montrs = GetMonthsFromRepeatInterval(task.RepeatInterval, task.StartTime);
			var minutes = GetRepeatIntervalInMinutes(task.RepeatType == Skyline.DataMiner.Net.Messages.SchedulerRepeatType.Daily ? task.RepeatInterval : task.RepeatIntervalInMinutes);
			return string.Empty;
		}

		private static HashSet<int> GetDaysFromRepeatInterval(string repeatInterval, DateTime taskStart)
		{
			var days = new HashSet<int>();

			if (!string.IsNullOrEmpty(repeatInterval))
			{
				string[] parts = repeatInterval.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var part in parts)
				{
					if (int.TryParse(part, out int value) && value < 100)
					{
						days.Add(value); // Day of the month
					}
				}
			}

			if (days.Count == 0)
			{
				days.Add(taskStart.Day);
			}

			return days;
		}

		private static int GetRepeatIntervalInMinutes(string repeatIntervalInMinutes)
		{
			int intervalMinutes = 0;
			if (!string.IsNullOrEmpty(repeatIntervalInMinutes) && int.TryParse(repeatIntervalInMinutes, out int parsedMinutes) && parsedMinutes > 0)
			{
				intervalMinutes = parsedMinutes;
			}

			return intervalMinutes;
		}

		private static HashSet<int> GetMonthsFromRepeatInterval(string repeatInterval, DateTime taskStart)
		{
			var months = new HashSet<int>();

			if (!string.IsNullOrEmpty(repeatInterval))
			{
				string[] parts = repeatInterval.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var part in parts)
				{
					if (int.TryParse(part, out int value) && value >= 100)
					{
						months.Add(value - 100); // Convert 101 > Jan, 102 > Feb, etc.
					}
				}
			}

			if (months.Count == 0)
			{
				months.Add(taskStart.Month);
			}

			return months;
		}
	}
}
