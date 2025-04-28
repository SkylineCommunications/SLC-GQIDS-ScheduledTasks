namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;

	public static class TimeIntervalParser
	{
		/// <summary>
		/// Parses daily tasks based on repeat interval in minutes.
		/// </summary>
		/// <returns> Returns list of timings when the task will be executed.</returns>
		public static List<DateTime> ParseDailyTask(string repeatInterval, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart, DateTime taskEnd)
		{
			var occurrences = new List<DateTime>();
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatInterval);
			var overallUpperBound = (taskEnd == DateTime.MaxValue) ? rangeEnd : (taskEnd > rangeEnd ? rangeEnd : taskEnd);

			var currentDay = rangeStart.Date;
			while (currentDay <= overallUpperBound.Date)
			{
				var baseOccurrence = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);

				// Daily cutoff:
				// - If taskEnd is MaxValue then cutoff is the start of the next day.
				// - Otherwise, the cutoff is the current day with taskEnd's time.
				var dailyCutoff = (taskEnd == DateTime.MaxValue) ? currentDay.AddDays(1) : new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second);
				occurrences.AddRange(CalculateDayOccurrences(baseOccurrence, rangeStart, rangeEnd, overallUpperBound, dailyCutoff, taskStart, intervalMinutes, currentDay));
				currentDay = currentDay.AddDays(1);
			}

			return occurrences;
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
			var overallUpperBound = (taskEnd == DateTime.MaxValue) ? rangeEnd : (taskEnd > rangeEnd ? rangeEnd : taskEnd);

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
	}
}
