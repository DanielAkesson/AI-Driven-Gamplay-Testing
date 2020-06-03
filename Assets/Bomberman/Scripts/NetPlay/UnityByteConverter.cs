using Netkraft.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnityByteConverter
{
    internal static byte[] buffer = new byte[1024];

    //Vector2
    [WriteFunction(typeof(Vector2))]
    internal static void WriteVector2(Stream stream, object value)
    {
        stream.Write(BitConverter.GetBytes(((Vector2)value).x), 0, 4);
        stream.Write(BitConverter.GetBytes(((Vector2)value).y), 0, 4);
    }
    [ReadFunction(typeof(Vector2))]
    internal static object ReadVector2(Stream stream)
    {
        stream.Read(buffer, 0, 8);
        return new Vector2(BitConverter.ToSingle(buffer, 0), BitConverter.ToSingle(buffer, 4));
    }
    //Vector3
    [WriteFunction(typeof(Vector3))]
    internal static void WriteVector3(Stream stream, object value)
    {
        stream.Write(BitConverter.GetBytes(((Vector3)value).x), 0, 4);
        stream.Write(BitConverter.GetBytes(((Vector3)value).y), 0, 4);
        stream.Write(BitConverter.GetBytes(((Vector3)value).z), 0, 4);
    }
    [ReadFunction(typeof(Vector2))]
    internal static object ReadVector3(Stream stream)
    {
        stream.Read(buffer, 0, 12);
        return new Vector3(BitConverter.ToSingle(buffer, 0), BitConverter.ToSingle(buffer, 4), BitConverter.ToSingle(buffer, 8));
    }
}
