using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace ZEDRatApp.ZEDRAT.NetSerializer.TypeSerializers
{
	public class GenericSerializer : IDynamicTypeSerializer, ITypeSerializer
	{
		public bool Handles(Type type)
		{
			if (!type.IsSerializable)
			{
				throw new NotSupportedException($"Type {type.FullName} is not marked as Serializable");
			}
			if (typeof(ISerializable).IsAssignableFrom(type))
			{
				throw new NotSupportedException($"Cannot serialize {type.FullName}: ISerializable not supported");
			}
			return true;
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			foreach (FieldInfo fieldInfo in Helpers.GetFieldInfos(type))
			{
				yield return fieldInfo.FieldType;
			}
		}

		public void GenerateWriterMethod(Type type, CodeGenContext ctx, ILGenerator il)
		{
			foreach (FieldInfo fieldInfo in Helpers.GetFieldInfos(type))
			{
				TypeData typeDataForCall = ctx.GetTypeDataForCall(fieldInfo.FieldType);
				if (typeDataForCall.NeedsInstanceParameter)
				{
					il.Emit(OpCodes.Ldarg_0);
				}
				il.Emit(OpCodes.Ldarg_1);
				if (type.IsValueType)
				{
					il.Emit(OpCodes.Ldarga_S, 2);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_2);
				}
				il.Emit(OpCodes.Ldfld, fieldInfo);
				il.Emit(OpCodes.Call, typeDataForCall.WriterMethodInfo);
			}
			il.Emit(OpCodes.Ret);
		}

		public void GenerateReaderMethod(Type type, CodeGenContext ctx, ILGenerator il)
		{
			if (type.IsClass)
			{
				il.Emit(OpCodes.Ldarg_2);
				MethodInfo method = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public);
				MethodInfo method2 = typeof(FormatterServices).GetMethod("GetUninitializedObject", BindingFlags.Static | BindingFlags.Public);
				il.Emit(OpCodes.Ldtoken, type);
				il.Emit(OpCodes.Call, method);
				il.Emit(OpCodes.Call, method2);
				il.Emit(OpCodes.Castclass, type);
				il.Emit(OpCodes.Stind_Ref);
			}
			foreach (FieldInfo fieldInfo in Helpers.GetFieldInfos(type))
			{
				TypeData typeDataForCall = ctx.GetTypeDataForCall(fieldInfo.FieldType);
				if (typeDataForCall.NeedsInstanceParameter)
				{
					il.Emit(OpCodes.Ldarg_0);
				}
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldarg_2);
				if (type.IsClass)
				{
					il.Emit(OpCodes.Ldind_Ref);
				}
				il.Emit(OpCodes.Ldflda, fieldInfo);
				il.Emit(OpCodes.Call, typeDataForCall.ReaderMethodInfo);
			}
			if (typeof(IDeserializationCallback).IsAssignableFrom(type))
			{
				MethodInfo method3 = typeof(IDeserializationCallback).GetMethod("OnDeserialization", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(object) }, null);
				il.Emit(OpCodes.Ldarg_2);
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Constrained, type);
				il.Emit(OpCodes.Callvirt, method3);
			}
			il.Emit(OpCodes.Ret);
		}
	}
}
