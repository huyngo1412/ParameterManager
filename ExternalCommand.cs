using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ParameterManager.Contracts;
using ParameterManager.Services;
using ParameterManager.Services.ParameterSetters;
using ParameterManager.ViewModels;
using ParameterManager.Views;
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
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
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
                new RevitParameterService(doc, valueSetterFactory);

            MainViewModel vm = new MainViewModel(
                familyTypeRepository,
                elementOptionRepository,
                selectionService,
                parameterService);

            MainView view = new MainView
            {
                DataContext = vm
            };

            WindowInteropHelper helper = new WindowInteropHelper(view);
            helper.Owner = commandData.Application.MainWindowHandle;

            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}