namespace Appwrite.Helpers;


public static class QueryBuilder
{
    public static Query Equal(this Query content, string property, string value)
    {
        content.Add(new QueryEqual(property, value));

        return content;
    }

    public static string[] BuildUrl(this Query content)
    {
        var list = content.Queries.ToList();

        var urls = list.Select(queryBase => queryBase.ToUrl()).ToArray();

        return urls;
    }
}

public class Query
{
    public List<QueryBase> Queries { get; set; } = new List<QueryBase>();

    public void Add(QueryBase query)
    {
        Queries.Add(query);
    }
}

public abstract class QueryBase
{
    public virtual string ToUrl()
    {
        throw new NotImplementedException();
    }
}

public class QueryEqual : QueryBase
{
    public QueryEqual(){}
    
    public QueryEqual(string property, string value)
    {
        Property = property;
        Value = value;
    }
    
    public string Property { get; set; }
    public string Value { get; set; }

    public override string ToUrl()
    {
        return $"equal(\"{Property}\",[\"{Value}\"])";
    }
}

