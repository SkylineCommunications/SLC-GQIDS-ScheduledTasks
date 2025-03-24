namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Analytics.GenericInterface;

	public static class TimeIntervalParser
	{
		/// <summary>
		/// Parses daily tasks based on repeat interval in minutes.
		/// </summary>
		public static List<DateTime> ParseDailyTask(string repeatInterval, string repeatIntervalInMinutes, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart, DateTime taskEnd)
		{
			List<DateTime> occurrences = new List<DateTime>();
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatInterval);
			DateTime overallUpperBound = (taskEnd == DateTime.MaxValue) ? rangeEnd : taskEnd;
			DateTime currentDay = rangeStart.Date;
			while (currentDay <= overallUpperBound.Date)
			{
				DateTime baseOccurrence = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);

				// Daily cutoff:
				// - If taskEnd is MaxValue then cutoff is the start of the next day.
				// - Otherwise, the cutoff is the current day with taskEnd's time.
				DateTime dailyCutoff = (taskEnd == DateTime.MaxValue) ? currentDay.AddDays(1) : new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second);

				if (intervalMinutes == 0)
				{
					if (baseOccurrence >= rangeStart && baseOccurrence <= overallUpperBound && baseOccurrence <= dailyCutoff)
					{
						occurrences.Add(baseOccurrence);
					}
				}
				else
				{
					// With minute interval: start at baseOccurrence and add until reaching the daily cutoff.
					DateTime occ = baseOccurrence;
					while (occ < dailyCutoff && occ <= overallUpperBound)
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

			return occurrences;
		}

		public static List<DateTime> ParseWeeklyTask(string repeatInterval,	string repeatIntervalInMinutes,	DateTime rangeStart,DateTime rangeEnd,	DateTime taskStart,	DateTime taskEnd)
		{
			List<DateTime> occurrences = new List<DateTime>();
			List<DayOfWeek> allowedDays = GetValidDays(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);

			DateTime overallUpperBound = (taskEnd == DateTime.MaxValue) ? rangeEnd : taskEnd;

			DateTime current = rangeStart.Date;
			while (current <= overallUpperBound.Date)
			{
				if (allowedDays.Contains(current.DayOfWeek))
				{
					DateTime baseOccurrence = new DateTime(current.Year, current.Month, current.Day,  taskStart.Hour, taskStart.Minute, taskStart.Second, taskStart.Kind);

					// Daily cutoff: if taskEnd is MaxValue then cutoff is the start of next day;
					// otherwise, cutoff is current day combined with taskEnd's time.
					DateTime dailyCutoff = (taskEnd == DateTime.MaxValue)? current.AddDays(1): new DateTime(current.Year, current.Month, current.Day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second, taskEnd.Kind);

					if (intervalMinutes == 0)
					{
						if (baseOccurrence >= rangeStart && baseOccurrence <= overallUpperBound && baseOccurrence < dailyCutoff)
						{
							occurrences.Add(baseOccurrence);
						}
					}
					else
					{
						DateTime occ = baseOccurrence;
						while (occ < dailyCutoff && occ <= overallUpperBound)
						{
							if (occ >= rangeStart)
							{
								occurrences.Add(occ);
							}
							occ = occ.AddMinutes(intervalMinutes);
						}
					}
				}
				current = current.AddDays(1);
			}

			return occurrences;
		
		}
		public static List<DateTime> ParseMonthlyTask(string repeatInterval,	string repeatIntervalInMinutes,	DateTime rangeStart,	DateTime rangeEnd,	DateTime taskStart,	DateTime taskEnd)
		{
			List<DateTime> occurrences = new List<DateTime>();
			HashSet<int> allowedDays = GetDaysFromRepeatInterval(repeatInterval, taskStart);
			HashSet<int> allowedMonths = GetMonthsFromRepeatInterval(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);
			bool useCutoff = (taskEnd != DateTime.MaxValue) && (taskEnd.Date == DateTime.MinValue);

			if (intervalMinutes == 0)
			{
				// One occurrence per allowed day.
				DateTime currentMonth = new DateTime(rangeStart.Year, rangeStart.Month, 1).ToUniversalTime();
				while (currentMonth <= rangeEnd)
				{
					if (allowedMonths.Contains(currentMonth.Month))
					{
						foreach (int day in allowedDays)
						{
							if (day <= DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month))
							{
								DateTime occ = new DateTime(currentMonth.Year, currentMonth.Month, day, taskStart.Hour, taskStart.Minute, taskStart.Second);
								DateTime dailyCutoff = useCutoff
									? new DateTime(currentMonth.Year, currentMonth.Month, day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second)
									: occ.Date.AddDays(1);
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
				DateTime currentDay = rangeStart.Date;
				while (currentDay <= rangeEnd.Date)
				{
					if (allowedMonths.Contains(currentDay.Month) && allowedDays.Contains(currentDay.Day))
					{
						DateTime baseOccurrence = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);
						DateTime dailyCutoff = useCutoff
							? new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskEnd.Hour, taskEnd.Minute, taskEnd.Second)
							: currentDay.AddDays(1);
						DateTime occ = baseOccurrence;
						while (occ < dailyCutoff && occ <= rangeEnd)
						{
							if (occ >= rangeStart)
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
		private static List<DayOfWeek> GetValidDays(string repeatInterval, DateTime taskStart)
		{
			List<DayOfWeek> validDays = new List<DayOfWeek>();

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
		private static int GetRepeatIntervalInMinutes(string repeatIntervalInMinutes)
		{
			int intervalMinutes = 0;
			if (!string.IsNullOrEmpty(repeatIntervalInMinutes) && int.TryParse(repeatIntervalInMinutes, out int parsedMinutes) && parsedMinutes > 0)
			{
				intervalMinutes = parsedMinutes;
			}

			return intervalMinutes;
		}
		private static HashSet<int> GetDaysFromRepeatInterval(string repeatInterval, DateTime taskStart)
		{
			HashSet<int> days = new HashSet<int>();

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

		private static HashSet<int> GetMonthsFromRepeatInterval(string repeatInterval, DateTime taskStart)
		{
			HashSet<int> months = new HashSet<int>();

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
