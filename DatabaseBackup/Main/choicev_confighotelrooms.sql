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
-- Table structure for table `confighotelrooms`
--

DROP TABLE IF EXISTS `confighotelrooms`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `confighotelrooms` (
  `id` int NOT NULL AUTO_INCREMENT,
  `hotelId` int NOT NULL,
  `level` int NOT NULL,
  `roomNumber` int NOT NULL,
  `collPosition` text NOT NULL,
  `collWidth` float NOT NULL,
  `collHeight` float NOT NULL,
  `collRotation` float NOT NULL,
  `maxBookings` int NOT NULL,
  `price` decimal(10,0) NOT NULL,
  `door1Id` int NOT NULL,
  `door2Id` int DEFAULT NULL,
  `door3Id` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_confighotelrooms_confighotels_hotelId_idx` (`hotelId`),
  CONSTRAINT `fk_confighotelrooms_confighotels_hotelId` FOREIGN KEY (`hotelId`) REFERENCES `confighotels` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `confighotelrooms`
--

LOCK TABLES `confighotelrooms` WRITE;
/*!40000 ALTER TABLE `confighotelrooms` DISABLE KEYS */;
INSERT INTO `confighotelrooms` VALUES (2,3,1,2,'{\"X\":63.052746,\"Y\":-1010.9846,\"Z\":47.337036}',8.4,9.3,69.2,99999,45,500,NULL,NULL),(4,3,0,1,'{\"X\":54.41099,\"Y\":-1007.55054,\"Z\":47.337036}',8.3,8.3,69,99999,45,501,NULL,NULL),(5,3,0,3,'{\"X\":70.821976,\"Y\":-1013.9846,\"Z\":47.337036}',8.2,8.4,69,99999,45,508,NULL,NULL),(6,3,0,4,'{\"X\":68.96264,\"Y\":-1025.1714,\"Z\":47.337036}',7.9,7.6,69,99999,45,509,NULL,NULL),(7,4,0,1,'{\"X\":336.79892,\"Y\":-813.38025,\"Z\":45.33191}',6.5,5.5,69.8,99999,21,524,NULL,NULL),(8,3,0,5,'{\"X\":58.342857,\"Y\":-1021.4857,\"Z\":47.370728}',9,8.5,69.4,99999,33,525,NULL,NULL),(9,3,0,6,'{\"X\":65.55275,\"Y\":-1033.6758,\"Z\":47.337036}',9.1,8.2,69.5,99999,45,526,NULL,NULL),(10,4,0,2,'{\"X\":331.18683,\"Y\":-806.88794,\"Z\":45.348755}',6.6,5.5,69.8,99999,27,529,NULL,NULL),(11,4,0,3,'{\"X\":340.54288,\"Y\":-803.5583,\"Z\":45.33191}',-6.4,5.9,69.9,99999,21,530,NULL,NULL),(12,4,0,4,'{\"X\":333.27582,\"Y\":-800.8571,\"Z\":45.3656}',6.2,5.5,69.4,99999,27,531,NULL,NULL),(13,4,0,5,'{\"X\":342.5352,\"Y\":-797.44836,\"Z\":45.3656}',6.2,5.3,69.5,99999,21,532,NULL,NULL),(14,4,0,6,'{\"X\":335.5824,\"Y\":-794.8385,\"Z\":45.33191}',6.6,5.5,70.1,99999,21,533,NULL,NULL),(15,6,0,2,'{\"X\":-152.39342,\"Y\":6294.6846,\"Z\":35.255737}',7.6,6.4,44.9,99999,35,538,NULL,NULL),(16,6,0,1,'{\"X\":-145.16704,\"Y\":6301.9277,\"Z\":35.221924}',7.3,6.3,45,9999,35,539,NULL,NULL),(17,6,0,3,'{\"X\":-140.6055,\"Y\":6297.167,\"Z\":35.255737}',7.1,6.4,45.9,9999,35,540,NULL,NULL),(18,6,0,5,'{\"X\":-135.90878,\"Y\":6292.546,\"Z\":35.255737}',7.6,6.5,45,99999,35,541,NULL,NULL),(19,6,0,4,'{\"X\":-147.89781,\"Y\":6289.933,\"Z\":35.255737}',7.5,6.3,44.9,9999,35,542,NULL,NULL),(20,6,0,6,'{\"X\":-143.17912,\"Y\":6285.3496,\"Z\":35.255737}',7.4,6.5,44.5,99999,35,543,NULL,NULL);
/*!40000 ALTER TABLE `confighotelrooms` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:36
