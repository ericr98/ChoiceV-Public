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
-- Table structure for table `hotelroomfeatureinstances`
--

DROP TABLE IF EXISTS `hotelroomfeatureinstances`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hotelroomfeatureinstances` (
  `id` int NOT NULL AUTO_INCREMENT,
  `bookingId` int DEFAULT NULL,
  `featureId` int NOT NULL,
  `codeFeature` varchar(45) NOT NULL,
  `isSaved` int NOT NULL,
  `savedCharId` int DEFAULT NULL,
  `savedDeleteDate` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_hotelroombookings_hotelroomfeatureinstances_featureInsta_idx` (`bookingId`),
  KEY `fk_confighotefeatures_hotelroomfeatureinstances_hotelroomId_idx` (`featureId`),
  KEY `fk_hotelroomBookings_characters_charId_idx` (`savedCharId`),
  CONSTRAINT `fk_confighotefeatures_hotelroomfeatureinstances_hotelroomId` FOREIGN KEY (`featureId`) REFERENCES `confighotelroomfeatures` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_hotelroomBookings_characters_charId` FOREIGN KEY (`savedCharId`) REFERENCES `characters` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_hotelroombookings_hotelroomfeatureinstances_featureInstances` FOREIGN KEY (`bookingId`) REFERENCES `hotelroombookings` (`id`) ON DELETE SET NULL ON UPDATE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=698 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `hotelroomfeatureinstances`
--

LOCK TABLES `hotelroomfeatureinstances` WRITE;
/*!40000 ALTER TABLE `hotelroomfeatureinstances` DISABLE KEYS */;
/*!40000 ALTER TABLE `hotelroomfeatureinstances` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:17:12
