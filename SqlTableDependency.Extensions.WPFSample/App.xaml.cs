﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommonServiceLocator;
using Ninject;
using Prism.Ioc;
using Prism.Ninject.Ioc;
using Sample.Data.Context;
using Sample.Domain.Models;
using SqlTableDependency.Extensions.WPFSample.Modularity;
using SqlTableDependency.Extensions.WPFSample.Providers.Scheduling;
using SqlTableDependency.Extensions.WPFSample.SqlTableDependencies;
using SqlTableDependency.Extensions.WPFSample.ViewModels;

namespace SqlTableDependency.Extensions.WPFSample
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    private readonly IKernel kernel = new StandardKernel();

    #region CreateContainerExtension

    protected override IContainerExtension CreateContainerExtension()
    {
      return new NinjectContainerExtension(kernel);
    }

    #endregion

    #region RegisterTypes

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
      kernel.Bind<ShellViewModel>().ToSelf().InSingletonScope();

      var connectionString = ConfigurationManager.ConnectionStrings["FargoEntities"].ConnectionString;

      kernel.Bind<ISchedulerProvider>().To<SchedulerProvider>().InSingletonScope();
      kernel.Bind<ISampleDbContext>().To<SampleDbContext>().InTransientScope().WithConstructorArgument("nameOrConnectionString", connectionString);
      
      kernel.Bind<ISqlTableDependencyProvider<Product>>().To<ProductsSqlTableDependencyProvider>()
        .InTransientScope()
        .WithConstructorArgument("connectionString", connectionString);
    }

    #endregion

    protected override Window CreateShell()
    {
      return new Shell();
    }

    #region InitializeShell

    protected override void InitializeShell(Window shell)
    {
      base.InitializeShell(shell);

      if (shell is FrameworkElement shellFrameworkElement)
        shellFrameworkElement.DataContext = kernel.Get<ShellViewModel>();
    }

    #endregion

    #region ConfigureServiceLocator

    protected override void ConfigureServiceLocator()
    {
      kernel.Bind<IServiceLocator>().To<NinjectServiceLocator>().InSingletonScope();

      base.ConfigureServiceLocator();
    }

    #endregion

    #region OnExit

    protected override void OnExit(ExitEventArgs e)
    {
      base.OnExit(e);

      kernel.Dispose();
    }

    #endregion
  }
}