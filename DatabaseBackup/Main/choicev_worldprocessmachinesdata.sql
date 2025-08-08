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
-- Table structure for table `worldprocessmachinesdata`
--

DROP TABLE IF EXISTS `worldprocessmachinesdata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `worldprocessmachinesdata` (
  `machineId` int NOT NULL,
  `key` varchar(45) NOT NULL,
  `type` int NOT NULL,
  `value` varchar(45) NOT NULL,
  PRIMARY KEY (`machineId`,`key`),
  CONSTRAINT `fk_worldprocessmachinesdata_machineId` FOREIGN KEY (`machineId`) REFERENCES `worldprocessmachines` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `worldprocessmachinesdata`
--

LOCK TABLES `worldprocessmachinesdata` WRITE;
/*!40000 ALTER TABLE `worldprocessmachinesdata` DISABLE KEYS */;
INSERT INTO `worldprocessmachinesdata` VALUES (2,'Drucken',0,'0'),(2,'Kalibrierung',0,'0'),(2,'Konfiguration',0,'1.358719523041108'),(2,'LesbareZeitung',0,'3'),(2,'MACHINEPOWERSTATUS',0,'0'),(2,'Papier',0,'0'),(2,'Platten',0,'4'),(2,'Tinte',0,'1'),(2,'UnlesbareZeitung',0,'0'),(2,'VERSION',0,'0'),(4,'Bio',0,'66'),(4,'Glass',0,'133'),(4,'GrossMuell',0,'0'),(4,'KleinMuell',0,'0'),(4,'MACHINEPOWERSTATUS',0,'0'),(4,'Metall',0,'94'),(4,'MittelMuell',0,'0'),(4,'Papier',0,'100'),(4,'Plastik',0,'134'),(4,'Rest',0,'130'),(4,'Sonder',0,'92'),(5,'Flakes',0,'26.500000000000107'),(5,'MACHINEPOWERSTATUS',0,'1'),(5,'Messer',0,'24.349999999999586'),(5,'Plastikmuell',0,'0'),(5,'Restmuell',0,'417.4999999999964'),(5,'RMV',0,'0.9'),(5,'Wasser',0,'0');
/*!40000 ALTER TABLE `worldprocessmachinesdata` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:13:02
