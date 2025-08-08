-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: game.choicev.net    Database: choicev
-- ------------------------------------------------------
-- Server version	8.0.25

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
-- Table structure for table `configgasstations`
--

DROP TABLE IF EXISTS `configgasstations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configgasstations` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `type` int NOT NULL,
  `bankAccount` bigint DEFAULT NULL,
  `priceKerosin` float NOT NULL,
  `priceDiesel` float NOT NULL,
  `pricePetrol` float NOT NULL,
  `priceElecricity` float NOT NULL,
  `remainPosition` text NOT NULL,
  `width` float NOT NULL,
  `height` float NOT NULL,
  `rotation` float NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_bankaccounts_configgasstations_idx` (`bankAccount`),
  CONSTRAINT `fk_bankaccounts_configgasstations` FOREIGN KEY (`bankAccount`) REFERENCES `bankaccounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configgasstations`
--

LOCK TABLES `configgasstations` WRITE;
/*!40000 ALTER TABLE `configgasstations` DISABLE KEYS */;
INSERT INTO `configgasstations` VALUES (7,'Xero Gas Strawberry',4,655232541,3.75,1.5,1.75,0.5,'{\"X\":288.56705,\"Y\":-1267.0879,\"Z\":28.431519}',2.9,4.2,0),(8,'Globe Oil Senora Freeway',3,655639786,3.5,1.25,1.5,0.5,'{\"X\":1706.2946,\"Y\":6424.8086,\"Z\":31.632935}',2,4,65),(9,'Ron Paleto Bay',2,655548435,3.5,1.25,1.5,0.5,'{\"X\":161.38791,\"Y\":6635.7505,\"Z\":30.588257}',3.1,4.9,45.9),(10,'Xero Gas Paleto',4,655828330,3.5,1.25,1.5,0.5,'{\"X\":-93.315384,\"Y\":6410.0044,\"Z\":30.470337}',4.7,2.4,47),(11,'Xero Gas Youtool',4,655521015,3.5,1.25,1.5,0.5,'{\"X\":2680.2395,\"Y\":3275.6714,\"Z\":54.228516}',5,3,61.3),(12,'Ltd Grapeseed',1,655747468,3.5,1.25,1.5,0.5,'{\"X\":1702.5077,\"Y\":4936.489,\"Z\":41.068848}',4,2,55),(13,'Ron Lago Zancudo',2,655326525,3.5,1.25,1.5,0.5,'{\"X\":-2544.29,\"Y\":2317.0847,\"Z\":32.20581}',5,2.7,4.2),(14,'Xero Station Grand-Senora-Wüste',4,655694679,3.5,1.25,1.5,0.5,'{\"X\":46.536263,\"Y\":2787.7683,\"Z\":56.874023}',2,3,60),(15,'Globe Oil Harmony',3,655242959,3.5,1.25,1.5,0.5,'{\"X\":265.97363,\"Y\":2599.6165,\"Z\":43.832275}',5,3,11.9),(16,'Gobal Oil Grand-Senora-Wüste',3,655310608,3.5,1.25,1.5,0.5,'{\"X\":1039.3187,\"Y\":2665.599,\"Z\":38.54138}',2,3.3,0),(17,'Xero Gas Pacific Bluffs',4,655293865,3.75,1.5,1.75,0.5,'{\"X\":-2073.9648,\"Y\":-327.15604,\"Z\":12.306274}',3.8,2.4,83.7),(18,'Davis Quarz',3,655904520,3.5,1.25,1.5,0.5,'{\"X\":2546.3318,\"Y\":2587.5713,\"Z\":36.940674}',1.5,2.8,21.2),(19,'Xero Gas Sandy Shores',4,655969602,3.5,1.25,1.5,0.5,'{\"X\":2002.3715,\"Y\":3779.9177,\"Z\":31.177979}',2.3,1.5,27.6),(20,'Globle Oil Grand-Senora-Wüste',3,655825010,3.5,1.25,1.5,0.5,'{\"X\":1777.4615,\"Y\":3327.2043,\"Z\":40.243164}',2,3,30),(21,'Route 68 Store',3,655504858,3.5,1.25,1.5,0.5,'{\"X\":1201.845,\"Y\":2655.4473,\"Z\":36.8396}',2,3.6,46.6),(22,'Ron Morningwood',2,655287475,3.75,1.5,1.75,0.5,'{\"X\":-1428.8308,\"Y\":-268.97583,\"Z\":45.19702}',2.3,3.8,40.4),(23,'Ltd Richman',1,655229516,3.75,1.5,1.75,0.5,'{\"X\":-1820.5494,\"Y\":799.24835,\"Z\":137.16333}',2.1,3,42.1),(25,'Globle Oil Vinewood Mitte',3,655741857,3.75,1.5,1.75,0.5,'{\"X\":646.2945,\"Y\":268.85275,\"Z\":102.11572}',6,2,59),(26,'LTD Mirror Park',1,655305297,3.75,1.5,1.75,0.5,'{\"X\":1164.0791,\"Y\":-327.58243,\"Z\":68.01172}',4,3,12),(27,'Ltd Vespucci',1,655836447,3.75,1.5,1.75,0.5,'{\"X\":-707.95715,\"Y\":-917.66595,\"Z\":18.203613}',4.1,1.8,0),(28,'Xero Gas Little Seoul',4,655285871,3.5,1.5,1.75,0.5,'{\"X\":-531.1824,\"Y\":-1220.5846,\"Z\":17.445435}',2.1,4.7,65.4),(29,'Globe Oil La Puerta',3,655357238,3.75,1.5,1.75,0.5,'{\"X\":-341.62415,\"Y\":-1484.0615,\"Z\":29.594116}',2.4,4.5,0),(30,'Ltd Davis',1,655894184,3.75,1.5,1.75,0.5,'{\"X\":-50.983517,\"Y\":-1759.9418,\"Z\":28.431519}',2.2,3.8,50.1),(31,'Ron El Burro Heights',2,655439005,3.75,1.5,1.75,0.5,'{\"X\":1211.0374,\"Y\":-1389.7131,\"Z\":34.210938}',4.6,2.3,0),(32,'Ron Davis Southside',2,655476750,3.75,1.5,1.75,0.5,'{\"X\":167.62198,\"Y\":-1553.7406,\"Z\":28.212402}',4,2,46),(33,'Ron La Mesa',2,655628474,3.75,1.5,1.75,0.5,'{\"X\":817.9121,\"Y\":-1040.1989,\"Z\":25.499634}',5,2.2,0),(34,'Ron Tataviam Bergkette',2,655951587,3.5,1.25,1.5,0.5,'{\"X\":2560.3386,\"Y\":373.73627,\"Z\":107.608765}',5,3,87),(35,'Ron Pier 400',2,655461215,4,1.75,1.6,0.25,'{\"X\":-63.593407,\"Y\":-2545.232,\"Z\":4.201416}',2.9,4.1,46.2),(36,'Elektrosäulen',5,655942211,0,0,0,0,'{\"X\":0,\"Y\":0,\"Z\":0}',0,0,0);
/*!40000 ALTER TABLE `configgasstations` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:11:36
