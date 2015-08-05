//
// MonoTlsSettings.cs
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

namespace Mono.Security.Interface
{
	public sealed class MonoTlsSettings
	{
		public MonoRemoteCertificateValidationCallback ServerCertificateValidationCallback {
			get; set;
		}

		public MonoLocalCertificateSelectionCallback ClientCertificateSelectionCallback {
			get; set;
		}

		public bool CheckCertificateName {
			get { return checkCertName; }
			set { checkCertName = value; }
		}

		public bool CheckCertificateRevocationStatus {
			get { return checkCertRevocationStatus; }
			set { checkCertRevocationStatus = value; }
		}

		public bool UseServicePointManagerCallback {
			get { return useServicePointManagerCallback; }
			set { useServicePointManagerCallback = value; }
		}

		public bool SkipSystemValidators {
			get { return skipSystemValidators; }
			set { skipSystemValidators = value; }
		}

		public bool CallbackNeedsCertificateChain {
			get { return callbackNeedsChain; }
			set { callbackNeedsChain = value; }
		}

		public object UserSettings {
			get; set;
		}

		bool checkCertName = true;
		bool checkCertRevocationStatus = false;
		bool useServicePointManagerCallback = false;
		bool skipSystemValidators = false;
		bool callbackNeedsChain = true;
	}
}

