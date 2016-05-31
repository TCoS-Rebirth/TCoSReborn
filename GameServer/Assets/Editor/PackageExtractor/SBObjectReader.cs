using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Common;

namespace PackageExtractor
{
    public class SBProperty
    {
        public Dictionary<string, SBProperty> Array = new Dictionary<string, SBProperty>(StringComparer.OrdinalIgnoreCase);
            //If the property is an ArrayProperty, this member is filled //or a struct now

        public string ArrayIndex = "";
        public string EnumValueName = "";
        public bool hasSkippedBytes;
        public string Name = "";
        public int serialSize;
        public string Size = "";
        public string StructName = "";
        public PropertyType Type;
        public string Value = "";

        public string ToString(int indentLevel)
        {
            var padding = new string(' ', indentLevel);
            var builder = new StringBuilder();
            if (Array.Count == 0)
            {
                if (Type == PropertyType.ByteProperty && EnumValueName != "") //then it's most likely an enum, so try parse from overwritten name
                {
                    builder.AppendLine(padding + string.Format("<b>Type:</b> <i>{0}</i>", Name));
                    builder.AppendLine(padding + string.Format("<b>Value:</b> ({0}) {1}", Value, EnumValueName));
                }
                else
                {
                    builder.AppendLine(padding + string.Format("<b>Name:</b> <i>{0}</i> ({1})", Name, Type));
                    builder.AppendLine(padding + "<b>Value:</b> " + Value);
                }
            }
            else
            {
                builder.AppendLine(padding + string.Format("<b>Name:</b> <i>{0}</i> ({1})", Name, Type));
            }
            if (ArrayIndex.Length > 0)
                builder.AppendLine(padding + "ArrayIndex: " + ArrayIndex);
            if (StructName.Length > 0)
                builder.AppendLine(padding + "StructName: " + StructName);
            if (Array.Count > 0)
            {
                builder.AppendLine(padding + "{");
                var curIndex = 0;
                foreach (var prop in IterateInnerProperties())
                {
                    if (Type == PropertyType.ArrayProperty || Type == PropertyType.FixedArrayProperty)
                    {
                        builder.AppendLine(padding + string.Format("[{0}]:", curIndex));
                    }
                    builder.Append(prop.ToString(indentLevel + 4));
                    if (curIndex < Array.Count - 1)
                    {
                        builder.AppendLine();
                    }
                    curIndex++;
                }
                builder.AppendLine(padding + "}");
            }
            return builder.ToString();
        }

        public T GetValue<T>() where T : IConvertible
        {
            return (T) Convert.ChangeType(Value.Replace("\0", string.Empty), typeof (T));
        }

        public IEnumerable<SBProperty> IterateInnerProperties()
        {
            foreach (var sbp in Array.Values)
            {
                yield return sbp;
            }
        }

        public SBProperty GetInnerProperty(string propName)
        {
            foreach (var sbProp in Array.Values)
            {
                if (sbProp.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                {
                    return sbProp;
                }
            }
            return null;
        }
    }

    public class SBObject
    {
        public SBClass Class;
        public string ClassName;
        public List<ObjectFlags> Flags;
        public string Name;
        public string Package;
        public int PackageID = -1;
        //Name => SBproperty
        public Dictionary<string, SBProperty> Properties;
        public int Size;
        public string SuperClassName;

        public IEnumerable<SBProperty> IterateProperties()
        {
            foreach (var sbp in Properties.Values)
            {
                yield return sbp;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("--------------------------------------------");
            builder.AppendLine("Object name: " + Name);
            builder.AppendLine("Object class: " + ClassName);
            //builder.AppendLine("Object super class: " + SuperClassName);
            builder.AppendLine("Object Package: " + Package);
            builder.AppendLine("Object Size: " + Size);
            if (Flags.Count > 0)
            {
                builder.Append("Object Flags: ");
                foreach (var flag in Flags)
                    builder.Append(flag + "; ");
            }
            builder.AppendLine("");
            builder.AppendLine("Class details: ");
            builder.AppendLine(Class.ToString());
            if (Properties.Count > 0)
            {
                builder.AppendLine("Properties (" + Properties.Count + "):");
                builder.AppendLine();
                foreach (var prop in IterateProperties())
                {
                    //builder.AppendLine(string.Format("<i><b>{0}:</b></i> ", prop.Name));
                    builder.AppendLine(prop.ToString(0));
                }
            }
            else
                builder.AppendLine("No Properties");

            return builder.ToString();
        }
    }

    public class SBClass
    {
        public SBClass Ancestor;
        //Name => Value
        public Dictionary<string, string> Fields;

        //temp
        public string HexaDump;
        public string Name;

        public SBClass()
        {
            Name = "null";
            Fields = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Class name: " + Name);
            if (Ancestor != null)
            {
                builder.AppendLine("");
                builder.AppendLine("Class ancestor: " + Ancestor.Name);
                builder.Append(Ancestor.FieldsToString());
            }
            else
                builder.AppendLine("Class ancestor: null");

            builder.Append(FieldsToString());

            return builder.ToString();
        }

