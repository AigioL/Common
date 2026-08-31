using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace DNS.Protocol.Utils;

public sealed class ObjectStringifier<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    public static ObjectStringifier<T> New(object obj)
    {
        return new ObjectStringifier<T>(obj);
    }

    public static string? Stringify(object? obj)
    {
        return StringifyObject(obj);
    }

    static string? StringifyObject(object? obj)
    {
        if (obj is string v)
        {
            return v;
        }
        else if (obj is IDictionary dictionary)
        {
            return StringifyDictionary(dictionary);
        }
        else if (obj is IEnumerable enumerable)
        {
            return StringifyList(enumerable);
        }
        else
        {
            return obj == null ? "null" : obj.ToString();
        }
    }

    static string StringifyList(IEnumerable enumerable)
    {
        return "[" + string.Join(", ", enumerable.Cast<object>().Select(o => StringifyObject(o)).ToArray()) + "]";
    }

    static string StringifyDictionary(IDictionary dict)
    {
        StringBuilder result = new StringBuilder();

        result.Append('{');

        foreach (DictionaryEntry pair in dict)
        {
            result
                .Append(pair.Key)
                .Append('=')
                .Append(StringifyObject(pair.Value))
                .Append(", ");
        }

        if (result.Length > 1)
        {
            result.Remove(result.Length - 2, 2);
        }

        return result.Append('}').ToString();
    }

    object obj;
    Dictionary<string, string?> pairs;

    public ObjectStringifier(object obj)
    {
        this.obj = obj;
        this.pairs = new Dictionary<string, string?>();
    }

    public ObjectStringifier<T> Remove(params string[] names)
    {
        foreach (string name in names)
        {
            pairs.Remove(name);
        }

        return this;
    }

    public ObjectStringifier<T> Add(params string[] names)
    {
        Type type = typeof(T);

        foreach (string name in names)
        {
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            var value = property?.GetValue(obj);

            pairs.Add(name, StringifyObject(value));
        }

        return this;
    }

    public ObjectStringifier<T> Add(string name, object value)
    {
        pairs.Add(name, StringifyObject(value));
        return this;
    }

    public ObjectStringifier<T> AddAll()
    {
        var properties = typeof(T).GetProperties(
            BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            pairs.Add(property.Name, StringifyObject(value));
        }

        return this;
    }

    public override string ToString()
    {
        return StringifyDictionary(pairs);
    }
}
