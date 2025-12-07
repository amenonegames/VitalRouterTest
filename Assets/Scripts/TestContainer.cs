using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using VitalRouter.VContainer;

namespace DefaultNamespace
{
    public class TestContainer : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterVitalRouter( router =>
            {
                router.FanOut(parallelGroup =>
                {
                    parallelGroup.Ordering = CommandOrdering.Parallel;
                    parallelGroup.Map<ParallelPresenter>();
                });

                router.FanOut(sequentialGroup =>
                {
                    sequentialGroup.Ordering = CommandOrdering.Sequential;
                    sequentialGroup.Map<SequentialPresenter>();
                });
            });

            builder.RegisterEntryPoint<Entrypoint>();
        }
    }

    public class Entrypoint : IStartable
    {
        private readonly ICommandPublisher _commandPublisher;
        public Entrypoint(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }


        public void Start() => _commandPublisher.PublishAsync(new SequenceMsg());
    }

    public struct ParallelMsg:ICommand
    {}
    public struct SequenceMsg:ICommand
    {}

    [Routes]
    public partial class ParallelPresenter
    {
        [Route]
        void On(ParallelMsg parallelMsg)
        {
            Debug.Log("parallelMsg is published");
        }
    }
    [Routes]
    public partial class SequentialPresenter
    {
        private readonly ICommandPublisher _commandPublisher;

        public SequentialPresenter(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }
        [Route]
        async UniTask On(SequenceMsg sequenceMsg)
        {
            Debug.Log("sequenceMsg is published. start processing");
            await _commandPublisher.PublishAsync(new ParallelMsg());
            Debug.Log("sequenceMsg is published. finish processing");
        }
    }
}