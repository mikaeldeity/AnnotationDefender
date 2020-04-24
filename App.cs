using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using System;

namespace AnnotationDefender
{
    public class App : IExternalApplication
    {
        bool check = false;

        UIControlledApplication app = null;

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentSynchronizingWithCentral += new EventHandler<DocumentSynchronizingWithCentralEventArgs>(OnSyncing);
            application.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnChanged);
            application.ControlledApplication.FailuresProcessing += FailureProcessor;            
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {            
            return Result.Succeeded;
        }
        public void OnSyncing(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            check = true;
        }
        public void OnChanged (object sender, DocumentChangedEventArgs e)
        {
            if (e.GetTransactionNames().Contains("Reload Latest"))
            {
                check = true;
            }
            else
            {
                check = false;
            }
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
}