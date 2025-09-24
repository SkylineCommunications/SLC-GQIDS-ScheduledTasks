namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class TimeIntervalParser
	{
		/// <summary>
		/// Parses daily tasks based on repeat interval in minutes.
		/// </summary>
		/// <param name="repeatInterval">Number of times that task will be repeated daily.</param>
		/// <param name="rangeStart">Selected start time by user.</param>
		/// <param name="rangeEnd">Selected end time by user.</param>
		/// <param name="taskStart">Actual start time of task.</param>
		/// <param name="taskEnd">Actual end time of task.</param>
		/// <returns> Returns list of timings when the task will be executed.</returns>
		public static List<DateTime> ParseDailyTask(string repeatInterval, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart, DateTime taskEnd)
		{
			var results = new List<DateTime>();
			if (!int.TryParse(repeatInterval, out int intervalMinutes) || intervalMinutes < 0)
			{
				return results;
			}

			var startTimeOfDay = taskStart.TimeOfDay;
			var endTimeOfDay = taskEnd.TimeOfDay;
			bool wrapsAround = startTimeOfDay > endTimeOfDay;

			var effectiveStart = rangeStart < taskStart ? taskStart : rangeStart;
			var effectiveEnd = GetEffectiveEnd(rangeEnd, taskEnd);

			if (wrapsAround && effectiveEnd.Date == taskEnd.Date)
				effectiveEnd = effectiveEnd.AddDays(1);

			if (effectiveStart >= effectiveEnd)
				return results;

			for (var day = effectiveStart.Date; day <= effectiveEnd.Date; day = day.AddDays(1))
			{
				if (!wrapsAround)
				{
					GenerateDailyOccurrences(day + startTimeOfDay, day + endTimeOfDay, intervalMinutes, effectiveStart, effectiveEnd, results);
				}
				else
				{
					// Early morning segment (from 00:00 to taskEnd.TimeOfDay)
					GenerateDailyOccurrences(day, day + endTimeOfDay, intervalMinutes, effectiveStart, effectiveEnd, results);
					// Evening segment (from taskStart.TimeOfDay to next day's taskEnd)
					GenerateDailyOccurrences(day + startTimeOfDay, day.AddDays(1) + endTimeOfDay, intervalMinutes, effectiveStart, effectiveEnd, results);
				}
			}

			return results.Distinct().ToList();
		}

		private static DateTime GetEffectiveEnd(DateTime rangeEnd, DateTime taskEnd)
		{
			if (taskEnd.Date != DateTime.MinValue.Date && rangeEnd > taskEnd)
				return taskEnd;
			else
				return rangeEnd;
		}

		private static void GenerateDailyOccurrences(DateTime segmentStart, DateTime segmentEnd, int intervalMinutes, DateTime effectiveStart, DateTime effectiveEnd, List<DateTime> results)
		{
			if (segmentStart >= segmentEnd)
			{
				return;
			}

			var start = segmentStart < effectiveStart ? effectiveStart : segmentStart;
			var end = segmentEnd > effectiveEnd ? effectiveEnd : segmentEnd;

			if (start >= end)
			{
				return;
			}

			var first = intervalMinutes > 0 ? new DateTime(start.Ticks - (start.Ticks % TimeSpan.FromMinutes(intervalMinutes).Ticks)) : start;

			if (first < start)
			{
				first = first.AddMinutes(intervalMinutes);
			}

			if (intervalMinutes <= 0)
			{
				// Take just first occurrence if interval is zero
				if (!results.Any())
					results.Add(first);

				return;
			}

			for (var current = first; current < end; current = current.AddMinutes(intervalMinutes))
			{
				results.Add(current);
			}
		}


		/// <summary>
		/// Parses daily tasks based on repeat interval in minutes.
		/// </summary>
		/// <returns> Returns list of timings when the task will be executed in one day.</returns>
		public static List<DateTime> CalculateDayOccurrences(DateTime baseOccurrence, DateTime rangeStart, DateTime rangeEnd, DateTime overallUpperBound, DateTime dailyCutoff, DateTime taskStart, int intervalMinutes, DateTime currentDay)
		{
			var occurrences = new List<DateTime>();

			if (intervalMinutes == 0)
			{
				if (baseOccurrence >= rangeStart && baseOccurrence <= overallUpperBound)
				{
					occurrences.Add(baseOccurrence);
				}
			}
			else
			{
				// With minute interval: start at baseOccurrence and add until reaching the daily cutoff.
				// range end still needed here for the tasks that have task end defined that is lower then  taks end
				var occ = baseOccurrence;
				while (occ < dailyCutoff && occ <= overallUpperBound)
				{
					if (occ >= rangeStart && occ >= taskStart)
					{
						occurrences.Add(occ);
					}

					occ = occ.AddMinutes(intervalMinutes);
				}
			}

			return occurrences;
		}

		/// <summary>
		/// Parses weekly tasks occurrences.
		/// </summary>
		/// <returns> Returns list of timings when the task will be executed.</returns>
		public static List<DateTime> ParseWeeklyTask(string repeatInterval, string repeatIntervalInMinutes, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart, DateTime taskEnd)
		{
			var occurrences = new List<DateTime>();
			var allowedDays = GetValidDays(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);
			var overallUpperBound = GetOverallUpperBound(taskEnd, rangeEnd);

			var current = rangeStart.Date;

			while (current <= overallUpperBound.Date)
			{
				if (allowedDays.Contains(current.DayOfWeek))
				{
					var baseOccurrence = new DateTime(current.Year, current.Month, current.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);
					var dailyCutoff = (taskEnd == DateTime.MaxValue) ? current.AddDays(1) : new DateTime(current.Year, current.Month, current.Day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second);

					occurrences.AddRange(CalculateDayOccurrences(baseOccurrence, rangeStart, rangeEnd, overallUpperBound, dailyCutoff, taskStart, intervalMinutes, current));
				}

				current = current.AddDays(1);
			}

			return occurrences;
		}

		/// <summary>
		/// Parses monthly tasks occurrences.
		/// </summary>
		/// <returns> Returns list of timings when the task will be executed.</returns>
		public static List<DateTime> ParseMonthlyTask(string repeatInterval, string repeatIntervalInMinutes, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart, DateTime taskEnd)
		{
			var occurrences = new List<DateTime>();
			var allowedDays = GetDaysFromRepeatInterval(repeatInterval, taskStart);
			var allowedMonths = GetMonthsFromRepeatInterval(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);
			bool useCutoff = (taskEnd != DateTime.MaxValue) && (taskEnd.Date == DateTime.MinValue);

			if (intervalMinutes == 0)
			{
				// One occurrence per allowed day.
				var currentMonth = new DateTime(rangeStart.Year, rangeStart.Month, 1).ToUniversalTime();
				while (currentMonth <= rangeEnd)
				{
					if (allowedMonths.Contains(currentMonth.Month))
					{
						foreach (int day in allowedDays)
						{
							if (day <= DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month))
							{
								var occ = new DateTime(currentMonth.Year, currentMonth.Month, day, taskStart.Hour, taskStart.Minute, taskStart.Second);
								var dailyCutoff = useCutoff ? new DateTime(currentMonth.Year, currentMonth.Month, day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second) : occ.Date.AddDays(1);
								if (occ >= rangeStart && occ <= rangeEnd && occ <= dailyCutoff)
								{
									occurrences.Add(occ);
								}
							}
						}
					}

					currentMonth = currentMonth.AddMonths(1);
				}
			}
			else
			{
				// With minute interval, iterate day by day.
				var currentDay = rangeStart.Date;
				while (currentDay <= rangeEnd.Date)
				{
					if (allowedMonths.Contains(currentDay.Month) && allowedDays.Contains(currentDay.Day))
					{
						var baseOccurrence = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);
						var dailyCutoff = useCutoff ? new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second) : currentDay.AddDays(1);
						var occ = baseOccurrence;
						while (occ < dailyCutoff && occ <= rangeEnd)
						{
							if (occ >= rangeStart && occ >= taskStart)
							{
								occurrences.Add(occ);
							}

							occ = occ.AddMinutes(intervalMinutes);
						}
					}

					currentDay = currentDay.AddDays(1);
				}
			}

			return occurrences;
		}

		/// <summary>
		/// Parses the repeat interval days and returns a list of valid days.
		/// </summary>
		public static List<DayOfWeek> GetValidDays(string repeatInterval, DateTime taskStart)
		{
			var validDays = new List<DayOfWeek>();

			if (!string.IsNullOrEmpty(repeatInterval))
			{
				string[] days = repeatInterval.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var day in days)
				{
					if (int.TryParse(day, out int dayNumber) && dayNumber >= 1 && dayNumber <= 7)
					{
						validDays.Add((DayOfWeek)dayNumber);
					}
				}
			}

			if (validDays.Count == 0)
			{
				validDays.Add(taskStart.DayOfWeek);
			}

			return validDays;
		}

		/// <summary>
		/// Parses the repeat interval in minutes and returns the parsed value.
		/// </summary>
		public static int GetRepeatIntervalInMinutes(string repeatIntervalInMinutes)
		{
			int intervalMinutes = 0;
			if (!string.IsNullOrEmpty(repeatIntervalInMinutes) && int.TryParse(repeatIntervalInMinutes, out int parsedMinutes) && parsedMinutes > 0)
			{
				intervalMinutes = parsedMinutes;
			}

			return intervalMinutes;
		}

		public static HashSet<int> GetDaysFromRepeatInterval(string repeatInterval, DateTime taskStart)
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

		public static HashSet<int> GetMonthsFromRepeatInterval(string repeatInterval, DateTime taskStart)
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

		/// <summary>
		/// Returns the effective upper bound between a task’s end and a range end.
		/// If the task end is unbounded (DateTime.MaxValue) or comes after the range end,
		/// this will return rangeEnd; otherwise it returns taskEnd.
		/// </summary>
		private static DateTime GetOverallUpperBound(DateTime taskEnd, DateTime rangeEnd)
		{
			// Treat MaxValue as “no end,” and cap any later date to the range end
			if (taskEnd.Date == DateTime.MinValue.Date || taskEnd > rangeEnd)
			{
				return rangeEnd.ToLocalTime();
			}

			return taskEnd;
		}
	}
}
