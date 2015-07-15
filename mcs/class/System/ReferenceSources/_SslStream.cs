//
// Mono-specific additions to Microsoft's _SslStream.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
namespace System.Net.Security
{
	using System.IO;
	using System.Threading;
	using System.Net.Sockets;

	partial class _SslStream
	{
		int _SentShutdown;

		internal IAsyncResult BeginShutdown (AsyncCallback asyncCallback, object asyncState)
		{
			LazyAsyncResult lazyResult = new LazyAsyncResult (this, asyncState, asyncCallback);
			AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest (lazyResult);

			if (Interlocked.Exchange (ref _NestedWrite, 1) == 1)
			{
				throw new NotSupportedException (SR.GetString (SR.net_io_invalidnestedcall, (asyncRequest != null ? "BeginShutdown" : "Shutdown"), "shutdown"));
			}

			if (Interlocked.CompareExchange (ref _SentShutdown, 1, 0) == 1) {
				asyncRequest.CompleteUser ();
				return lazyResult;
			}

			bool failed = false;
			try
			{
				ProtocolToken message = _SslState.CreateShutdownMessage ();
				if (asyncRequest != null)
					asyncRequest.SetNextRequest (message.Payload, 0, message.Size, _ResumeAsyncWriteCallback);

				StartHandshakeWrite (message, asyncRequest);
			}
			catch (Exception e)
			{
				_SslState.FinishWrite ();

				failed = true;
				if (e is IOException) {
					throw;
				}
				throw new IOException (SR.GetString (SR.mono_net_io_shutdown), e);
			}
			finally
			{
				if (asyncRequest == null || failed)
				{
					_NestedWrite = 0;
				}
			}

			return lazyResult;
		}

		internal void EndShutdown (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}

			LazyAsyncResult lazyResult = asyncResult as LazyAsyncResult;
			if (lazyResult == null)
			{
				throw new ArgumentException (SR.GetString (SR.net_io_async_result, asyncResult.GetType ().FullName), "asyncResult");
			}

			if (Interlocked.Exchange (ref _NestedWrite, 0) == 0)
			{
				throw new InvalidOperationException (SR.GetString (SR.net_io_invalidendcall, "EndShutdown"));
			}

			// No "artificial" timeouts implemented so far, InnerStream controls timeout.
			lazyResult.InternalWaitForCompletion();

			if (lazyResult.Result is Exception)
			{
				if (lazyResult.Result is IOException)
				{
					throw (Exception)lazyResult.Result;
				}
				throw new IOException(SR.GetString(SR.mono_net_io_shutdown), (Exception)lazyResult.Result);
			}
		}

		internal IAsyncResult BeginRenegotiate (AsyncCallback asyncCallback, object asyncState)
		{
			if (!_SslState.IsServer)
				throw new NotImplementedException ();

			LazyAsyncResult lazyResult = new LazyAsyncResult (this, asyncState, asyncCallback);
			AsyncProtocolRequest asyncRequest = new AsyncProtocolRequest (lazyResult);

			if (Interlocked.Exchange (ref _NestedWrite, 1) == 1)
			{
				throw new NotSupportedException (SR.GetString (SR.net_io_invalidnestedcall, (asyncRequest != null ? "BeginRenegotiate" : "Renegotiate"), "renegotiate"));
			}

			bool failed = false;
			try
			{
				ProtocolToken message = _SslState.CreateHelloRequestMessage ();
				if (asyncRequest != null)
					asyncRequest.SetNextRequest (message.Payload, 0, message.Size, _ResumeAsyncWriteCallback);

				StartHandshakeWrite (message, asyncRequest);
			}
			catch (Exception e)
			{
				_SslState.FinishWrite ();

				failed = true;
				if (e is IOException) {
					throw;
				}
				throw new IOException (SR.GetString (SR.mono_net_io_renegotiate), e);
			}
			finally
			{
				if (asyncRequest == null || failed)
				{
					_NestedWrite = 0;
				}
			}

			return lazyResult;
		}

		internal void EndRenegotiate (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}

			LazyAsyncResult lazyResult = asyncResult as LazyAsyncResult;
			if (lazyResult == null)
			{
				throw new ArgumentException (SR.GetString (SR.net_io_async_result, asyncResult.GetType ().FullName), "asyncResult");
			}

			if (Interlocked.Exchange (ref _NestedWrite, 0) == 0)
			{
				throw new InvalidOperationException (SR.GetString (SR.net_io_invalidendcall, "EndRenegotiate"));
			}

			// No "artificial" timeouts implemented so far, InnerStream controls timeout.
			lazyResult.InternalWaitForCompletion();

			if (lazyResult.Result is Exception)
			{
				if (lazyResult.Result is IOException)
				{
					throw (Exception)lazyResult.Result;
				}
				throw new IOException(SR.GetString(SR.mono_net_io_renegotiate), (Exception)lazyResult.Result);
			}
		}

		void StartHandshakeWrite (ProtocolToken message, AsyncProtocolRequest asyncRequest)
		{
			// request a write IO slot
			if (_SslState.CheckEnqueueWrite (asyncRequest)) {
				// operation is async and has been queued, return.
				return;
			}

			if (_SslState.LastPayload != null)
				throw new NotImplementedException ();

			if (asyncRequest != null) {
				// prepare for the next request
				IAsyncResult ar = ((NetworkStream)_SslState.InnerStream).BeginWrite (message.Payload, 0, message.Size, HandshakeWriteCallback, asyncRequest);
				if (!ar.CompletedSynchronously)
					return;

				((NetworkStream)_SslState.InnerStream).EndWrite (ar);
			} else {
				((NetworkStream)_SslState.InnerStream).Write (message.Payload, 0, message.Size);
			}

			// release write IO slot
			_SslState.FinishWrite ();

			if (asyncRequest != null)
				asyncRequest.CompleteUser ();
		}

		static void HandshakeWriteCallback (IAsyncResult transportResult)
		{
			if (transportResult.CompletedSynchronously)
				return;

			AsyncProtocolRequest asyncRequest = (AsyncProtocolRequest)transportResult.AsyncState;

			_SslStream sslStream = (_SslStream)asyncRequest.AsyncObject;
			try {
				sslStream._SslState.InnerStream.EndWrite (transportResult);
				sslStream._SslState.FinishWrite ();
				asyncRequest.CompleteUser ();
			} catch (Exception e) {
				if (asyncRequest.IsUserCompleted) {
					// This will throw on a worker thread.
					throw;
				}
				sslStream._SslState.FinishWrite ();
				asyncRequest.CompleteWithError (e);
			}
		}
	}
}
#endif
