﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExampleDialogViewModel.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orchestra.Examples.MahApps.ViewModels
{
    using Catel.MVVM;

    public class ExampleDialogViewModel : ViewModelBase
    {
        #region Properties
        public override string Title
        {
            get { return "Bindable title"; }
        }
        #endregion
    }
}