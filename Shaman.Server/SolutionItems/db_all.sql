-- MySQL dump 10.13  Distrib 8.0.17, for macos10.14 (x86_64)
--
-- Host: 127.0.0.1    Database: db
-- ------------------------------------------------------
-- Server version	8.0.17

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `db`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `db` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `db`;

--
-- Table structure for table `player_wallet_item`
--

DROP TABLE IF EXISTS `player_wallet_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `player_wallet_item` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `player_id` int(11) NOT NULL,
  `currency_id` int(11) NOT NULL,
  `quantity` int(10) unsigned NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5022 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `player_wallet_item`
--

LOCK TABLES `player_wallet_item` WRITE;
/*!40000 ALTER TABLE `player_wallet_item` DISABLE KEYS */;
INSERT INTO `player_wallet_item` VALUES (4969,2850,1,5),(4970,2850,2,100),(4971,2850,3,70),(4972,2851,1,5),(4973,2851,2,100),(4974,2851,3,70),(4975,2852,1,5),(4976,2852,2,100),(4977,2852,3,85),(4978,2852,4,1),(4979,2853,1,5),(4980,2853,2,100),(4981,2853,3,90),(4982,2853,4,1),(4983,2854,1,5),(4984,2854,2,100),(4985,2854,3,71),(4986,2854,4,1),(4987,2855,1,5),(4988,2855,2,100),(4989,2855,3,31),(4990,2855,4,4),(4991,2856,1,5),(4992,2856,2,100),(4993,2856,3,70),(4994,2857,1,5),(4995,2857,2,100),(4996,2857,3,90),(4997,2857,4,1),(4998,2858,1,5),(4999,2858,2,100),(5000,2858,3,90),(5001,2858,4,1),(5002,2859,1,5),(5003,2859,2,100),(5004,2859,3,90),(5005,2859,4,1),(5006,2860,1,5),(5007,2860,2,100),(5008,2860,3,201),(5009,2860,4,3),(5010,2861,1,5),(5011,2861,2,100),(5012,2861,3,75700),(5013,2861,4,5),(5014,2862,1,5),(5015,2862,2,100),(5016,2862,3,90),(5017,2862,4,1),(5018,2863,1,5),(5019,2863,2,100),(5020,2863,3,89006),(5021,2863,4,2);
/*!40000 ALTER TABLE `player_wallet_item` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `players`
--

DROP TABLE IF EXISTS `players`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `players` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `guest_id` varchar(50) NOT NULL,
  `nickname` varchar(20) NOT NULL,
  `registration_date` datetime NOT NULL,
  `last_online` datetime NOT NULL,
  `blocked` tinyint(1) NOT NULL,
  `level` int(11) NOT NULL,
  `experience` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `googleId` (`guest_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2864 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `players`
--

