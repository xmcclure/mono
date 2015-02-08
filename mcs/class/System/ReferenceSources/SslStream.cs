//
// Mono-specific additions to Microsoft's SslStream.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace System.Net.Security
{
	using System.Net.Sockets;
	using System.IO;

	partial class SslStream
	{
		#if SECURITY_DEP
		SSPIConfiguration _Configuration;

		internal SslStream (Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, 
		                         LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy, MonoTlsSettings settings)
			: base (innerStream, leaveInnerStreamOpen)
		{
			if (encryptionPolicy != EncryptionPolicy.RequireEncryption && encryptionPolicy != EncryptionPolicy.AllowNoEncryption && encryptionPolicy != EncryptionPolicy.NoEncryption)
				throw new ArgumentException (SR.GetString (SR.net_invalid_enum, "EncryptionPolicy"), "encryptionPolicy");

			_userCertificateValidationCallback = userCertificateValidationCallback;
			_userCertificateSelectionCallback = userCertificateSelectionCallback;
			RemoteCertValidationCallback _userCertValidationCallbackWrapper = new RemoteCertValidationCallback (userCertValidationCallbackWrapper);
			LocalCertSelectionCallback _userCertSelectionCallbackWrapper = userCertificateSelectionCallback == null ? null : new LocalCertSelectionCallback (userCertSelectionCallbackWrapper);
			_Configuration = WrapSettings (settings);
			_SslState = new SslState (innerStream, _userCertValidationCallbackWrapper, _userCertSelectionCallbackWrapper, encryptionPolicy, _Configuration);
		}

		static SSPIConfiguration WrapSettings (MonoTlsSettings settings)
		{
			return settings != null ? new MyConfiguration (settings) : null;
		}

		class MyConfiguration : SSPIConfiguration
		{
			MonoTlsSettings settings;

			public MyConfiguration (MonoTlsSettings settings)
			{
				this.settings = settings;
			}

			public MonoTlsSettings Settings {
				get { return settings; }
			}
		}
		#endif

		internal bool IsClosed {
			get { return _SslState.IsClosed; }
		}

		internal Exception LastError {
			get { return _SslState.LastError; }
		}

		internal IAsyncResult BeginShutdown (bool waitForReply, AsyncCallback asyncCallback, object asyncState)
		{
			return _SslState.BeginShutdown (waitForReply, asyncCallback, asyncState);
		}

		internal void EndShutdown (IAsyncResult asyncResult)
		{
			_SslState.EndShutdown (asyncResult);
		}
	}
}
#endif