        public string FieldsToString()
        {
            var builder = new StringBuilder();
            if (Fields.Count > 0)
            {
                builder.AppendLine("Fields: ");
                foreach (var keyValue in Fields)
                {
                    builder.AppendLine(keyValue.Key + ": " + keyValue.Value);
                }
                return builder.ToString();
            }
            return string.Empty;
        }
    }

    internal class SBObjectReader
    {
        Package package;

        public SBObjectReader(Package package)
        {
            this.package = package;
        }

        public SBObject ReadObject(SBFileReader fileReader, ExportEntry entry, PackageReadConfig arrayDefinitions, bool showAncestors = true)
        {
            var activeObject = new SBObject();
            activeObject.Class = new SBClass();
            activeObject.Name = package.NameTable[entry.ObjectName].Replace("\0", string.Empty);
            activeObject.Flags = new List<ObjectFlags>();
            activeObject.Properties = new Dictionary<string, SBProperty>(StringComparer.OrdinalIgnoreCase);
            activeObject.ClassName = package.FindReferenceName(entry.ClassReference, showAncestors);
            activeObject.Package = package.FindReferenceName(entry.PackageReference, showAncestors);
            activeObject.Size = entry.SerialSize;
            activeObject.SuperClassName = package.FindReferenceName(entry.SuperReference, showAncestors);
            activeObject.PackageID = entry.PackageReference;

            var hasExecutionStack = false;

            //Detect stack
            if ((entry.ObjectFlags & (int) ObjectFlags.RF_HasStack) != 0)
            {
                hasExecutionStack = true;
                activeObject.Flags.Add(ObjectFlags.RF_HasStack);
            }

            //Currently we cannot read null class
            if (entry.SerialSize <= 0 || activeObject.ClassName == "null")
                return activeObject;

            //Read object data if present

            fileReader.Seek(entry.SerialOffset, SeekOrigin.Begin);

            foreach (var s in arrayDefinitions.skippableObjects)
            {
                if (activeObject.ClassName.Replace("\0", string.Empty) == s)
                {
                    fileReader.Seek(entry.SerialSize, SeekOrigin.Current);
                    return null;
                }
            }
            //If object has a stack, read it but do nothing with it
            if (hasExecutionStack)
            {
                var stateFrameNode = fileReader.ReadIndex();
                fileReader.ReadIndex();
                fileReader.ReadInt64();
                fileReader.ReadInt32();
                if (stateFrameNode != 0)
                {
                    fileReader.ReadIndex();
                }
            }

            ReadProperties(fileReader, activeObject, entry, arrayDefinitions, showAncestors);

            //Try to read class

            string className;

            if (activeObject.ClassName.Contains("("))
            {
                className = activeObject.ClassName.Substring(0, activeObject.ClassName.IndexOf("(") - 1).Trim();
            }
            else
            {
                className = activeObject.ClassName;
            }

            switch (className)
            {
                case "Const":
                    activeObject.Class = ReadConstClass(fileReader);
                    break;
                case "Enum":
                    activeObject.Class = ReadEnumClass(fileReader);
                    break;
                case "Property":
                    activeObject.Class = ReadPropertyClass(fileReader);
                    break;
                case "ByteProperty":
                    activeObject.Class = ReadBytePropertyClass(fileReader);
                    break;
                case "ObjectProperty":
                    activeObject.Class = ReadObjectPropertyClass(fileReader);
                    break;
                case "FixedArrayProperty":
                    activeObject.Class = ReadFixedArrayPropertyClass(fileReader);
                    break;
                case "ArrayProperty":
                    activeObject.Class = ReadArrayPropertyClass(fileReader);
                    break;
                case "MapProperty":
                    activeObject.Class = ReadMapPropertyClass(fileReader);
                    break;
                case "ClassProperty":
                    activeObject.Class = ReadClassPropertyClass(fileReader);
                    break;
                case "StructProperty":
                    activeObject.Class = ReadStructPropertyClass(fileReader);
                    break;
                case "IntProperty":
                    activeObject.Class = ReadIntPropertyClass(fileReader);
                    break;
                case "BoolProperty":
                    activeObject.Class = ReadBoolPropertyClass(fileReader);
                    break;
                case "FloatProperty":
                    activeObject.Class = ReadFloatPropertyClass(fileReader);
                    break;
                case "NameProperty":
                    activeObject.Class = ReadNamePropertyClass(fileReader);
                    break;
                case "StrProperty":
                    activeObject.Class = ReadStrPropertyClass(fileReader);
                    break;
                case "StringProperty":
                    activeObject.Class = ReadIntPropertyClass(fileReader);
                    break;
                case "Struct":
                    activeObject.Class = ReadStructClass(fileReader);
                    break;
                case "Function":
                    activeObject.Class = ReadFunctionClass(fileReader);
                    break;
                case "State":
                    activeObject.Class = ReadStateClass(fileReader);
                    break;
                case "null":
                case "None":
                    activeObject.Class = ReadNullClass(fileReader);
                    break;
                default:
                    //package.Log("Encountered unknown class while reading properties", 1);
                    break;
            }
            return activeObject;
        }

