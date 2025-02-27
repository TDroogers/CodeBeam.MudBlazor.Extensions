﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MudBlazor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MudExtensions.UnitTests.Mocks
{
    /// <summary>
    /// Mock for popover
    /// </summary>
    internal class MockPopoverService : IPopoverService
    {
        public PopoverOptions PopoverOptions { get; } = new();

        public IEnumerable<IMudPopoverHolder> ActivePopovers { get; } = Enumerable.Empty<IMudPopoverHolder>();

        public bool IsInitialized => false;

        public void Subscribe(IPopoverObserver observer)
        {
        }

        public void Unsubscribe(IPopoverObserver observer)
        {
        }

        public Task CreatePopoverAsync(IPopover popover) => Task.CompletedTask;

        public Task<bool> UpdatePopoverAsync(IPopover popover) => Task.FromResult(true);

        public Task<bool> DestroyPopoverAsync(IPopover popover) => Task.FromResult(true);

        public ValueTask<int> GetProviderCountAsync() => ValueTask.FromResult(0);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
