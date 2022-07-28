using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Pulse.UI
{
    public class UiLazyContainerNode : UiContainerNode
    {
        private static readonly Semaphore Semaphore = new(Environment.ProcessorCount, Environment.ProcessorCount);

        private volatile object _lock;
        private bool _childsCreated;

        public UiLazyContainerNode(string name, UiNodeType type)
            : base(name, type)
        {
            _lock = new();
        }

        protected override UiNode[] RequestChilds()
        {
            if (_childsCreated)
                return Childs ?? EmptyChilds;

            Task.Factory.StartNew(() => InitializeChilds(false));
            return EmptyChilds;
        }

        public override UiNode[] GetChilds()
        {
            if (_childsCreated)
                return Childs ?? EmptyChilds;

            return InitializeChilds(true);
        }

        protected internal override UiNode[] SetChilds(UiNode[] childs)
        {
            throw new NotSupportedException("UiLazyChildsNode.SetChilds");
        }

        private UiNode[] InitializeChilds(bool waitingForResult)
        {
            object lockObject = _lock;
            if (lockObject == null)
                return Childs ?? EmptyChilds;

            if (waitingForResult)
            {
                Monitor.Enter(lockObject);
            }
            else
            {
                if (!Monitor.TryEnter(lockObject))
                    return EmptyChilds;
            }

            try
            {
                if (_childsCreated)
                    return Childs;

                UiNode[] childs = ProvideChilds();
                Childs = childs;
                _childsCreated = true;
                OnPropertyChanged("Icon");

                if (Childs != null)
                    OnPropertyChanged("BindableHierarchyChilds");

                _lock = null;
                return childs;
            }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }

        private UiNode[] ProvideChilds()
        {
            Semaphore.WaitOne();
            try
            {
                return ExpandChilds();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        protected virtual UiNode[] ExpandChilds()
        {
            return EmptyChilds;
        }

        public sealed override ImageSource Icon => _childsCreated ? GetIcon() : Icons.PendingIcon;

        protected virtual ImageSource GetIcon()
        {
            return base.Icon;
        }
    }
}