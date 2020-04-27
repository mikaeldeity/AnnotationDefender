using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace AnnotationDefender
{
    public class App : IExternalApplication
    {
        private static bool check = false;
        static void AddRibbonPanel(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Annotation\nDefender");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData b1Data = new PushButtonData("Annotation", "Annotation", thisAssemblyPath, "AnnotationDefender.ShowActive");
            b1Data.AvailabilityClassName = "AnnotationDefender.Availability";
            PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
            pb1.ToolTip = "Prevent annotation loss at sync with out of date links.";
            Uri addinImage =
                new Uri("pack://application:,,,/AnnotationDefender;component/Resources/AnnotationDefender.png");
            BitmapImage pb1Image = new BitmapImage(addinImage);
            pb1.LargeImage = pb1Image;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonPanel(application);
            application.ControlledApplication.FailuresProcessing += FailureProcessor;            
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {            
            return Result.Succeeded;
        }
        private void FailureProcessor(object sender, FailuresProcessingEventArgs e)
        {
            if (!check) { return; }

            FailuresAccessor fas = e.GetFailuresAccessor();

            List<FailureMessageAccessor> fma = fas.GetFailureMessages().ToList();

            string error1 = "Coordination Monitor alert : A hosting element no longer exists in the link.";
            string error2 = "One or more dimension references are or have become invalid.";

            bool catched = false;

            foreach (FailureMessageAccessor fa in fma)
            {
                string failuremessage = fa.GetDescriptionText();

                if (failuremessage == error2 | failuremessage == error1)
                {
                    e.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                    catched = true;
                }
            }
            if (catched)
            {
                TaskDialog.Show("Error", "Some Revit links have been modified.\n  1) 'Unload' the modified Revit links\n  2) 'Reload Latest'");
            }
        }
    }
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            return true;
        }
    }
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class ShowActive : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Status", "Active");
            return Result.Succeeded;
        }
    }
}