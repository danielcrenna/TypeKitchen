// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace TypeKitchen.Tests.Serialization
{
    public class TemporaryFileFixture : IDisposable
    {
        private const int BufferSize = 4096;

        private readonly bool _persistent;

        private bool _disposed;

        public TemporaryFileFixture(bool persistent = false)
        {
            _persistent = persistent;
            FilePath = Path.GetTempFileName();

            // keep the file contents close to memory
            var attributes = File.GetAttributes(FilePath);
            File.SetAttributes(FilePath, attributes | FileAttributes.Temporary);

            // hint to the OS to purge the file on cleanup
            FileStream = File.Create(FilePath, BufferSize, !persistent ? FileOptions.DeleteOnClose : 0);
        }

        public string FilePath { get; }

        public FileStream FileStream { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                FileStream.Dispose();

            if (!_persistent)
            {
                try
                {
                    File.Delete(FilePath);
                }
                catch
                {
                    // best effort
                }
            }

            _disposed = true;
        }

        ~TemporaryFileFixture()
        {
            Dispose(false);
        }
    }
}
