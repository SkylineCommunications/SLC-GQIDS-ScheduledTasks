namespace GetScheduledTasksGQI
{
	using System;
	using Skyline.DataMiner.Analytics.GenericInterface;

	public class Arguments
	{
		private readonly GQIStringArgument nameFilter = new GQIStringArgument("Name Filter") { IsRequired = false, DefaultValue = ".*" };
		private readonly GQIDateTimeArgument start = new GQIDateTimeArgument("Start") { IsRequired = true };
		private readonly GQIDateTimeArgument end = new GQIDateTimeArgument("End") { IsRequired = true };
		private readonly GQIIntArgument duration = new GQIIntArgument("Duration (s)") { IsRequired = true };

		public string NameFilter { get; private set; }

		public DateTime Start { get; private set; }

		public DateTime End { get; private set; }

		public int Duration { get; private set; }

		internal GQIArgument[] GetArguments()
		{
			return new GQIArgument[] { nameFilter, start, end, duration };
		}

		public void ProcessArguments(OnArgumentsProcessedInputArgs args)
		{
			NameFilter = args.GetArgumentValue(nameFilter);
			Start = args.GetArgumentValue(start);
			End =args.GetArgumentValue(end);
			Duration = args.GetArgumentValue(duration);
		}
	}
}
