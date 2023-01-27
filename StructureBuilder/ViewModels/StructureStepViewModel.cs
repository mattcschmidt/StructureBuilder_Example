using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace StructureBuilder.ViewModels
{
    public class StructureStepViewModel:BindableBase
    {
        private string _resultStructure;

        public string ResultStructure
        {
            get { return _resultStructure; }
            set { SetProperty(ref _resultStructure,value); }
        }
        private string _selectedBaseStructure;

        public string SelectedBaseStructure
        {
            get { return _selectedBaseStructure; }
            set { SetProperty(ref _selectedBaseStructure,value); }
        }

        private string _selectedTargetStructure;

        public string SelectedTargetStructure
        {
            get { return _selectedTargetStructure; }
            set { SetProperty(ref _selectedTargetStructure,value); }
        }
        private string _selectedOperation;

        public string SelectedOperation
        {
            get { return _selectedOperation; }
            set { SetProperty(ref _selectedOperation,value); }
        }
        private bool _bTemp;

        public bool bTemp
        {
            get { return _bTemp; }
            set { SetProperty(ref _bTemp,value); }
        }
        private double _margin;

        public double Margin
        {
            get { return _margin; }
            set { SetProperty(ref _margin,value); }
        }

        public ObservableCollection<string> Structures { get; set; }
        public ObservableCollection<string> Operations { get; set; }

        private StructureSet _structureSet;

        public StructureStepViewModel(StructureSet structureSet)
        {
            Structures = new ObservableCollection<string>();
            Operations = new ObservableCollection<string>();
            _structureSet = structureSet;
            InitializeCollections();
        }

        private void InitializeCollections()
        {
            foreach(var structure in _structureSet.Structures)
            {
                Structures.Add(structure.Id);
            }
            Operations.Add("Margin");
            Operations.Add("And");
            Operations.Add("Or");
            Operations.Add("Sub");
        }
    }
}
