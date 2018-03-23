/**********************************************************************/
/*                                                                    */
/*          Add ApplicationEndpoint to DiscoveryServiceBASE           */
/*                                                                    */
/**********************************************************************/

DECLARE @ServiceNM varchar(255) = 'IdentityProviderSearchService'
DECLARE @ServiceVersion int = 1
DECLARE @ServiceUrl varchar(500) = 'ServiceUrlPlaceHolder' + '/v1'
DECLARE @DiscoveryTypeCD varchar(60) = 'Service'
DECLARE @HiddenFLG bit = 1
DECLARE @FriendlyNM varchar(255) = 'IdentityProviderSearchService'
DECLARE @IconTXT varchar(max) = 'data:image/png;base64,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'
DECLARE @DescriptionTXT nvarchar(4000)= 'A service for searching principals in a member directory.'

IF EXISTS
(
    SELECT *
    FROM [CatalystAdmin].[DiscoveryServiceBASE]
    WHERE ServiceNM = @ServiceNM  AND ServiceVersion = @ServiceVersion
)
BEGIN
    UPDATE [CatalystAdmin].[DiscoveryServiceBASE]
        SET [ServiceUrl] = @ServiceUrl, 
	   DiscoveryTypeCD = @DiscoveryTypeCD, 
	   HiddenFLG = @HiddenFLG, 
	   FriendlyNM = @FriendlyNM, 
	   IconTXT = @IconTXT, 
	   DescriptionTXT = @DescriptionTXT
    WHERE ServiceNM = @ServiceNM  AND ServiceVersion = @ServiceVersion;
END
ELSE
BEGIN
    INSERT INTO [CatalystAdmin].[DiscoveryServiceBASE]
    ([ServiceNM],
    [ServiceUrl],
    [ServiceVersion],
    [DiscoveryTypeCD],
    [HiddenFLG],
    [FriendlyNM],
    [IconTXT],
    [DescriptionTXT]
    )
		  VALUES
    (@ServiceNM ,
    @ServiceUrl,
    @ServiceVersion,
    @DiscoveryTypeCD,
    @HiddenFLG,
    @FriendlyNM,
    @IconTXT,
    @DescriptionTXT
    );
END