        void ReadProperties(SBFileReader fileReader, SBObject activeObject, ExportEntry entry, PackageReadConfig arrayDefinitions, bool showAncestors)
        {
            //.::Loop through all the properties of the current object::.
            var trashInt = 0;
            do
            {
                var activeProperty = ReadProperty(activeObject, fileReader, arrayDefinitions, showAncestors, ref trashInt);
                if (activeProperty == null)
                {
                    break;
                }

                ReadPropertyValue(activeObject, fileReader, activeProperty, arrayDefinitions, showAncestors);

                var success = false;
                var key = activeProperty.Name;
                var cpt = 0;
                do
                {
                    try
                    {
                        activeObject.Properties.Add(key, activeProperty);
                        success = true;
                    }
                    catch (ArgumentException)
                    {
                        success = false;
                        key = key + cpt;
                        ++cpt;
                    }
                } while (!success);
            } while (fileReader.Position < entry.SerialOffset + entry.SerialSize);
        }

        SBProperty ReadProperty(SBObject activeObject, SBFileReader fileReader, PackageReadConfig arrayDefinitions, bool showAncestors, ref int returnBytesRead,
            PropertyType overrideType = PropertyType.UnknownProperty)
        {
            var readIndexBytes = 0;
            int propertyNameIndex;
            var activeProperty = new SBProperty();
            propertyNameIndex = fileReader.ReadIndex(ref readIndexBytes);
            returnBytesRead += readIndexBytes;
            try
            {
                activeProperty.Name = package.NameTable[propertyNameIndex].Replace("\0", string.Empty);
                //activeProperty.Name = package.FindReferenceName(propertyNameIndex).Replace("\0", string.Empty);
            }
            catch (IndexOutOfRangeException)
            {
                package.Log("Error: failed to read a Property's name on: " + activeObject.Name, 2);
                return null;
            }

            //Detect end of property list
            if (activeProperty.Name == "DRFORTHEWIN" || activeProperty.Name == "None")
                return null;

            //Read the property info byte
            var infoByte = fileReader.ReadByte();
            returnBytesRead += 1;

            //Parse property info byte
            var type = (byte) (infoByte & 0x0F);
            var size = (byte) ((infoByte & 0x70) >> 4);
            var arrayFlag = (byte) (infoByte & 0x80);

            if (overrideType != PropertyType.UnknownProperty)
            {
                activeProperty.Type = overrideType;
            }
            else
            {
                activeProperty.Type = (PropertyType) type;
            }

            //Arrayflag can also be the value of the property if it's a boolean property (see doc)
            if (arrayFlag != 0 && activeProperty.Type != PropertyType.BooleanProperty)
            {
                //Read arrayIndex, array index format is in the doc
                int arrayIndex = fileReader.ReadByte();
                returnBytesRead += 1;

                if (arrayIndex >= 128 || arrayIndex == 0)
                {
                    fileReader.Seek(-1, SeekOrigin.Current);
                    arrayIndex = fileReader.ReadInt16();
                    returnBytesRead += 1;

                    if (arrayIndex >= 16384)
                    {
                        fileReader.Seek(-2, SeekOrigin.Current);
                        arrayIndex = fileReader.ReadInt32() - (0xC0 << 24);
                        returnBytesRead += 2;
                    }
                    else
                        arrayIndex -= 0x80 << 8;
                }
                activeProperty.ArrayIndex = arrayIndex.ToString();
            }
            //Display value if not array flag but boolean value
            else if (activeProperty.Type == PropertyType.BooleanProperty)
                activeProperty.Value = arrayFlag > 0 ? "True" : "False";

            //If type = struct, you have to read its name (but not always)
            if (activeProperty.Type == PropertyType.StructProperty)
            {
                int structNameIndex;
                structNameIndex = fileReader.ReadIndex(ref readIndexBytes);
                returnBytesRead += readIndexBytes;
                activeProperty.StructName = package.NameTable[structNameIndex].Replace("\0", string.Empty);
            }
            if (activeProperty.Type != PropertyType.BooleanProperty)
            {
                var realSize = 0;
                //Read real size (see doc)
                if (size == 0)
                    realSize = 1;
                else if (size == 1)
                    realSize = 2;
                else if (size == 2)
                    realSize = 4;
                else if (size == 3)
                    realSize = 12;
                else if (size == 4)
                    realSize = 16;
                else if (size == 5)
                {
                    realSize = fileReader.ReadByte();
                    returnBytesRead += 1;
                }
                else if (size == 6)
                {
                    realSize = fileReader.ReadInt16();
                    returnBytesRead += 2;
                }
                else if (size == 7)
                {
                    realSize = fileReader.ReadInt32();
                    returnBytesRead += 4;
                }

                activeProperty.Size = realSize.ToString();
                activeProperty.serialSize = realSize;
            }
            else
            {
                activeProperty.Size = "1";
                activeProperty.serialSize = 1;
            }
            return activeProperty;
        }

