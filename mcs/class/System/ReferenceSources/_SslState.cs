//
// Mono-specific additions to Microsoft's _SslState.cs
//
#if MONO_FEATURE_NEW_TLS && SECURITY_DEP
namespace System.Net.Security
{
	using System.IO;
	using System.Threading;
	using System.Net.Sockets;

	partial class SslState
	{
		int sentShutdown;

		internal bool IsClosed {
			get { return Context.IsClosed; }
		}

		internal IAsyncResult BeginShutdown (AsyncCallback asyncCallback, object asyncState)
		{
			var lazyResult = new LazyAsyncResult (this, asyncState, asyncCallback);
			var asyncRequest = new AsyncProtocolRequest (lazyResult);

			if (Interlocked.CompareExchange (ref sentShutdown, 1, 0) == 1) {
				asyncRequest.CompleteUser ();
			} else {
				ProtocolToken message = Context.CreateShutdownMessage ();
				StartWriteShutdown (message.Payload, asyncRequest);
			}

			return lazyResult;
		}

		internal void EndShutdown (IAsyncResult asyncResult)
		{
			if (asyncResult == null) {
				throw new ArgumentNullException ("asyncResult");
			}

			LazyAsyncResult lazyResult = asyncResult as LazyAsyncResult;
			if (lazyResult == null) {
				throw new ArgumentException (SR.GetString (SR.net_io_async_result, asyncResult.GetType ().FullName), "asyncResult");
			}

			// No "artificial" timeouts implemented so far, InnerStream controls timeout.
			lazyResult.InternalWaitForCompletion ();

			if (lazyResult.Result is Exception) {
				if (lazyResult.Result is IOException) {
					throw (Exception)lazyResult.Result;
				}
				throw new IOException (SR.GetString (SR.mono_net_io_shutdown), (Exception)lazyResult.Result);
			}
		}

		void StartWriteShutdown (byte[] buffer, AsyncProtocolRequest asyncRequest)
		{
			// request a write IO slot
			if (CheckEnqueueWrite (asyncRequest)) {
				// operation is async and has been queued, return.
				return;
			}

			byte[] lastHandshakePayload = null;
			if (LastPayload != null)
				throw new NotImplementedException ();

			if (asyncRequest != null) {
				// prepare for the next request
				IAsyncResult ar = ((NetworkStream)InnerStream).BeginWrite (buffer, 0, buffer.Length, ShutdownCallback, asyncRequest);
				if (!ar.CompletedSynchronously)
					return;

				((NetworkStream)InnerStream).EndWrite (ar);
			} else {
				((NetworkStream)InnerStream).Write (buffer, 0, buffer.Length);
			}

			// release write IO slot
			FinishWrite ();

			if (asyncRequest != null)
				asyncRequest.CompleteUser ();
		}

		static void ShutdownCallback (IAsyncResult transportResult)
		{
			if (transportResult.CompletedSynchronously)
				return;

			AsyncProtocolRequest asyncRequest = (AsyncProtocolRequest)transportResult.AsyncState;

			SslState sslState = (SslState)asyncRequest.AsyncObject;
			try {
				((NetworkStream)(sslState.InnerStream)).EndWrite (transportResult);
				sslState.FinishWrite ();
				asyncRequest.CompleteUser ();
			} catch (Exception e) {
				if (asyncRequest.IsUserCompleted) {
					// This will throw on a worker thread.
					throw;
				}
				sslState.FinishWrite ();
				asyncRequest.CompleteWithError (e);
			}
		}
	}
}
#endif
