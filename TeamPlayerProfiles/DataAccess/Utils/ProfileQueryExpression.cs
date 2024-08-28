﻿using Common.Models.Enums;
using DataAccess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using static Common.Models.ConditionalQuery;
using static Common.Models.ConditionalQuery.Filter;

namespace DataAccess.Utils
{
    public static class ProfileQueryExpression
    {
        public static Expression GetStringFilteringExpression<TProfile>(this Expression expr, StringFilter? filter, string propertyName, ParameterExpression parameter) where TProfile : class, IProfile
        {
            if (filter == null) return expr;
            Type strType = typeof(string);
            Type profileType = typeof(TProfile);
            var property = profileType.GetProperty(propertyName);
            if (property == null || property.PropertyType != strType) throw new ArgumentException();
            var inputParam = Expression.Constant(filter.Input, strType);
            var memberAccess = Expression.MakeMemberAccess(parameter, property);
            var containsMethodInfo = strType.GetMethod("Contains", [strType]);
            var startsWithMethodInfo = strType.GetMethod("StartsWith", [strType]);
            var endsWithMethodInfo = strType.GetMethod("EndsWith", [strType]);
            return filter.FilterType switch
            {
                StringValueFilterType.Equal => Expression.AndAlso(expr, Expression.Equal(memberAccess, inputParam)),
                StringValueFilterType.NotEqual => Expression.AndAlso(expr, Expression.NotEqual(memberAccess, inputParam)),
                StringValueFilterType.Contains => Expression.AndAlso(expr, Expression.Call(memberAccess, containsMethodInfo, inputParam)),
                StringValueFilterType.DoesNotContain => Expression.AndAlso(expr, Expression.Not(Expression.Call(memberAccess, containsMethodInfo, inputParam))),
                StringValueFilterType.StartsWith =>Expression.AndAlso(expr,  Expression.Call(memberAccess, startsWithMethodInfo, inputParam)),
                StringValueFilterType.EndsWith => Expression.AndAlso(expr, Expression.Call(memberAccess, endsWithMethodInfo, inputParam)),
                _ => throw new ArgumentException(),
            };
        }

        public static Expression GetDateTimeFilteringExpression<TProfile>(this Expression expr, TimeFilter? filter, string timePropertyName, ParameterExpression parameter) where TProfile : class, IProfile
        {
            if (filter == null) return expr;
            var property = typeof(TProfile).GetProperty(timePropertyName);
            var dateTimeType = typeof(DateTime);
            if (property == null || property.PropertyType != dateTimeType) throw new ArgumentException();
            var inputParam = Expression.Constant(filter.DateTime, dateTimeType);
            var memberAccess = Expression.MakeMemberAccess(parameter, property);
            return filter.FilterType switch
            {
                DateTimeFilter.Before => Expression.AndAlso(expr, Expression.LessThan(memberAccess, inputParam)),
                DateTimeFilter.AtOrBefore => Expression.AndAlso(expr, Expression.LessThanOrEqual(memberAccess, inputParam)),
                DateTimeFilter.Exact => Expression.AndAlso(expr, Expression.Equal(memberAccess, inputParam)),
                DateTimeFilter.AtOrAfter => Expression.AndAlso(expr, Expression.GreaterThanOrEqual(memberAccess, inputParam)),
                DateTimeFilter.After => Expression.AndAlso(expr, Expression.GreaterThan(memberAccess, inputParam)),
                _ => throw new ArgumentException(),
            };
        }

        public static BinaryExpression? GetDateTimeFilteringExpression<TProfile>(TimeFilter? filter, string timePropertyName, ParameterExpression parameter) where TProfile : class, IProfile
        {
            var property = typeof(TProfile).GetProperty(timePropertyName);
            var dateTimeType = typeof(DateTime);
            if (filter == null || property == null || property.PropertyType != dateTimeType) return null;
            var inputParam = Expression.Constant(filter.DateTime, dateTimeType);
            var memberAccess = Expression.MakeMemberAccess(parameter, property);
            return filter.FilterType switch
            {
                DateTimeFilter.Before => Expression.LessThan(memberAccess, inputParam),
                DateTimeFilter.AtOrBefore => Expression.LessThanOrEqual(memberAccess, inputParam),
                DateTimeFilter.Exact => Expression.Equal(memberAccess, inputParam),
                DateTimeFilter.AtOrAfter => Expression.GreaterThanOrEqual(memberAccess, inputParam),
                DateTimeFilter.After => Expression.GreaterThan(memberAccess, inputParam),
                _ => throw new ArgumentException(),
            };
        }

        public static Expression GetValueListFilteringExpression<TProfile, TListItemProp>(this Expression expr, ValueFilter<TListItemProp> filter, string valuePropertyName, ParameterExpression parameter) where TProfile : class, IProfile
        {
            if (filter == null) return expr;
            var propertyType = typeof(TListItemProp);
            var property = typeof(TProfile).GetProperty(valuePropertyName);
            if (property == null || property.PropertyType != propertyType) throw new ArgumentException();
            var valueList = Expression.Constant(filter.ValueList, typeof(IEnumerable<TListItemProp>));
            var memberAccess = Expression.MakeMemberAccess(parameter, property);
            var containsMethod = typeof(Enumerable)
                .GetMethods()
                .Where(x => x.Name == "Contains")
                .Single(x => x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TListItemProp));
            Expression? finalExpression = null;
            switch (filter.FilterType)
            {
                case ValueListFilterType.Exact:
                    {
                        if (filter.ValueList.Count > 1) break;
                        finalExpression = Expression.AndAlso(expr, Expression.Call(containsMethod, valueList, memberAccess));
                        break;
                    }
                case ValueListFilterType.Any:
                case ValueListFilterType.Including:
                    {
                        finalExpression = Expression.AndAlso(expr, Expression.Call(containsMethod, valueList, memberAccess));
                        break;
                    }
                case ValueListFilterType.Excluding:
                    {
                        finalExpression = Expression.AndAlso(expr, Expression.Not(Expression.Call(containsMethod, valueList, memberAccess)));
                        break;
                    }
                default:
                    throw new ArgumentException();
            }
            return finalExpression;
        }

