using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Config;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Shaman.Common.Utils.Logging;
using Shaman.DAL.Exceptions;
using Shaman.DAL.Repositories;
using Shaman.Messages.General.Entity;

namespace Sample.BackEnd.Data.Repositories
{
    public class TempRepository : RepositoryBase, ITempRepository
    {
        public TempRepository(IOptions<BackendConfiguration> config, IShamanLogger logger)
        {
            Initialize(config.Value.DbServerTemp, config.Value.DbNameTemp, config.Value.DbUserTemp, config.Value.DbPasswordTemp, config.Value.DbMaxPoolSize,
                logger);
        }
        
        private List<CustomVersion> GetVersionsFromDataTable(DataTable dt)
        {
            var result = new List<CustomVersion>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(new CustomVersion
                {
                    //Type = (VersionType)GetByte(dt.Rows[i]["type"]),
                    Build = (ushort)GetShort(dt.Rows[i]["build"]),
                    Minor = (ushort)GetShort(dt.Rows[i]["minor"]),
                    Major = (ushort)GetShort(dt.Rows[i]["major"])
                });
            }

            return result;
        }
        
        public async Task<CustomVersion> GetVersion(VersionType type)
        {
            try
            {
                var sql = $@"SELECT `versions`.`type`,
                                `versions`.`major`,
                                `versions`.`minor`,
                                `versions`.`build`
                            FROM `{DbName}`.`versions`
                            WHERE `versions`.`type` = {Value((byte)type)}";

                var version = GetVersionsFromDataTable(await dal.Select(sql)).FirstOrDefault();
                if (version == null)
                    throw new DalException(DalExceptionCode.VersionWasNotFound, $"Version {type} was not found");

                return version;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(TempRepository)}.{nameof(this.GetVersion)}", ex.ToString());                                                                                
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(TempRepository)}.{nameof(this.GetVersion)}", ex.ToString());                                                                                
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }
        
        public async Task<CustomVersion> IncrementVersion(VersionType type, VersionComponent component)
        {
            try
            {
                var version = await GetVersion(type);
                version.Increment(component);
                var sql = $@"UPDATE `{DbName}`.`versions`
                                SET
                                `major` = {Value(version.Major)},
                                `minor` = {Value(version.Minor)},
                                `build` = {Value(version.Build)}
                                WHERE `type` = {Value((byte)type)}";

                dal.Update(sql);

                return version;
            }
            catch (DalException ex)
            {
                LogError($"{typeof(TempRepository)}.{nameof(this.IncrementVersion)}", ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                LogError($"{typeof(TempRepository)}.{nameof(this.IncrementVersion)}", ex.ToString());
                throw new DalException(DalExceptionCode.GeneralException, "DAL Exception", ex);
            }
        }

    }
}