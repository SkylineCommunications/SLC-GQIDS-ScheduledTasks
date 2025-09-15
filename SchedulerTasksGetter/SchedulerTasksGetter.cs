namespace SchedulerTasksGetter
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;

	using SchedulerTask = Skyline.DataMiner.Net.Messages.SchedulerTask;

	/// <summary> Represents a data source. See: https://aka.dataminer.services/gqi-external-data-source for a complete example. </summary>
	[GQIMetaData(Name = "SLC - Get Scheduled Tasks")]
	public sealed class SchedulerTasksGetter : IGQIDataSource, IGQIOnPrepareFetch, IGQIOnInit, IGQIInputArguments
	{
		private readonly GQIStringArgument nameFilterArgument = new GQIStringArgument("Name Filter") { IsRequired = false, DefaultValue = ".*" };
		private readonly List<SchedulerTask> scheduledTasks = new List<SchedulerTask>();
		private GQIDMS dms;

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
			return new OnArgumentsProcessedOutputArgs();
		}

		public OnPrepareFetchOutputArgs OnPrepareFetch(OnPrepareFetchInputArgs args)
		{
			var tasks = GetTasks(task => Regex.IsMatch(task.TaskName, nameFilter, RegexOptions.IgnoreCase));
			scheduledTasks.AddRange(tasks);
			return new OnPrepareFetchOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			var columns = new List<GQIColumn>
			{
				new GQIStringColumn("Name"),
				new GQIStringColumn("Description"),
				new GQIStringColumn("Type"),
				new GQIStringColumn("DataMiner"),
				new GQIStringColumn("Interval"),
				new GQIStringColumn("Last Result"),
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
					new GQICell { Value = IntervalBuilder.BuildIntervalString(task)},
					new GQICell { Value = task.LastExecuteResult},
				};

				rows.Add(new GQIRow(cells.ToArray()));
			}

			return rows;
		}

		private IEnumerable<SchedulerTask> GetTasks(Func<SchedulerTask, bool> selector)
		{
			var dmaId = GetDmaInfo();
			var tasksList = dms.SendMessages(new GetInfoMessage{ Type = InfoType.SchedulerTasks }).OfType<GetSchedulerTasksResponseMessage>().FirstOrDefault();
			if (tasksList?.Tasks == null)
				yield break;

			var additionalSchedulerInfo = (GetSchedulerInfoResponseMessage)dms.SendMessage(new GetSchedulerInfoMessage(13, dmaId));
			var taskDetailsLookup = additionalSchedulerInfo.psaRet.Psa.Where(t => t.Sa.Count() > 3).GroupBy(t => t.Sa[0]).ToDictionary(g => g.Key, g => g.First().Sa[2]);

			foreach (var scheduleTask in tasksList.Tasks.OfType<SchedulerTask>().Where(selector))
			{
				scheduleTask.LastExecuteResult = taskDetailsLookup.TryGetValue(scheduleTask.TaskName, out var result) ? result : "N/A";
				yield return scheduleTask;
			}
		}

		private int GetDmaInfo()
		{
			GetInfoMessage getInfoMessage = new GetInfoMessage
			{
				Type = InfoType.DataMinerInfo,
			};

			var info = (GetDataMinerInfoResponseMessage)dms.SendMessage(getInfoMessage);
			return info.ID;
		}
	}
}
