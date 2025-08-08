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
-- Table structure for table `configblips`
--

DROP TABLE IF EXISTS `configblips`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configblips` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `sprite` int NOT NULL,
  `colorId` int NOT NULL,
  `position` text NOT NULL,
  `island` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=76 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configblips`
--

LOCK TABLES `configblips` WRITE;
/*!40000 ALTER TABLE `configblips` DISABLE KEYS */;
INSERT INTO `configblips` VALUES (25,'Wisdahl Hotel',476,4,'{\"X\":56.228573,\"Y\":-1010.2154,\"Z\":29.330444}',0),(26,'Saint Roper MD',61,1,'{\"X\":-341.88132,\"Y\":-256.8264,\"Z\":46.146484}',0),(29,'Los Santos Police Department',60,63,'{\"X\":833.84174,\"Y\":-1289.6044,\"Z\":28.235107}',0),(30,'Gerichtsgebäude',408,4,'{\"X\":246.36923,\"Y\":-1086.3561,\"Z\":44.882812}',0),(32,'LS Logistics',477,4,'{\"X\":1205.1824,\"Y\":-3255.3757,\"Z\":7.088623}',0),(33,'Los Santos Fire Department',436,1,'{\"X\":1191.178,\"Y\":-1462.6285,\"Z\":34.89087}',0),(37,'Bonds Hotel',476,4,'{\"X\":330.6989,\"Y\":-807.74506,\"Z\":29.414673}',0),(38,'The Chicken Coop Hotel',476,4,'{\"X\":-145.74066,\"Y\":6293.552,\"Z\":41.091553}',0),(39,'Los Santos Sheriff Department',60,56,'{\"X\":-445.16043,\"Y\":6004.4175,\"Z\":41.31067}',0),(47,'City Hall',419,4,'{\"X\":-1312.4572,\"Y\":-555.9297,\"Z\":34.098877}',0),(50,'YouTool',544,4,'{\"X\":2740.246,\"Y\":3476.7166,\"Z\":55.666626}',0),(57,'Outdoor Shop',52,4,'{\"X\":-774.7253,\"Y\":5603.222,\"Z\":33.744995}',0),(62,'Parkplatz',267,29,'{\"X\":343.25275,\"Y\":-1688.6505,\"Z\":32.515015}',0),(63,'Parkplatz',267,29,'{\"X\":25.318682,\"Y\":-1099.9253,\"Z\":38.142822}',0),(64,'Parkplatz',267,29,'{\"X\":886.32526,\"Y\":-0.34285736,\"Z\":78.75098}',0),(65,'Parkplatz',267,29,'{\"X\":361.87253,\"Y\":298.24615,\"Z\":103.87402}',0),(66,'Parkplatz',267,29,'{\"X\":-1184.888,\"Y\":-1510.0747,\"Z\":4.6453857}',0),(67,'Parkplatz',267,29,'{\"X\":1525.0945,\"Y\":3773.0637,\"Z\":34.503296}',0),(68,'Parkplatz',267,29,'{\"X\":-331.05493,\"Y\":-780.6989,\"Z\":33.947266}',0),(69,'Parkplatz',267,29,'{\"X\":-997.9912,\"Y\":-2609.1033,\"Z\":14.11499}',0),(70,'Parkplatz',267,29,'{\"X\":-308.98022,\"Y\":-58.167034,\"Z\":54.4198}',0),(71,'Parkplatz',267,29,'{\"X\":-1667.9604,\"Y\":43.173626,\"Z\":62.91211}',0),(72,'Parkplatz',267,29,'{\"X\":1707.8506,\"Y\":4791.89,\"Z\":41.967773}',0),(73,'Parkplatz',267,29,'{\"X\":83.85495,\"Y\":6420.567,\"Z\":31.756836}',0),(74,'Parkplatz',267,29,'{\"X\":797.23517,\"Y\":-2974.7869,\"Z\":6.010254}',0),(75,'Fahrradhändler',494,4,'{\"X\":-1008.55383,\"Y\":-2690.3208,\"Z\":13.980225}',0);
/*!40000 ALTER TABLE `configblips` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:09
