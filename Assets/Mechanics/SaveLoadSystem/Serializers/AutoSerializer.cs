using System;
using System.Reflection;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class AutoSerializeFieldAttribute : System.Attribute
{
	public readonly System.Type serializerType;
	// This is a positional argument
	public AutoSerializeFieldAttribute(System.Type serializerType)
	{
		this.serializerType = serializerType;
	}
}

public static class AutoSerializer
{


	public static void AutoWrite<T>(in GameDataWriter writer, T data)
	{
		foreach (FieldInfo field in typeof(T).GetFields())
		{
			var attrib = field.GetCustomAttribute<AutoSerializeFieldAttribute>();
			if (attrib != null)
			{
				// var type = typeof(IBinaryVariableSerializer<>);
				// type = type.MakeGenericType(field.FieldType);
				var s = Activator.CreateInstance(attrib.serializerType, field.GetValue(data));
				var writeMethod = s.GetType().GetMethod("Write");
				writeMethod.Invoke(s, new object[] { writer });
			}
			else if (field.FieldType.GetMethod("Write") is var info)
			{
				info.Invoke(field.GetValue(data), new object[] { writer });
			}
		}
	}
	public static T AutoRead<T>(in GameDataReader reader) where T : new()
	{
		//data is object for boxing
		object data = new T();
		foreach (FieldInfo field in typeof(T).GetFields())
		{
			var attrib = field.GetCustomAttribute<AutoSerializeFieldAttribute>();
			if (attrib != null)
			{
				//Get the type of the serializer
				object s = Activator.CreateInstance(attrib.serializerType, field.GetValue(data));


				MethodInfo readMethod = s.GetType().GetMethod("Read");

				object value = readMethod.Invoke(s, new object[] { reader });
				if (field.FieldType != readMethod.ReturnType)
				{
					//Convert
					MethodInfo converterMethod = s.GetType().GetMethod("op_Implicit", new[] { attrib.serializerType });
					value = converterMethod.Invoke(s, new object[] { value });
				}
				field.SetValue(data, value);
			}
			else if (field.FieldType.GetMethod("Read") is var info)
			{
				field.SetValue(data, info.Invoke(field.GetValue(data), new object[] { reader }));
			}
		}
		return (T)data;
	}
}