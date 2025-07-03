namespace GetScheduledTasksGQI
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Analytics.GenericInterface;

	public class Arguments
	{
		private readonly GQIStringArgument nameFilter = new GQIStringArgument("Name Filter") { IsRequired = false, DefaultValue = ".*" };
		private readonly GQIDateTimeArgument start = new GQIDateTimeArgument("Start") { IsRequired = true };
		private readonly GQIDateTimeArgument end = new GQIDateTimeArgument("End") { IsRequired = true };
		private readonly GQIIntArgument duration = new GQIIntArgument("Scheduled Duration (s)") { IsRequired = true };
		private readonly GQIStringArgument scriptParameterInputs = new GQIStringArgument("Script Parameter Inputs") { IsRequired = false };

		public string NameFilter { get; private set; }

		public DateTime Start { get; private set; }

		public DateTime End { get; private set; }

		public int Duration { get; private set; }

		public List<ScriptRunData> ScriptParameterInputs { get; } = new List<ScriptRunData>();

		public void ProcessArguments(OnArgumentsProcessedInputArgs args)
		{
			NameFilter = args.GetArgumentValue(nameFilter);
			Start = args.GetArgumentValue(start);
			End = args.GetArgumentValue(end);
			Duration = args.GetArgumentValue(duration);
			var scriptRunData = ParseScriptData(args.GetArgumentValue(scriptParameterInputs));
			ScriptParameterInputs.AddRange(scriptRunData);
		}

		public List<ScriptRunData> ParseScriptData(string input)
		{
			var scriptDates = new List<ScriptRunData>();

			string[] items = input.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var item in items)
			{
				string[] parts = item.Split('.');
				if (parts.Length == 2)
				{
					// Try to parse the second part as an integer
					if (int.TryParse(parts[1], out int id))
					{
						scriptDates.Add(new ScriptRunData
						{
							scriptName = parts[0],
							scriptParameterID = id,
						});
					}

				}
			}

			return scriptDates;
		}

		internal GQIArgument[] GetArguments()
		{
			return new GQIArgument[] { nameFilter, start, end, duration, scriptParameterInputs };
		}

	}
}
