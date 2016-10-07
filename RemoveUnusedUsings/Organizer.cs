using System;
using EnvDTE;

namespace RemoveUnusedUsings
{
    public class Organizer : IOrganizer
    {
        private readonly Logger logger;

        public Organizer(Logger logger)
        {
            this.logger = logger;
        }

        public void OrganizeSolution(string solutionPath)
        {
            var type = Type.GetTypeFromProgID("VisualStudio.DTE.14.0", true);
            var dte = (DTE)Activator.CreateInstance(type, true);

            MessageFilter.Register();

            logger.Log($"Working with {dte.FullName}, {dte.Edition} edition");
            dte.SuppressUI = true;
            dte.UserControl = false;

            logger.Log($"Open {solutionPath}");
            dte.Solution.Open(solutionPath);
            for (var i = 1; i <= dte.Solution.Projects.Count; i++)
            {
                OrganizeProjectItems(dte.Solution.Projects.Item(i).ProjectItems);
            }
            dte.Solution.Close(true);

            dte.Quit();

            MessageFilter.Revoke();
        }

        private void OrganizeProjectItems(ProjectItems projectItems)
        {
            if (projectItems == null)
            {
                return;
            }

            for (var i = 1; i <= ComAttempter.Try(() => projectItems.Count); ++i)
            {
                OrganizeProjectItem(ComAttempter.Try(() => projectItems.Item(i)));
            }
        }

        private void OrganizeProjectItem(ProjectItem projectItem)
        {
            if (projectItem.Kind == Constants.vsProjectItemKindPhysicalFile)
            {
                if (projectItem.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    RemoveAndSort(projectItem);
                }
            }

            OrganizeProjectItems(projectItem.ProjectItems);

            if (projectItem.SubProject != null)
            {
                OrganizeProjectItems(projectItem.SubProject.ProjectItems);
            }
        }

        private void RemoveAndSort(ProjectItem projectItem)
        {
            logger.Log($"Try RemoveAndSort {projectItem.Name}");
            var fileWasOpen = projectItem.IsOpen;
            var window = projectItem.Open(Constants.vsViewKindCode);
            window.Activate();
            projectItem.Document.DTE.ExecuteCommand("Edit.RemoveAndSort");
            if (!fileWasOpen)
            {
                window.Close(vsSaveChanges.vsSaveChangesYes);
            }
        }
    }
}
