/* ****************************************************************************
 *
 * Copyright (c) Francesco Abbruzzese. All rights reserved.
 * francesco@dotnet-programming.com
 * http://www.dotnet-programming.com/
 * 
 * This software is subject to the the license at http://mvccontrolstoolkit.codeplex.com/license  
 * and included in the license.txt file of this distribution.
 * 
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using MvcControlsToolkit.Core.Business;
using System.Runtime.CompilerServices;
using System.Collections;

namespace MvcControlsToolkit.Core.Linq
{
    public enum FilterLogicalOperator{And, ExclusiveOr, Or};
    public enum FilterCondition { None = 0, Equal = 1, NotEqual = 2, LessThan = 4, LessThanOrEqual = 8, GreaterThan = 16, GreaterThanOrEqual = 32, StartsWith = 64, EndsWith = 128, Contains = 256, IsContainedIn = 512 }
    public class FilterBuilder<T>
    {
        private Expression currFilter;
        private ParameterExpression param;
        private FilterLogicalOperator logicalOperator;
        private FilterBuilder<T> father = null;
        private static MethodInfo contains = typeof(Enumerable).GetMethods().Where(m => m.Name == "Contains" && m.GetParameters().Count() == 2).SingleOrDefault();
        private static Expression BuildAccessExpressionRec(Expression obj, string[] properties, int level, Type previousType)
        {
            string currpropertyName = properties[level];
            if (string.IsNullOrWhiteSpace(currpropertyName)) return null;
            PropertyInfo currProperty = previousType.GetProperty(currpropertyName);
            if (currProperty == null) return null;
            Expression pRes = Expression.MakeMemberAccess(obj, currProperty);
            if (level >= properties.Length - 1) return pRes;
            else return
                BuildAccessExpressionRec(pRes, properties, level + 1, currProperty.PropertyType);
        }
        public static LambdaExpression BuildAccessExpression(string path)
        {
            if (path == null) return null;
            if (string.IsNullOrWhiteSpace(path))
            {
                Expression<Func<T, T>> res = m => m;
                return res;
            } 
            string[] properties = path.Split('.');
            if (properties.Length == 0) return null;
            ParameterExpression parameter = Expression.Parameter(typeof(T), "m");
            Expression body = BuildAccessExpressionRec(parameter, properties, 0, typeof(T));
            if (body == null) return null;
            return Expression.Lambda(body, parameter);
        }
        public FilterBuilder (FilterLogicalOperator logicalOperator=FilterLogicalOperator.And)
        {
            this.logicalOperator=logicalOperator;
        }
        private Expression istantiateP(Expression exp, object constantValue)
        {
            if (exp == null)
            {
                if (constantValue is TypedConstant)
                {
                    TypedConstant tc = constantValue as TypedConstant;
                    return Expression.Constant(tc.ConstantValue, tc.ConstantType);
                }
                else return Expression.Constant(constantValue);
            }
            BinaryExpression expB = exp as BinaryExpression;
            if (expB != null) return expB.Update(istantiateP(expB.Left, constantValue), expB.Conversion, istantiateP(expB.Right, constantValue));
            UnaryExpression expU = exp as UnaryExpression;
            if (expU != null) return expU.Update(istantiateP(expU.Operand, constantValue));
            MemberExpression expM = exp as MemberExpression;
            if (expM != null) return expM.Update(istantiateP(expM.Expression, constantValue));
            MethodCallExpression expC = exp as MethodCallExpression;
            if (expC != null)
            {
                List<Expression> nArgs = new List<Expression>();
                foreach (Expression e in expC.Arguments) nArgs.Add(istantiateP(e, constantValue));
                return expC.Update(istantiateP(expC.Object, constantValue), nArgs);
            }
            if (exp is ParameterExpression)
            {
                if (constantValue is TypedConstant){
                    TypedConstant tc = constantValue as TypedConstant;
                    return Expression.Constant(tc.ConstantValue, tc.ConstantType);
                }
                else return Expression.Constant(constantValue);
            }
            return exp;
        }
        private LambdaExpression createCall(MethodInfo call, LambdaExpression constantExp, object constantValue, LambdaExpression parameterExpression)
        {
            
            return Expression.Lambda(
                Expression.Call(parameterExpression.Body, call, istantiateP(constantExp, constantValue)),
                parameterExpression.Parameters[0]);
        }
        private LambdaExpression createInverseCall(MethodInfo call, LambdaExpression constantExp, object constantValue, LambdaExpression parameterExpression)
        {

            if (call.IsStatic)
            {
                return Expression.Lambda(
                    Expression.Call(call, istantiateP(constantExp, constantValue) , parameterExpression.Body),
                    parameterExpression.Parameters[0]);
            }
            else
                return Expression.Lambda(
                    Expression.Call(istantiateP(constantExp, constantValue), call, parameterExpression.Body),
                    parameterExpression.Parameters[0]);
        }
        private LambdaExpression createComparison(ExpressionType comparison, LambdaExpression constantExp, object constantValue, LambdaExpression parameterExpression, bool specialString=false)
        {
            if (specialString)
                return Expression.Lambda(
                Expression.MakeBinary(comparison,
                Expression.Call(
                    parameterExpression.Body,
                    typeof(string).GetMethod(
                    "CompareTo",
                    new Type[] { typeof(string) }),
                    istantiateP(constantExp,
                    constantValue)),
                istantiateP(null, 0)),
                parameterExpression.Parameters[0]);
            return Expression.Lambda(
                Expression.MakeBinary(comparison, parameterExpression.Body, istantiateP(constantExp, constantValue)),
                parameterExpression.Parameters[0]);
        }
        
        private Expression adjustP(Expression exp)
        {

            BinaryExpression expB = exp as BinaryExpression;
            if (expB != null) return expB.Update(adjustP(expB.Left), expB.Conversion, adjustP(expB.Right));
            UnaryExpression expU = exp as UnaryExpression;
            if (expU != null) return expU.Update(adjustP(expU.Operand));
            MemberExpression expM = exp as MemberExpression;
            if (expM != null) return expM.Update(adjustP(expM.Expression));
            MethodCallExpression expC = exp as MethodCallExpression;
            if (expC != null)
            {
                List<Expression> nArgs = new List<Expression>();
                foreach (Expression e in expC.Arguments) nArgs.Add(adjustP(e));
                return expC.Update(adjustP(expC.Object), nArgs);
            }
            if (exp is ParameterExpression) return param;
            return exp;
        }
        
        public FilterBuilder<T> Add(
            FilterCondition condition,
            string path,
            dynamic value)
        {
            dynamic fieldSelector = BuildAccessExpression(path);
            if (fieldSelector == null) return this;
            return
                Add(true, condition, fieldSelector, value);
        }
        
        public FilterBuilder<T> Add<F>(
            bool toAdd,
            FilterCondition condition,
            Expression<Func<T, F>> fieldSelector,
            dynamic value)
        {
            if (fieldSelector == null) throw new ArgumentNullException("fieldSelector");
            if (value == null) return this;
            LambdaExpression clause = null;
            switch (condition)
            {
                case FilterCondition.Equal:
                    clause = createComparison(
                        ExpressionType.Equal,
                        null,
                        value,
                        fieldSelector);
                    break;
                case FilterCondition.NotEqual:
                    clause = createComparison(
                        ExpressionType.NotEqual,
                        null,
                        value,
                        fieldSelector);
                    break;
                case FilterCondition.LessThan:
                    clause = createComparison(
                        ExpressionType.LessThan,
                        null,
                        value,
                        fieldSelector, 
                        typeof(F) == typeof(string));
                    break;
                case FilterCondition.LessThanOrEqual:
                    clause = createComparison(
                        ExpressionType.LessThanOrEqual,
                        null,
                        value,
                        fieldSelector, 
                        typeof(F) == typeof(string));
                    break;
                case FilterCondition.GreaterThan:
                    clause = createComparison(
                        ExpressionType.GreaterThan,
                        null,
                        value,
                        fieldSelector,
                        typeof(F) == typeof(string));
                    break;
                case FilterCondition.GreaterThanOrEqual:
                    clause = createComparison(
                        ExpressionType.GreaterThanOrEqual,
                        null,
                        value,
                        fieldSelector, 
                        typeof(F) == typeof(string));
                    break;
                case FilterCondition.StartsWith:
                    clause = createCall(
                        typeof(string).GetMethod("StartsWith", new Type[]{typeof(string)}),
                        null,
                        value,
                        fieldSelector);
                    break;
                case FilterCondition.EndsWith:
                    clause = createCall(
                        typeof(string).GetMethod("EndsWith", new Type[]{ typeof(string) }),
                        null,
                        value,
                        fieldSelector);
                    break;
                case FilterCondition.Contains:
                    clause = createCall(
                        typeof(string).GetMethod("Contains"),
                        null,
                        value,
                        fieldSelector);
                    break;
                default:
                    if (value != null && value is IEnumerable && !(value is string))
                    {
                        var nVal = new List<F>();
                        foreach(var x in value as IEnumerable)
                        {
                            nVal.Add((F)x);
                        }
                        clause = createInverseCall(
                            nVal.GetType().GetMethod("Contains") ?? contains.MakeGenericMethod(new Type[] { typeof(F) }),
                            null,
                            nVal,
                            fieldSelector);
                    }
                    else
                    {
                        clause = createInverseCall(
                            value.GetType().GetMethod("Contains") ?? contains.MakeGenericMethod(new Type[] { typeof(F) }),
                            null,
                            value,
                            fieldSelector);
                    }
                    break;
            }
            return Add(toAdd, clause as Expression<Func<T, bool>>);
        }
        public FilterBuilder<T> Add(bool toAdd, Expression<Func<T, bool>> filterClause)
        {
            if (filterClause == null) throw new ArgumentNullException("fieldSelector");
            if (!toAdd) return this;
            if (filterClause.Parameters == null || filterClause.Parameters.Count != 1)
                throw (new ArgumentException(Resources.FilterParameterNumber, "filterClause"));
            if (param == null) param = filterClause.Parameters[0];
            else if (filterClause.Parameters[0].Name != param.Name) 
                throw (new ArgumentException(Resources.FilterParameterName, "filterClause"));

            if (currFilter == null) currFilter = filterClause.Body;
            else
            {
                if(logicalOperator == FilterLogicalOperator.And)
                    currFilter = Expression.And(currFilter, adjustP(filterClause.Body));
                else if (logicalOperator == FilterLogicalOperator.Or) 
                    currFilter = Expression.Or(currFilter, adjustP(filterClause.Body));
                else
                    currFilter = Expression.ExclusiveOr(currFilter, adjustP(filterClause.Body));
            }
            return this;
        }
        public Expression<Func<T, bool>> Get()
        {
            if(currFilter==null) return null;
            return Expression.Lambda<Func<T, bool>>(currFilter, param);
        }
        public FilterBuilder<T> Open(bool toAdd, FilterLogicalOperator logicalOperator)
        {
            FilterBuilder<T> res = new FilterBuilder<T>(logicalOperator);
            res.father = this;
            return res;
        }
        public FilterBuilder<T> Close(bool toAdd)
        {
            if (father == null) return this;
            if (currFilter != null && toAdd) father.Add(true, this.Get());
            return father;
        }
    }
}
