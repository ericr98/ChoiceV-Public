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
-- Table structure for table `crimetraderrequests`
--

DROP TABLE IF EXISTS `crimetraderrequests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `crimetraderrequests` (
  `id` int NOT NULL AUTO_INCREMENT,
  `configCrimeTraderItemId` int NOT NULL,
  `requesterCharId` int NOT NULL,
  `lockerDrawerId` int DEFAULT NULL,
  `informPhoneNumber` bigint DEFAULT NULL,
  `amount` int NOT NULL,
  `buyNotSell` tinyint(1) NOT NULL,
  `arriveDate` datetime NOT NULL,
  `stopDate` datetime NOT NULL,
  `isFullfilled` tinyint(1) NOT NULL,
  `isFullfilledDate` datetime DEFAULT NULL,
  `canBeCollected` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_configcrimetraderrequests_configCrimeTraderItemId_idx` (`configCrimeTraderItemId`),
  KEY `fk_configcrimetraderrequests_lockerDrawerId_idx` (`lockerDrawerId`),
  KEY `fk_configcrimetraderrequests_requesterCharId_idx` (`requesterCharId`),
  CONSTRAINT `fk_configcrimetraderrequests_configCrimeTraderItemId` FOREIGN KEY (`configCrimeTraderItemId`) REFERENCES `configcrimetradershopitems` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_configcrimetraderrequests_lockerDrawerId` FOREIGN KEY (`lockerDrawerId`) REFERENCES `configlockerdrawer` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_configcrimetraderrequests_requesterCharId` FOREIGN KEY (`requesterCharId`) REFERENCES `characters` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=974 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `crimetraderrequests`
--

LOCK TABLES `crimetraderrequests` WRITE;
/*!40000 ALTER TABLE `crimetraderrequests` DISABLE KEYS */;
/*!40000 ALTER TABLE `crimetraderrequests` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:14:11
