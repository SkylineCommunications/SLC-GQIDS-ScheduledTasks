namespace GetScheduledTasksGQI.Tests
{
	using System;
	using System.Collections.Generic;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class TimeIntervalParserTests
	{
		[TestMethod]
		public void ParseDailyTaskEndBeforeStartTest()
		{
			var taskStart = new DateTime(2025, 9, 17, 13, 0, 0);
			var taskEnd = new DateTime(2025, 9, 27, 6, 0, 0);
			var rangeStart = new DateTime(2025, 9, 20, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 21, 0, 0, 0);
			var repeatInterval = "60"; // every 60 minutes
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 20, 0, 0, 0),
				new DateTime(2025, 9, 20, 1, 0, 0),
				new DateTime(2025, 9, 20, 2, 0, 0),
				new DateTime(2025, 9, 20, 3, 0, 0),
				new DateTime(2025, 9, 20, 4, 0, 0),
				new DateTime(2025, 9, 20, 5, 0, 0),
				new DateTime(2025, 9, 20, 13, 0, 0),
				new DateTime(2025, 9, 20, 14, 0, 0),
				new DateTime(2025, 9, 20, 15, 0, 0),
				new DateTime(2025, 9, 20, 16, 0, 0),
				new DateTime(2025, 9, 20, 17, 0, 0),
				new DateTime(2025, 9, 20, 18, 0, 0),
				new DateTime(2025, 9, 20, 19, 0, 0),
				new DateTime(2025, 9, 20, 20, 0, 0),
				new DateTime(2025, 9, 20, 21, 0, 0),
				new DateTime(2025, 9, 20, 22, 0, 0),
				new DateTime(2025, 9, 20, 23, 0, 0),
			};

			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
        }

		[TestMethod]
		public void ParseDailyTaskStartBeforeEndTest()
		{
			var taskStart = new DateTime(2025, 9, 17, 10, 0, 0);
			var taskEnd = new DateTime(2025, 9, 27, 20, 0, 0);
			var rangeStart = new DateTime(2025, 9, 20, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 21, 0, 0, 0);
			var repeatInterval = "60"; // every 60 minutes
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 20, 10, 0, 0),
				new DateTime(2025, 9, 20, 11, 0, 0),
				new DateTime(2025, 9, 20, 12, 0, 0),
				new DateTime(2025, 9, 20, 13, 0, 0),
				new DateTime(2025, 9, 20, 14, 0, 0),
				new DateTime(2025, 9, 20, 15, 0, 0),
				new DateTime(2025, 9, 20, 16, 0, 0),
				new DateTime(2025, 9, 20, 17, 0, 0),
				new DateTime(2025, 9, 20, 18, 0, 0),
				new DateTime(2025, 9, 20, 19, 0, 0),
			};
			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}

		[TestMethod()]
		public void ParseDailyTaskStartBeforeEnd_EarlyRangeTest()
		{
			var taskStart = new DateTime(2025, 9, 20, 10, 0, 0);
			var taskEnd = new DateTime(2025, 9, 27, 20, 0, 0);
			var rangeStart = new DateTime(2025, 9, 17, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 21, 0, 0, 0);
			var repeatInterval = "60"; // every 60 minutes
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 20, 10, 0, 0),
				new DateTime(2025, 9, 20, 11, 0, 0),
				new DateTime(2025, 9, 20, 12, 0, 0),
				new DateTime(2025, 9, 20, 13, 0, 0),
				new DateTime(2025, 9, 20, 14, 0, 0),
				new DateTime(2025, 9, 20, 15, 0, 0),
				new DateTime(2025, 9, 20, 16, 0, 0),
				new DateTime(2025, 9, 20, 17, 0, 0),
				new DateTime(2025, 9, 20, 18, 0, 0),
				new DateTime(2025, 9, 20, 19, 0, 0),
			};
			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}

		[TestMethod()]
		public void ParseDailyTaskStartBeforeEnd_LateRangeTest()
		{
			var taskStart = new DateTime(2025, 9, 20, 10, 0, 0);
			var taskEnd = new DateTime(2025, 9, 27, 20, 0, 0);
			var rangeStart = new DateTime(2025, 9, 27, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 30, 0, 0, 0);
			var repeatInterval = "60"; // every 60 minutes
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 27, 10, 0, 0),
				new DateTime(2025, 9, 27, 11, 0, 0),
				new DateTime(2025, 9, 27, 12, 0, 0),
				new DateTime(2025, 9, 27, 13, 0, 0),
				new DateTime(2025, 9, 27, 14, 0, 0),
				new DateTime(2025, 9, 27, 15, 0, 0),
				new DateTime(2025, 9, 27, 16, 0, 0),
				new DateTime(2025, 9, 27, 17, 0, 0),
				new DateTime(2025, 9, 27, 18, 0, 0),
				new DateTime(2025, 9, 27, 19, 0, 0),
			};

			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}

		[TestMethod()]
		public void ParseDailyTaskEndBeforeStart_LateRangeTest()
		{
			var taskStart = new DateTime(2025, 9, 20, 13, 0, 0);
			var taskEnd = new DateTime(2025, 9, 27, 6, 0, 0);
			var rangeStart = new DateTime(2025, 9, 27, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 30, 0, 0, 0);
			var repeatInterval = "60"; // every 60 minutes
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 27, 0, 0, 0),
				new DateTime(2025, 9, 27, 1, 0, 0),
				new DateTime(2025, 9, 27, 2, 0, 0),
				new DateTime(2025, 9, 27, 3, 0, 0),
				new DateTime(2025, 9, 27, 4, 0, 0),
				new DateTime(2025, 9, 27, 5, 0, 0),
				new DateTime(2025, 9, 27, 13, 0, 0),
				new DateTime(2025, 9, 27, 14, 0, 0),
				new DateTime(2025, 9, 27, 15, 0, 0),
				new DateTime(2025, 9, 27, 16, 0, 0),
				new DateTime(2025, 9, 27, 17, 0, 0),
				new DateTime(2025, 9, 27, 18, 0, 0),
				new DateTime(2025, 9, 27, 19, 0, 0),
				new DateTime(2025, 9, 27, 20, 0, 0),
				new DateTime(2025, 9, 27, 21, 0, 0),
				new DateTime(2025, 9, 27, 22, 0, 0),
				new DateTime(2025, 9, 27, 23, 0, 0),
				new DateTime(2025, 9, 28, 0, 0, 0),
				new DateTime(2025, 9, 28, 1, 0, 0),
				new DateTime(2025, 9, 28, 2, 0, 0),
				new DateTime(2025, 9, 28, 3, 0, 0),
				new DateTime(2025, 9, 28, 4, 0, 0),
				new DateTime(2025, 9, 28, 5, 0, 0),
			};

			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}

		[TestMethod]
		public void ParseDailyTask_WithoutEndDateTest()
		{
			var taskStart = new DateTime(2025, 9, 17, 10, 0, 0);
			var taskEnd = new DateTime(1, 1 ,1, 20, 0 ,0);
			var rangeStart = new DateTime(2025, 9, 20, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 21, 0, 0, 0);
			var repeatInterval = "60"; // every 60 minutes
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 20, 10, 0, 0),
				new DateTime(2025, 9, 20, 11, 0, 0),
				new DateTime(2025, 9, 20, 12, 0, 0),
				new DateTime(2025, 9, 20, 13, 0, 0),
				new DateTime(2025, 9, 20, 14, 0, 0),
				new DateTime(2025, 9, 20, 15, 0, 0),
				new DateTime(2025, 9, 20, 16, 0, 0),
				new DateTime(2025, 9, 20, 17, 0, 0),
				new DateTime(2025, 9, 20, 18, 0, 0),
				new DateTime(2025, 9, 20, 19, 0, 0),
			};

			Assert.AreEqual(10, occurrences.Count);
			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}

		[TestMethod]
		public void ParseDailyTask_WithEndDateTimeBeforeStartTimeTest()
		{
			var taskStart = new DateTime(2025, 9, 17, 20, 0, 0);
			var taskEnd = new DateTime(2025, 9, 19, 06, 0, 0);
			var rangeStart = new DateTime(2025, 9, 18, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 21, 0, 0, 0);
			var repeatInterval = "60"; // every 60 minutes
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 18, 0, 0, 0),
				new DateTime(2025, 9, 18, 1, 0, 0),
				new DateTime(2025, 9, 18, 2, 0, 0),
				new DateTime(2025, 9, 18, 3, 0, 0),
				new DateTime(2025, 9, 18, 4, 0, 0),
				new DateTime(2025, 9, 18, 5, 0, 0),
				new DateTime(2025, 9, 18, 20, 0, 0),
				new DateTime(2025, 9, 18, 21, 0, 0),
				new DateTime(2025, 9, 18, 22, 0, 0),
				new DateTime(2025, 9, 18, 23, 0, 0),
				new DateTime(2025, 9, 19, 0, 0, 0),
				new DateTime(2025, 9, 19, 1, 0, 0),
				new DateTime(2025, 9, 19, 2, 0, 0),
				new DateTime(2025, 9, 19, 3, 0, 0),
				new DateTime(2025, 9, 19, 4, 0, 0),
				new DateTime(2025, 9, 19, 5, 0, 0),
				new DateTime(2025, 9, 19, 20, 0, 0),
				new DateTime(2025, 9, 19, 21, 0, 0),
				new DateTime(2025, 9, 19, 22, 0, 0),
				new DateTime(2025, 9, 19, 23, 0, 0),
				new DateTime(2025, 9, 20, 0, 0, 0),
				new DateTime(2025, 9, 20, 1, 0, 0),
				new DateTime(2025, 9, 20, 2, 0, 0),
				new DateTime(2025, 9, 20, 3, 0, 0),
				new DateTime(2025, 9, 20, 4, 0, 0),
				new DateTime(2025, 9, 20, 5, 0, 0),
			};

			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}

		[TestMethod]
		public void ParseDailyTaskStartBeforeEnd_RepeatIntervalZeroTest()
		{
			var taskStart = new DateTime(2025, 9, 20, 10, 0, 0);
			var taskEnd = new DateTime(2025, 9, 27, 20, 0, 0);
			var rangeStart = new DateTime(2025, 9, 17, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 21, 0, 0, 0);
			var repeatInterval = "0"; // Only once a day
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);

			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 20, 10, 0, 0),
			};

			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}

		[TestMethod]
		public void ParseDailyRangeStartBeforeEnd_RepeatIntervalZeroTest()
		{
			var taskStart = new DateTime(2025, 9, 16, 10, 0, 0);
			var taskEnd = new DateTime(2025, 9, 27, 20, 0, 0);
			var rangeStart = new DateTime(2025, 9, 17, 0, 0, 0);
			var rangeEnd = new DateTime(2025, 9, 21, 0, 0, 0);
			var repeatInterval = "0"; // Only once a day
			var occurrences = TimeIntervalParser.ParseDailyTask(repeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);

			var expectedOccurrences = new List<DateTime>
			{
				new DateTime(2025, 9, 17, 10, 0, 0),
			};

			CollectionAssert.AreEqual(expectedOccurrences, occurrences);
		}
	}
}