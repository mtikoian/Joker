﻿#region License
// TableDependency, SqlTableDependency
// Copyright (c) 2019-2020 Tomas Fabian. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Data;
using Autofac;
using Joker.Factories.Schedulers;
using Joker.OData.Batch;
using Joker.OData.Extensions.OData;
using Joker.OData.Startup;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;
using Sample.Domain.Models;
using SelfHostedODataService.AutofacModules;
using SelfHostedODataService.Configuration;

namespace SelfHostedODataService
{
  public class StartupBaseWithOData : ODataStartup
  {
    public StartupBaseWithOData(IWebHostEnvironment env)
      : base(env)
    {
    }

    protected override ODataModelBuilder OnCreateEdmModel(ODataModelBuilder oDataModelBuilder)
    {
      oDataModelBuilder.Namespace = "Example";

      oDataModelBuilder.EntitySet<Product>("Products");
      oDataModelBuilder.AddPluralizedEntitySet<Book>();
      oDataModelBuilder.AddPluralizedEntitySet<Author>();

      return oDataModelBuilder;
    }
    
    private void ConfigureNLog()
    {
      var config = new LoggingConfiguration();

      // Targets where to log to: File and Console
      var logfile = new FileTarget("logfile") { FileName = "logs.txt" };
      var logconsole = new ConsoleTarget("logconsole");

      // Rules for mapping loggers to targets            
      config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
      config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

      LogManager.Configuration = config;
    }

    protected override void OnConfigureServices(IServiceCollection services)
    {
      base.OnConfigureServices(services);

      ConfigureNLog();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
      base.RegisterTypes(builder);

      ContainerBuilder.RegisterModule(new ProductsAutofacModule());

      ContainerBuilder.RegisterType<ProductsConfigurationProvider>()
        .As<IProductsConfigurationProvider>()
        .SingleInstance();

      ContainerBuilder.RegisterType<SchedulersFactory>().As<ISchedulersFactory>()
        .SingleInstance();
    }

    protected override ODataBatchHandler OnCreateODataBatchHandler()
    {
      var batchHandler = (TransactionScopeODataBatchHandler)base.OnCreateODataBatchHandler();

      return batchHandler;
    }

    protected override void OnRegisterMiddleWares(IApplicationBuilder app)
    {
      base.OnRegisterMiddleWares(app);
    }
  }
}