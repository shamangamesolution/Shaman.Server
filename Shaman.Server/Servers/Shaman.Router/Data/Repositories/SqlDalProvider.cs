using Microsoft.Extensions.Options;
using Shaman.DAL.SQL;
using Shaman.Router.Config;

namespace Shaman.Router.Data.Repositories
{
    public interface IRouterSqlDalProvider
    {
        ISqlDal Get();
    }

    public class RouterSqlDalProvider : IRouterSqlDalProvider
    {
        private readonly IOptions<RouterConfiguration> _config;

        public RouterSqlDalProvider(IOptions<RouterConfiguration> config)
        {
            _config = config;
        }

        public ISqlDal Get()
        {
            return new AutoConnectSqlDal(new SqlDal(_config.Value.DbConfig));
        }
    }
}