        public static Expression GetValueListFilteringOnListExpression<TProfile, TListItem, TListItemProp>(this Expression expr, ValueFilter<TListItemProp>? filter, string listPropertyName, string listItemPropertyName, ParameterExpression parameter) where TProfile : class, IProfile
        {
            if (filter == null) return expr;
            var listType = typeof(ICollection<TListItem>);
            var property = typeof(TProfile).GetProperty(listPropertyName);
            var listItemProperty = typeof(TListItem).GetProperty(listItemPropertyName);
            if (property == null || property.PropertyType != listType || listItemProperty == null || listItemProperty.PropertyType != typeof(TListItemProp)) throw new ArgumentException();
            var valueList = Expression.Constant(filter.ValueList, typeof(ICollection<TListItemProp>));
            var memberAccess = Expression.Property(parameter, listPropertyName);
            var asQueryable = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == "AsQueryable" && m.IsGenericMethodDefinition);
            var asQueryableListItemProp = asQueryable.MakeGenericMethod(typeof(TListItemProp));
            var asQueryableListItem = asQueryable.MakeGenericMethod(typeof(TListItem));
            var select = typeof(Queryable)
                .GetMethods()
                .Where(m => m.Name == "Select" && m.IsGenericMethodDefinition)
                .Single(m =>
                {
                    var parameters = m.GetParameters();
                    if (parameters.Length != 2)
                        return false;

                    var delegateType = parameters[1].ParameterType.GetGenericArguments()[0];
                    return delegateType.IsGenericType && delegateType.GetGenericTypeDefinition() == typeof(Func<,>);
                }).MakeGenericMethod(typeof(TListItem), typeof(TListItemProp));
            var intersect = typeof(Queryable)
                .GetMethods()
                .Where(x => x.Name == "Intersect")
                .Single(x => x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TListItemProp));
            var count = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TListItemProp));
            var listItemParameter = Expression.Parameter(typeof(TListItem), "li");
            var listItemPropertyEx = Expression.Property(listItemParameter, listItemPropertyName);
            var listItemPropertyLambda = Expression.Lambda(listItemPropertyEx, listItemParameter);
            var asQueryableExpression = Expression.Call(asQueryableListItem, memberAccess);
            var valueListAsQueryable = Expression.Call(asQueryableListItemProp, valueList);
            var selectExpression = Expression.Call(select, asQueryableExpression, listItemPropertyLambda);
            var intersectExpression = Expression.Call(intersect, selectExpression, valueListAsQueryable);
            var intersectedCount = Expression.Call(count, intersectExpression);
            Expression? finalExpression = null;
            switch (filter.FilterType)
            {
                case ValueListFilterType.Including:
                    {
                        var valueCount = Expression.Constant(filter.ValueList.Count);
                        finalExpression = Expression.AndAlso(expr, Expression.GreaterThanOrEqual(intersectedCount, valueCount));
                        break;
                    }
                case ValueListFilterType.Exact:
                    {
                        var valueCount = Expression.Constant(filter.ValueList.Count);
                        finalExpression =  Expression.AndAlso(expr, Expression.Equal(intersectedCount, valueCount));
                        break;
                    }
                case ValueListFilterType.Excluding:
                    {
                        var valueCount = Expression.Constant(0);
                        finalExpression =  Expression.AndAlso(expr, Expression.Equal(intersectedCount, valueCount));
                        break;
                    }
                case ValueListFilterType.Any:
                    {
                        var valueCount = Expression.Constant(0);
                        finalExpression =  Expression.AndAlso(expr, Expression.GreaterThan(intersectedCount, valueCount));
                        break;
                    }
                default:
                    throw new ArgumentException();
            }
            return finalExpression;
        }

        public static Expression GetSingleValueFilteringExpression<TProfile, TProperty>(this Expression expr, SingleValueFilter<TProperty> filter, string propertyName, ParameterExpression parameter) where TProfile : class, IProfile
        {
            if (filter == null) return expr;
            var propertyType = typeof(TProperty);
            var property = typeof(TProfile).GetProperty(propertyName);
            if (property == null || property.PropertyType != propertyType) throw new ArgumentException();
            var value = Expression.Constant(filter.Value, propertyType);
            var memberAccess = Expression.MakeMemberAccess(parameter, property);
            return filter.FilterType switch
            {
                SingleValueFilterType.Equals => Expression.AndAlso(expr, Expression.Equal(value, memberAccess)),
                SingleValueFilterType.DoesNotEqual =>  Expression.AndAlso(expr, Expression.NotEqual(value, memberAccess)),
                _ => throw new ArgumentException(),
            };
        }

        public static IQueryable<TProfile> SortWith<TProfile>(this IQueryable<TProfile> query, Sort? config) where TProfile : class, IProfile
        {
            if (config == null) return query;
            string command = config.SortDirection switch
            {
                SortDirection.Asc => "OrderBy",
                SortDirection.Desc => "OrderByDescending",
                _ => throw new ArgumentException(),
            };
            var type = typeof(TProfile);
            var property = type.GetProperty(config.SortBy);
            var parameter = Expression.Parameter(type, "profile");
            if (property == null) return query;
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, [type, property.PropertyType], query.Expression, Expression.Quote(orderByExpression));
            return query.Provider.CreateQuery<TProfile>(resultExpression);
        }
        
    }
}
