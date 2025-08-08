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
-- Table structure for table `configdoorsadditonaldimensions`
--

DROP TABLE IF EXISTS `configdoorsadditonaldimensions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configdoorsadditonaldimensions` (
  `doorId` int NOT NULL,
  `dimension` int NOT NULL,
  `locked` int NOT NULL DEFAULT '1',
  `lockIndex` int NOT NULL,
  PRIMARY KEY (`doorId`,`dimension`),
  CONSTRAINT `fk_configdoorsadditonaldimensions_configdoors_doorId` FOREIGN KEY (`doorId`) REFERENCES `configdoors` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configdoorsadditonaldimensions`
--

LOCK TABLES `configdoorsadditonaldimensions` WRITE;
/*!40000 ALTER TABLE `configdoorsadditonaldimensions` DISABLE KEYS */;
INSERT INTO `configdoorsadditonaldimensions` VALUES (499,-3,0,0),(499,-2,0,0),(499,1,1,0),(500,-6,1,0),(500,-5,1,0),(500,-4,1,0),(500,-3,1,18),(500,-2,1,9),(500,-1,1,43),(501,-7,1,0),(501,-6,1,0),(501,-5,1,0),(501,-4,1,0),(501,-3,1,0),(501,-2,1,39),(501,-1,1,40),(501,0,1,1),(508,-6,1,1),(508,-5,1,0),(508,-4,1,0),(508,-3,0,0),(508,-2,1,5),(508,-1,1,23),(509,-6,1,3),(509,-5,1,0),(509,-4,1,0),(509,-3,1,0),(509,-2,1,4),(509,-1,1,12),(524,-3,1,3),(524,-2,1,4),(524,-1,0,27),(525,-18,1,0),(525,-17,1,0),(525,-16,1,0),(525,-15,1,0),(525,-14,1,0),(525,-13,1,0),(525,-12,1,2),(525,-11,1,3),(525,-10,1,2),(525,-9,1,0),(525,-8,1,0),(525,-7,1,2),(525,-6,0,1),(525,-5,1,0),(525,-4,0,1),(525,-3,1,0),(525,-2,1,1),(525,-1,1,10),(526,-6,1,0),(526,-5,1,2),(526,-4,1,0),(526,-3,1,0),(526,-2,1,0),(526,-1,1,5),(529,-7,1,0),(529,-6,1,2),(529,-5,1,3),(529,-4,0,0),(529,-3,1,0),(529,-2,1,9),(529,-1,1,18),(530,-2,1,1),(530,-1,0,4),(531,-6,1,0),(531,-5,1,6),(531,-4,1,1),(531,-3,1,0),(531,-2,1,0),(531,-1,1,69),(532,-2,1,1),(532,-1,1,1),(533,-2,1,1),(533,-1,1,10),(538,-3,1,0),(538,-2,1,1),(538,-1,1,11),(539,-3,1,4),(539,-2,1,4),(539,-1,1,22),(540,-2,1,0),(540,-1,1,6),(541,-2,1,0),(541,-1,1,4),(542,-2,1,0),(542,-1,1,6),(543,-2,1,0),(543,-1,1,1);
/*!40000 ALTER TABLE `configdoorsadditonaldimensions` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:07
