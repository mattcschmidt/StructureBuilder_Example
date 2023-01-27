using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using StructureBuilder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace StructureBuilder.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private Application _application;
        private StructureSet _structureSet;

        public ObservableCollection<StructureStepViewModel> StructureCreationSteps { get; set; }
        public DelegateCommand ImportTemplateCommand { get; set; }
        public DelegateCommand ExportTemplateCommand { get; set; }
        public DelegateCommand AddStepCommand { get; set; }
        public DelegateCommand RunStepsCommand { get; set; }
        public MainViewModel(Application application, StructureSet structureSet)
        {
            _application = application;
            _structureSet = structureSet;
            StructureCreationSteps = new ObservableCollection<StructureStepViewModel>();
            ImportTemplateCommand = new DelegateCommand(OnImport);
            ExportTemplateCommand = new DelegateCommand(OnExport);
            AddStepCommand = new DelegateCommand(OnAddStep);
            RunStepsCommand = new DelegateCommand(OnRunSteps);

        }

        private void OnRunSteps()
        {
            //build structure with ESAPI
            foreach (var step in StructureCreationSteps)
            {
                Structure newStructure = null;
                //first check if structure exists, if so modify, if no, create.
                if (_structureSet.Structures.Any(s => s.Id.Equals(step.ResultStructure, StringComparison.OrdinalIgnoreCase)))
                {
                    newStructure = _structureSet.Structures.First(s => s.Id.Equals(step.ResultStructure, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    newStructure = _structureSet.AddStructure("CONTROL", step.ResultStructure);
                }
                //comment about auto generated structure.
                newStructure.Comment = $"Auto Generated Structure {Assembly.GetExecutingAssembly().GetName()}";
                var baseStructure = _structureSet.Structures.First(s => s.Id.Equals(step.SelectedBaseStructure));
                if (step.SelectedOperation == "Margin")
                {
                    //symmetric margins only supported.
                    newStructure.SegmentVolume = baseStructure.SegmentVolume.Margin(step.Margin);
                }
                else
                {
                    //Other steps require the operation and a target structure.
                    var targetStructure = _structureSet.Structures.First(s => s.Id.Equals(step.SelectedTargetStructure));
                    if (step.SelectedOperation == "And")
                    {
                        newStructure.SegmentVolume = baseStructure.SegmentVolume.And(targetStructure);
                    }
                    else if (step.SelectedOperation == "Or")
                    {
                        newStructure.SegmentVolume = baseStructure.SegmentVolume.Or(targetStructure);
                    }
                    else if (step.SelectedOperation == "Sub")
                    {
                        newStructure.SegmentVolume = baseStructure.SegmentVolume.Sub(targetStructure);
                    }
                }
            }

            //remove all structures that were only temporary.
            //TODO:Add some validation that makes sure the resultStructure wasn't an already existing structure. 
            //We CANNOT accidentally delete a manually generated contour. 
            foreach (var tempStep in StructureCreationSteps.Where(scs => scs.bTemp))
            {
                _structureSet.RemoveStructure(_structureSet.Structures.FirstOrDefault(s => s.Id == tempStep.ResultStructure));
            }
            if (System.Windows.MessageBox.Show("Save Modifications?", "Save", System.Windows.MessageBoxButton.YesNo)
                == System.Windows.MessageBoxResult.Yes)
            {
                _application.SaveModifications();
            }
        }

        private void OnAddStep()
        {
            List<string> priorSteps = StructureCreationSteps.Select(scs => scs.ResultStructure).ToList();
            var creationStep = new StructureStepViewModel(_structureSet);
            if (priorSteps.Any())
            {
                foreach (var step in priorSteps)
                {
                    creationStep.Structures.Add(step);
                }

            }
            StructureCreationSteps.Add(creationStep);
        }

        private void OnExport()
        {
            List<StructureCreationModel> scmList = new List<StructureCreationModel>();
            foreach (var step in StructureCreationSteps)
            {
                StructureCreationModel scModel = new StructureCreationModel();
                scModel.ResultStructure = step.ResultStructure;
                scModel.BaseStructure = step.SelectedBaseStructure;
                scModel.StructureOperation = step.SelectedOperation;
                scModel.TargetStructure = step.SelectedTargetStructure;
                scModel.Margin = step.Margin;
                scModel.bTemp = step.bTemp;
                scmList.Add(scModel);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JSON (*json)|*.json";
            sfd.Title = "Save Structure Template";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(scmList));
            }
        }

        private void OnImport()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON (*json)|*.json";
            ofd.Title = "Open Structure Template";
            if (ofd.ShowDialog() == true)
            {
                //get list from template
                //TODO error checking onJson conversion.
                List<StructureCreationModel> scmList = JsonConvert.DeserializeObject<List<StructureCreationModel>>(File.ReadAllText(ofd.FileName));
                foreach(var scm in scmList)
                {
                    OnAddStep();//add the step manually, then fill the data from JSON. 
                    var scStep = StructureCreationSteps.Last();
                    scStep.bTemp = scm.bTemp;
                    scStep.ResultStructure = scm.ResultStructure;
                    scStep.SelectedBaseStructure = scm.BaseStructure;
                    scStep.SelectedTargetStructure = scm.TargetStructure;
                    scStep.Margin = scm.Margin;
                }
            }
        }
    }
}
