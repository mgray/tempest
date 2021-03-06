﻿//
// Extensions.cs
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tempest
{
	public static class Extensions
	{
		/// <summary>
		/// Writes a date value.
		/// </summary>
		public static void WriteDate (this IValueWriter writer, DateTime date)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			writer.WriteInt64 (date.ToBinary());
		}

		/// <summary>
		/// Reads a date value.
		/// </summary>
		public static DateTime ReadDate (this IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return DateTime.FromBinary (reader.ReadInt64());
		}

		#if NET_4
		private static readonly ConcurrentDictionary<Type, ObjectSerializer> Serializers = new ConcurrentDictionary<Type, ObjectSerializer>();
		#else
		private static readonly Dictionary<Type, ObjectSerializer> Serializers = new Dictionary<Type, ObjectSerializer> ();
		#endif

		public static void Write (this IValueWriter writer, object value)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			if (value == null)
			{
				writer.WriteBool (false);
				return;
			}

			ObjectSerializer serializer = GetSerializer (value.GetType());
			serializer.Serialize (writer, value);
		}

		private static ObjectSerializer GetSerializer(Type type)
		{
			ObjectSerializer serializer;
			#if NET_4
			serializer = Serializers.GetOrAdd (type, t => new ObjectSerializer (t));
			#else
			bool exists;
			lock (Serializers)
				exists = Serializers.TryGetValue (type, out serializer);

			if (!exists)
			{
				serializer = new ObjectSerializer (type);
				lock (Serializers)
				{
					if (!Serializers.ContainsKey (type))
						Serializers.Add (type, serializer);
				}
			}
			#endif

			return serializer;
		}

		public static T Read<T> (this IValueReader reader)
			where T : new()
		{
			return (T)reader.Read (typeof (T));
		}

		internal static object Read (this IValueReader reader, Type type)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return GetSerializer (type).Deserialize (reader);
		}
	}
}