using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Vml.Office;
using DuckDB.NET.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Enumerations;        
using N2K_BackboneBackEnd.Models;
using N2K_BackboneBackEnd.Models.backbone_db;
using N2K_BackboneBackEnd.Models.release_db;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Security.Policy;
using System.Text;
using Level = N2K_BackboneBackEnd.Enumerations.Level;


namespace N2K_BackboneBackEnd.Helpers
{
    public class DuckDBLoader:IDisposable
    {
        private readonly N2KBackboneContext _dataContext;
        private DuckDB.NET.Data.DuckDBConnection _duckDBConnection;


        public DuckDBLoader(N2KBackboneContext dataContext)
        {
            _dataContext = dataContext;
            _duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db;ACCESS_MODE=READ_ONLY");
            _duckDBConnection.Open();
        }

        public async Task CreateDuckDBSchema()
        {
            await Task.Delay(10);

            try
            {
                using (var duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db"))
                {
                    duckDBConnection.Open();

                    //create schema for ActiveSites
                    try
                    {
                        using (var command = duckDBConnection.CreateCommand())
                        {
                            command.CommandText = @"CREATE TABLE IF NOT EXISTS ActiveSites (
	                        SiteCode VARCHAR  ,
	                        Version BIGINT  ,
	                        Current Boolean ,
	                        Name VARCHAR ,
	                        CompilationDate DATE ,
	                        ModifyTS DATE ,
	                        CurrentStatus INTEGER ,
	                        CountryCode VARCHAR ,
	                        SiteType VARCHAR ,
	                        AltitudeMin DECIMAL(18, 3) ,
	                        AltitudeMax DECIMAL(18, 3) ,
	                        N2KVersioningVersion INTEGER ,
	                        N2KVersioningRef INTEGER ,
	                        Area DECIMAL(38, 4) ,
	                        Length DECIMAL(38, 2) ,
	                        JustificationRequired Boolean ,
	                        JustificationProvided Boolean ,
	                        DateConfSCI TIMESTAMP ,
	                        SCIOverwriten INTEGER ,
	                        Priority Boolean ,
	                        DatePropSCI TIMESTAMP ,
	                        DateSpa TIMESTAMP ,
	                        DateSac TIMESTAMP ,
	                        Latitude DECIMAL(38, 6) ,
	                        Longitude DECIMAL(38, 6) ,
	                        DateUpdate TIMESTAMP ,
	                        SpaLegalReference VARCHAR ,
	                        SacLegalReference VARCHAR ,
	                        Explanations VARCHAR ,
	                        MarineArea DECIMAL(38, 4) 
                        ) ";
                            var executeNonQuery = command.ExecuteNonQuery();


                        }
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CreateDuckDB Schema - ActiveSites", "", _dataContext.Database.GetConnectionString());
                        throw ex;

                    }

                    //create schema for ChangeData
                    try
                    {
                        using (var command = duckDBConnection.CreateCommand())
                        {
                            command.CommandText = @"CREATE TABLE IF NOT EXISTS Changes ( 
                        ChangeId HUGEINT ,
	                    SiteCode VARCHAR,
                        Version BIGINT ,
                        Country VARCHAR,
	                    Status VARCHAR,
                        Tags VARCHAR,
	                    Level VARCHAR,
                        ChangeCategory VARCHAR,
                        ChangeType VARCHAR,
                        NewValue VARCHAR,
                        OldValue VARCHAR,
                        Detail VARCHAR,
                        Code VARCHAR,
                        Section VARCHAR,
                        VersionReferenceId BIGINT,
                        FieldName VARCHAR,
                        ReferenceSiteCode VARCHAR,
                        N2KVersioningVersion BIGINT

                         )";
                            var executeNonQuery = command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CreateDuckDB Schema - Changes", "", _dataContext.Database.GetConnectionString());
                        throw ex;

                    }

                    //create schema for UserEditionActivities
                    try
                    {
                        using (var command = duckDBConnection.CreateCommand())
                        {
                            command.CommandText = @"CREATE TABLE IF NOT EXISTS UserEditionActivities (
                                ID HUGEINT,  
                                SiteCode VARCHAR, 
                                Version BIGINT,
                                Author NVARCHAR,
                                Date TIMESTAMP,
                                Action VARCHAR,
                                Deleted INTEGER
                        )";
                            var executeNonQuery = command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CreateDuckDB Schema - UserEditionActivities", "", _dataContext.Database.GetConnectionString());
                        throw ex;

                    }

                    //Create schema for LineageData
                    try
                    {
                        using (var command = duckDBConnection.CreateCommand())
                        {
                            command.CommandText = @"CREATE TABLE IF NOT EXISTS Lineage (
                                ID HUGEINT,
		                        SiteCode VARCHAR,
		                        Version BIGINT,
		                        N2KVersioningVersion INTEGER,
		                        Type INTEGER,
		                        Status INTEGER ,
		                        Release HUGEINT,
		                        Name VARCHAR,
		                        AntecessorsSiteCodes VARCHAR
                            )";
                            var executeNonQuery = command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CreateDuckDB Schema - Lineage", "", _dataContext.Database.GetConnectionString());
                        throw ex;
                    }



                }
            }
            catch (Exception ex)
            {

                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "CreateDuckDB Schema", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
        }

          
        public async Task<int> LoadActiveSites()
        {
            var sql = @"
select 
	sites.*
FROM 
	[dbo].[ProcessedEnvelopes] pe inner join
	(select  CountryCode, sitecode, n2kversioningversion, max(version)  as version
	from sites
	group by
		CountryCode, sitecode, n2kversioningversion
	) S on pe.Country=s.CountryCode and pe.Version= s.N2KVersioningVersion
	inner join Sites on s.SiteCode= sites.SiteCode and s.version= sites.Version

WHERE pe.[Status] = 3
";
            List<Sites> activeSites = await _dataContext.Set<Sites>().FromSqlRaw(sql).ToListAsync();
            try
            {
                using (var duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db"))
                {
                    duckDBConnection.Open();
                    using (var command = duckDBConnection.CreateCommand())
                    {
                        command.CommandText = @"truncate table ActiveSites";
                        command.ExecuteNonQuery();
                    }

                    using (var command = duckDBConnection.CreateCommand())
                    {
                        command.CommandText = @"copy ActiveSites from 'sites.parquet' ";
                        command.ExecuteNonQuery();
                    }


                    using (var command = duckDBConnection.CreateCommand())
                    {
                        command.CommandText = "select count(*) from ActiveSites";
                        var cc = await command.ExecuteScalarAsync();
                    }


                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB LoadActiveSites", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }
            return 1;
        }


        public async Task<int> LoadLineageData()
        {
            await Task.Delay(10);
            try
            {
                using (var duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db"))
                {
                    duckDBConnection.Open();
                    /*
                    var sql = @"    SELECT L.[ID],
		L.[SiteCode],
		L.[Version],
		L.[N2KVersioningVersion],
		[Type],
		L.[Status],
		[Release],
		S.[Name],
		STRING_AGG(subQuery.[SiteCode], ',') AntecessorsSiteCodes
	FROM [dbo].[Lineage] L
	INNER JOIN [dbo].[Sites] S ON L.[SiteCode] = S.[SiteCode]
		AND L.[Version] = S.[Version]
	INNER JOIN (
		SELECT DISTINCT [SiteCode],
			[dbo].[Changes].[Version],
			[N2KVersioningVersion]
		FROM [dbo].[Changes]
		INNER JOIN [dbo].[ProcessedEnvelopes] ON [dbo].[Changes].[Country] = [dbo].[ProcessedEnvelopes].[Country]
			AND [dbo].[Changes].[N2KVersioningVersion] = [dbo].[ProcessedEnvelopes].[Version]
		WHERE [dbo].[ProcessedEnvelopes].[Status] = 3
		) C ON L.[SiteCode] = C.[SiteCode]
		AND L.[N2KVersioningVersion] = C.[N2KVersioningVersion]
	INNER JOIN [dbo].[ProcessedEnvelopes] PE ON LEFT(L.[SiteCode], 2) = PE.[Country]
		AND L.[N2KVersioningVersion] = PE.[Version]

		left join 
		(

	SELECT DISTINCT LA.[SiteCode],
				LA.[Version],
				LA.[LineageID]

			FROM [dbo].[LineageAntecessors] LA
			
			GROUP BY LA.[SiteCode],
				LA.[Version],
				LA.[LineageID]

		) 	
		 subquery ON L.[ID] = subQuery.[LineageID] 


	WHERE 
		 PE.[Status] = 3
	GROUP BY L.[ID],
		L.[SiteCode],
		L.[Version],
		L.[N2KVersioningVersion],
		[Type],
		L.[Status],
		[Release],
		S.[Name]";
                    List<Lineage> lineage = await _dataContext.Set<Lineage>().FromSqlRaw(sql).ToListAsync();

 
                    using (var command = duckDBConnection.CreateCommand())
                    {
                        foreach (var line in lineage) {
                            command.CommandText =
                                string.Format(@"INSERT INTO Lineage (ID ,SiteCode,Version,N2KVersioningVersion,Type,Status,Release,Name,AntecessorsSiteCodes) VALUES (
                                    {0},
                                    '{1}',
                                    {2},
                                    {3},
                                    {4},
                                    {5},
                                    NULL,
                                    NULL,
                                    '{6}'
                                )", line.ID, line.SiteCode, line.Version, line.N2KVersioningVersion,(int) line.Type, (int) line.Status,line.AntecessorsSiteCodes );



                            var executeNonQuery = command.ExecuteNonQuery();
                        }
                    }
                    */

                    using (var command = duckDBConnection.CreateCommand())
                    {

                        command.CommandText = "truncate table Lineage";
                        command.ExecuteNonQuery();
                    }


                    using (var command = duckDBConnection.CreateCommand())
                    {

                        command.CommandText = "COPY Lineage from 'lineage.parquet' ";
                        await command.ExecuteNonQueryAsync();
                    }

                    using (var command = duckDBConnection.CreateCommand())
                    {
                        command.CommandText = "select count(*) from Lineage";
                        var cc = await command.ExecuteScalarAsync();
                    }


                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB LoadLineageData", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }

            return 1;


        }


        public async Task<int> LoadChanges()
        {
            /*
                        var sql = @"select 
                c.*
            FROM 
                [dbo].[ProcessedEnvelopes] pe inner join
                (select  CountryCode, sitecode, n2kversioningversion, max(version)  as version
                from sites
                group by
                    CountryCode, sitecode, n2kversioningversion
                ) S on pe.Country=s.CountryCode and pe.Version= s.N2KVersioningVersion
                inner join
                    changes C on 
                        S.SiteCode=C.SiteCode and s.version= c.Version
            WHERE pe.[Status] = 3";
            List<SiteChangeDb> lstChanges = await _dataContext.Set<SiteChangeDb>().FromSqlRaw(sql).ToListAsync();
            */
            try
            {
                using (var duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db"))
                {

                    duckDBConnection.Open();

                    using (var command = duckDBConnection.CreateCommand())
                    {
                        command.CommandText= "select count(*) from changes";
                        var cc = await command.ExecuteScalarAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB LoadChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }
            return 1;

        }


        public async Task<int> LoadSiteActivitiesUserEdition()
        {
            try
            {
                using (var duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db"))
                {
                    duckDBConnection.Open();
                    /*
                    string sql = @"
    SELECT DISTINCT
		SA.[ID], SA.[SiteCode], SA.[Version], SA.[Author], SA.[Date], SA.[Action], SA.[Deleted]
	FROM
		[dbo].[SiteActivities] SA
		INNER JOIN (SELECT
						[SiteCode], [Version], MAX([Date]) as MaxDate
					FROM
						[dbo].[SiteActivities]
					WHERE
						 [Action] LIKE 'User edition%'
						AND [Deleted] != 1
					GROUP BY
						[SiteCode], [Version]) SA2 ON SA.SiteCode = SA2.SiteCode AND SA.Version = SA2.Version AND SA.Date = SA2.MaxDate
	WHERE
	1=1 
		AND SA.[Action] LIKE 'User edition%'
		AND SA.[Deleted] != 1
";
                    */


                    //List<SiteActivities> activities = await _dataContext.Set<SiteActivities>().FromSqlRaw(sql).ToListAsync();
                    using (var command = duckDBConnection.CreateCommand())
                    {

                        command.CommandText = "truncate table UserEditionActivities";
                        command.ExecuteNonQuery();
                    }


                    using (var command = duckDBConnection.CreateCommand())
                    {

                        command.CommandText = "COPY UserEditionActivities from 'UserEdition.parquet' ";
                        await command.ExecuteNonQueryAsync();
                    }

                    using (var command = duckDBConnection.CreateCommand())
                    {
                        command.CommandText = "select count(*) from UserEditionActivities";
                        var cc = await command.ExecuteScalarAsync();
                    }


                        /*
                            foreach (var actv in activities)
                            {


                                command.CommandText = string.Format(@"INSERT INTO UserEditionActivities (ID,SiteCode,Version,Author,Date,Action,Deleted) VALUES (
    {0},'{1}',{2},'{3}',?, '{4}',{5} )", actv.ID, actv.SiteCode, actv.Version, actv.Author, actv.Action, actv.Deleted.Value ? 1:0
                                );
                                command.Parameters.Add(new  DuckDBParameter(actv.Date));

                                command.ExecuteNonQuery();

                            }
                        }
                        */


                    }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB UserEditionActivities", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }
            return 1;
        }



        public async  Task<List<SiteCodeVersion>> GetActiveSitesByCountryAndStatusAndLevel(string country, SiteChangeStatus? status, Enumerations.Level? level) 
        {

            List<SiteCodeVersion> result = new List<SiteCodeVersion>();
            try
            {
                //using (var duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db"))
                //{
                //    duckDBConnection.Open();

                    using (var command = _duckDBConnection.CreateCommand()) {
                        var sql = @"
        SELECT T.SiteCode, T.Version, T.NumCritical, T.NumWarning, T.NumInfo, S.Name, S.SiteType
    FROM (
	SELECT changes.SiteCode,
		changes.Version,
		sum(CASE 
				WHEN LEVEL = 'Critical'
					THEN 1
				ELSE 0
				END) AS NumCritical,
		sum(CASE 
				WHEN LEVEL = 'Warning'
					THEN 1
				ELSE 0
				END) AS NumWarning,
		sum(CASE 
				WHEN LEVEL = 'Info'
					THEN 1
				ELSE 0
				END) AS NumInfo

	FROM Changes
	WHERE changes.Country = ?
		AND changes.Status = ?
	GROUP BY SiteCode,
		Version,
		N2KVersioningVersion) T  JOIN
 ActiveSites S ON
    T.sitecode = S.sitecode and t.version = S.version";


                        command.CommandText = sql;
                        command.Parameters.Add(new DuckDBParameter(country));
                        command.Parameters.Add(new DuckDBParameter(status.ToString()));
                        try
                        {
                            var reader1 = await command.ExecuteReaderAsync();
                            while (reader1.Read())
                            {
                                var sitecode = reader1.GetString(0);
                                var version = reader1.GetInt32(1);
                                var numCritical = reader1.GetInt32(2);
                                var numWarning = reader1.GetInt32(3);
                                var numInfo = reader1.GetInt32(4);
                                var siteName = reader1.GetString(5);
                                var siteType = reader1.GetFieldValue<string>(6);

                                var _add = false;
                                switch (level)
                                {
                                    case Level.Critical:
                                        if (numCritical > 0) _add = true;
                                        break;
                                    case Level.Warning:
                                        if (numWarning > 0) _add = true;
                                        break;
                                    case Level.Info:
                                        if (numInfo > 0) _add = true;
                                        break;
                                    default:
                                        break;
                                }

                                if (_add)
                                {
                                    result.Add(
                                        new SiteCodeVersion
                                        {
                                            SiteCode = sitecode,
                                            Version = version,
                                            Name = siteName,
                                            Type = siteType
                                        }
                                    );
                                }

                            }
                        }
                        finally
                        {
                            await command.DisposeAsync();
                        }


                    //}
                    //await duckDBConnection.CloseAsync();

                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB GetActiveSites", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }
            return result;
        }

        public async Task<List<SiteActivities>> LoadSiteActivitiesUserEdition(string country)
        {

            List<SiteActivities> result= new List<SiteActivities>();
            try
            {
                //using (var duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db"))
                //{
                //    try
                //    {
                      //  duckDBConnection.Open();

                        using (var command = _duckDBConnection.CreateCommand())
                        {
                            var sql = "select * from UserEditionActivities where left(SiteCode,2) = ? ";
                            command.CommandText = sql;
                            command.Parameters.Add(new DuckDBParameter(country));
                            try
                            {
                                var reader1 = await command.ExecuteReaderAsync();
                                while (reader1.Read())
                                {
                                    result.Add(

                                        new SiteActivities
                                        {
                                            ID = reader1.GetInt64(0),
                                            SiteCode = reader1.GetString(1),
                                            Version = reader1.GetInt32(2),
                                            Author = reader1.GetString(3),
                                            Date= reader1.GetDateTime(4),
                                            Action = reader1.GetString(5),
                                            Deleted = Convert.ToBoolean(reader1.GetInt16(6)),
                                        }
                                    );
                                }
                            }
                            finally
                            {
                                await command.DisposeAsync();
                            }
                        }
                    //}
                    //finally { 
                    //    //await duckDBConnection.CloseAsync(); 
                    //}   
                    
                //}
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB LoadSiteActivitiesUserEdition", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }
            return result;


        }


        public async Task<List<Lineage>> LoadLineageChanges(string country)
        {

            List<Lineage> result= new List<Lineage >();
            try
            {
                using (var command = _duckDBConnection.CreateCommand())
                {
                    var sql = "select * from Lineage where left(SiteCode,2) = ? ";
                    command.CommandText = sql;
                    command.Parameters.Add(new DuckDBParameter(country));
                    var reader1 = await command.ExecuteReaderAsync();
                    while (reader1.Read())
                    {
                        result.Add(
                            new Lineage
                            {
                                 ID = reader1.GetInt64(0),
                                 SiteCode = reader1.GetString(1),
                                 Version = reader1.GetInt32(2),
                                 N2KVersioningVersion= reader1.GetInt32(3),
                                 Type= (LineageTypes) reader1.GetInt32(4),
                                 Status = (LineageStatus)  reader1.GetInt32(5),
                                 Release=!reader1.IsDBNull(6)? reader1.GetInt64(5) : null,
                                 Name= reader1.GetString(7),
                                 AntecessorsSiteCodes= !reader1.IsDBNull(8)?  reader1.GetString(7): null
                            }
                            );
                    }
                }

            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB LoadLineageChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }
            return result;

        }


        public async Task<List<SiteChangeDb>> LoadChanges(string country, SiteChangeStatus? status)
        {
            List<SiteChangeDb> result = new List<SiteChangeDb>();
            try
            {
                using (var command = _duckDBConnection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder(@"select * from Changes  where Country=?");
                    command.Parameters.Add(new DuckDBParameter(country));

                    if (status != null)
                    {
                        sql.Append(" and Status=?");
                        command.Parameters.Add(new DuckDBParameter(status.ToString()));
                    }
                    command.CommandText = sql.ToString();
                    var reader1 = await command.ExecuteReaderAsync();
                    while (reader1.Read())
                    {
                        SiteChangeStatus _status;
                        Enum.TryParse(reader1.GetString(4), out _status);
                        Level _level;
                        Enum.TryParse(reader1.GetString(6), out _level);

                        result.Add(
                            new SiteChangeDb
                            {
                                ChangeId= reader1.GetInt64(0),
                                SiteCode= reader1.GetString(1),
                                Version =  reader1.GetInt32(2),
                                Country = !reader1.IsDBNull(3) ? reader1.GetString(3): null,
                                Status = _status,
                                Tags = !reader1.IsDBNull(5)? reader1.GetString(5):null,
                                Level = _level,
                                ChangeCategory = !reader1.IsDBNull(7) ? reader1.GetString(7) : null,
                                ChangeType= !reader1.IsDBNull(8) ? reader1.GetString(8) : null,
                                NewValue= !reader1.IsDBNull(9) ? reader1.GetString(9) : null,
                                OldValue= !reader1.IsDBNull(10) ? reader1.GetString(10) : null,
                                Detail=!reader1.IsDBNull(11) ? reader1.GetString(11) : null,
                                Code= !reader1.IsDBNull(12) ? reader1.GetString(12) : null,
                                Section= !reader1.IsDBNull(13) ? reader1.GetString(13) : null,
                                VersionReferenceId = reader1.GetInt32(14),
                                FieldName= !reader1.IsDBNull(15) ? reader1.GetString(15) : null,
                                ReferenceSiteCode=!reader1.IsDBNull(16) ? reader1.GetString(16) : null,
                                N2KVersioningVersion = !reader1.IsDBNull(17) ? reader1.GetInt32(17): null,
                            }
                            );
                        /*
                                                 ChangeId HUGEINT ,
                                                SiteCode VARCHAR,
                                                Version BIGINT ,
                                                Country VARCHAR,
                                                Status VARCHAR,
                                                Tags VARCHAR,
                                                Level VARCHAR,
                                                ChangeCategory VARCHAR,
                                                ChangeType VARCHAR,
                                                NewValue VARCHAR,
                                                OldValue VARCHAR,
                                                Detail VARCHAR,
                                                Code VARCHAR,
                                                Section VARCHAR,
                                                VersionReferenceId BIGINT,
                                                FieldName VARCHAR,
                                                ReferenceSiteCode VARCHAR,
                                                N2KVersioningVersion BIGINT
                         */


                    }
                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB LoadChanges", "", _dataContext.Database.GetConnectionString());
                throw ex;

            }
            return result;
        }

        public async Task<bool>  SiteJustificationRequired(string sitecode, int version)
        {

            bool result = false;
            try
            {
                if (_duckDBConnection.State == System.Data.ConnectionState.Closed)
                {
                    _duckDBConnection = new DuckDB.NET.Data.DuckDBConnection("Data Source = file.db");
                    _duckDBConnection.Open();
                }

                using (var command = _duckDBConnection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder(@"select JustificationRequired from ActiveSites where SiteCode=? and Version=?");
                    command.Parameters.Add(new DuckDBParameter(sitecode));
                    command.Parameters.Add(new DuckDBParameter(version));
                    command.CommandText = sql.ToString();

                    var reader1 = await command.ExecuteReaderAsync();
                    while (reader1.Read())
                    {
                        if (!reader1.IsDBNull(0))
                        {
                            result= reader1.GetBoolean(0);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                await SystemLog.WriteAsync(SystemLog.errorLevel.Error, ex, "DuckDB SiteJustificationRequired", "", _dataContext.Database.GetConnectionString());
                throw ex;
            }
            return result;


        }


        public void Dispose()
        {
            if (_duckDBConnection.State != System.Data.ConnectionState.Closed)
                _duckDBConnection.Dispose();
        }


        public async Task  DisposeAsync()
        {
            if (_duckDBConnection.State != System.Data.ConnectionState.Closed)
                await _duckDBConnection.DisposeAsync();
        }


    }
}
