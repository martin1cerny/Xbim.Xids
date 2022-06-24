﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using Xbim.InformationSpecifications.UI.mvvm;

namespace Xbim.InformationSpecifications.UI.VM
{
    internal class ReqGrpVM : ViewModelBase
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private readonly SpecificationsGroup requirementsCollection;

        private readonly Xids ids;

        private ICommand addRequirementCommand;
        public ICommand AddRequirementCommand
        {
            get
            {
                if (addRequirementCommand == null)
                {
                    addRequirementCommand = new ActionCommand(i => AddRequirement());
                }
                return addRequirementCommand;
            }
        }

        private void AddRequirement()
        {
            var t = ids.PrepareSpecification(IfcSchemaVersion.IFC2X3);
            Reqs.Add(new ReqViewModel(t));
        }

        public ReqGrpVM(SpecificationsGroup requirementsCollection, Xids ids)
        {
            this.requirementsCollection = requirementsCollection;
            this.ids = ids;

            Reqs = new ObservableCollection<ReqViewModel>();
            foreach (var item in requirementsCollection.Specifications)
            {
                Reqs.Add(new ReqViewModel(item));
            }
        }

        public ObservableCollection<ReqViewModel> Reqs { get; set; }
    }
}