namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Automation;

	using Skyline.DataMiner.Net.Messages;

	using SchedulerTask = Skyline.DataMiner.Net.Messages.SchedulerTask;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "GetScheduledTasksGQI")]
	public sealed class GetScheduledTasksGQI : IGQIDataSource, IGQIInputArguments
	{
		private readonly Arguments arguments = new Arguments();
		List<GQIColumn> columns = new List<GQIColumn>();
		List<GQIRow> rows = new List<GQIRow>();

		private List<SchedulerTask> scheduledTasks = new List<SchedulerTask>();

		public GQIArgument[] GetInputArguments()
		{
			return arguments.GetArguments();
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			arguments.ProcessArguments(args);
			var tasks = GetTasks(task => Regex.IsMatch(task.TaskName, arguments.NameFilter) && task.StartTime > arguments.Start && task.EndTime < arguments.End);
			scheduledTasks.AddRange(tasks);
			InitializeRows();
			return new OnArgumentsProcessedOutputArgs();
		}
		private IEnumerable<SchedulerTask> GetTasks(Func<Skyline.DataMiner.Net.Messages.SchedulerTask, bool> selector)
		{
			GetInfoMessage getInfoMessage = new GetInfoMessage
			{
				Type = InfoType.SchedulerTasks
			};
			if (!(Engine.SLNet.SendSingleResponseMessage(getInfoMessage) is GetSchedulerTasksResponseMessage schedulerInfo))
			{
				throw new InvalidOperationException("FAILED: Sending GetInfoMessage: " + JsonConvert.SerializeObject(getInfoMessage));
			}

			foreach (object task in schedulerInfo.Tasks)
			{
				Skyline.DataMiner.Net.Messages.SchedulerTask scheduleTask = task as Skyline.DataMiner.Net.Messages.SchedulerTask;
				if (selector(scheduleTask))
				{
					yield return scheduleTask;
				}
			}
		}

		public GQIColumn[] GetColumns()
		{
			columns = new List<GQIColumn>
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
			return new GQIPage(rows.ToArray())
			{
				HasNextPage = false,
			};
		}

		private void InitializeRows()
		{
			foreach (var task in scheduledTasks)
			{
				rows.Add(new GQIRow(GetTaskRow(task).ToArray()));
			}
		}

		private List<GQICell> GetTaskRow(SchedulerTask task)
		{
			var gqiCells = new List<GQICell>();
			foreach (var column in columns)
			{
				if (ColumnMappings.columnNameToTask.TryGetValue(column.Name, out var valueExtractor))
				{
					gqiCells.Add(new GQICell { Value = valueExtractor(task) });
				}
			}

			return gqiCells;
		}
	}
}
