using Autodesk.Revit.UI;
using System.Linq;
using System.Reflection;

namespace ParameterManager
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            CreateAddInsPanel(application);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void CreateAddInsPanel(UIControlledApplication application)
        {
            const string panelName = "Parameter Manager";

            RibbonPanel panel = application
                .GetRibbonPanels("Add-Ins")
                .FirstOrDefault(x => x.Name == panelName);

            if (panel == null)
            {
                panel = application.CreateRibbonPanel(panelName);
            }

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                "OpenParameterManager",
                "Parameter\nManager",
                assemblyPath,
                "ParameterManager.ExternalCommand");

            buttonData.ToolTip = "Open Parameter Manager";
            buttonData.LongDescription =
                "Browse categories, families, types and assign editable type/instance parameters.";

            panel.AddItem(buttonData);
        }
    }
}