        int ReadPropertyValue(SBObject activeObject, SBFileReader fileReader, SBProperty activeProperty, PackageReadConfig arrayDefinitions, bool showAncestors)
        {
            var localBytesRead = 0;
            //Handle specific properties
            var readIndexBytes = 0;

            switch (activeProperty.Type)
            {
                case PropertyType.ByteProperty:
                    activeProperty.Value = fileReader.ReadByte().ToString();
                    //try parsing enum from custom definition
                    var enums = Assembly.GetAssembly(typeof (GameConfiguration)).GetTypes().Where(t => t.IsEnum && t.IsPublic).ToList();
                    foreach (var et in enums)
                    {
                        if (et.Name.Equals(activeProperty.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            var nameIndexer = activeProperty.GetValue<int>();
                            var value = (Enum) Enum.ToObject(et, nameIndexer);
                            activeProperty.EnumValueName = value.ToString();
                            break;
                        }
                    }
                    //
                    localBytesRead += 1;
                    break;

                case PropertyType.IntegerProperty:
                    activeProperty.Value = fileReader.ReadInt32().ToString();
                    localBytesRead += 4;
                    break;

                case PropertyType.FloatProperty:
                    activeProperty.Value = fileReader.ReadFloat().ToString();
                    localBytesRead += 4;
                    break;

                case PropertyType.NameProperty:
                    activeProperty.Value = package.NameTable[fileReader.ReadIndex(ref readIndexBytes)].Replace("\0", string.Empty);
                    localBytesRead += readIndexBytes;
                    break;
                case PropertyType.VectorProperty:
                    activeProperty.Value = fileReader.ReadFloat() + "," + fileReader.ReadFloat() + "," + fileReader.ReadFloat();
                    localBytesRead += 12;
                    break;
                case PropertyType.RotatorProperty:
                    activeProperty.Value = fileReader.ReadInt32() + "," + fileReader.ReadInt32() + "," + fileReader.ReadInt32();
                    localBytesRead += 12;
                    break;
                case PropertyType.StrProperty:
                    int stringSize;
                    stringSize = fileReader.ReadIndex(ref readIndexBytes);
                    localBytesRead += readIndexBytes;
                    var stringBytes = fileReader.ReadBytes(stringSize);
                    var stringValue = Encoding.ASCII.GetString(stringBytes).Replace("\0", string.Empty);
                    activeProperty.Value = stringValue;
                    localBytesRead += stringBytes.Length;
                    break;

                case PropertyType.ObjectProperty:
                    var objectReference = fileReader.ReadIndex(ref readIndexBytes);
                    activeProperty.Value = package.FindReferenceName(objectReference, showAncestors) /*+ " (reference)"*/;
                    localBytesRead += readIndexBytes;
                    break;

                //Structs are a special case
                case PropertyType.CustomStruct:
                case PropertyType.StructProperty:
                    if (activeProperty.StructName == "Vector")
                    {
                        activeProperty.Value = fileReader.ReadFloat() + "," + fileReader.ReadFloat() + "," + fileReader.ReadFloat();
                        localBytesRead += 12;
                    }
                    else if (activeProperty.StructName == "Rotator")
                    {
                        activeProperty.Value = fileReader.ReadInt32() + "," + fileReader.ReadInt32() + "," + fileReader.ReadInt32();
                        localBytesRead += 12;
                    }
                    else if (activeProperty.StructName == "Color")
                    {
                        activeProperty.Value = "(R=" + fileReader.ReadByte() + "; G=" + fileReader.ReadByte() + "; B=" + fileReader.ReadByte() + "; A=" +
                                               fileReader.ReadByte() + ")";
                        localBytesRead += 3;
                    }
                    else if (activeProperty.StructName == "LightHSB")
                    {
                        activeProperty.Value = "(H=" + fileReader.ReadByte() + "; S=" + fileReader.ReadByte() + "; B=" + fileReader.ReadByte() + ")";
                        localBytesRead += 3;
                    }
                    else if (activeProperty.StructName == "LocalizedString")
                    {
                        activeProperty.Value = fileReader.ReadInt32().ToString();
                        localBytesRead += 4;
                    }
                    else if (activeProperty.StructName == "Slot")
                    {
                        activeProperty.Value = "Rank: " + fileReader.ReadInt32() + " SlotType: " + fileReader.ReadByte();
                        localBytesRead += 5;
                    }
                    else
                    {
                        SBProperty structProp = null;
                        do
                        {
                            var structBytesRead = 0;
                            structProp = ReadProperty(activeObject, fileReader, arrayDefinitions, showAncestors, ref structBytesRead);
                            localBytesRead += structBytesRead;
                            if (structProp == null)
                            {
                                break;
                            }
                            localBytesRead += ReadPropertyValue(activeObject, fileReader, structProp, arrayDefinitions, showAncestors);
                            activeProperty.Array.Add(structProp.Name, structProp);
                        } while (structProp != null);
                    }
                    break;
                case PropertyType.ArrayProperty:
                    var insideType = PropertyType.UnknownProperty;
                    var insideName = "";
                    if (arrayDefinitions.FindArrayType(activeObject.ClassName, activeProperty.Name, out insideType, out insideName))
                    {
                        var arraySize = fileReader.ReadIndex(ref readIndexBytes);
                        localBytesRead += readIndexBytes;
                        var insideArrayProp = new SBProperty();
                        for (var i = 0; i < arraySize; i++)
                        {
                            var propBytesRead = 0;
                            if (insideType == PropertyType.StructProperty)
                            {
                                insideArrayProp = ReadProperty(activeObject, fileReader, arrayDefinitions, showAncestors, ref propBytesRead, insideType);
                                localBytesRead += propBytesRead;
                            }
                            else
                            {
                                insideArrayProp = new SBProperty();
                                insideArrayProp.Type = insideType;
                                insideArrayProp.Name = insideName;
                            }
                            var currentPropSize = ReadPropertyValue(activeObject, fileReader, insideArrayProp, arrayDefinitions, showAncestors);
                            localBytesRead += currentPropSize;
                            insideArrayProp.Size = currentPropSize.ToString();
                            activeProperty.Array.Add(i.ToString(), insideArrayProp);
                        }
                        if (localBytesRead < activeProperty.serialSize)
                        {
                            activeProperty.hasSkippedBytes = true;
                        }
                        activeProperty.Size = arraySize.ToString();
                    }
                    else
                    {
                        package.Log(
                            string.Format("ArrayDefinition not found for ({2}){0} - {1}, skipping", activeObject.Name, activeProperty.Name, activeObject.ClassName), 1);
                        fileReader.Seek(activeProperty.serialSize, SeekOrigin.Current);
                    }
                    break;
                default:
                    fileReader.Seek(activeProperty.serialSize, SeekOrigin.Current);
                    break;
            }
            return localBytesRead;
        }

        //Unreal specific classes readers
        SBClass ReadFieldClass(SBFileReader reader)
        {
            var superField = reader.ReadIndex();
            var next = reader.ReadIndex();
            var field = new SBClass();
            field.Name = "Field";
            field.Fields.Add("SuperField", superField.ToString());
            field.Fields.Add("Next", next.ToString());
            return field;
        }

        SBClass ReadConstClass(SBFileReader reader)
        {
            var constClass = new SBClass();
            constClass.Ancestor = ReadFieldClass(reader);
            constClass.Name = "Const";

            var size = reader.ReadIndex();
            var constant = Encoding.ASCII.GetString(reader.ReadBytes(size)).Replace("\0", string.Empty);
            constClass.Fields.Add("Size", size.ToString());
            constClass.Fields.Add("Constant", constant);
            return constClass;
        }

        SBClass ReadEnumClass(SBFileReader reader)
        {
            var enumClass = new SBClass();
            enumClass.Name = "Enum";
            enumClass.Ancestor = ReadFieldClass(reader);
            var arraySize = reader.ReadIndex();
            enumClass.Fields.Add("ArraySize", arraySize.ToString());
            for (var i = 0; i < arraySize; ++i)
            {
                enumClass.Fields.Add("ElementName" + i, package.NameTable[reader.ReadIndex()].Replace("\0", string.Empty));
            }

            return enumClass;
        }

        SBClass ReadPropertyClass(SBFileReader reader)
        {
            var property = new SBClass();
            property.Name = "Property";
            property.Ancestor = ReadFieldClass(reader);

            var arrayDimension = reader.ReadInt16();
            var elementSize = reader.ReadInt16();
            var propertyFlags = reader.ReadInt32();
            var category = reader.ReadIndex();
            property.Fields.Add("ArrayDimension", arrayDimension.ToString());
            property.Fields.Add("ElementSize", elementSize.ToString());
            property.Fields.Add("PropertyFlags", propertyFlags.ToString());
            property.Fields.Add("Category", package.NameTable[category].Replace("\0", string.Empty));
            if ((propertyFlags & (int) PropertyFlags.CPF_Net) != 0)
            {
                int replicationOffset = reader.ReadInt16();
                property.Fields.Add("Replication Offset", replicationOffset.ToString());
            }

            return property;
        }

        SBClass ReadBytePropertyClass(SBFileReader reader)
        {
            var byteProperty = new SBClass();
            byteProperty.Name = "ByteProperty";
            byteProperty.Ancestor = ReadPropertyClass(reader);
            var enumType = reader.ReadIndex();
            byteProperty.Fields.Add("EnumType", enumType.ToString());

            return byteProperty;
        }

        SBClass ReadObjectPropertyClass(SBFileReader reader)
        {
            var objectProperty = new SBClass();
            objectProperty.Name = "ObjectProperty";
            objectProperty.Ancestor = ReadPropertyClass(reader);
            var objectType = reader.ReadIndex();
            objectProperty.Fields.Add("ObjectType", package.FindReferenceName(objectType));

            return objectProperty;
        }

        SBClass ReadFixedArrayPropertyClass(SBFileReader reader)
        {
            var fixedArrayProperty = new SBClass();
            fixedArrayProperty.Name = "FixedArrayProperty";
            fixedArrayProperty.Ancestor = ReadPropertyClass(reader);

            var inner = reader.ReadIndex();
            var count = reader.ReadIndex();

            fixedArrayProperty.Fields.Add("Inner", package.FindReferenceName(inner));
            fixedArrayProperty.Fields.Add("Count", package.FindReferenceName(count));

            return fixedArrayProperty;
        }

        SBClass ReadArrayPropertyClass(SBFileReader reader)
        {
            var arrayProperty = new SBClass();
            arrayProperty.Name = "ArrayProperty";
            arrayProperty.Ancestor = ReadPropertyClass(reader);

            var inner = reader.ReadIndex();

            arrayProperty.Fields.Add("Inner", package.FindReferenceName(inner));

            return arrayProperty;
        }

        SBClass ReadMapPropertyClass(SBFileReader reader)
        {
            var mapProperty = new SBClass();
            mapProperty.Name = "MapProperty";
            mapProperty.Ancestor = ReadPropertyClass(reader);

            var key = reader.ReadIndex();
            var value = reader.ReadIndex();

            mapProperty.Fields.Add("Key", key.ToString());
            mapProperty.Fields.Add("Value", value.ToString());

            return mapProperty;
        }

        SBClass ReadClassPropertyClass(SBFileReader reader)
        {
            var classProperty = new SBClass();
            classProperty.Name = "ClassProperty";
            classProperty.Ancestor = ReadPropertyClass(reader);

            var mclass = reader.ReadIndex();
            classProperty.Fields.Add("Class", package.NameTable[mclass].Replace("\0", string.Empty));

            return classProperty;
        }

        SBClass ReadStructPropertyClass(SBFileReader reader)
        {
            var structProperty = new SBClass();
            structProperty.Name = "StructProperty";
            structProperty.Ancestor = ReadPropertyClass(reader);

            var structType = reader.ReadIndex();

            structProperty.Fields.Add("StructType", structType.ToString());

            return structProperty;
        }

        SBClass ReadIntPropertyClass(SBFileReader reader)
        {
            var intProperty = new SBClass();
            intProperty.Name = "IntProperty";
            intProperty.Ancestor = ReadPropertyClass(reader);

            return intProperty;
        }

        SBClass ReadBoolPropertyClass(SBFileReader reader)
        {
            var boolProperty = new SBClass();
            boolProperty.Name = "BoolProperty";
            boolProperty.Ancestor = ReadPropertyClass(reader);

            return boolProperty;
        }

        SBClass ReadFloatPropertyClass(SBFileReader reader)
        {
            var floatProperty = new SBClass();
            floatProperty.Name = "FloatProperty";
            floatProperty.Ancestor = ReadPropertyClass(reader);

            return floatProperty;
        }

        SBClass ReadNamePropertyClass(SBFileReader reader)
        {
            var nameProperty = new SBClass();
            nameProperty.Name = "NameProperty";
            nameProperty.Ancestor = ReadPropertyClass(reader);

            return nameProperty;
        }

        SBClass ReadStrPropertyClass(SBFileReader reader)
        {
            var strProperty = new SBClass();
            strProperty.Name = "StrProperty";
            strProperty.Ancestor = ReadPropertyClass(reader);

            return strProperty;
        }

        SBClass ReadStringPropertyClass(SBFileReader reader)
        {
            var stringProperty = new SBClass();
            stringProperty.Name = "StringProperty";
            stringProperty.Ancestor = ReadPropertyClass(reader);

            return stringProperty;
        }

        SBClass ReadStructClass(SBFileReader reader)
        {
            var structClass = new SBClass();
            structClass.Name = "Struct";
            structClass.Ancestor = ReadFieldClass(reader);
            var scriptText = reader.ReadIndex();
            var children = reader.ReadIndex();
            var friendlyName = reader.ReadIndex();
            var line = reader.ReadInt32();
            var textPos = reader.ReadInt32();
            var scriptSize = reader.ReadInt32();
            //Try to "eat" the script code for children classes, but according
            //to the doc, it should not work, we have to reverse the bytecode...
            if (scriptSize > 0)
                reader.ReadBytes(scriptSize);
            structClass.Fields.Add("ScriptText", package.FindReferenceName(scriptText));
            structClass.Fields.Add("Children", package.FindReferenceName(children));
            structClass.Fields.Add("FriendlyName", package.NameTable[friendlyName].Replace("\0", string.Empty));
            structClass.Fields.Add("Line", line.ToString());
            structClass.Fields.Add("TextPos", textPos.ToString());
            structClass.Fields.Add("ScriptSize", scriptSize.ToString());

            return structClass;
        }

        SBClass ReadFunctionClass(SBFileReader reader)
        {
            var function = new SBClass();
            function.Name = "Function";
            function.Ancestor = ReadStructClass(reader);

            var inative = reader.ReadInt16();
            var operatorPrecedence = reader.ReadByte();
            var functionFlags = reader.ReadInt32();
            function.Fields.Add("iNative", inative.ToString());
            function.Fields.Add("OperatorPrecedence", operatorPrecedence.ToString());
            function.Fields.Add("FunctionFlags", functionFlags.ToString());

            if ((functionFlags & (int) FunctionFlags.FUNC_Net) != 0)
            {
                var replicationOffset = reader.ReadInt16();
                function.Fields.Add("ReplicationOffset", replicationOffset.ToString());
            }

            return function;
        }

        SBClass ReadStateClass(SBFileReader reader)
        {
            var state = new SBClass();
            state.Name = "State";
            state.Ancestor = ReadStructClass(reader);

            var probeMask = reader.ReadInt64();
            var ignoreMask = reader.ReadInt64();
            var labelTableOffset = reader.ReadInt16();
            var stateFlags = reader.ReadInt32();

            state.Fields.Add("ProbeMask", probeMask.ToString());
            state.Fields.Add("IgnoreMask", ignoreMask.ToString());
            state.Fields.Add("LabelTableOffset", labelTableOffset.ToString());
            state.Fields.Add("StateFlags", stateFlags.ToString());

            return state;
        }

        SBClass ReadNullClass(SBFileReader reader)
        {
            var nullClass = new SBClass();
            nullClass.Name = "null";
            nullClass.Ancestor = ReadStateClass(reader);

            var classFlags = reader.ReadInt32();
            var classGuid = reader.ReadInt32();
            var dependencies_count = reader.ReadIndex();
            //for now skip the dependencies, just "eat" them
            for (var i = 0; i < dependencies_count; ++i)
            {
                reader.ReadIndex(); //Class
                reader.ReadInt32(); //Deep
                reader.ReadInt32(); //ScriptTextCRC
            }
            var packageImports_count = reader.ReadIndex();
            //for now skip the package Imports, just "eat" them
            for (var i = 0; i < packageImports_count; ++i)
                reader.ReadIndex(); //PackageImport
            var classWithin = reader.ReadIndex();
            var classConfigName = reader.ReadIndex();

            nullClass.Fields.Add("ClassFlags", classFlags.ToString());
            nullClass.Fields.Add("ClassGuid", classGuid.ToString());
            nullClass.Fields.Add("Dependencies Count", dependencies_count.ToString());
            nullClass.Fields.Add("Package Imports Count", packageImports_count.ToString());
            nullClass.Fields.Add("Class Within", package.FindReferenceName(classWithin));
            nullClass.Fields.Add("Class Config Name", package.NameTable[classConfigName].Replace("\0", string.Empty));

            return nullClass;
        }
    }
}

//backup case: arrayProperty:
//WIP Reversing "Actions" arrayproperty of InteractiveLevelElement
//if ((activeObject.Name.Contains("InteractiveLevelElement") && activeProperty.Name.Contains("Actions")))
//{
//    activeProperty.StructName = "MenuInteraction";
//    int bytesReaded = 0;
//    int indexSize = 0;
//    int numElementsInArrayPropery = fileReader.ReadIndex(ref indexSize);
//    bytesReaded += indexSize;
//
//    int fieldName = 0;
//    int fieldSize = 0;
//    int fieldValue = 0;
//
//    //Loop into arrayproperty's elements
//    for (int k = 0; k < numElementsInArrayPropery; ++k)
//    {
//        fieldName = fileReader.ReadIndex(ref indexSize);
//        bytesReaded += indexSize;
//
//
//        fieldSize = fileReader.ReadIndex(ref indexSize);
//        bytesReaded += indexSize;
//
//        fieldValue = fileReader.ReadByte();
//        ++bytesReaded;
//
//        SBProperty menuOption = new SBProperty();
//        menuOption.Name = package.NameTable[fieldName];
//        menuOption.Size = fieldSize.ToString();
//        menuOption.Value = fieldValue.ToString();
//        menuOption.Type = PropertyType.ByteProperty;
//        activeProperty.Array.Add(package.NameTable[fieldName], menuOption);
//
//        //--------------------
//        fieldName = fileReader.ReadIndex(ref indexSize);
//        //Console.WriteLine("Field Name: " + package.NameTable[fieldName]);//StackedActions
//        bytesReaded += indexSize;
//
//        fileReader.ReadIndex(ref indexSize); //int unknownData
//        bytesReaded += indexSize;
//        //Console.WriteLine("???: " + unknownData); //41
//
//        int numElements = fileReader.ReadIndex(ref indexSize);
//        bytesReaded += indexSize;
//        //Console.WriteLine("Num elements: " + numElements);//1
//
//        SBProperty stackedAction = new SBProperty();
//        stackedAction.Name = package.NameTable[fieldName];
//        stackedAction.Size = numElements.ToString();
//        stackedAction.Type = PropertyType.ArrayProperty;
//        //Loop into StackedActions's elements
//        for (int i = 0; i < numElements; ++i)
//        {
//            fieldValue = fileReader.ReadIndex(ref indexSize);
//            bytesReaded += indexSize;
//            //Console.WriteLine("Field value: " + package.FindReferenceName(fieldValue, false));//Interaction_Action17
//            SBProperty stackedActionEntry = new SBProperty();
//            stackedActionEntry.Name = stackedAction.Name + i;
//            stackedActionEntry.Value = package.FindReferenceName(fieldValue, false);
//            stackedActionEntry.Type = PropertyType.ObjectProperty;
//            stackedAction.Array.Add(stackedAction.Name + i, stackedActionEntry);
//        }
//        activeProperty.Array.Add(stackedAction.Name, stackedAction);
//
//        //-------------------
//        fieldName = fileReader.ReadIndex(ref indexSize);
//        //Console.WriteLine("Field Name: " + package.NameTable[fieldName]);//Requirements
//        bytesReaded += indexSize;
//
//        fileReader.ReadIndex(ref indexSize); //unknownData
//        bytesReaded += indexSize;
//        //Console.WriteLine("???: " + unknownData);
//
//        numElements = fileReader.ReadIndex(ref indexSize);
//        bytesReaded += indexSize;
//        //Console.WriteLine("Num elements: " + numElements);//1
//        SBProperty requirements = new SBProperty();
//        requirements.Name = package.NameTable[fieldName];
//        requirements.Type = PropertyType.ArrayProperty;
//        requirements.Size = numElements.ToString();
//        //Loop into Requirements's elements
//        for (int i = 0; i < numElements; ++i)
//        {
//            fieldValue = fileReader.ReadIndex(ref indexSize);
//            bytesReaded += indexSize;
//            //Console.WriteLine("Field value: " + package.FindReferenceName(fieldValue));//Interaction_Action17
//            SBProperty requirementsEntry = new SBProperty();
//            requirementsEntry.Name = requirements.Name + i;
//            requirementsEntry.Value = package.FindReferenceName(fieldValue);
//            requirementsEntry.Type = PropertyType.ObjectProperty;
//            requirements.Array.Add(requirementsEntry.Name, requirementsEntry);
//        }
//        activeProperty.Array.Add(requirements.Name, requirements);
//
//    }//For loop through "Action" ArrayProperty elements
//
//    fileReader.Seek(-bytesReaded, SeekOrigin.Current);
//
//    fileReader.ReadBytes(realSize); //byte[] blob
//    //Console.WriteLine(BitConverter.ToString(blob));
//}
/*else if(property.Name.Contains("Requirements"))
{
  Console.WriteLine("Array size (bytes): " + realSize );
  int indexSize = 0;
  int supposedArraySize = fileReader.ReadIndex(ref indexSize);
  Console.WriteLine("array size: " + supposedArraySize);
  int supposedObjectReference = fileReader.ReadIndex(ref indexSize);
  Console.WriteLine("Index size: " + indexSize);
  Console.WriteLine("SUpposed reference: " + package.FindReferenceName(supposedObjectReference, true));
  Console.WriteLine("Last byte: " + fileReader.ReadByte());
}*/
//else if ((activeObject.Name.Contains("Interaction_Action") && activeProperty.Name.Contains("Actions")))
//{
//    activeProperty.StructName = "Content_Event";
//    //Console.WriteLine(result.Name);
//    int indexSize = 0;
//    int bytesReaded = 0;
//
//    int arraySize = fileReader.ReadIndex(ref indexSize);
//    bytesReaded += indexSize;
//
//    for (int i = 0; i < arraySize; ++i)
//    {
//        int supposedReference = fileReader.ReadIndex(ref indexSize);
//        bytesReaded += indexSize;
//        //Console.WriteLine("Value: " + package.FindReferenceName(supposedReference));
//        //Console.WriteLine("Index size: " + indexSize);
//        SBProperty contentEvent = new SBProperty();
//        contentEvent.Name = activeProperty.Name + i;
//        contentEvent.Value = package.FindReferenceName(supposedReference);
//        contentEvent.Type = PropertyType.ObjectProperty;
//        activeProperty.Array.Add(contentEvent.Name, contentEvent);
//    }
//
//    fileReader.Seek(-bytesReaded, SeekOrigin.Current);
//
//    fileReader.ReadBytes(realSize); //byte[] blob
//    //Console.WriteLine(BitConverter.ToString(blob));
//
//    // Console.ReadKey();
//}