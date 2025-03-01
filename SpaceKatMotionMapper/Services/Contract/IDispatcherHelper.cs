using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace SpaceKatMotionMapper.Services.Contract
{
    public interface IDispatcherHelper
    {
        public Dispatcher Dispatcher { get; }
        Task RunAsync(Action action);
    }
}