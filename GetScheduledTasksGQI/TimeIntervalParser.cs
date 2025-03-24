namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;

	public static class TimeIntervalParser
	{
		/// <summary>
		/// Parses daily tasks based on repeat interval in minutes.
		/// </summary>
		public static List<DateTime> ParseDailyTask(string repeatInterval, string repeatIntervalInMinutes, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart)
		{
			List<DateTime> occurrences = new List<DateTime>();
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatInterval);
			DateTime dailyOccurrence = new DateTime(rangeStart.Year, rangeStart.Month, rangeStart.Day, rangeStart.Hour, taskStart.Minute, taskStart.Second);

			if (intervalMinutes == 0)
			{
				AddDailyOccurrences(dailyOccurrence, rangeStart, rangeEnd, occurrences);
			}
			else
			{
				if (dailyOccurrence <= rangeEnd)
				{
					DateTime dayOccurrence = dailyOccurrence;

					while (dayOccurrence <= rangeEnd)
					{
						if (dayOccurrence >= rangeStart)
						{
							occurrences.Add(dayOccurrence);
						}

						dayOccurrence = dayOccurrence.AddMinutes(intervalMinutes);
					}
				}
			}

			return occurrences;
		}

		/// <summary>
		/// Parses weekly tasks considering the day of the week and repeat interval in minutes.
		/// </summary>
		public static List<DateTime> ParseWeeklyTask(string repeatInterval, string repeatIntervalInMinutes, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart)
		{
			List<DateTime> occurrences = new List<DateTime>();
			List<DayOfWeek> allowedDays = GetValidDays(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);

			if (intervalMinutes == 0)
			{
				DateTime current = rangeStart.Date;
				while (current <= rangeEnd.Date)
				{
					if (allowedDays.Contains(current.DayOfWeek))
					{
						DateTime occ = new DateTime(current.Year, current.Month, current.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);
						if (occ >= rangeStart && occ <= rangeEnd)
						{
							occurrences.Add(occ);
						}
					}
					current = current.AddDays(1);
				}
			}
			else
			{
				DateTime currentDay = rangeStart.Date;
				while (currentDay <= rangeEnd.Date)
				{
					if (allowedDays.Contains(currentDay.DayOfWeek))
					{
						DateTime baseOccurrence = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);
						DateTime dayEnd = currentDay.AddDays(1);
						DateTime occ = baseOccurrence;
						while (occ < dayEnd && occ <= rangeEnd)
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
		/// Parses monthly tasks considering the specified days and months.
		/// </summary>
		public static List<DateTime> ParseMonthlyTask(string repeatInterval, string repeatIntervalInMinutes, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart)
		{
			List<DateTime> occurrences = new List<DateTime>();
			HashSet<int> allowedDays = GetDaysFromRepeatInterval(repeatInterval, taskStart);
			HashSet<int> allowedMonths = GetMonthsFromRepeatInterval(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);

			if (intervalMinutes == 0)
			{
				DateTime currentMonth = new DateTime(rangeStart.Year, rangeStart.Month, 1);
				while (currentMonth <= rangeEnd)
				{
					if (allowedMonths.Contains(currentMonth.Month))
					{
						foreach (int day in allowedDays)
						{
							if (day <= DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month))
							{
								DateTime occ = new DateTime(currentMonth.Year, currentMonth.Month, day, taskStart.Hour, taskStart.Minute, taskStart.Second);
								if (occ >= rangeStart && occ <= rangeEnd)
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
				DateTime currentDay = rangeStart.Date;
				while (currentDay <= rangeEnd.Date)
				{
					if (allowedMonths.Contains(currentDay.Month))
					{
						DateTime baseOccurrence = new DateTime(currentDay.Year, currentDay.Month, currentDay.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);
						if (allowedDays.Contains(baseOccurrence.Day))
						{
							DateTime endOfDay = currentDay.AddDays(1);
							DateTime occ = baseOccurrence;
							while (occ < endOfDay && occ <= rangeEnd)
							{
								if (occ >= rangeStart)
								{
									occurrences.Add(occ);
								}
								occ = occ.AddMinutes(intervalMinutes);
							}
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

		/// <summary>
		/// Adds occurrences daily between the specified range.
		/// </summary>
		private static void AddDailyOccurrences(DateTime initialOccurrence, DateTime rangeStart, DateTime rangeEnd, List<DateTime> occurrences)
		{
			DateTime dailyOccurrence = initialOccurrence;

			while (dailyOccurrence <= rangeEnd)
			{
				if (dailyOccurrence >= rangeStart)
				{
					occurrences.Add(dailyOccurrence);
				}

				dailyOccurrence = dailyOccurrence.AddDays(1);
			}
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
