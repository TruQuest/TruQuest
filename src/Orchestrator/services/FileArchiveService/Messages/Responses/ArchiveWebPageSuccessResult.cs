namespace Messages.Responses;

internal class ArchiveWebPageSuccessResult
{
    public required string HtmlIpfsCid { get; init; }
    public required string JpgIpfsCid { get; init; }
}