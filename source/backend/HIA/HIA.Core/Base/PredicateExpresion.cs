using System.Linq.Expressions;
using System.Reflection;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace HIA.Core.Base;

public static class PredicateExpresion<T> where T : class
{
    public static Expression<Func<T, bool>> BuildExpressionForFilter(Expression<Func<T, bool>> filterSearch, PropertyInfo propertyInfo, string key)
    {
        if (propertyInfo.PropertyType == typeof(string))
        {
            filterSearch = filterSearch.Or(x => EF.Property<string>(x, propertyInfo.Name).ToLower().Contains(key));
        }
        else if (propertyInfo.PropertyType == typeof(int))
        {
            filterSearch = filterSearch.Or(x => EF.Property<int>(x, propertyInfo.Name).ToString().Contains(key));
        }
        // '!.Value' (not GetValueOrDefault) keeps SQL without COALESCE so NULL rows never match the key.
        else if (propertyInfo.PropertyType == typeof(int?))
        {
            filterSearch = filterSearch.Or(x => EF.Property<int?>(x, propertyInfo.Name)!.Value.ToString().Contains(key));
        }
        else if (propertyInfo.PropertyType == typeof(double?))
        {
            filterSearch = filterSearch.Or(x => EF.Property<double?>(x, propertyInfo.Name)!.Value.ToString().Contains(key));
        }
        else if (propertyInfo.PropertyType == typeof(DateTime?))
        {
            filterSearch = filterSearch.Or(x => EF.Property<DateTime?>(x, propertyInfo.Name)!.Value.ToString().Contains(key));
        }
        else if (propertyInfo.PropertyType == typeof(DateTime))
        {
            filterSearch = filterSearch.Or(x => EF.Property<DateTime>(x, propertyInfo.Name).ToString().Contains(key));
        }

        return filterSearch;
    }
}
