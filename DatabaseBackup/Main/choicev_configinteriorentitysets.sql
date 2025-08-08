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
-- Table structure for table `configinteriorentitysets`
--

DROP TABLE IF EXISTS `configinteriorentitysets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configinteriorentitysets` (
  `id` int NOT NULL AUTO_INCREMENT,
  `entitySetId` int NOT NULL,
  `gtaName` varchar(45) NOT NULL,
  `setName` varchar(45) NOT NULL,
  `displayName` varchar(45) NOT NULL,
  `isLoaded` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_configinteriorentitysetoptions_entitySetId_idx` (`entitySetId`),
  CONSTRAINT `fk_configinteriorentitysetoptions_entitySetId` FOREIGN KEY (`entitySetId`) REFERENCES `configinteriorentitysetspots` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=39 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configinteriorentitysets`
--

LOCK TABLES `configinteriorentitysets` WRITE;
/*!40000 ALTER TABLE `configinteriorentitysets` DISABLE KEYS */;
INSERT INTO `configinteriorentitysets` VALUES (7,2,'clean','Besprechung','abgebaut',1),(8,2,'besprechung','Besprechung','aufgebaut',0),(10,3,'open_sign','Open-/Close Screen','geöffnet',1),(11,3,'closed_sign','Open-/Close Screen','geschlossen',0),(12,4,'bar_chairs_in','Barstühle','reingestellt',1),(13,4,'bar_chairs_out','Barstühle','ausgestellt',0),(14,5,'south_courtroom_curtains_open','Vorhänge','geöffnet',1),(15,5,'south_courtroom_curtains_closed','Vorhänge','geschlossen',0),(16,6,'north_courtroom_curtains_open','Vorhänge','geöffnet',1),(17,6,'north_courtroom_curtains_closed','Vorhänge','geschlossen',0),(18,7,'high_security_setup','Security Setup','aufgebaut',1),(19,7,'high_security_setup_reduced','Security Setup','abgebaut',0),(20,8,'csr_beforeMission','PDM Einrichtung','eingerichtet',1),(21,9,'shutter_closed','PDM Garagentor','geschlossen',1),(22,9,'shutter_open','PDM Garagentor','geöffnet',0),(23,10,'neon_lila_cyan','Neonlicht Steuerung','Lila - Cyan',1),(24,10,'neon_lila','Neonlicht Steuerung','Lila',0),(25,10,'neon_cyan','Neonlicht Steuerung','Cyan',0),(26,10,'neon_aus','Neonlicht Steuerung','Aus',0),(27,10,'neon_red_orange','Neonlicht Steuerung','Rot- Orange',0),(28,10,'neon_orange','Neonlicht Steuerung','Orange',0),(29,10,'neon_green_blue','Neonlicht Steuerung','Grün - Blau',0),(30,10,'neon_green','Neonlicht Steuerung','Grün',0),(31,10,'neon_blue','Neonlicht Steuerung','Blau',0),(32,10,'neon_white_pink','Neonlicht Steuerung','Weiß - Pink',0),(33,10,'neon_pink','Neonlicht Steuerung','Pink',0),(34,10,'neon_white','Neonlicht Steuerung','Weiß',0),(35,11,'chairs_under_table','Stühle','reingestellt',1),(36,11,'chairs_away_from_table','Stühle','rausgestellt',0),(37,12,'v_michael_s_items','Michaels Gegenstaende','Wohnung',1),(38,12,'v_michael_l_items','Michaels Gegenstaende','Wohnung',1);
/*!40000 ALTER TABLE `configinteriorentitysets` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:16:21
