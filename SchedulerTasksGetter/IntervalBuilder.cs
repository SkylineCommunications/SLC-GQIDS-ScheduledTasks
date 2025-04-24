namespace SchedulerTasksGetter
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages;

	using SchedulerTask = Skyline.DataMiner.Net.Messages.SchedulerTask;

	public class IntervalBuilder
	{

		public static string BuildIntervalString(SchedulerTask task)
		{
			var days = GetDaysFromRepeatInterval(task.RepeatInterval, task.StartTime);
			var months = GetMonthsFromRepeatInterval(task.RepeatInterval, task.StartTime);
			var minutes = GetRepeatIntervalInMinutes(task.RepeatType == Skyline.DataMiner.Net.Messages.SchedulerRepeatType.Daily ? task.RepeatInterval : task.RepeatIntervalInMinutes);

			switch (task.RepeatType)
			{
				case SchedulerRepeatType.Daily:

					return minutes > 0 ? $"Every {minutes} minutes"	: string.Empty;

				case SchedulerRepeatType.Weekly:
					if (!days.Any())
					{
						return string.Empty;
					}

					var weekNames = days.Select(d => CultureInfo.InvariantCulture.DateTimeFormat.GetDayName((DayOfWeek)(d % 7))).ToArray();
					return $"At: {string.Join(", ", weekNames)}";

				case SchedulerRepeatType.Monthly:
					if (!days.Any() || !months.Any())
					{
						return string.Empty;
					}

					var dayParts = days.Select(d => $"Day {d}");
					var monthNames = months.Select(m => CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m));
					return $"At: {string.Join(", ", dayParts)} of months {string.Join(", ", monthNames)}";

				default:
					return string.Empty;
			}
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
