namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using global::GetScheduledTasksGQI.DataHelpers;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;

	using SchedulerTask = Skyline.DataMiner.Net.Messages.SchedulerTask;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "Get Scheduled Tasks")]
	public class GetScheduledTasksGQI : IGQIDataSource, IGQIInputArguments, IGQIOnInit, IGQIOnPrepareFetch
	{
		private readonly Arguments arguments = new Arguments();
		private readonly List<GQIRow> rows = new List<GQIRow>();
		private readonly List<SchedulerTask> scheduledTasks = new List<SchedulerTask>();
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
			var tasks = GetTasks(task => Regex.IsMatch(task.TaskName, arguments.NameFilter, RegexOptions.IgnoreCase) && task.Enabled);
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

			foreach (var scriptData in arguments.ScriptParameterInputs)
			{
				columns.Add(new GQIStringColumn($"{scriptData.scriptName}.{scriptData.scriptParameterID}"));
			}

			return columns.ToArray();
		}
		public OnPrepareFetchOutputArgs OnPrepareFetch(OnPrepareFetchInputArgs args)
		{
			throw new NotImplementedException();
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
				// DM returns scheduler Task info in local Timestamps
				var taskStart = DateTime.SpecifyKind(task.StartTime, DateTimeKind.Local);
				var taskEnd = task.EndTime == DateTime.MinValue ? DateTime.MaxValue : DateTime.SpecifyKind(task.EndTime, DateTimeKind.Local);

				List<DateTime> occurrences = new List<DateTime>();
				if (!string.IsNullOrEmpty(task.RepeatInterval))
				{
					switch (task.RepeatType)
					{
						case SchedulerRepeatType.Monthly:
							occurrences = TimeIntervalParser.ParseMonthlyTask(task.RepeatInterval, task.RepeatIntervalInMinutes, rangeStart, rangeEnd, taskStart, taskEnd);
							break;

						case SchedulerRepeatType.Weekly:
							occurrences = TimeIntervalParser.ParseWeeklyTask(task.RepeatInterval, task.RepeatIntervalInMinutes, rangeStart, rangeEnd, taskStart, taskEnd);
							break;

						case SchedulerRepeatType.Daily:
							occurrences = TimeIntervalParser.ParseDailyTask(task.RepeatInterval, rangeStart, rangeEnd, taskStart, taskEnd);
							break;
						case SchedulerRepeatType.Once:
							if (taskStart >= rangeStart && taskStart <= rangeEnd)
							{
								occurrences.Add(taskStart);
							}

							break;
						default:
							// do nothing
							break;
					}
				}

				if (task.Repeat > 0)
				{
					occurrences = occurrences.Take(task.Repeat).ToList();
				}

				foreach (var occurrence in occurrences)
				{
					AddRow(task, occurrence);
				}
			}
		}

		private void AddRow(SchedulerTask task, DateTime occurrenceTime)
		{
			var cells = new List<GQICell>
			{
				new GQICell { Value = DateTime.SpecifyKind(occurrenceTime, DateTimeKind.Utc) },
				new GQICell { Value = DateTime.SpecifyKind(occurrenceTime.AddSeconds(arguments.Duration), DateTimeKind.Utc)},
				new GQICell { Value = task.TaskName },
				new GQICell { Value = task.Description },
				new GQICell { Value = task.RepeatType.ToString() },
				new GQICell { Value = task.HandlingDMA.ToString() },
			};
			cells.AddRange(AddScriptCellsToRow(task));
			rows.Add(new GQIRow(cells.ToArray()));
		}

		private List<GQICell> AddScriptCellsToRow(SchedulerTask task)
		{
			var additionalCells = new List<GQICell>();
			string cellValue = String.Empty;

			foreach (var scriptRun in arguments.ScriptParameterInputs)
			{
				if (task.Actions != null)
				{
					foreach (var action in task.Actions)
					{
						var script = action.ScriptInstance;
						if (script == null || script.ParameterIdToValue == null || script.ParameterIdToValue.Count == 0 || !string.Equals(script.ScriptName, scriptRun.scriptName, StringComparison.OrdinalIgnoreCase))
						{
							cellValue = String.Empty;
							continue;
						}

						var automationInfo = script.ParameterIdToValue.OfType<AutomationScriptInstanceInfo>().FirstOrDefault(info => info.Key == scriptRun.scriptParameterID);

						if (automationInfo != null)
						{
							cellValue = automationInfo.Value;
							break;
						}
					}
				}

				additionalCells.Add(new GQICell { Value = cellValue });
			}

			return additionalCells;
		}
	}
}
