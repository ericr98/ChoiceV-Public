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
-- Table structure for table `configzonegroupings`
--

DROP TABLE IF EXISTS `configzonegroupings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configzonegroupings` (
  `groupingIdentifier` varchar(45) NOT NULL,
  `groupingName` varchar(45) NOT NULL,
  PRIMARY KEY (`groupingIdentifier`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configzonegroupings`
--

LOCK TABLES `configzonegroupings` WRITE;
/*!40000 ALTER TABLE `configzonegroupings` DISABLE KEYS */;
INSERT INTO `configzonegroupings` VALUES ('ALAMO_LAKE','Alamosee'),('BANHAM_MOUNTAINS','Banham Bergkette'),('CAYO_PERICO','Cayo Perico'),('CHUMASH','Chumash'),('DOWNTOWN','Downtown'),('EASTSIDE','Eastside'),('GRAPESEED','Grapeseed'),('GREAT_CHAPARRAL','Great Chaprarral'),('HARBOR','Hafen'),('LSIA','Flughafen'),('MIRROR_PARK','Mirror Park'),('MOUNT_CHILIAD','Mount Chiliad National Park'),('MOUNT_GORDO','Mount Gordo'),('MOUNT_JOSIA','Mount Josia'),('NORTH_YANKTON','North Yankton'),('OCEAN','Pazifik'),('PALETO_BAY','Paleto Bay'),('PALOMINO_HIGHLANDS','Palimo Highlands'),('RATON_CANYON','Raton Canyon'),('ROCKFORD','Rockford'),('SAN_CHIANSKI','San Chianski Bergkette'),('SANDY_SHORES','Sandy Shores'),('SEOUL','Little Seoul'),('SOUTHSIDE','Southside'),('TATAVIAM_MOUNTAINS','Tataviam Bergkette'),('VINEWOOD','Vinewood'),('VINEWOOD_HILLS','Vinewood Hils'),('WESTSIDE','Westside'),('ZANCUDO','Zancudo');
/*!40000 ALTER TABLE `configzonegroupings` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:17:28
