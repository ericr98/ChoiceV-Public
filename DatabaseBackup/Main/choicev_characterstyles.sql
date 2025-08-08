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
-- Table structure for table `characterstyles`
--

DROP TABLE IF EXISTS `characterstyles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characterstyles` (
  `charId` int NOT NULL,
  `gender` char(1) NOT NULL DEFAULT 'M',
  `faceFather` float NOT NULL DEFAULT '0',
  `faceMother` float NOT NULL DEFAULT '0',
  `faceEyes` int NOT NULL DEFAULT '0',
  `faceFeature_0` float NOT NULL DEFAULT '0',
  `faceFeature_1` float NOT NULL DEFAULT '0',
  `faceFeature_10` float NOT NULL DEFAULT '0',
  `faceFeature_11` float NOT NULL DEFAULT '0',
  `faceFeature_12` float NOT NULL DEFAULT '0',
  `faceFeature_13` float NOT NULL DEFAULT '0',
  `faceFeature_14` float NOT NULL DEFAULT '0',
  `faceFeature_15` float NOT NULL DEFAULT '0',
  `faceFeature_16` float NOT NULL DEFAULT '0',
  `faceFeature_17` float NOT NULL DEFAULT '0',
  `faceFeature_18` float NOT NULL DEFAULT '0',
  `faceFeature_2` float NOT NULL DEFAULT '0',
  `faceFeature_3` float NOT NULL DEFAULT '0',
  `faceFeature_4` float NOT NULL DEFAULT '0',
  `faceFeature_5` float NOT NULL DEFAULT '0',
  `faceFeature_6` float NOT NULL DEFAULT '0',
  `faceFeature_7` float NOT NULL DEFAULT '0',
  `faceFeature_8` float NOT NULL DEFAULT '0',
  `faceFeature_9` float NOT NULL DEFAULT '0',
  `faceShape` float NOT NULL DEFAULT '0',
  `faceSkin` float NOT NULL DEFAULT '0',
  `hairColor` int NOT NULL DEFAULT '0',
  `hairHighlight` int NOT NULL DEFAULT '0',
  `hairStyle` int NOT NULL DEFAULT '0',
  `overlay_0` int NOT NULL DEFAULT '0',
  `overlay_1` int NOT NULL DEFAULT '0',
  `overlay_10` int NOT NULL DEFAULT '255',
  `overlay_11` int NOT NULL DEFAULT '0',
  `overlay_12` int NOT NULL DEFAULT '0',
  `overlay_2` int NOT NULL DEFAULT '0',
  `overlay_3` int NOT NULL DEFAULT '0',
  `overlay_4` int NOT NULL DEFAULT '0',
  `overlay_5` int NOT NULL DEFAULT '0',
  `overlay_6` int NOT NULL DEFAULT '0',
  `overlay_7` int NOT NULL DEFAULT '0',
  `overlay_8` int NOT NULL DEFAULT '0',
  `overlay_9` int NOT NULL DEFAULT '0',
  `overlaycolor_1` int NOT NULL DEFAULT '0',
  `overlaycolor_10` int NOT NULL DEFAULT '0',
  `overlaycolor_2` int NOT NULL DEFAULT '0',
  `overlaycolor_5` int NOT NULL DEFAULT '0',
  `overlaycolor_8` int NOT NULL DEFAULT '0',
  `overlaycolor_4` int NOT NULL DEFAULT '0',
  `overlayhighlight_1` int NOT NULL DEFAULT '0',
  `overlayhighlight_2` int NOT NULL DEFAULT '0',
  `overlayhighlight_5` int NOT NULL DEFAULT '0',
  `overlayhighlight_8` int NOT NULL DEFAULT '0',
  `overlayhighlight_4` int NOT NULL DEFAULT '0',
  `overlay_8_opacity` float NOT NULL DEFAULT '1',
  `overlay_4_opacity` float NOT NULL DEFAULT '1',
  PRIMARY KEY (`charId`),
  KEY `charId` (`charId`),
  CONSTRAINT `character_id_fk` FOREIGN KEY (`charId`) REFERENCES `characters` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characterstyles`
--

LOCK TABLES `characterstyles` WRITE;
/*!40000 ALTER TABLE `characterstyles` DISABLE KEYS */;
/*!40000 ALTER TABLE `characterstyles` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:16:28
