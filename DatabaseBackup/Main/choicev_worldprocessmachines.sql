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
-- Table structure for table `worldprocessmachines`
--

DROP TABLE IF EXISTS `worldprocessmachines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `worldprocessmachines` (
  `id` int NOT NULL AUTO_INCREMENT,
  `type` int NOT NULL,
  `processMachineId` int NOT NULL,
  `colShape` text NOT NULL,
  `loadingZone` text,
  PRIMARY KEY (`id`),
  KEY `fk_worldprocessmachines_processMachineId_idx` (`processMachineId`),
  CONSTRAINT `fk_worldprocessmachines_processMachineId` FOREIGN KEY (`processMachineId`) REFERENCES `configprocessmachines` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `worldprocessmachines`
--

LOCK TABLES `worldprocessmachines` WRITE;
/*!40000 ALTER TABLE `worldprocessmachines` DISABLE KEYS */;
INSERT INTO `worldprocessmachines` VALUES (2,1,8,'{\"X\":763.98083,\"Y\":225.19121,\"Z\":128.27258}#3.2406414#3.5113118#0.0#true#false#true',NULL),(4,1,7,'{\"X\":-512.012,\"Y\":-1737.7909,\"Z\":19.220459}#5.0#5.0#0.0#true#false#true','{\"X\":-517.39813,\"Y\":-1734.3063,\"Z\":19.085693}#28.105484#41.521286#56.441986#false#true#false'),(5,1,6,'{\"X\":-570.06177,\"Y\":-1612.2218,\"Z\":27.005127}#4.7293296#3.5113118#83.55847#true#false#true','{\"X\":-580.80707,\"Y\":-1605.9016,\"Z\":27.005127}#20.059711#12.228662#0.0#false#true#false');
/*!40000 ALTER TABLE `worldprocessmachines` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:11:29
