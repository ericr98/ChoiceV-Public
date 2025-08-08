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
-- Table structure for table `characterinjuries`
--

DROP TABLE IF EXISTS `characterinjuries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characterinjuries` (
  `id` int NOT NULL AUTO_INCREMENT,
  `charId` int NOT NULL,
  `bodyPart` int NOT NULL DEFAULT '0',
  `damage` float NOT NULL DEFAULT '0',
  `wastedPain` float NOT NULL DEFAULT '0',
  `damageType` int NOT NULL DEFAULT '0',
  `isHealed` int NOT NULL DEFAULT '0',
  `isMakeShiftTreated` int NOT NULL DEFAULT '0',
  `configInjury` int DEFAULT NULL,
  `seed` int NOT NULL,
  `createDate` datetime NOT NULL,
  `retreatPossible` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_charId` (`charId`),
  KEY `fk_operations_injuries_idx` (`configInjury`),
  CONSTRAINT `fk_characterinjuries_configinjurie` FOREIGN KEY (`configInjury`) REFERENCES `configinjuries` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_charId` FOREIGN KEY (`charId`) REFERENCES `characters` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=75520 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characterinjuries`
--

LOCK TABLES `characterinjuries` WRITE;
/*!40000 ALTER TABLE `characterinjuries` DISABLE KEYS */;
/*!40000 ALTER TABLE `characterinjuries` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:17:42
