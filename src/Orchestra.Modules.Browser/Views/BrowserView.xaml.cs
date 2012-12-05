﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowserView.xaml.cs" company="Orchestra development team">
//   Copyright (c) 2008 - 2012 Orchestra development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orchestra.Modules.Browser.Views
{
    using Catel.IoC;
    using Catel.Messaging;
    using Orchestra.Views;

    /// <summary>
    /// Interaction logic for BrowserView.xaml.
    /// </summary>
    public partial class BrowserView : DocumentView
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserView"/> class.
        /// </summary>
        public BrowserView()
        {
            InitializeComponent();

            var messageMediator = ServiceLocator.Default.ResolveType<IMessageMediator>();
            messageMediator.Register<string>(this, OnBrowse, BrowserModule.Name);
        }
        #endregion

        #region Methods
        private void OnBrowse(string url)
        {
            webBrowser.Navigate(url);
        }
        #endregion
    }
}