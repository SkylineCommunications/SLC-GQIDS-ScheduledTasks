namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;

	using SchedulerTask = Skyline.DataMiner.Net.Messages.SchedulerTask;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "GetScheduledTasksGQI")]
	public class GetScheduledTasksGQI : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		private readonly Arguments arguments = new Arguments();
		private List<GQIRow> rows = new List<GQIRow>();
		private List<SchedulerTask> scheduledTasks = new List<SchedulerTask>();
		private GQIDMS dms;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			dms = args.DMS;
			return default;
		}

		public GQIArgument[] GetInputArguments()
		{
			return arguments.GetArguments();
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			arguments.ProcessArguments(args);

			string userInput = arguments.NameFilter ?? string.Empty;
			string regexPattern;
			string escapedInput = Regex.Escape(userInput);

			regexPattern = escapedInput.Replace("\\*", ".*"); // supporting * as wildcard
			var tasks = GetTasks(task => Regex.IsMatch(task.TaskName, regexPattern));

			scheduledTasks.AddRange(tasks);

			return new OnArgumentsProcessedOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			var columns = new List<GQIColumn>
			{
				new GQIDateTimeColumn("Start"),
				new GQIDateTimeColumn("End"),
				new GQIStringColumn("Name"),
				new GQIStringColumn("Description"),
				new GQIStringColumn("Type"),
				new GQIStringColumn("DataMiner"),
			};
			return columns.ToArray();
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			if (scheduledTasks.IsNotNullOrEmpty())
			{
				ProcessScheduledTasks();
			}

			return new GQIPage(rows.ToArray()) { HasNextPage = false };
		}

		private IEnumerable<SchedulerTask> GetTasks(Func<Skyline.DataMiner.Net.Messages.SchedulerTask, bool> selector)
		{
			var result = new List<Skyline.DataMiner.Net.Messages.SchedulerTask>();
			GetInfoMessage getInfoMessage = new GetInfoMessage
			{
				Type = InfoType.SchedulerTasks,
			};

			var schedulerInfo = dms.SendMessages(getInfoMessage).OfType<GetSchedulerTasksResponseMessage>();

			var tasksList = schedulerInfo.FirstOrDefault();

			if (tasksList?.Tasks != null)
			{
				foreach (var task in tasksList.Tasks)
				{
					if (task is Skyline.DataMiner.Net.Messages.SchedulerTask scheduleTask && selector(scheduleTask))
					{
						result.Add(scheduleTask);
					}
				}
			}

			return result;
		}

		private void ProcessScheduledTasks()
		{
			DateTime rangeStart = arguments.Start;
			DateTime rangeEnd = arguments.End;

			foreach (var task in scheduledTasks)
			{
				DateTime taskStart = DateTime.SpecifyKind(task.StartTime, DateTimeKind.Utc);

				// Handle missing end time (ongoing tasks)
				DateTime taskEnd = task.EndTime == DateTime.MinValue ? DateTime.MaxValue : DateTime.SpecifyKind(task.EndTime, DateTimeKind.Utc);

				switch (task.RepeatType)
				{
					case SchedulerRepeatType.Once:
						if (taskStart >= rangeStart && taskStart <= rangeEnd)
						{
							AddRow(task, taskStart);
						}

						break;

					case SchedulerRepeatType.Daily:
						AddRepeatingTasks(task, taskStart, taskEnd, rangeStart, rangeEnd, TimeSpan.FromDays(1));
						break;

					case SchedulerRepeatType.Weekly:
						AddRepeatingTasks(task, taskStart, taskEnd, rangeStart, rangeEnd, TimeSpan.FromDays(7));
						break;

					case SchedulerRepeatType.Monthly:
						AddMonthlyRepeatingTasks(task, taskStart, taskEnd, rangeStart, rangeEnd);
						break;
					case SchedulerRepeatType.Undefined:
						break;
					default:
						// do nothing
						break;
				}
			}
		}

		private void AddRepeatingTasks(SchedulerTask task, DateTime taskStart, DateTime taskEnd, DateTime rangeStart, DateTime rangeEnd, TimeSpan interval)
		{
			// Find the first valid occurrence in the given range
			DateTime occurrence = taskStart;

			while (occurrence < rangeStart)
			{
				occurrence = occurrence.Add(interval);
			}

			while (occurrence <= rangeEnd && occurrence <= taskEnd)
			{
				AddRow(task, occurrence);
				occurrence = occurrence.Add(interval);
			}
		}

		private void AddMonthlyRepeatingTasks(SchedulerTask task, DateTime taskStart, DateTime taskEnd, DateTime rangeStart, DateTime rangeEnd)
		{
			DateTime occurrence = taskStart;

			while (occurrence < rangeStart)
			{
				occurrence = occurrence.AddMonths(1);
			}

			while (occurrence <= rangeEnd && occurrence <= taskEnd)
			{
				AddRow(task, occurrence);
				occurrence = occurrence.AddMonths(1);
			}
		}

		private void AddRow(SchedulerTask task, DateTime occurrenceTime)
		{
			rows.Add(new GQIRow(new[]
			{
				new GQICell { Value = DateTime.SpecifyKind(occurrenceTime ,DateTimeKind.Utc) },
				new GQICell { Value = DateTime.SpecifyKind(occurrenceTime.AddSeconds(arguments.Duration) ,DateTimeKind.Utc) },
				new GQICell { Value = task.TaskName },
				new GQICell { Value = task.Description },
				new GQICell { Value = task.RepeatType.ToString() },
				new GQICell { Value = task.HandlingDMA.ToString() },
			}));
		}
	}
}
