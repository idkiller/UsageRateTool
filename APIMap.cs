using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace UsageRateTool
{
    class APIMap
    {
        List<API> apis = new List<API>();
        Dictionary<MemberInfo, API> nameMap = new Dictionary<MemberInfo, API>();

        public APIMap(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                Load(path);
            }
        }

        public IEnumerable<API> APIList => apis;
        public Dictionary<MemberInfo, API> NameMap => nameMap;

        void Load(string path)
        {
            var assembly = AssemblyLoader.GetAssembly(path);
            var types = assembly.GetTypes().Where(t =>
                    !t.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                    !t.IsEnum &&
                    !t.IsSubclassOf(typeof(Delegate)));

            foreach (Type type in types)
            {
                string typeName = type.Name;
                var t = new API(Category.Type, typeName);
                apis.Add(t);

                var staticFieldsInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(f => f.IsFamily || f.IsPublic);
                foreach (var field in staticFieldsInfo)
                {
                    var api = new API(t, Category.StaticField, field.Name, field.FieldType.Name);
                    apis.Add(api);
                    NameMap[field] = api;
                }

                var propertyInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(p => (p.GetMethod != null && !p.GetMethod.IsPrivate) || (p.SetMethod != null && !p.SetMethod.IsPrivate));
                foreach (var property in propertyInfo)
                {
                    var api = new API(t, Category.Property, property.Name, property.PropertyType.Name);
                    apis.Add(api);

                    if (property.GetMethod != null)
                    {
                        NameMap[property.GetMethod] = api;
                        //Console.WriteLine($"{property.GetMethod.Name} => {property.GetMethod.GetMetadataToken():x} / {property.GetMethod.MetadataToken:x}");
                    }
                    if (property.SetMethod != null)
                    {
                        NameMap[property.SetMethod] = api;
                        //Console.WriteLine($"{property.SetMethod.Name} => {property.SetMethod.GetMetadataToken():x} / {property.SetMethod.MetadataToken:x}");
                    }
                }

                var methodInfo = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => {
                    if (!m.IsFamily && !m.IsPublic)
                    {
                        return false;
                    }
                    var method = m as MethodBase;
                    return method == null || !method.IsSpecialName;
                });
                foreach (var method in methodInfo)
                {
                    var pars = method.GetParameters();
                    var psb = new StringBuilder();
                    psb.Append(' ');
                    foreach (var p in pars)
                    {
                        psb.Append(p.ParameterType.Name);
                        psb.Append(' ');
                        psb.Append(p.Name);
                        psb.Append(' ');
                    }

                    var api = new API(t, Category.Method, $"{method.Name}({psb.ToString()})", method.ReturnType.Name);
                    apis.Add(api);
                    NameMap[method] = api;
                }

                var fieldsInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.IsFamily || m.IsPublic);
                foreach (var field in fieldsInfo)
                {
                    var api = new API(t, Category.Field, field.Name, field.FieldType.Name);
                    apis.Add(api);
                    NameMap[field] = api;
                }

                var eventsInfo = type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(e => !e.AddMethod.IsPrivate || !e.RemoveMethod.IsPrivate);
                foreach (var evnt in eventsInfo)
                {
                    var api = new API(t, Category.Event, evnt.Name, evnt.EventHandlerType.Name);
                    apis.Add(api);

                    if (evnt.AddMethod != null)
                    {
                        NameMap[evnt.AddMethod] = api;
                    }
                    if (evnt.RemoveMethod != null)
                    {
                        NameMap[evnt.RemoveMethod] = api;
                    }
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    class PrintableAttribute : Attribute
    {
        public PrintableAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }
    }

    enum Category
    {
        None, Type, Property, StaticField, Method, Field, Event
    }

    class API
    {
        public static API Empty = new API();

        List<MemberInfo> caller = new List<MemberInfo>();

        public API Parent { get; set; }

        public string DeclaredType { get; set; }
        [Printable(2)]
        public Category Category { get; set; }
        [Printable(4)]
        public string Name { get; set; }
        [Printable(3)]
        public string Type { get; set; }

        [Printable(5)]
        public string References => $"{caller.Count()}";

        public IList<MemberInfo> Caller => caller;

        public API (API parent, Category c, string name, string type)
        {
            Parent = parent;
            DeclaredType = parent?.Name;
            Category = c;
            Name = name;
            Type = type;
        }

        public API (API parent, Category c, string name) : this(parent, c, name, null)
        {
        }

        public API (Category c, string name) : this(Empty, c, name, null)
        {
        }

        public API () : this(null, Category.None, null)
        {
        }
    }
}
