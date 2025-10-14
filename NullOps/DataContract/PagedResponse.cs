using JetBrains.Annotations;

namespace NullOps.DataContract;

public class PagedResponse<T>
{
    public IEnumerable<T>? Data { get; set; }
    
    public int? Page { get; set; }
    
    public int? PageSize { get; set; }
    
    public int? TotalCount { get; set; }
    
    [PublicAPI]
    public ResponseError? Error { get; set; }
}
