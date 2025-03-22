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
				if (dailyOccurrence <= rangeEnd )
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
			List<DayOfWeek> validDays = GetValidDays(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);

			DateTime occurrence = rangeStart;
			while (occurrence <= rangeEnd)
			{
				if (validDays.Contains(occurrence.DayOfWeek))
				{
					DateTime dayOccurrence = new DateTime(occurrence.Year, occurrence.Month, occurrence.Day, taskStart.Hour, taskStart.Minute, taskStart.Second);
					AddOccurrencesForInterval(dayOccurrence, rangeStart, rangeEnd, intervalMinutes, occurrences);
				}

				occurrence = occurrence.AddDays(1);
			}

			return occurrences;
		}

		/// <summary>
		/// Parses monthly tasks considering the specified days and months.
		/// </summary>
		public static List<DateTime> ParseMonthlyTask(string repeatInterval, string repeatIntervalInMinutes, DateTime rangeStart, DateTime rangeEnd, DateTime taskStart)
		{
			List<DateTime> occurrences = new List<DateTime>();
			HashSet<int> days = GetDaysFromRepeatInterval(repeatInterval, taskStart);
			HashSet<int> months = GetMonthsFromRepeatInterval(repeatInterval, taskStart);
			int intervalMinutes = GetRepeatIntervalInMinutes(repeatIntervalInMinutes);


			DateTime current = new DateTime(rangeStart.Year, rangeStart.Month, 1);
			while (current <= rangeEnd)
			{
				if (months.Contains(current.Month))
				{
					foreach (int day in days)
					{
						if (day <= DateTime.DaysInMonth(current.Year, current.Month)) // Ensure valid day
						{
							DateTime occurrenceStart = new DateTime(current.Year, current.Month, day, taskStart.Hour, taskStart.Minute, taskStart.Second);
							AddOccurrencesForInterval(occurrenceStart, rangeStart, rangeEnd, intervalMinutes, occurrences);
						}
					}
				}

				current = current.AddMonths(1);
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
						validDays.Add((DayOfWeek)(dayNumber - 1));
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

			// If no months are provided, use taskStart's month
			if (months.Count == 0)
			{
				months.Add(taskStart.Month);
			}

			return months;
		}

		private static void AddOccurrencesForInterval(DateTime occurrenceStart, DateTime rangeStart, DateTime rangeEnd, int intervalMinutes, List<DateTime> occurrences)
		{
			DateTime occurrence = occurrenceStart;

			if (intervalMinutes == 0)
			{
				if (occurrence >= rangeStart && occurrence <= rangeEnd)
				{
					occurrences.Add(occurrence);
				}
			}
			else
			{
				while (occurrence <= rangeEnd)
				{
					if (occurrence >= rangeStart && occurrence <= rangeEnd)
					{
						occurrences.Add(occurrence);
					}

					occurrence = occurrence.AddMinutes(intervalMinutes);
				}
			}
		}
	}
}