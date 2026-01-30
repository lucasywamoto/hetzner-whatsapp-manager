namespace HetznerWhatsApp.Api.Models;

public class HetznerServer
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public HetznerPublicNet? PublicNet { get; set; }
    public HetznerServerType? ServerType { get; set; }
    public HetznerDatacenter? Datacenter { get; set; }
}

public class HetznerPublicNet
{
    public HetznerIpv4? Ipv4 { get; set; }
    public HetznerIpv6? Ipv6 { get; set; }
}

public class HetznerIpv4
{
    public string Ip { get; set; } = string.Empty;
}

public class HetznerIpv6
{
    public string Ip { get; set; } = string.Empty;
}

public class HetznerServerType
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Cores { get; set; }
    public double Memory { get; set; }
    public int Disk { get; set; }
}

public class HetznerDatacenter
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public HetznerLocation? Location { get; set; }
}

public class HetznerLocation
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class HetznerServersResponse
{
    public List<HetznerServer> Servers { get; set; } = new();
}

public class HetznerServerResponse
{
    public HetznerServer? Server { get; set; }
}

public class HetznerActionResponse
{
    public HetznerAction? Action { get; set; }
}

public class HetznerAction
{
    public long Id { get; set; }
    public string Command { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
}
