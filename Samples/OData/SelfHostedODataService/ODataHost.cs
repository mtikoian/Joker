﻿using Joker.OData.Hosting;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace SelfHostedODataService
{
  public class ODataHost : ODataHost<StartupForBlazorAndOData>
  {
    protected override void OnConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
      webHostBuilder
        .UseSerilog();

      base.OnConfigureWebHostBuilder(webHostBuilder);
    }
  }
}