﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Misakai.Mongo.Core.Clusters;
using Misakai.Mongo.Core.Misc;

namespace Misakai.Mongo.Core.Bindings
{
    public sealed class ReadBindingHandle : IReadBindingHandle
    {
        // fields
        private bool _disposed;
        private readonly ReferenceCounted<IReadBinding> _reference;

        // constructors
        public ReadBindingHandle(IReadBinding readBinding)
            : this(new ReferenceCounted<IReadBinding>(readBinding))
        {
        }

        private ReadBindingHandle(ReferenceCounted<IReadBinding> reference)
        {
            _reference = reference;
        }

        // properties
        public ReadPreference ReadPreference
        {
            get { return _reference.Instance.ReadPreference; }
        }

        // methods
        public Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _reference.Instance.GetReadChannelSourceAsync(cancellationToken);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _reference.DecrementReferenceCount();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public IReadBindingHandle Fork()
        {
            ThrowIfDisposed();
            _reference.IncrementReferenceCount();
            return new ReadBindingHandle(_reference);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
