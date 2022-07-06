using System.Threading.Tasks;

namespace Shaman.Router.Data.Repositories;

public interface IRouterDbInitializer
{
    Task Initialize();
}

public class RouterDbInitializer : IRouterDbInitializer
{
    private readonly IRouterSqlDalProvider _routerSqlDalProvider;

    public RouterDbInitializer(IRouterSqlDalProvider routerSqlDalProvider)
    {
        _routerSqlDalProvider = routerSqlDalProvider;
    }

    public async Task Initialize()
    {
        var dal = _routerSqlDalProvider.Get();
        await dal.Execute(@"
CREATE TABLE IF NOT EXISTS `bundles` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL,
  `uri` varchar(128) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `servers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `address` varchar(256) NOT NULL DEFAULT '',
  `ports` varchar(64) NOT NULL DEFAULT '',
  `http_port` smallint(6) NOT NULL DEFAULT '0',
  `https_port` smallint(6) NOT NULL DEFAULT '0',
  `server_role` tinyint(4) NOT NULL,
  `name` varchar(45) NOT NULL DEFAULT '',
  `region` varchar(45) NOT NULL DEFAULT '',
  `client_version` varchar(45) NOT NULL DEFAULT '',
  `actualized_on` datetime DEFAULT NULL,
  `peers_count` int(11) NOT NULL DEFAULT '0',
  `is_approved` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


CREATE TABLE IF NOT EXISTS `states` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `server_id` int(11) NOT NULL DEFAULT '0',
  `state` varchar(256) NOT NULL DEFAULT '',
  `created_on` datetime DEFAULT NULL,
  `actualized_on` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
");
    }
}