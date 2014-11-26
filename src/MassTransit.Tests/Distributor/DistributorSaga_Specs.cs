// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Tests.Distributor
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Load;
    using Load.Messages;
    using Magnum.Extensions;
    using MassTransit.Distributor.Messages;
    using MassTransit.Pipeline.Inspectors;
    using NUnit.Framework;
    using TestFramework;

    [TestFixture]
    public class Using_the_distributor_for_a_saga :
        DistributorSagaTestFixture
    {
        [Test]
        public void Should_have_a_subscription_for_the_first_command()
        {
            LocalBus.ShouldHaveRemoteSubscriptionFor<FirstCommand>();
        }

        [Test]
        public void Should_have_a_subscription_for_the_pending_command()
        {
            LocalBus.ShouldHaveRemoteSubscriptionFor<FirstPending>();
        }
    }

    [TestFixture]
    public class Using_the_distributor_saga_worker_for_a_saga :
        DistributorSagaTestFixture
    {
        protected override void EstablishContext()
        {
            base.EstablishContext();

            AddInstance("A", "loopback://localhost/a",
                bus => bus.Worker(w => w.Saga(FirstSagaRepository)));
            
            AddInstance("B", "loopback://localhost/b",
                bus => bus.Worker(w => w.Saga(FirstSagaRepository)));
            
            AddInstance("C", "loopback://localhost/c",
                bus => bus.Worker(w => w.Saga(FirstSagaRepository)));
        }

        [Test]
        public void Should_register_the_message_consumers()
        {
            Instances["A"].DataBus.ShouldHaveRemoteSubscriptionFor<Distributed<FirstCommand>>();
            Instances["B"].DataBus.ShouldHaveRemoteSubscriptionFor<Distributed<FirstCommand>>();
            Instances["C"].DataBus.ShouldHaveRemoteSubscriptionFor<Distributed<FirstCommand>>();
        }

        [Test, Explicit]
        public async void Using_the_load_generator_should_share_the_load()
        {
            await Task.Run(() =>
            {
                var generator1 = new LoadGenerator<FirstCommand, FirstResponse>();
                generator1.Run(RemoteBus, LocalBus.Endpoint, Instances.Values.Select(x => x.DataBus), 100,
                    x => new FirstCommand(x));

            }, new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);
        }
    }

    [TestFixture]
    public class Using_multiple_distributors_with_the_same_worker_pool :
        LoopbackMultipleDistributorSagaTestFixture
    {
        protected override void EstablishContext()
        {
            base.EstablishContext();

            AddInstance("A", "loopback://localhost/a",
                        bus => bus.Worker(w => w.Saga(FirstSagaRepository)));
    
            AddInstance("B", "loopback://localhost/b",
                bus => bus.Worker(w => w.Saga(FirstSagaRepository)));
            
            AddInstance("C", "loopback://localhost/c",
                bus => bus.Worker(w => w.Saga(FirstSagaRepository)));
        }

        [Test, Explicit]
        public async void Using_the_load_generator_should_share_the_load()
        {
            var source = new CancellationTokenSource(TimeSpan.FromMinutes(3));

            var task1 = Task.Run(() =>
                {
                    var generator1 = new LoadGenerator<FirstCommand, FirstResponse>();
                    generator1.Run(RemoteBus, RemoteBus.Endpoint, Instances.Values.Select(x => x.DataBus), 100, x => new FirstCommand(x));
                }, source.Token);

            var task2 = Task.Run(() =>
                {
                    var generator2 = new LoadGenerator<FirstCommand, FirstResponse>();
                    generator2.Run(RemoteBus, LocalBus.Endpoint, Instances.Values.Select(x => x.DataBus), 100, x => new FirstCommand(x));
                }, source.Token);

            await Task.WhenAll(task1, task2);
        }
    }
}