﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Joker.WPF.Sample.Factories.ViewModels;
using Joker.WPF.Sample.Providers.Scheduling;
using Prism.Mvvm;
using Sample.Data.Context;
using Sample.Domain.Models;
using SqlTableDependency.Extensions;
using SqlTableDependency.Extensions.Notifications;
using TableDependency.SqlClient.Base.Enums;

namespace Joker.WPF.Sample.ViewModels.Products
{
  public class ProductsViewModel : BindableBase, IDisposable
  {
    private readonly ISampleDbContext sampleDbContext;
    private readonly ISqlTableDependencyProvider<Product> productsChangesProvider;
    private readonly ISchedulerProvider schedulerProvider;
    private readonly ViewModelsFactory viewModelsFactory;

    public ProductsViewModel(ISampleDbContext sampleDbContext, ISqlTableDependencyProvider<Product> productsChangesProvider, ISchedulerProvider schedulerProvider, ViewModelsFactory viewModelsFactory)
    {
      this.sampleDbContext = sampleDbContext ?? throw new ArgumentNullException(nameof(sampleDbContext));
      this.productsChangesProvider = productsChangesProvider ?? throw new ArgumentNullException(nameof(productsChangesProvider));
      this.schedulerProvider = schedulerProvider ?? throw new ArgumentNullException(nameof(schedulerProvider));
      this.viewModelsFactory = viewModelsFactory ?? throw new ArgumentNullException(nameof(viewModelsFactory));
    }

    public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();

    private bool isOffline = true;

    public bool IsOffline
    {
      get => isOffline;
      set
      {
        if(isOffline == value)
          return;

        isOffline = value;

        RaisePropertyChanged();
      }
    }

    private IDisposable statusChangesSubscription;

    public void Initialize()
    {
      statusChangesSubscription =
        productsChangesProvider.WhenStatusChanges
          .ObserveOn(schedulerProvider.Dispatcher)
          .Subscribe(OnStatusChanged);

      ReInitialize();

      productsChangesProvider.SubscribeToEntityChanges();
    }

    private void ReInitialize()
    {
      Products.Clear();

      LoadProducts();

      SubscribeToProductRecordChanges();
    }

    private void OnStatusChanged(TableDependencyStatus tableDependencyStatus)
    {
      IsOffline = tableDependencyStatus != TableDependencyStatus.Started &&
                  tableDependencyStatus != TableDependencyStatus.WaitingForNotification;
    }

    private SerialDisposable productsChangesSubscription;

    private void SubscribeToProductRecordChanges()
    {
      if (productsChangesSubscription == null)
        productsChangesSubscription = new SerialDisposable();

      productsChangesSubscription.Disposable = productsChangesProvider.WhenEntityRecordChanges
        .Buffer(TimeSpan.FromMilliseconds(250), 100, schedulerProvider.TaskPool)
        .Where(c => c.Count > 0)
        .ObserveOn(schedulerProvider.Dispatcher)
        .Subscribe(OnProductRecordReceived);
    }

    private IDisposable loadProductsSubscription;

    private void LoadProducts()
    {
      using (loadProductsSubscription)
      {
      }

      loadProductsSubscription =
        Observable.Start(() => sampleDbContext.Products.ToList())
          .ObserveOn(schedulerProvider.Dispatcher)
          .Subscribe(loadedProducts => {
                       Products.AddRange(loadedProducts.Select(c => viewModelsFactory.CreateProductViewModel(c)));
                     });
    }

    private void OnProductRecordReceived(IList<RecordChangedNotification<Product>> recordChangedNotifications)
    {
      foreach (var recordChangedNotification in recordChangedNotifications)
      {
        var entity = recordChangedNotification.Entity;
        
        switch (recordChangedNotification.ChangeType)
        {
          case ChangeType.Insert:
            if(Products.Any(c => c.Id == entity.Id))
              return;
            Products.Add(viewModelsFactory.CreateProductViewModel(recordChangedNotification.Entity));
            break;

          case ChangeType.Update:
            var viewModelToUpdate = Products.FirstOrDefault(c => c.Id == entity.Id);
            viewModelToUpdate?.UpdateFrom(entity);
            break;
          
          case ChangeType.Delete:
            var viewModelToRemove = Products.FirstOrDefault(c => c.Id == entity.Id);
            Products.Remove(viewModelToRemove);
            break;
        }
      }
    }

    public void Dispose()
    {
      using (productsChangesProvider)
      using (loadProductsSubscription)
      using (statusChangesSubscription)
      {
      }

      productsChangesSubscription?.Dispose();
    }
  }
}