﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Orchestra development team">
//   Copyright (c) 2008 - 2013 Orchestra development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orchestra.Views
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.MVVM.Providers;
    using Catel.MVVM.Views;
    using Catel.Windows;
    using Catel.Windows.Media.Imaging;
    using Fluent;
    using Orchestra.ViewModels;
    using Xceed.Wpf.AvalonDock.Layout;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : RibbonWindow, IView
    {
        #region Constants
        /// <summary>
        /// Name of the trace output anchorable.
        /// </summary>
        public const string TraceOutputAnchorable = "traceOutputAnchorable";

        private const string ApplicationIconLocation = "Resources\\Images\\ApplicationIcon.png";
        private const string ApplicationIconFallbackLocation = "/Orchestra.Shell;component/Resources/Images/ApplicationIcon.png";

        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The status bar property.
        /// </summary>
        public static readonly DependencyProperty StatusBarProperty = DependencyProperty.Register("StatusBar", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));
        #endregion

        #region Fields
        private readonly WindowLogic _windowLogic;

        private event EventHandler<EventArgs> _viewLoaded;
        private event EventHandler<EventArgs> _viewUnloaded;
        private event EventHandler<EventArgs> _viewDataContextChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            InitializeMainWindow();

            _windowLogic = new WindowLogic(this, typeof(MainWindowViewModel));
            _windowLogic.ViewModelChanged += (s, e) =>
            {
                ViewModelChanged.SafeInvoke(this, e);
                PropertyChanged.SafeInvoke(this, new PropertyChangedEventArgs("ViewModel"));
            };
            _windowLogic.ViewModelPropertyChanged += (s, e) => ViewModelPropertyChanged.SafeInvoke(this, e);
            _windowLogic.PropertyChanged += (sender, e) => PropertyChanged.SafeInvoke(this, e);
            _windowLogic.ViewLoading += (sender, e) => ViewLoading.SafeInvoke(this);
            _windowLogic.ViewLoaded += (sender, e) => ViewLoaded.SafeInvoke(this);
            _windowLogic.ViewUnloading += (sender, e) => ViewUnloading.SafeInvoke(this);
            _windowLogic.ViewUnloaded += (sender, e) => ViewUnloaded.SafeInvoke(this);

            Loaded += (sender, e) => _viewLoaded.SafeInvoke(this);
            Unloaded += (sender, e) => _viewUnloaded.SafeInvoke(this);
            this.AddDataContextChangedHandler((sender, e) => _viewDataContextChanged.SafeInvoke(this));

            // _windowLogic.TargetControlPropertyChanged += (s, e) => PropertyChanged.SafeInvoke(this, new AdvancedPropertyChangedEventArgs(s, this, e.PropertyName, e.OldValue, e.NewValue));
            var serviceLocator = ServiceLocator.Default;

            serviceLocator.RegisterInstance(this);
            serviceLocator.RegisterInstance(ribbon);
            serviceLocator.RegisterInstance(dockingManager);
            serviceLocator.RegisterInstance(layoutDocumentPane);
            serviceLocator.RegisterInstance(typeof(LayoutAnchorablePane), rightPropertiesPane, "rightPropertiesPane");
            serviceLocator.RegisterInstance(typeof(LayoutAnchorablePane), leftPropertiesPane, "leftPropertiesPane");
            serviceLocator.RegisterInstance(typeof(LayoutAnchorGroup), bottomPropertiesPane, "bottomPropertiesPane");
            serviceLocator.RegisterInstance(typeof(LayoutAnchorGroup), topPropertiesPane, "topPropertiesPane");

            ribbon.AutomaticStateManagement = true;
            ribbon.EnsureTabItem(Library.Properties.Resources.HomeRibbonTabName);

            SetIcon();

            Loaded += (sender, e) => { traceOutputAnchorable.Hide(); };
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the status bar.
        /// </summary>
        /// <value>The status bar.</value>
        public string StatusBar
        {
            get { return (string)GetValue(StatusBarProperty); }
            set { SetValue(StatusBarProperty, value); }
        }
        #endregion

        #region IView Members
        /// <summary>
        /// Gets the view model that is contained by the container.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public IViewModel ViewModel
        {
            get { return _windowLogic.ViewModel; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view model container should prevent the creation of a view model.
        /// <para />
        /// This property is very useful when using views in transitions where the view model is no longer required.
        /// </summary>
        /// <value><c>true</c> if the view model container should prevent view model creation; otherwise, <c>false</c>.</value>
        public bool PreventViewModelCreation
        {
            get { return _windowLogic.PreventViewModelCreation; }
            set { _windowLogic.PreventViewModelCreation = value; }
        }

        /// <summary>
        /// Occurs when the <see cref="ViewModel"/> property has changed.
        /// </summary>
        public event EventHandler<EventArgs> ViewModelChanged;

        /// <summary>
        /// Occurs when a property on the <see cref="ViewModel"/> has changed.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ViewModelPropertyChanged;

        /// <summary>
        /// Occurs when a property on the container has changed.
        /// </summary>
        /// <remarks>
        /// This event makes it possible to externally subscribe to property changes of a <see cref="DependencyObject"/>
        /// (mostly the container of a view model) because the .NET Framework does not allows us to.
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the view model container is loading.
        /// </summary>
        public event EventHandler<EventArgs> ViewLoading;

        /// <summary>
        /// Occurs when the view model container is loaded.
        /// </summary>
        public event EventHandler<EventArgs> ViewLoaded;

        /// <summary>
        /// Occurs when the view model container starts unloading.
        /// </summary>
        public event EventHandler<EventArgs> ViewUnloading;

        /// <summary>
        /// Occurs when the view model container is unloaded.
        /// </summary>
        public event EventHandler<EventArgs> ViewUnloaded;

        /// <summary>
        /// Gets the logical parent  element of this element.
        /// </summary>
        /// <value>The parent.</value>
        /// <returns>This element's logical parent.</returns>
        object IView.Parent { get { return null; } }

        /// <summary>
        /// Occurs when the view is loaded.
        /// </summary>
        event EventHandler<EventArgs> IView.Loaded
        {
            add { _viewLoaded += value; }
            remove { _viewLoaded -= value; }
        }

        /// <summary>
        /// Occurs when the view is unloaded.
        /// </summary>
        event EventHandler<EventArgs> IView.Unloaded
        {
            add { _viewUnloaded += value; }
            remove { _viewUnloaded -= value; }
        }

        /// <summary>
        /// Occurs when the data context has changed.
        /// </summary>
        event EventHandler<EventArgs> IView.DataContextChanged
        {
            add { _viewDataContextChanged += value; }
            remove { _viewDataContextChanged -= value; }
        }
        #endregion

        #region Methods
        private void SetIcon()
        {
            try
            {
                var assemblyPath = Assembly.GetEntryAssembly().Location;
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(assemblyPath);
                if (icon != null)
                {
                    var bitmap = icon.ToBitmap();
                    var bitmapSource = bitmap.ConvertBitmapToBitmapSource();

                    Icon = bitmapSource;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve the icon, cannot set the window icon");
            }
        }

        /// <summary>
        /// Determines whether the anchorable with the specified name is currently visible.
        /// </summary>
        /// <param name="name">
        /// The name of the anchorable.
        /// </param>
        /// <returns>
        /// <c>true</c> if the anchorable with the specified name is visible; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="name"/> is <c>null</c> or whitespace.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The anchorable with the specified name cannot be found.
        /// </exception>
        public bool IsAnchorableVisible(string name)
        {
            Argument.IsNotNullOrWhitespace("name", name);

            var anchorable = FindAnchorable(name, true);
            return anchorable.IsVisible;
        }

        /// <summary>
        /// Shows the anchorable with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the anchorable.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="name"/> is <c>null</c> or whitespace.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The anchorable with the specified name cannot be found.
        /// </exception>
        public void ShowAnchorable(string name)
        {
            Argument.IsNotNullOrWhitespace("name", name);

            var anchorable = FindAnchorable(name, true);
            anchorable.IsVisible = true;
        }

        /// <summary>
        /// Hides the anchorable with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the anchorable.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="name"/> is <c>null</c> or whitespace.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The anchorable with the specified name cannot be found.
        /// </exception>
        public void HideAnchorable(string name)
        {
            Argument.IsNotNullOrWhitespace("name", name);

            var anchorable = FindAnchorable(name, true);
            anchorable.IsVisible = false;
        }

        /// <summary>
        /// Finds the anchorable with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the anchorable.
        /// </param>
        /// <param name="throwExceptionWhenNotFound">
        /// If set to <c>true</c>, this method will throw an <see cref="InvalidOperationException"/> when the anchorable cannot be found.
        /// </param>
        /// <returns>
        /// The <see cref="LayoutAnchorable"/> or <c>null</c> if the anchorable cannot be found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="name"/> is <c>null</c> or whitespace.
        /// </exception>
        private LayoutAnchorable FindAnchorable(string name, bool throwExceptionWhenNotFound = false)
        {
            Argument.IsNotNullOrWhitespace("name", name);

            var visibleAnchorable = (from child in dockingManager.Layout.Children
                                     where child is LayoutAnchorable && TagHelper.AreTagsEqual(((LayoutAnchorable)child).ContentId, name)
                                     select (LayoutAnchorable)child).FirstOrDefault();
            if (visibleAnchorable != null)
            {
                return visibleAnchorable;
            }

            var invisibleAnchorable = (from child in dockingManager.Layout.Hidden
                                       where TagHelper.AreTagsEqual((child).ContentId, name)
                                       select child).FirstOrDefault();
            if (invisibleAnchorable != null)
            {
                return invisibleAnchorable;
            }

            if (throwExceptionWhenNotFound)
            {
                string error = string.Format("Anchorable with name '{0}' cannot be found", name);
                Log.Error(error);
                throw new InvalidOperationException(error);
            }

            return null;
        }

        /// <summary>
        /// Loads the application Icon.
        /// </summary>
        private void InitializeMainWindow()
        {
            var directory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

            try
            {
                string firstAttemptFile = Path.Combine(directory, ApplicationIconLocation);

                if (File.Exists(firstAttemptFile))
                {
                    Icon = BitmapFrame.Create(new Uri(firstAttemptFile, UriKind.Absolute));
                    return;
                }
            }
            catch (Exception ex)
            {
                // Don't change default Icon.            
                Log.Error(ex);
            }

            Icon = new BitmapImage(new Uri("pack://application:,,," + ApplicationIconFallbackLocation));
        }
        #endregion
    }
}