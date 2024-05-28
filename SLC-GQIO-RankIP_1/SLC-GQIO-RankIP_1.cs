using Skyline.DataMiner.Analytics.GenericInterface;
using System.Net;

[GQIMetaData(Name = "Rank IPv4 address")]
public sealed class IPv4RankOperator : IGQIColumnOperator, IGQIRowOperator, IGQIInputArguments
{
    private readonly GQIColumnDropdownArgument _ipAddressColumnArg;

    private GQIColumn<string> _ipAddressColumn;
    private GQIColumn<int> _ipRankColumn;

    public IPv4RankOperator()
    {
        _ipAddressColumnArg = new GQIColumnDropdownArgument("IP address")
        {
            Types = new[] { GQIColumnType.String },
            IsRequired = true,
        };
    }

    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { _ipAddressColumnArg };
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _ipAddressColumn = (GQIColumn<string>)args.GetArgumentValue(_ipAddressColumnArg);
        return default;
    }

    public void HandleColumns(GQIEditableHeader header)
    {
        var ipRankName = $"RANK({_ipAddressColumn.Name})";

        _ipRankColumn = new GQIIntColumn(ipRankName);
        header.AddColumns(_ipRankColumn);
    }

    public void HandleRow(GQIEditableRow row)
    {
        string ipAddress = row.GetValue(_ipAddressColumn);
        int ranking = GetRanking(ipAddress);
        row.SetValue(_ipRankColumn, ranking);
    }

    private int GetRanking(string ipAddress)
    {
        byte[] bytes = IPAddress.Parse(ipAddress).GetAddressBytes();
        uint ranking = (uint)bytes[0] << 24 | (uint)bytes[1] << 16 | (uint)bytes[2] << 8 | bytes[3];
        return UnsignedToSigned(ranking);
    }

    private int UnsignedToSigned(uint ranking)
    {
        if (ranking > int.MaxValue)
            return (int)(ranking - int.MaxValue);
        else
            return (int)ranking - int.MaxValue;
    }
}