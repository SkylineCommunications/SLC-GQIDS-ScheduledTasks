namespace SchedulerTasksGetter
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;

	using GetScheduledTasksGQI;
	using Skyline.DataMiner.Net.Messages;

	using SchedulerTask = Skyline.DataMiner.Net.Messages.SchedulerTask;

	public class IntervalBuilder
	{

		public static string BuildIntervalString(SchedulerTask task)
		{
			var days = TimeIntervalParser.GetDaysFromRepeatInterval(task.RepeatInterval, task.StartTime);
			var months = TimeIntervalParser.GetMonthsFromRepeatInterval(task.RepeatInterval, task.StartTime);
			var minutes = TimeIntervalParser.GetRepeatIntervalInMinutes(task.RepeatType == Skyline.DataMiner.Net.Messages.SchedulerRepeatType.Daily ? task.RepeatInterval : task.RepeatIntervalInMinutes);

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
	}
}
