//
// CertificateValidationHelper.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;
using MX = Mono.Security.X509;
#if MOBILE
using Mono.Net.Security;
#endif

namespace Mono.Security.Interface
{
	public static class CertificateValidationHelper
	{
		#if !INSIDE_SYSTEM
		const string InternalHelperTypeName = "Mono.Net.Security.ChainValidationHelper";
		static readonly Type internalHelperType;
		static readonly MethodInfo createMethod;
		static readonly MethodInfo validateClientCertificateMethod;
		static readonly MethodInfo validateChainMethod;
		#endif

		static CertificateValidationHelper ()
		{
			#if !INSIDE_SYSTEM
			internalHelperType = Type.GetType (InternalHelperTypeName + ", " + Consts.AssemblySystem, true);
			if (internalHelperType == null)
				throw new NotSupportedException ();
			createMethod = internalHelperType.GetMethod ("GetDefaultValidator", BindingFlags.Static | BindingFlags.NonPublic);
			if (createMethod == null)
				throw new NotSupportedException ();
			validateClientCertificateMethod = internalHelperType.GetMethod ("ValidateClientCertificate", BindingFlags.Instance | BindingFlags.Public);
			if (validateClientCertificateMethod == null)
				throw new NotSupportedException ();
			validateChainMethod = internalHelperType.GetMethod ("ValidateChain", BindingFlags.Instance | BindingFlags.Public);
			if (validateChainMethod == null)
				throw new NotSupportedException ();
			#endif
		}

		internal static ICertificateValidator GetDefaultValidator (MonoTlsSettings settings)
		{
			#if INSIDE_SYSTEM
			return ChainValidationHelper.GetDefaultValidator (settings);
			#else
			return (ICertificateValidator)createMethod.Invoke (null, new object[] { settings });
			#endif
		}

		public static ICertificateValidator GetValidator (MonoTlsSettings settings)
		{
			return GetDefaultValidator (settings);
		}
	}
}
