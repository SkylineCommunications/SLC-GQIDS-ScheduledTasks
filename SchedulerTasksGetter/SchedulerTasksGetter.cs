namespace SchedulerTasksGetter
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC - GQI - Scheduled - Tasks")]
	public sealed class SchedulerTasksGetter : IGQIDataSource
		, IGQIOnInit
		, IGQIInputArguments
	{
		private GQIDMS dms;
		private GQIStringArgument nameFilterArgument = new GQIStringArgument("Name Filter") { IsRequired = false, DefaultValue = ".*" };
		private List<SchedulerTask> scheduledTasks = new List<SchedulerTask>();
		private string nameFilter;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			dms = args.DMS;
			return default;
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { nameFilterArgument };
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			nameFilter = args.GetArgumentValue(nameFilterArgument);
			var tasks = GetTasks(task => Regex.IsMatch(task.TaskName, nameFilter, RegexOptions.IgnoreCase));
			scheduledTasks.AddRange(tasks);
			return new OnArgumentsProcessedOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			var columns = new List<GQIColumn>
			{
				new GQIStringColumn("Name"),
				new GQIStringColumn("Description"),
				new GQIStringColumn("Type"),
				new GQIStringColumn("DataMiner"),
			};
			return columns.ToArray();
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			var rows = ProcessRows();
			return new GQIPage(rows.ToArray());
		}

		private List<GQIRow> ProcessRows()
		{
			var rows = new List<GQIRow>();
			foreach (var task in scheduledTasks)
			{
				var cells = new List<GQICell>
				{
					new GQICell { Value = task.TaskName },
					new GQICell { Value = task.Description },
					new GQICell { Value = task.RepeatType.ToString() },
					new GQICell { Value = task.HandlingDMA.ToString() },
				};

				rows.Add(new GQIRow(cells.ToArray()));
			}

			return rows;
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
	}
}
