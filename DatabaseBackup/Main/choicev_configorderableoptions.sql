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
-- Table structure for table `configorderableoptions`
--

DROP TABLE IF EXISTS `configorderableoptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configorderableoptions` (
  `setId` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `extraPrice` decimal(10,2) NOT NULL,
  `data` text NOT NULL,
  PRIMARY KEY (`setId`,`name`),
  CONSTRAINT `fk_configorderableoptions_setId` FOREIGN KEY (`setId`) REFERENCES `configorderableoptionsets` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configorderableoptions`
--

LOCK TABLES `configorderableoptions` WRITE;
/*!40000 ALTER TABLE `configorderableoptions` DISABLE KEYS */;
INSERT INTO `configorderableoptions` VALUES (1,'Blau',0.00,'62'),(1,'Grün',0.00,'50'),(1,'Rot',0.00,'27'),(1,'Schwarz',0.00,'0'),(1,'Silber',0.00,'3'),(1,'Weiß',0.00,'111'),(2,'Boote',0.00,'14'),(2,'Compacts',0.00,'0'),(2,'Coupes',0.00,'3'),(2,'Einsatz',0.00,'18'),(2,'Fahrräder',0.00,'13'),(2,'Flugzeuge',0.00,'16'),(2,'Helikopter',0.00,'15'),(2,'Industrie',0.00,'10'),(2,'Kommerziell',0.00,'20'),(2,'Militär',0.00,'19'),(2,'Motorräder',0.00,'8'),(2,'Muscle',0.00,'4'),(2,'Nutz',0.00,'11'),(2,'Off-road',0.00,'9'),(2,'Open Wheels',0.00,'22'),(2,'Sedans',0.00,'1'),(2,'Service',0.00,'17'),(2,'Sports',0.00,'6'),(2,'Sports Classics',0.00,'5'),(2,'Super',0.00,'7'),(2,'SUVs',0.00,'2'),(2,'Vans',0.00,'12'),(2,'Züge',0.00,'21'),(3,'Brushed',0.00,'2'),(3,'Matt',0.00,'1'),(3,'Metallic',0.00,'0'),(3,'Spezial',0.00,'3'),(3,'Util',0.00,'4'),(3,'Worn',0.00,'5');
/*!40000 ALTER TABLE `configorderableoptions` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:17:32
