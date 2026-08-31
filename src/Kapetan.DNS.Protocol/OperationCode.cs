namespace DNS.Protocol;

public enum OperationCode
{
    Query = 0,

    IQuery = 1,

    Status = 2,

    Reserved = 3,

    Notify = 4,

    Update = 5,
}
