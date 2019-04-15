// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace TypeKitchen.Tests
{
    public class SnippetTests
    {
        [Fact]
        public void StaticMethod_NoArgs()
        {
            var method = Snippet.CreateMethod("public static int Method() { return 1; }");
            var result = method.Invoke(null, null);
            Assert.Equal(1, result);

            var wrap = CallAccessor.Create(method);
            result = wrap.Call(null);
            Assert.Equal(1, result);
        }
    }
}