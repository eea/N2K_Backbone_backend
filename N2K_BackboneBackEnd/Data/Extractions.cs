namespace N2K_BackboneBackEnd.Data
{
    public class Extractions
    {
        public Extractions() { }

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
				--AND [ChangeType] != 'Addition of Spatial Area'
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
				--AND [ChangeType] != 'Addition of Spatial Area'
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

        public static string SpatialChanges = @"
			SELECT DISTINCT S.BioRegions,
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				CAST(ISNULL(spadec.NewValue, '') AS NVARCHAR(MAX)) AS 'Spatial Area Decrease',
				CAST(ISNULL(spainc.NewValue, '') AS NVARCHAR(MAX)) AS 'Spatial Area Increase',
				CAST(ISNULL(CAST(area.[NewValue] AS DECIMAL(38, 4)), 0) - ISNULL(CAST(area.[OldValue] AS DECIMAL(38, 4)), 0) AS NVARCHAR(MAX)) AS 'SDF Area Difference'
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
			SELECT DISTINCT S.[BioRegions],
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				CAST(ISNULL(spAreaDeleted.area, 0) AS NVARCHAR(MAX)) AS 'Spatial area deleted (ha)',
				CAST(ISNULL(spAreaAdded.area, 0) AS NVARCHAR(MAX)) AS 'Spatial area added (ha)',
				CAST(SpatialAreaChanged.OldValue AS NVARCHAR(MAX)) AS 'Spatial former area (ha)',
				CAST(SpatialAreaChanged.NewValue AS NVARCHAR(MAX)) AS 'Spatial current area (ha)',
				CAST(AreaChanged.OldValue AS NVARCHAR(MAX)) AS 'SDF former area (ha)',
				CAST(AreaChanged.NewValue AS NVARCHAR(MAX)) AS 'SDF current area (ha)',
				CAST(ISNULL(CAST(AreaChanged.NewValue AS DECIMAL(38, 4)), 0) - ISNULL(CAST(AreaChanged.OldValue AS DECIMAL(38, 4)), 0) AS NVARCHAR(MAX)) AS 'SDF area difference (ha)'
			FROM [dbo].[Changes] C
			INNER JOIN (
				SELECT DISTINCT STRING_AGG(B.[RefBioGeoName], ', ') WITHIN
				GROUP (
						ORDER BY B.[RefBioGeoName]
						) AS 'BioRegions',
					S.[SiteCode],
					S.[Name],
					S.[SiteType],
					S.[N2KVersioningVersion]
				FROM [dbo].[Sites] S
				INNER JOIN (
					SELECT DISTINCT [SiteCode],
						[Version],
						[RefBioGeoName],
						[Percentage]
					FROM [dbo].[BioRegions] B
					INNER JOIN [dbo].[BioRegionTypes] BT ON B.BGRID = BT.[Code]
					) B ON S.[SiteCode] = B.[SiteCode]
					AND S.[Version] = B.[Version]
				GROUP BY S.[SiteCode],
					S.[Name],
					S.[SiteType],
					S.[N2KVersioningVersion]
				) S ON S.[SiteCode] = C.[SiteCode]
				AND S.[N2KVersioningVersion] = C.[N2KVersioningVersion]
				AND (
					C.[ChangeType] = 'Deletion of Spatial Area'
					OR C.[ChangeType] = 'Addition of Spatial Area'
					OR C.[ChangeType] = 'Spatial Area Decrease'
					OR C.[ChangeType] = 'Spatial Area Increase'
					OR C.[ChangeType] = 'SDF Area Increase'
					OR C.[ChangeType] = 'SDF Area Decrease'
					OR C.[ChangeType] = 'SDF Area Change'
					)
			LEFT JOIN (
				SELECT SiteCode,
					N2KVersioningVersion,
					SUM(CONVERT(DECIMAL(20, 10), NewValue)) AS 'area'
				FROM Changes c
				WHERE ChangeType = 'Deletion of Spatial Area'
				GROUP BY SiteCode,
					N2KVersioningVersion,
					NewValue
				) spAreaDeleted ON spAreaDeleted.SiteCode = S.[SiteCode]
				AND spAreaDeleted.N2KVersioningVersion = S.[N2KVersioningVersion]
			LEFT JOIN (
				SELECT SiteCode,
					N2KVersioningVersion,
					SUM(CONVERT(DECIMAL(20, 10), NewValue)) AS 'area'
				FROM Changes c
				WHERE ChangeType = 'Addition of Spatial Area'
				GROUP BY SiteCode,
					N2KVersioningVersion,
					NewValue
				) spAreaAdded ON spAreaAdded.SiteCode = S.[SiteCode]
				AND spAreaAdded.N2KVersioningVersion = S.[N2KVersioningVersion]
			LEFT JOIN (
				SELECT SiteCode,
					N2KVersioningVersion,
					NewValue,
					OldValue
				FROM Changes c
				WHERE C.[ChangeType] = 'Spatial Area Decrease'
					OR C.[ChangeType] = 'Spatial Area Increase'
				) SpatialAreaChanged ON SpatialAreaChanged.SiteCode = S.[SiteCode]
				AND SpatialAreaChanged.N2KVersioningVersion = S.[N2KVersioningVersion]
			LEFT JOIN (
				SELECT SiteCode,
					N2KVersioningVersion,
					NewValue,
					OldValue
				FROM Changes c
				WHERE C.[ChangeType] = 'SDF Area Increase'
					OR C.[ChangeType] = 'SDF Area Decrease'
					OR C.[ChangeType] = 'SDF Area Change'
				) AreaChanged ON AreaChanged.SiteCode = S.[SiteCode]
				AND AreaChanged.N2KVersioningVersion = S.[N2KVersioningVersion]
			WHERE C.[Country] = @COUNTRYCODE
				AND C.[N2KVersioningVersion] = @COUNTRYVERSION
			GROUP BY S.[BioRegions],
				C.[SiteCode],
				S.[Name],
				S.[SiteType],
				spAreaDeleted.area,
				spAreaAdded.area,
				SpatialAreaChanged.OldValue,
				SpatialAreaChanged.NewValue,
				AreaChanged.OldValue,
				AreaChanged.NewValue

			UNION

			SELECT DISTINCT S.[BioRegions],
				S.[SiteCode],
				S.[Name],
				S.[SiteType],
				'0.0000000000' AS 'Spatial area deleted (ha)',
				'0.0000000000' AS 'Spatial area added (ha)',
				CAST(SS.[area] AS NVARCHAR(MAX)) AS 'Spatial former area (ha)',
				CAST(SS.[area] AS NVARCHAR(MAX)) AS 'Spatial current area (ha)',
				CAST(S.[Area] AS NVARCHAR(MAX)) AS 'SDF former area (ha)',
				CAST(S.[Area] AS NVARCHAR(MAX)) AS 'SDF current area (ha)',
				'0.0000' AS 'SDF area difference (ha)'
			FROM (
				SELECT DISTINCT STRING_AGG(B.[RefBioGeoName], ', ') WITHIN
				GROUP (
						ORDER BY B.[RefBioGeoName]
						) AS 'BioRegions',
					S.[SiteCode],
					S.[Version],
					S.[Name],
					S.[SiteType],
					S.[CountryCode],
					S.[N2KVersioningVersion],
					S.[Area]
				FROM [dbo].[Sites] S
				INNER JOIN (
					SELECT DISTINCT [SiteCode],
						[Version],
						[RefBioGeoName],
						[Percentage]
					FROM [dbo].[BioRegions] B
					INNER JOIN [dbo].[BioRegionTypes] BT ON B.BGRID = BT.[Code]
					) B ON S.[SiteCode] = B.[SiteCode]
					AND S.[Version] = B.[Version]
				GROUP BY S.[SiteCode],
					S.[Version],
					S.[Name],
					S.[SiteType],
					S.[CountryCode],
					S.[N2KVersioningVersion],
					S.[Area]
				) S
			LEFT JOIN [dbo].[SiteSpatial] SS ON SS.[SiteCode] = S.[SiteCode]
				AND SS.[Version] = S.[Version]
			WHERE S.[CountryCode] = @COUNTRYCODE
				AND S.[N2KVersioningVersion] = @COUNTRYVERSION
				AND S.[SiteCode] NOT IN (
					SELECT [SiteCode]
					FROM [dbo].[Changes] C
					WHERE C.[Country] = @COUNTRYCODE
						AND C.[N2KVersioningVersion] = @COUNTRYVERSION
						AND (
							C.[ChangeType] = 'Deletion of Spatial Area'
							OR C.[ChangeType] = 'Addition of Spatial Area'
							OR C.[ChangeType] = 'Spatial Area Decrease'
							OR C.[ChangeType] = 'Spatial Area Increase'
							OR C.[ChangeType] = 'SDF Area Increase'
							OR C.[ChangeType] = 'SDF Area Decrease'
							OR C.[ChangeType] = 'SDF Area Change'
							)
					)
			GROUP BY S.[BioRegions],
				S.[SiteCode],
				S.[Name],
				S.[SiteType],
				SS.[area],
				S.[Area]
			ORDER BY C.[SiteCode]
			";
    }
}