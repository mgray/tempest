﻿//
// IClientConnection.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2010 Eric Maupin
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
using System.Linq;
using System.Net;

namespace Tempest
{
	public interface IClientConnection
		: IConnection
	{
		/// <summary>
		/// Raised when the connection has connected.
		/// </summary>
		event EventHandler<ClientConnectionEventArgs> Connected;

		/// <summary>
		/// Raised when a connection attempt fails.
		/// </summary>
		event EventHandler<ClientConnectionEventArgs> ConnectionFailed;

		/// <summary>
		/// Attempts to connect to the <paramref name="endpoint"/> for <paramref name="messageTypes"/>.
		/// </summary>
		/// <param name="endpoint">The endpoint to connect to.</param>
		/// <param name="messageTypes"></param>
		/// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
		void Connect (EndPoint endpoint, MessageTypes messageTypes);
	}

	/// <summary>
	/// Holds data for client-connection based events.
	/// </summary>
	public class ClientConnectionEventArgs
		: EventArgs
	{
		/// <summary>
		/// Creates a new instance of the <see cref="ClientConnectionEventArgs"/> class.
		/// </summary>
		/// <param name="connection">The connection for the event.</param>
		public ClientConnectionEventArgs (IClientConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			Connection = connection;
		}

		/// <summary>
		/// Gets the connection for the event.
		/// </summary>
		public IClientConnection Connection
		{
			get;
			private set;
		}
	}
}