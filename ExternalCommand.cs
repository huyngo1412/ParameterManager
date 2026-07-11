using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ParameterManager.Contracts;
using ParameterManager.Services;
using ParameterManager.Services.ParameterSetters;
using ParameterManager.ViewModels;
using ParameterManager.Views;
using System;
using System.Windows.Interop;

namespace ParameterManager
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Result Execute(
    ExternalCommandData commandData,
    ref string message,
    ElementSet elements)
        {
            try
            {
                UIDocument uidoc =
                    commandData.Application.ActiveUIDocument;

                if (uidoc == null)
                {
                    TaskDialog.Show(
                        "Parameter Manager",
                        "Please open a Revit document first.");

                    return Result.Cancelled;
                }

                Document doc = uidoc.Document;

                IFamilyTypeRepository familyTypeRepository =
                    new RevitFamilyTypeRepository(doc);

                IElementOptionRepository elementOptionRepository =
                    new RevitElementOptionRepository(doc);

                ISelectionService selectionService =
                    new RevitSelectionService(uidoc);

                IParameterValueSetterFactory valueSetterFactory =
                    new ParameterValueSetterFactory();

                IParameterService parameterService =
                    new RevitParameterService(
                        doc,
                        valueSetterFactory);

                MainViewModel vm = new MainViewModel(
                    familyTypeRepository,
                    elementOptionRepository,
                    selectionService,
                    parameterService);

                MainView view = new MainView
                {
                    DataContext = vm
                };

                WindowInteropHelper helper =
                    new WindowInteropHelper(view);

                helper.Owner =
                    commandData.Application.MainWindowHandle;

                view.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.ToString();

                TaskDialog.Show(
                    "Parameter Manager – Error",
                    ex.ToString());

                return Result.Failed;
            }
        }
    }



    //public class ExternalCommand : IExternalCommand
    //{
    //    public Result Execute(
    //        ExternalCommandData commandData,
    //        ref string message,
    //        ElementSet elements)
    //    {
    //        try
    //        {
    //            TaskDialog.Show(
    //                "Parameter Manager",
    //                "ExternalCommand loaded successfully.");

    //            return Result.Succeeded;
    //        }
    //        catch (Exception ex)
    //        {
    //            TaskDialog.Show(
    //                "Parameter Manager – Command Error",
    //                ex.ToString());

    //            return Result.Failed;
    //        }
    //    }
    //}
}