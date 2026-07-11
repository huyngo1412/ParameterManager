using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Reflection;

namespace ParameterManager
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                CreateAddInsPanel(application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show(
                    "Parameter Manager – Startup Error",
                    ex.ToString());

                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private static void CreateAddInsPanel(
            UIControlledApplication application)
        {
            const string panelName = "Parameter Manager";

            RibbonPanel panel = application
                .GetRibbonPanels()
                .FirstOrDefault(x => x.Name == panelName);

            if (panel == null)
            {
                panel = application.CreateRibbonPanel(panelName);
            }

            string assemblyPath =
                Assembly.GetExecutingAssembly().Location;

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