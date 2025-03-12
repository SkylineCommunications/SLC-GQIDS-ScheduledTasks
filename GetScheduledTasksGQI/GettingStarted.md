# Getting Started with Skyline DataMiner DevOps

Welcome to the Skyline DataMiner DevOps environment!  
This quick-start guide will help you get up and running. It was auto-generated based on the initial project setup during creation.  
For more details and comprehensive instructions, please visit [DataMiner Docs](https://docs.dataminer.services/).

## Creating a DataMiner Application Package

This project was configured to create a `.dmapp` file every time you build the project.  
When you compile or build the project, you will find the generated `.dmapp` in the standard output folder, typically the `bin` folder of your project.

When you publish the project (see Publishing topic below), a corresponding item will be created in the online DataMiner Catalog.

## Migrating to a Multi-Artifact DataMiner Application Package

If you need to combine additional components in your `.dmapp` file, you should:

1. Open the `GetScheduledTasksGQI.csproj` file and ensure the `<GenerateDataminerPackage>` property is set to `False`.

2. Right-click your solution and select **Add > New Project**.

3. Select the **Skyline DataMiner Package Project** template.

Follow the provided **Getting Started** guide in the new project for further instructions.

## Publishing to the Catalog

This project was created with support for publishing to the DataMiner Catalog.  
You can publish your artifact either manually via the Visual Studio IDE or by setting up a CI/CD workflow.

### Manual Publishing

1. Obtain an **Organization Key** from [admin.dataminer.services](https://admin.dataminer.services/) with the following scopes:
   - **Register catalog items**
   - **Read catalog items**
   - **Download catalog versions**

1. Securely store the key using Visual Studio User Secrets:

   1. Right-click the project and select **Manage User Secrets**.

   1. Add the key in the following format:

      ```json
      {
        "skyline": {
          "sdk": {
            "dataminertoken": "MyKeyHere"
          }
        }
      }
      ```

1. Publish the package by right-clicking your project in Visual Studio and then selecting the **Publish** option.

   This will open a new window, where you will find a *Publish* button and a link where your item will eventually be registered.

**Recommendation:** To safeguard the quality of your product, consider using a CI/CD setup to run **dotnet publish** only after passing quality checks.

### Changing the Version

1. Navigate to your project in Visual Studio, right-click, and select Properties.

1. Search for Package Version.

1. Adjust the value as needed.

### Changing the Version - Alternative

1. Navigate to your project in Visual Studio and double-click it.

1. Adjust the "Version" XML tag to the version you want to register.

   ```xml
   <Version>1.0.1</Version>
   ```
