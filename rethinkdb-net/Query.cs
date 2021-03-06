using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IQuery
    {
        Term GenerateTerm(IDatumConverterFactory datumConverterFactory);
    }

    [ImmutableObject(true)]
    public interface ISingleObjectQuery<T> : IQuery
    {
    }

    [ImmutableObject(true)]
    public interface IMutableSingleObjectQuery<T> : ISingleObjectQuery<T>
    {
    }

    [ImmutableObject(true)]
    public interface ISequenceQuery<T> : IQuery
    {
    }

    [ImmutableObject(true)]
    public interface IWriteQuery<TResponseType> : IQuery
    {
    }

    [ImmutableObject(true)]
    public interface IGroupByReduction<TReductionType>
    {
        Term GenerateReductionObject(IDatumConverterFactory datumConverterFactory);
    }

    public static class Query
    {
        public static DbQuery Db(string db)
        {
            return new DbQuery(db);
        }

        public static DbCreateQuery DbCreate(string db)
        {
            return new DbCreateQuery(db);
        }

        public static DbDropQuery DbDrop(string db)
        {
            return new DbDropQuery(db);
        }

        public static DbListQuery DbList()
        {
            return new DbListQuery();
        }

        public static GetQuery<T> Get<T>(this ISequenceQuery<T> target, string primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(target, primaryKey, primaryAttribute);
        }

        public static GetQuery<T> Get<T>(this ISequenceQuery<T> target, double primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(target, primaryKey, primaryAttribute);
        }

        public static GetAllQuery<TSequence, TKey> GetAll<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey key, string indexName = null)
        {
            return new GetAllQuery<TSequence, TKey>(target, key, indexName);
        }

        public static FilterQuery<T> Filter<T>(this ISequenceQuery<T> target, Expression<Func<T, bool>> filterExpression)
        {
            return new FilterQuery<T>(target, filterExpression);
        }

        // LINQ-compatible alias for Filter
        public static FilterQuery<T> Where<T>(this ISequenceQuery<T> target, Expression<Func<T, bool>> filterExpression)
        {
            return target.Filter(filterExpression);
        }

        public static UpdateQuery<T> Update<T>(this ISequenceQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateQuery<T>(target, updateExpression, nonAtomic);
        }

        public static UpdateQuery<T> Update<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateQuery<T>(target, updateExpression, nonAtomic);
        }

        public static UpdateAndReturnValueQuery<T> UpdateAndReturnValue<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateAndReturnValueQuery<T>(target, updateExpression, nonAtomic);
        }

        public static DeleteQuery<T> Delete<T>(this ISequenceQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static DeleteQuery<T> Delete<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static DeleteAndReturnValueQuery<T> DeleteAndReturnValue<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteAndReturnValueQuery<T>(target);
        }

        public static ReplaceQuery<T> Replace<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceQuery<T>(target, newObject, nonAtomic);
        }

        public static ReplaceAndReturnValueQuery<T> ReplaceAndReturnValue<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceAndReturnValueQuery<T>(target, newObject, nonAtomic);
        }

        public static BetweenQuery<TSequence, TKey> Between<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey leftKey, TKey rightKey, string indexName = null, Bound leftBound = Bound.Closed, Bound rightBound = Bound.Open)
        {
            return new BetweenQuery<TSequence, TKey>(target, leftKey, rightKey, indexName, leftBound, rightBound);
        }

        public static CountQuery<T> Count<T>(this ISequenceQuery<T> target)
        {
            return new CountQuery<T>(target);
        }

        public static ExprQuery<T> Expr<T>(T @object)
        {
            return new ExprQuery<T>(@object);
        }

        public static ExprQuery<T> Expr<T>(Expression<Func<T>> objectExpr)
        {
            return new ExprQuery<T>(objectExpr);
        }

        public static ExprSequenceQuery<T> Expr<T>(IEnumerable<T> enumerable)
        {
            return new ExprSequenceQuery<T>(enumerable);
        }

        public static MapQuery<TOriginal, TTarget> Map<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return new MapQuery<TOriginal, TTarget>(sequenceQuery, mapExpression);
        }

        // LINQ-compatible alias for Map
        public static MapQuery<TOriginal, TTarget> Select<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return sequenceQuery.Map(mapExpression);
        }

        public enum OrderByDirection
        {
            Ascending,
            Descending,
        }

        public static OrderByQuery<T> OrderBy<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, object>> memberReferenceExpression, OrderByDirection direction)
        {
            return new OrderByQuery<T>(sequenceQuery, new Tuple<Expression<Func<T, object>>, OrderByDirection>(memberReferenceExpression, direction));
        }

        // LINQ-compatible OrderBy
        public static OrderByQuery<T> OrderBy<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, object>> memberReferenceExpression)
        {
            return sequenceQuery.OrderBy(memberReferenceExpression, OrderByDirection.Ascending);
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> OrderByDescending<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, object>> memberReferenceExpression)
        {
            return sequenceQuery.OrderBy(memberReferenceExpression, OrderByDirection.Descending);
        }

        public static OrderByQuery<T> ThenBy<T>(this OrderByQuery<T> orderByQuery, Expression<Func<T, object>> memberReferenceExpression, OrderByDirection direction)
        {
            return new OrderByQuery<T>(
                orderByQuery.SequenceQuery,
                orderByQuery.OrderByMembers.Concat(
                    Enumerable.Repeat(
                        new Tuple<Expression<Func<T, object>>, OrderByDirection>(memberReferenceExpression, direction),
                        1)).ToArray());
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> ThenBy<T>(this OrderByQuery<T> orderByQuery, Expression<Func<T, object>> memberReferenceExpression)
        {
            return orderByQuery.ThenBy(memberReferenceExpression, OrderByDirection.Ascending);
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> ThenByDescending<T>(this OrderByQuery<T> orderByQuery, Expression<Func<T, object>> memberReferenceExpression)
        {
            return orderByQuery.ThenBy(memberReferenceExpression, OrderByDirection.Descending);
        }

        public static SkipQuery<T> Skip<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return new SkipQuery<T>(sequenceQuery, count);
        }

        public static LimitQuery<T> Limit<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return new LimitQuery<T>(sequenceQuery, count);
        }

        // LINQ compatible alias for Limit
        public static LimitQuery<T> Take<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return sequenceQuery.Limit(count);
        }

        public static SliceQuery<T> Slice<T>(this ISequenceQuery<T> sequenceQuery, int startIndex, int? endIndex = null)
        {
            return new SliceQuery<T>(sequenceQuery, startIndex, endIndex);
        }

        public static InnerJoinQuery<TLeft, TRight> InnerJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new InnerJoinQuery<TLeft, TRight>(leftQuery, rightQuery, joinPredicate);
        }

        public static OuterJoinQuery<TLeft, TRight> OuterJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new OuterJoinQuery<TLeft, TRight>(leftQuery, rightQuery, joinPredicate);
        }

        public static ZipQuery<TLeft, TRight, TTarget> Zip<TLeft, TRight, TTarget>(this ISequenceQuery<Tuple<TLeft, TRight>> sequenceQuery)
        {
            return new ZipQuery<TLeft, TRight, TTarget>(sequenceQuery);
        }

        public static EqJoinQuery<TLeft, TRight> EqJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, Expression<Func<TLeft, object>> leftMemberReferenceExpression, ISequenceQuery<TRight> rightQuery)
        {
            return new EqJoinQuery<TLeft, TRight>(leftQuery, leftMemberReferenceExpression, rightQuery, null);
        }

        public static EqJoinQuery<TLeft, TRight> EqJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, Expression<Func<TLeft, object>> leftMemberReferenceExpression, ISequenceQuery<TRight> rightQuery, string indexName)
        {
            return new EqJoinQuery<TLeft, TRight>(leftQuery, leftMemberReferenceExpression, rightQuery, indexName);
        }

        public static ReduceQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction);
        }

        public static ReduceQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction, T @base)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction, @base);
        }

        public static NthQuery<T> Nth<T>(this ISequenceQuery<T> sequenceQuery, int index)
        {
            return new NthQuery<T>(sequenceQuery, index);
        }

        public static DistinctQuery<T> Distinct<T>(this ISequenceQuery<T> sequenceQuery)
        {
            return new DistinctQuery<T>(sequenceQuery);
        }

        public static GroupedMapReduceQuery<TOriginal, TGroup, TMap> GroupedMapReduce<TOriginal, TGroup, TMap>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction)
        {
            return new GroupedMapReduceQuery<TOriginal, TGroup, TMap>(sequenceQuery, grouping, mapping, reduction);
        }

        public static GroupedMapReduceQuery<TOriginal, TGroup, TMap> GroupedMapReduce<TOriginal, TGroup, TMap>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction, TMap @base)
        {
            return new GroupedMapReduceQuery<TOriginal, TGroup, TMap>(sequenceQuery, grouping, mapping, reduction, @base);
        }

        public static ConcatMapQuery<TOriginal, TTarget> ConcatMap<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, IEnumerable<TTarget>>> mapping)
        {
            return new ConcatMapQuery<TOriginal, TTarget>(sequenceQuery, mapping);
        }

        public static UnionQuery<T> Union<T>(this ISequenceQuery<T> query1, ISequenceQuery<T> query2)
        {
            return new UnionQuery<T>(query1, query2);
        }

        public static GroupByQuery<TObject, TReductionType, TGroupKeyType> GroupBy<TObject, TReductionType, TGroupKeyType>(this ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, Expression<Func<TObject, TGroupKeyType>> groupKeyConstructor)
        {
            return new GroupByQuery<TObject, TReductionType, TGroupKeyType>(sequenceQuery, reductionObject, groupKeyConstructor);
        }

        public static CountReduction Count()
        {
            return CountReduction.Instance;
        }

        public static SumReduction<TObject> Sum<TObject>(Expression<Func<TObject, double>> numericMemberReference)
        {
            return new SumReduction<TObject>(numericMemberReference);
        }

        public static AvgReduction<TObject> Avg<TObject>(Expression<Func<TObject, double>> numericMemberReference)
        {
            return new AvgReduction<TObject>(numericMemberReference);
        }

        public static NowQuery Now()
        {
            return new NowQuery();
        }
    }
}
