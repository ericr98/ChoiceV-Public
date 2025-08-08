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
-- Table structure for table `confighairoverlays`
--

DROP TABLE IF EXISTS `confighairoverlays`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `confighairoverlays` (
  `id` int NOT NULL AUTO_INCREMENT,
  `collection` varchar(45) NOT NULL,
  `hash` varchar(45) NOT NULL,
  `displayName` varchar(45) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `confighairoverlays`
--

LOCK TABLES `confighairoverlays` WRITE;
/*!40000 ALTER TABLE `confighairoverlays` DISABLE KEYS */;
INSERT INTO `confighairoverlays` VALUES (1,'mpbeach_overlays','FM_Hair_Fuzz','Kurz'),(2,'multiplayer_overlays','NG_M_Hair_001','Kurz 2'),(3,'multiplayer_overlays','NG_M_Hair_003','Sidecut'),(4,'multiplayer_overlays','NG_M_Hair_008','Cornrow'),(5,'multiplayer_overlays','NG_M_Hair_010','Geheimrat'),(6,'multiplayer_overlays','NGBea_M_Hair_000','Geheimrat 2'),(7,'multiplayer_overlays','NGHip_M_Hair_001','Undercut 2'),(8,'mplowrider_overlays','LR_M_Hair_000','Cornrow 2'),(9,'mplowrider_overlays','LR_M_Hair_001','Cornrow 3'),(10,'mplowrider_overlays','LR_M_Hair_002','Cornrow 4'),(11,'mplowrider2_overlays','LR_M_Hair_004','Cornrow 5'),(12,'mplowrider2_overlays','LR_M_Hair_005','Cornrow 6'),(13,'mpbiker_overlays','MP_Biker_Hair_001_M','Undercut 3'),(14,'mpbiker_overlays','MP_Biker_Hair_003_M','Undercut 4'),(15,'mpgunrunning_overlays','MP_Gunrunning_Hair_M_000_M','Uppercut'),(16,'mpgunrunning_overlays','MP_Gunrunning_Hair_M_001_M','Uppercut 2'),(17,'multiplayer_overlays','NG_F_Hair_001','Mittellang '),(18,'multiplayer_overlays','NG_F_Hair_002','Mittellang 2'),(19,'multiplayer_overlays','NG_F_Hair_005','Braided'),(20,'multiplayer_overlays','NG_F_Hair_006','Braided 2'),(21,'multiplayer_overlays','NG_F_Hair_013','Undercut'),(22,'multiplayer_overlays','NGBea_F_Hair_001','Hohe Stirn'),(23,'mpbiker_overlays','MP_Biker_Hair_002_F','Sidecut 1'),(24,'mpbiker_overlays','MP_Biker_Hair_006_F','Dünn');
/*!40000 ALTER TABLE `confighairoverlays` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:45
