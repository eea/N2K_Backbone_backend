namespace N2K_BackboneBackEnd.Data
{
	public class Extractions
	{
		public Extractions() {}
		
		//All changes by SiteCode
		public static string AllChangesBySiteCode = @"
			SELECT
				STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion) AS 'BioRegions',
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				ISNULL([NewValue], '') AS [NewValue],
				ISNULL([OldValue], '') AS [OldValue],
				ISNULL([Code], '') AS [Code]
			FROM [dbo].[Changes] C
			INNER JOIN [dbo].[Sites] S ON C.[SiteCode] = S.[SiteCode]
				AND C.[Version] = S.[Version]
			LEFT JOIN (
				SELECT BR.[SiteCode],
					BR.[Version],
					BT.[RefBioGeoName] AS 'BioRegion'
				FROM [dbo].[BioRegions] BR
				INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
				) sub ON sub.[SiteCode] = S.[SiteCode]
				AND sub.[Version] = S.[Version]
			WHERE [Country] = @COUNTRYCODE
				AND C.[N2KVersioningVersion] = @COUNTRYVERSION
				--AND [ChangeType] != 'Deletion of Spatial Area'
				--AND [ChangeType] != 'Additon of Spatial Area'
				AND [ChangeType] != 'Sites added due to a change of BGR'
				AND [ChangeType] != 'Sites deleted due to a change of BGR'
			GROUP BY C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				[NewValue],
				[OldValue],
				[Code]
			UNION
			SELECT DISTINCT STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion) AS 'BioRegions',
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				ISNULL(STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion), '') AS [NewValue],
				ISNULL(STRING_AGG(sub2.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion), '') AS [OldValue],
				ISNULL([Code], '') AS [Code]
			FROM [dbo].[Changes] C
			INNER JOIN [dbo].[Sites] S ON C.[SiteCode] = S.[SiteCode]
				AND C.[Version] = S.[Version]
			LEFT JOIN (
				SELECT DISTINCT BR.[SiteCode],
					BR.[Version],
					BT.[RefBioGeoName] AS 'BioRegion'
				FROM [dbo].[BioRegions] BR
				INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
				) sub ON sub.[SiteCode] = C.[SiteCode]
				AND sub.[Version] = C.[Version]
			LEFT JOIN (
				SELECT DISTINCT BR.[SiteCode],
					BR.[Version],
					BT.[RefBioGeoName] AS 'BioRegion'
				FROM [dbo].[BioRegions] BR
				INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
				) sub2 ON sub2.[SiteCode] = C.[SiteCode]
				AND sub2.[Version] = C.[VersionReferenceId]
			WHERE [ChangeType] LIKE '%BGR%'
				AND  [Country] = @COUNTRYCODE
				AND C.[N2KVersioningVersion] = @COUNTRYVERSION
				AND C.[Version] != C.[VersionReferenceId]
			GROUP BY C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				[NewValue],
				[OldValue],
				[Code]
			ORDER BY C.[SiteCode],
				[ChangeCategory],
				[ChangeType],
				[NewValue],
				[OldValue],
				[Code]
			";
		
		//All changes by Changes
		public static string AllChangesByChanges = @"
			SELECT
				STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion) AS 'BioRegions',
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				ISNULL([NewValue], '') AS [NewValue],
				ISNULL([OldValue], '') AS [OldValue],
				ISNULL([Code], '') AS [Code]
			FROM [dbo].[Changes] C
			INNER JOIN [dbo].[Sites] S ON C.[SiteCode] = S.[SiteCode]
				AND C.[Version] = S.[Version]
			LEFT JOIN (
				SELECT BR.[SiteCode],
					BR.[Version],
					BT.[RefBioGeoName] AS 'BioRegion'
				FROM [dbo].[BioRegions] BR
				INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
				) sub ON sub.[SiteCode] = S.[SiteCode]
				AND sub.[Version] = S.[Version]
			WHERE [Country] = @COUNTRYCODE
				AND C.[N2KVersioningVersion] = @COUNTRYVERSION
				--AND [ChangeType] != 'Deletion of Spatial Area'
				--AND [ChangeType] != 'Additon of Spatial Area'
				AND [ChangeType] != 'Sites added due to a change of BGR'
				AND [ChangeType] != 'Sites deleted due to a change of BGR'
			GROUP BY C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				[NewValue],
				[OldValue],
				[Code]
			UNION
			SELECT DISTINCT STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion) AS 'BioRegions',
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				ISNULL(STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion), '') AS [NewValue],
				ISNULL(STRING_AGG(sub2.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion), '') AS [OldValue],
				ISNULL([Code], '') AS [Code]
			FROM [dbo].[Changes] C
			INNER JOIN [dbo].[Sites] S ON C.[SiteCode] = S.[SiteCode]
				AND C.[Version] = S.[Version]
			LEFT JOIN (
				SELECT DISTINCT BR.[SiteCode],
					BR.[Version],
					BT.[RefBioGeoName] AS 'BioRegion'
				FROM [dbo].[BioRegions] BR
				INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
				) sub ON sub.[SiteCode] = C.[SiteCode]
				AND sub.[Version] = C.[Version]
			LEFT JOIN (
				SELECT DISTINCT BR.[SiteCode],
					BR.[Version],
					BT.[RefBioGeoName] AS 'BioRegion'
				FROM [dbo].[BioRegions] BR
				INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
				) sub2 ON sub2.[SiteCode] = C.[SiteCode]
				AND sub2.[Version] = C.[VersionReferenceId]
			WHERE [ChangeType] LIKE '%BGR%'
				AND  [Country] = @COUNTRYCODE
				AND C.[N2KVersioningVersion] = @COUNTRYVERSION
				AND C.[Version] != C.[VersionReferenceId]
			GROUP BY C.[SiteCode],
				S.[Name],
				S.[SiteType],
				[Level],
				[ChangeCategory],
				[ChangeType],
				[NewValue],
				[OldValue],
				[Code]
			ORDER BY [Level],
				[ChangeCategory],
				[ChangeType],
				C.[SiteCode],
				[NewValue],
				[OldValue],
				[Code]
			";
		
		//Spatial Changes
		public static string SpatialChanges = @"
			SELECT DISTINCT S.BioRegions,
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				ISNULL(spadec.NewValue, '') AS 'Spatial Area Decrease',
				ISNULL(spainc.NewValue, '') AS 'Spatial Area Increase',
				area.[NewValue] - area.[OldValue] AS 'SDF Area Difference'
			FROM [dbo].[Changes] C
			INNER JOIN (
				SELECT DISTINCT [dbo].[Sites].[SiteCode],
					[dbo].[Sites].[Version],
					[dbo].[Sites].[Name],
					[dbo].[Sites].[SiteType],
					STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion) AS 'BioRegions'
				FROM [dbo].[Sites]
				INNER JOIN (
					SELECT BR.[SiteCode],
						BR.[Version],
						BT.[RefBioGeoName] AS 'BioRegion'
					FROM [dbo].[BioRegions] BR
					INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
					) sub ON sub.[SiteCode] = [dbo].[Sites].[SiteCode]
					AND sub.[Version] = [dbo].[Sites].[Version]
				WHERE [CountryCode] = @COUNTRYCODE
					AND [N2KVersioningVersion] = @COUNTRYVERSION
				GROUP BY [dbo].[Sites].[SiteCode],
					[dbo].[Sites].[Version],
					[dbo].[Sites].[Name],
					[dbo].[Sites].[SiteType]
					--ORDER BY [dbo].[Sites].[SiteCode],
					--	[dbo].[Sites].[Version],
					--	[dbo].[Sites].[Name],
					--	[dbo].[Sites].[SiteType]
				) S ON C.[SiteCode] = S.[SiteCode]
				AND C.[Version] = S.[Version]
			LEFT JOIN (
				SELECT C.[SiteCode],
					[ChangeType],
					ISNULL([NewValue], '') AS [NewValue],
					ISNULL([OldValue], '') AS [OldValue]
				FROM [dbo].[Changes] C
				WHERE [Country] = @COUNTRYCODE
					AND C.[N2KVersioningVersion] = @COUNTRYVERSION
					AND [ChangeType] = 'Spatial Area Decrease'
				) spadec ON C.SiteCode = spadec.SiteCode
			LEFT JOIN (
				SELECT C.[SiteCode],
					[ChangeType],
					ISNULL([NewValue], '') AS [NewValue],
					ISNULL([OldValue], '') AS [OldValue]
				FROM [dbo].[Changes] C
				WHERE [Country] = @COUNTRYCODE
					AND C.[N2KVersioningVersion] = @COUNTRYVERSION
					AND [ChangeType] = 'Spatial Area Increase'
				) spainc ON C.SiteCode = spainc.SiteCode
			LEFT JOIN (
				SELECT C.[SiteCode],
					CONVERT(DECIMAL(10,4), [NewValue]) AS 'NewValue',
					CONVERT(DECIMAL(10,4), [OldValue]) AS 'OldValue'
				FROM [dbo].[Changes] C
				WHERE [Country] = @COUNTRYCODE
					AND C.[N2KVersioningVersion] = @COUNTRYVERSION
					AND (
						[ChangeType] = 'SDF Area Decrease'
						OR [ChangeType] = 'SDF Area Increase'
						)
				) area ON C.SiteCode = area.SiteCode
			WHERE (
					ISNULL(spadec.NewValue, '') != ''
					OR ISNULL(spainc.NewValue, '') != ''
					OR area.NewValue IS NOT NULL
					OR area.OldValue IS NOT NULL
				)
			
			ORDER BY C.[SiteCode]
			";
		
		public static string AreaChanges = @"
			SELECT
				STRING_AGG(sub.BioRegion, ',') WITHIN GROUP (ORDER BY sub.BioRegion) AS 'BioRegions'
				, s.SiteCode
				, s.Name
				, s.SiteType
				, ISNULL(spAreaDeleted.area, 0) as 'Spatial area deleted (ha)'
				, ISNULL(spAreaAdded.area, 0) as 'Spatial area added (ha)'
				, ISNULL(SUM(spFormerArea.area), 0) as 'Spatial former area (ha)'
				, ISNULL(SUM(spCurrentArea.area), 0) as 'Spatial current area (ha)'
				, ISNULL(SUM(tabFormerArea.area), 0) as 'SDF former area (ha)'
				, ISNULL(SUM(tabCurrentArea.area), 0) as 'SDF current area (ha)'
				, ISNULL(SUM(tabCurrentArea.area), 0) - ISNULL(SUM(tabFormerArea.area), 0) as 'SDF area difference (ha)'
			FROM Sites s
			LEFT JOIN (
				SELECT
					SiteCode,
					N2KVersioningVersion,
					SUM(CONVERT(DECIMAL(20,10), NewValue)) AS 'area'
				FROM Changes c
				WHERE ChangeType = 'Deletion of Spatial Area' OR ChangeType = 'Deleton of Spatial Area'
				GROUP BY SiteCode, N2KVersioningVersion, NewValue
				) spAreaDeleted ON spAreaDeleted.SiteCode = s.SiteCode
					AND spAreaDeleted.N2KVersioningVersion = s.N2KVersioningVersion
			LEFT JOIN (
				SELECT 
					SiteCode,
					N2KVersioningVersion,
					SUM(CONVERT(DECIMAL(20,10), NewValue)) AS 'area'
				FROM Changes c
				WHERE ChangeType = 'Additon of Spatial Area'
					OR ChangeType = 'Addition of Spatial Area'
				GROUP BY SiteCode, N2KVersioningVersion, NewValue
				) spAreaAdded ON spAreaAdded.SiteCode = s.SiteCode
					AND spAreaAdded.N2KVersioningVersion = s.N2KVersioningVersion
			LEFT JOIN (
				SELECT s.SiteCode, ss.area AS 'Area'
				FROM Sites s
				INNER JOIN SiteSpatial ss
					ON ss.SiteCode = s.SiteCode
					AND ss.Version = s.Version
				INNER JOIN ProcessedEnvelopes pe
					ON pe.Country = s.CountryCode 
					AND pe.Version = s.N2KVersioningVersion
				WHERE pe.Status = 8
				) spFormerArea ON spFormerArea.SiteCode = s.SiteCode
			LEFT JOIN (
				SELECT s.SiteCode, ss.area AS 'Area'
				FROM Sites s
				INNER JOIN SiteSpatial ss
					ON ss.SiteCode = s.SiteCode
					AND ss.Version = s.Version
				INNER JOIN ProcessedEnvelopes pe
					ON pe.Country = s.CountryCode 
					AND pe.Version = s.N2KVersioningVersion
				WHERE pe.Status = 3
				) spCurrentArea ON spCurrentArea.SiteCode = s.SiteCode
			LEFT JOIN (
				SELECT SiteCode, Area
				FROM Sites s
				INNER JOIN ProcessedEnvelopes pe
					ON pe.Country = s.CountryCode
					AND pe.Version = s.N2KVersioningVersion
				WHERE pe.Status = 8
				) tabFormerArea ON tabFormerArea.SiteCode = s.SiteCode
			LEFT JOIN (
				SELECT SiteCode, Area
				FROM Sites s
				INNER JOIN ProcessedEnvelopes pe
					ON pe.Country = s.CountryCode
					AND pe.Version = s.N2KVersioningVersion
				WHERE pe.Status = 3
				) tabCurrentArea ON tabCurrentArea.SiteCode = s.SiteCode
			LEFT JOIN (
				SELECT BR.[SiteCode],
					BR.[Version],
					BT.[RefBioGeoName] AS 'BioRegion'
				FROM [dbo].[BioRegions] BR
				INNER JOIN [dbo].[BioRegionTypes] BT ON BR.[BGRID] = BT.[Code]
				) sub ON sub.[SiteCode] = S.[SiteCode]
				AND sub.[Version] = S.[Version]
			WHERE
				s.CountryCode = @COUNTRYCODE AND s.N2KVersioningVersion = @COUNTRYVERSION
			GROUP BY s.SiteCode, s.SiteType, s.Name, spAreaDeleted.area, spAreaAdded.area
			";
	}
}
