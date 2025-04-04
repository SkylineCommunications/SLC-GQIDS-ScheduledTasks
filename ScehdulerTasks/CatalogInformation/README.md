## About
Package is consisted of two ad hoc data sources that are retrieving information about scheduled tasks in DataMiner System. The ad hoc data sources enable user to visualize data from Scheduler module in a custom way by implementing these ad hoc data sources in LCA and/or dashboards.
The **SLC - GQI - Scheduled - Tasks** provides list of scheduled tasks from Scheduler module of DataMiner. While **SLC - GQI - Scheduled - Tasks - Occurrences** provides the list of scheduled task occurrences based on user's input in specific period of time. In this way the customizable overview of tasks is provided.

## Key Features
The data sources offer a flexible framework for retrieving data about scheduled tasks by incorporating several features:

- **Regex Filtering:** Users can specify a **regex** pattern to match task names, allowing for highly customizable filtering. This feature is particularly useful when managing large numbers of tasks, as it helps narrow down the data to only those tasks that meet specific naming criteria.
- **Time Frame Specification:** With the ability to define a **time frame**, users can control the period over which task occurrences are displayed.
- **Duration Settings:** The option to set the **duration** of displayed tasks enhances timeline visualizations. 
- **Custom Integration:** These features support seamless integration into custom dashboards and LCA modules, allowing users to create tailored visualizations that align with specific operational needs.
- **Input Data Overview** There is possibility to see specified input data of scripts executed within the tasks.

## Use Cases 

The versatility of the package lends itself to a broad range of applications, including but not limited to:

- **Monitoring** The ability to retrieve scheduled tasks across various agents makes it easier to monitor. Users could use the data to set up dashboards that visualize task execution.
- **Troubleshooting and Diagnostics:** When issues arise, the ability to drill down into specific task occurrences using time frames and regex filtering can help pinpoint the root cause of performance issues or scheduling conflicts.

## Configuration 

Upon deploying the package the user will find two ad hoc data sources **SLC - GQI - Scheduled - Tasks**  and  **SLC - GQI - Scheduled - Tasks - Occurrences** made to be used as GQI. More about GQI can be found in DataMiner Docs.
See : [Generic Query Interface](https://docs.dataminer.services/user-guide/Advanced_Modules/Dashboards_and_Low_Code_Apps/GQI/About_GQI.html).

While implementing the **SLC - GQI - Scheduled - Tasks** user should specify following input parameters:
- Name Filter - regex pattern used to match task name (optional - by default will use .*)

As a result the GQI will retrieve name, description and type of the task as well as name of the agent on which the task was executed.


While implementing the **SLC - GQI - Scheduled - Tasks - Occurrences**  user should specify following input parameters:
- **Name Filter:** A regex pattern to match the task name. (Optional; defaults to `.*` if not specified.)
- **Start:** The beginning of the time frame during which task occurrences are recorded.
- **End:** The end of the time frame during which task occurrences are recorded.
- **Duration (s):** The duration value used to represent the length of each task occurrence.
- **Script Parameter Inputs:** Input values should be provided in the format `[ScriptName.InputParameterId]`. (Optional; by default, no values are retrieved.) Note that for tasks that do not include the specified script, an empty value will be returned.

As a result the GQI will retrieve start and end time, name, description and type of the task as well as name of the agent on which the task was executed. If specified values of input parameters of specific executed scripts will be retrieved as well.

> [!IMPORTANT]
> Duration of tasks does not reflect the actual time that task took to execute, it only enables task occurrence to be visualized in the specified number of seconds.

> [!NOTE]
> Only active scheduled tasks will be retrieved.
