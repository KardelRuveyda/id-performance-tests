using NUlid;

namespace IdStrategiesDemo.Models;

// 1) INT IDENTITY (clustered PK)
public class RecInt
{
    public int Id { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

// 2) GUID v4 (clustered PK)
public class RecGuidV4
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

// 3) ULID (string, 26 char) clustered PK
public class RecUlidString
{
    public string Id { get; set; } = Ulid.NewUlid().ToString(); // 26 char Crockford Base32
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

// 4) ULID (binary 16) clustered PK
public class RecUlidBinary
{
    public byte[] Id { get; set; } = Ulid.NewUlid().ToByteArray(); // 16-byte
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

// 5) GUID v4 (nonclustered PK) + CreatedOn (clustered index)
public class RecGuidV4_ClusterOnDate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

// 6) GUID v7 (clustered PK) - .NET 9
public class RecGuidV7
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTimeOffset.UtcNow);
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}