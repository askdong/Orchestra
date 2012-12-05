﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowserModule.cs" company="Orchestra development team">
//   Copyright (c) 2008 - 2012 Orchestra development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orchestra.Modules.Browser
{
    using Catel.MVVM;
    using Models;
    using Services;
    using ViewModels;
    using Views;

    /// <summary>
    /// Browser module.
    /// </summary>
    public class BrowserModule : ModuleBase
    {
        /// <summary>
        /// The module name.
        /// </summary>
        public const string Name = "Browser";

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserModule"/> class. 
        /// </summary>
        public BrowserModule()
            : base(Name)
        {
        }

        /// <summary>
        /// Called when the module has been initialized.
        /// </summary>
        protected override void OnInitialized()
        {
            var orchestraService = GetService<IOrchestraService>();
            orchestraService.ShowDocument<BrowserViewModel>();
        }

        /// <summary>
        /// Initializes the ribbon.
        /// <para />
        /// Use this method to hook up views to ribbon items.
        /// </summary>
        /// <param name="ribbonService">The ribbon service.</param>
        protected override void InitializeRibbon(IRibbonService ribbonService)
        {
            var orchestraService = GetService<IOrchestraService>();

            // Module specific
            ribbonService.RegisterRibbonItem(new RibbonItem(HomeRibbonTabName, ModuleName, "Open", new Command(() => orchestraService.ShowDocument<BrowserViewModel>())));

            // View specific
            ribbonService.RegisterViewSpecificRibbonItem<BrowserView>(new RibbonItem(Name, Name, "Back", "GoBack") { ItemImage = "/Orchestra.Modules.Browser;component/Resources/Images/action_left.png" });
            ribbonService.RegisterViewSpecificRibbonItem<BrowserView>(new RibbonItem(Name, Name, "Forward", "GoForward") { ItemImage = "/Orchestra.Modules.Browser;component/Resources/Images/action_right.png" });
            ribbonService.RegisterViewSpecificRibbonItem<BrowserView>(new RibbonItem(Name, Name, "Browse", "Browse") { ItemImage = "/Orchestra.Modules.Browser;component/Resources/Images/action_browse.png" });
        }
    }
}