LOCK TABLES `players` WRITE;
/*!40000 ALTER TABLE `players` DISABLE KEYS */;
INSERT INTO `players` VALUES (2850,'73d1e48c-9bee-45ef-8447-6b3cbbe5bc2a','Pilot_db889','2019-08-10 08:58:06','2019-08-10 09:11:51',0,1,0),(2851,'8c4a2cd0-9fbe-40c5-b215-019af0659eae','Pilot_f1c98','2019-08-10 09:14:00','2019-08-10 09:14:42',0,1,0),(2852,'9694ec59-dc0c-4320-95fd-6cdd9ae51fd8','Pilot_340c4','2019-08-10 09:15:18','2019-08-10 09:31:16',0,1,0),(2853,'b43db24e-bb5b-4538-80c7-9f38a6599b8a','Pilot_40ce2','2019-08-10 09:34:06','2019-08-10 09:37:20',0,1,2),(2854,'9414b6a1-c573-499f-88a9-c72f9131a20d','Pilot_35472','2019-08-10 09:46:12','2019-08-10 10:02:01',0,1,1),(2855,'80613777-655b-43f3-8585-64d4019cdf46','Pilot_589e0','2019-08-10 10:02:16','2019-08-10 10:41:31',0,2,5),(2856,'43f964c9-5f1d-4aa5-803d-253f33c2fcd9','Pilot_12eb0','2019-08-10 10:41:57','2019-08-10 10:43:08',0,1,0),(2857,'08eb9f32-b450-4de7-8ba3-f6a1b983180a','Pilot_4677b','2019-08-10 10:44:15','2019-08-10 10:48:00',0,1,1),(2858,'1387161e-7838-4920-ba5e-ab81c79e6f23','Pilot_f69c1','2019-08-10 10:55:15','2019-08-10 11:03:35',0,1,3),(2859,'b7733fda-1fdf-41b7-948f-08469d8eeed4','Pilot_04113','2019-08-10 11:04:35','2019-08-10 11:08:06',0,1,3),(2860,'b42542b6-95f7-45d8-896b-f6a11557e60c','Pilot_0951b','2019-08-11 15:41:44','2019-08-16 08:10:20',0,1,3),(2861,'9b9f8c76-6c57-45b3-9671-a55bcd2207fd','Pilot_24bbd','2019-08-30 09:35:44','2019-09-01 08:47:42',0,1,3),(2862,'0f69e117-d500-4f56-8556-82bdf7e10653','Pilot_df2e2','2019-09-01 08:49:50','2019-09-01 08:55:36',0,1,3),(2863,'7f229645-64fa-4621-b68f-1a474cf60d68','Pilot_2b252','2019-09-01 08:56:39','2019-09-03 19:33:33',0,1,3);
/*!40000 ALTER TABLE `players` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Current Database: `db_router`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `db_router` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `db_router`;

--
-- Table structure for table `backends`
--

DROP TABLE IF EXISTS `backends`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `backends` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `address` varchar(512) NOT NULL,
  `port` smallint(6) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `backends`
--

LOCK TABLES `backends` WRITE;
/*!40000 ALTER TABLE `backends` DISABLE KEYS */;
INSERT INTO `backends` VALUES (1,'http://127.0.0.1',7002);
/*!40000 ALTER TABLE `backends` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `matchmakers`
--

DROP TABLE IF EXISTS `matchmakers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `matchmakers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `game` tinyint(4) NOT NULL,
  `version` varchar(45) NOT NULL DEFAULT '<no>',
  `name` varchar(45) NOT NULL,
  `address` varchar(512) NOT NULL,
  `port` smallint(6) NOT NULL,
  `approved` tinyint(4) NOT NULL DEFAULT '0',
  `actualized_on` datetime NOT NULL,
  `backend_id` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `matchmakers`
--

LOCK TABLES `matchmakers` WRITE;
/*!40000 ALTER TABLE `matchmakers` DISABLE KEYS */;
/*!40000 ALTER TABLE `matchmakers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Current Database: `db_temp`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `db_temp` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `db_temp`;

--
-- Table structure for table `global_parameters`
--

DROP TABLE IF EXISTS `global_parameters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `global_parameters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `string_value` varchar(255) DEFAULT NULL,
  `int_value` int(11) DEFAULT NULL,
  `float_value` float DEFAULT NULL,
  `bool_value` tinyint(1) DEFAULT NULL,
  `datetime_value` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `global_parameters`
--

LOCK TABLES `global_parameters` WRITE;
/*!40000 ALTER TABLE `global_parameters` DISABLE KEYS */;
INSERT INTO `global_parameters` VALUES (1,'IsOnservice',NULL,NULL,NULL,0,NULL);
/*!40000 ALTER TABLE `global_parameters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `versions`
--

DROP TABLE IF EXISTS `versions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `versions` (
  `type` tinyint(4) NOT NULL,
  `major` smallint(5) unsigned NOT NULL,
  `minor` smallint(5) unsigned NOT NULL,
  `build` smallint(5) unsigned NOT NULL,
  PRIMARY KEY (`type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `versions`
--

LOCK TABLES `versions` WRITE;
/*!40000 ALTER TABLE `versions` DISABLE KEYS */;
INSERT INTO `versions` VALUES (1,0,0,181);
/*!40000 ALTER TABLE `versions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Current Database: `db_static`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `db_static` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `db_static`;

--
-- Table structure for table `currency`
--

DROP TABLE IF EXISTS `currency`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `currency` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `is_real_currency` tinyint(4) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `currency`
--

LOCK TABLES `currency` WRITE;
/*!40000 ALTER TABLE `currency` DISABLE KEYS */;
INSERT INTO `currency` VALUES (1,0),(2,0),(3,0),(4,0),(5,1);
/*!40000 ALTER TABLE `currency` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `global_parameters`
--

DROP TABLE IF EXISTS `global_parameters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `global_parameters` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `string_value` varchar(255) DEFAULT NULL,
  `int_value` int(11) DEFAULT NULL,
  `float_value` float DEFAULT NULL,
  `bool_value` tinyint(1) DEFAULT NULL,
  `datetime_value` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `global_parameters`
--

LOCK TABLES `global_parameters` WRITE;
/*!40000 ALTER TABLE `global_parameters` DISABLE KEYS */;
/*!40000 ALTER TABLE `global_parameters` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-09-05 17:51:58
