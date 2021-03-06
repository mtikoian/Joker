﻿using System.Reactive.Subjects;
using Joker.Redis.ConnectionMultiplexers;
using Joker.Redis.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using SqlTableDependency.Extensions;
using SqlTableDependency.Extensions.Notifications;
using StackExchange.Redis;
using TableDependency.SqlClient.Base.Enums;
using UnitTests;

namespace Joker.Redis.Tests.SqlTableDependency
{
  [TestClass]
  public class SqlTableDependencyRedisProviderTests : TestBase<TestSqlTableDependencyRedisProvider>
  {
    private readonly ISubject<RecordChangedNotification<TestModel>> recordChangedSubject =
      new Subject<RecordChangedNotification<TestModel>>();

    private readonly ISubject<TableDependencyStatus> statusChangedSubject = new Subject<TableDependencyStatus>();

    private readonly ISubject<bool> whenIsConnectedChangesSubject = new Subject<bool>();

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      var sqlTableDependencyProvider = MockingKernel.GetMock<ISqlTableDependencyProvider<TestModel>>();

      sqlTableDependencyProvider.Setup(c => c.WhenEntityRecordChanges)
        .Returns(recordChangedSubject);

      sqlTableDependencyProvider.Setup(c => c.WhenStatusChanges)
        .Returns(statusChangedSubject);

      MockingKernel.GetMock<IRedisPublisher>()
        .Setup(c => c.WhenIsConnectedChanges)
        .Returns(whenIsConnectedChangesSubject);

      ClassUnderTest = MockingKernel.Get<TestSqlTableDependencyRedisProvider>();
    }

    [TestMethod]
    public void OnSqlTableDependencyRecordChanged()
    {
      //Arrange
      ClassUnderTest.StartPublishing();

      //Act
      recordChangedSubject.OnNext(new RecordChangedNotification<TestModel>());
      RunSchedulers();

      ////Assert
      MockingKernel.GetMock<IRedisPublisher>().Verify(c => c.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), CommandFlags.None), Times.Exactly(1));
    }

    [TestMethod]
    public void OnSqlTableDependencyStatusChanged()
    {
      //Arrange
      ClassUnderTest.StartPublishing();

      //Act
      statusChangedSubject.OnNext(TableDependencyStatus.Started);
      RunSchedulers();

      ////Assert
      MockingKernel.GetMock<IRedisPublisher>()
        .Verify(c => c.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), CommandFlags.None), Times.Exactly(1));
    }
  }
}