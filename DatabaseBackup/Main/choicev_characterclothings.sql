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
-- Table structure for table `characterclothings`
--

DROP TABLE IF EXISTS `characterclothings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `characterclothings` (
  `characterid` int NOT NULL,
  `mask_drawable` int NOT NULL,
  `mask_texture` int NOT NULL,
  `maskDlc` varchar(100) DEFAULT NULL,
  `torso_drawable` int NOT NULL,
  `torso_texture` int NOT NULL,
  `torsoDlc` varchar(100) DEFAULT NULL,
  `top_drawable` int NOT NULL,
  `top_texture` int NOT NULL,
  `topDlc` varchar(100) DEFAULT NULL,
  `shirt_drawable` int NOT NULL,
  `shirt_texture` int NOT NULL,
  `shirtDlc` varchar(100) DEFAULT NULL,
  `legs_drawable` int NOT NULL,
  `legs_texture` int NOT NULL,
  `legsDlc` varchar(100) DEFAULT NULL,
  `feet_drawable` int NOT NULL,
  `feet_texture` int NOT NULL,
  `feetDlc` varchar(100) DEFAULT NULL,
  `bag_drawable` int NOT NULL,
  `bag_texture` int NOT NULL,
  `bagDlc` varchar(100) DEFAULT NULL,
  `accessoire_drawable` int NOT NULL,
  `accessoire_texture` int NOT NULL,
  `accessoireDlc` varchar(100) DEFAULT NULL,
  `armor_drawable` int NOT NULL,
  `armor_texture` int NOT NULL,
  `armorDlc` varchar(100) DEFAULT NULL,
  `hat_drawable` int NOT NULL,
  `hat_texture` int NOT NULL,
  `hatDlc` varchar(100) DEFAULT NULL,
  `glasses_drawable` int NOT NULL,
  `glasses_texture` int NOT NULL,
  `glassesDlc` varchar(100) DEFAULT NULL,
  `ears_drawable` int NOT NULL,
  `ears_texture` int NOT NULL,
  `earsDlc` varchar(100) DEFAULT NULL,
  `watches_drawable` int NOT NULL,
  `watches_texture` int NOT NULL,
  `watchesDlc` varchar(100) DEFAULT NULL,
  `bracelets_drawable` int NOT NULL,
  `bracelets_texture` int NOT NULL,
  `braceletsDlc` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`characterid`),
  KEY `characters` (`characterid`),
  CONSTRAINT `characters_id_fk` FOREIGN KEY (`characterid`) REFERENCES `characters` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `characterclothings`
--

LOCK TABLES `characterclothings` WRITE;
/*!40000 ALTER TABLE `characterclothings` DISABLE KEYS */;
/*!40000 ALTER TABLE `characterclothings` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:08:39
