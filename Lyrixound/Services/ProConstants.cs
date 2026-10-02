namespace Lyrixound.Services;

internal static class ProConstants
{
    /// <summary>Parent app Store ID (Lyrixound).</summary>
    public const string StoreAppId = "9MSQSDJH510N";

    /// <summary>
    /// Developer Product ID from Partner Center (add-on creation).
    /// Matches <see cref="Windows.Services.Store.StoreLicense.InAppOfferToken"/>.
    /// </summary>
    public const string ProProductId = "LyrixoundPro";

    /// <summary>
    /// Store ID of the Durable add-on from Partner Center overview (required by RequestPurchaseAsync).
    /// </summary>
    public const string ProStoreId = "9NB91P434L55";

    public const string SupportEmail = "a.arekhva@gmail.com";
}
