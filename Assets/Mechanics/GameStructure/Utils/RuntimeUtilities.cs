using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// A set of runtime utilities used by the post-processing stack.
/// </summary>
public static class RuntimeUtilities
{
    #region Reflection

    static IEnumerable<Type> m_AssemblyTypes;

    /// <summary>
    /// Gets all currently available assembly types.
    /// </summary>
    /// <returns>A list of all currently available assembly types</returns>
    /// <remarks>
    /// This method is slow and should be use with extreme caution. We recommend you use
    /// <see cref="GetAllTypesDerivedFrom{T}"/> instead if possible.
    /// </remarks>
    /// <seealso cref="GetAllTypesDerivedFrom{T}"/>
    public static IEnumerable<Type> GetAllAssemblyTypes()
    {
        if (m_AssemblyTypes == null)
        {
            m_AssemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t =>
                {
                    // Ugly hack to handle mis-versioned dlls
                    var innerTypes = new Type[0];
                    try
                    {
                        innerTypes = t.GetTypes();
                    }
                    catch { }
                    return innerTypes;
                });
        }

        return m_AssemblyTypes;
    }


    /// <summary>
    /// Properly destroys a given Unity object.
    /// </summary>
    /// <param name="obj">The object to destroy</param>
    public static void Destroy(UnityEngine.Object obj)
    {
        if (obj != null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
#else
                UnityObject.Destroy(obj);
#endif
        }
    }

    /// <summary>
    /// Gets all currently available assembly types derived from type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to look for</typeparam>
    /// <returns>A list of all currently available assembly types derived from type <typeparamref name="T"/></returns>
    public static IEnumerable<Type> GetAllTypesDerivedFrom<T>()
    {
#if UNITY_EDITOR && UNITY_2019_2_OR_NEWER
        return UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
#else
        return GetAllAssemblyTypes().Where(t => t.IsSubclassOf(typeof(T)));
#endif
    }

    /// <summary>
    /// Helper method to get the first attribute of type <c>T</c> on a given type.
    /// </summary>
    /// <typeparam name="T">The attribute type to look for</typeparam>
    /// <param name="type">The type to explore</param>
    /// <returns>The attribute found</returns>
    public static T GetAttribute<T>(this Type type) where T : Attribute
    {
        Assert.IsTrue(type.IsDefined(typeof(T), false), "Attribute not found");
        return (T)type.GetCustomAttributes(typeof(T), false)[0];
    }

    /// <summary>
    /// Returns all attributes set on a specific member.
    /// </summary>
    /// <typeparam name="TType">The class type where the member is defined</typeparam>
    /// <typeparam name="TValue">The member type</typeparam>
    /// <param name="expr">An expression path to the member</param>
    /// <returns>An array of attributes</returns>
    /// <remarks>
    /// This method doesn't return inherited attributes, only explicit ones.
    /// </remarks>
    public static Attribute[] GetMemberAttributes<TType, TValue>(Expression<Func<TType, TValue>> expr)
    {
        Expression body = expr;

        if (body is LambdaExpression)
            body = ((LambdaExpression)body).Body;

        switch (body.NodeType)
        {
            case ExpressionType.MemberAccess:
                var fi = (FieldInfo)((MemberExpression)body).Member;
                return fi.GetCustomAttributes(false).Cast<Attribute>().ToArray();
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Returns a string path from an expression. This is mostly used to retrieve serialized
    /// properties without hardcoding the field path as a string and thus allowing proper
    /// refactoring features.
    /// </summary>
    /// <typeparam name="TType">The class type where the member is defined</typeparam>
    /// <typeparam name="TValue">The member type</typeparam>
    /// <param name="expr">An expression path fo the member</param>
    /// <returns>A string representation of the expression path</returns>
    public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
    {
        MemberExpression me;
        switch (expr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                me = expr.Body as MemberExpression;
                break;
            default:
                throw new InvalidOperationException();
        }

        var members = new List<string>();
        while (me != null)
        {
            members.Add(me.Member.Name);
            me = me.Expression as MemberExpression;
        }

        var sb = new StringBuilder();
        for (int i = members.Count - 1; i >= 0; i--)
        {
            sb.Append(members[i]);
            if (i > 0) sb.Append('.');
        }

        return sb.ToString();
    }

    #endregion
}
