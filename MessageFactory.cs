﻿//
// MessageFactory.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if !SAFE
using System.Reflection.Emit;
#endif

namespace Tempest
{
	public class MessageFactory
	{
		internal MessageFactory()
		{
		}

		/// <summary>
		/// Discovers and registers message types from <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly">The assembly to discover message types from.</param>
		/// <seealso cref="Discover()"/>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <c>null</c>.</exception>
		public void Discover (Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			Type mtype = typeof (Message);
			Register (assembly.GetTypes().Where (t => t.IsPublic && t.IsClass && mtype.IsAssignableFrom (t)));
		}

		/// <summary>
		/// Discovers and registers messages from the calling assembly.
		/// </summary>
		/// <seealso cref="Discover(System.Reflection.Assembly)"/>
		public void Discover ()
		{
			Discover (Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Registers types with their parameter-less constructor.
		/// </summary>
		/// <param name="messageTypes"></param>
		public void Register (IEnumerable<KeyValuePair<Type, Func<Message>>> messageTypes)
		{
			if (messageTypes == null)
				throw new ArgumentNullException ("messageTypes");

			lock (messageCtors)
			{
				foreach (var kvp in messageTypes)
				{
					Message m = kvp.Value();
					if (this.messageCtors.ContainsKey (m.MessageType))
						throw new ArgumentException (String.Format ("A message of type {0} has already been registered.", m.MessageType), "messageTypes");

					this.messageCtors.Add (m.MessageType, kvp.Value);
				}
			}
		}

		#if !SAFE

		/// <exception cref="ArgumentNullException"><paramref name="messageTypes"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="messageTypes"/> contains a type that is not an implementation of <see cref="Message"/>,
		/// has no parameter-less constructor or has no <see cref="IValueReader"/> constructor.
		/// </exception>
		public void Register (IEnumerable<Type> messageTypes)
		{
			if (messageTypes == null)
				throw new ArgumentNullException ("messageTypes");
			
			Type mtype = typeof (Message);

			Dictionary<Type, Func<Message>> types = new Dictionary<Type, Func<Message>>();
			foreach (Type t in messageTypes)
			{
				if (!t.IsPublic || !t.IsClass || !mtype.IsAssignableFrom (t))
					throw new ArgumentException (String.Format ("{0} is not an implementation of Message", t.Name), "messageTypes");

				ConstructorInfo plessCtor = t.GetConstructor (Type.EmptyTypes);
				if (plessCtor == null)
					throw new ArgumentException (String.Format ("{0} has no parameter-less constructor", t.Name), "messageTypes");

				var dplessCtor = new DynamicMethod ("plessCtor", mtype, Type.EmptyTypes);
				var il = dplessCtor.GetILGenerator();
				il.Emit (OpCodes.Newobj, plessCtor);
				il.Emit (OpCodes.Ret);

				types.Add (t, (Func<Message>)dplessCtor.CreateDelegate (typeof (Func<Message>)));
			}

			Register (types);
		}

		#endif

		/// <summary>
		/// Creates a new instance of the <paramref name="messageType"/>.
		/// </summary>
		/// <param name="messageType"></param>
		/// <returns>A new instance of the <paramref name="messageType"/>, or <c>null</c> if this type has not been registered.</returns>
		public Message Create (ushort messageType)
		{
			Func<Message> mCtor;
			lock (this.messageCtors)
			{
				if (!this.messageCtors.TryGetValue (messageType, out mCtor))
					return null;
			}

			return mCtor();
		}

		private readonly Dictionary<ushort, Func<Message>> messageCtors = new Dictionary<ushort, Func<Message>>();
	}
}