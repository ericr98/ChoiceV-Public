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
-- Table structure for table `configsshopshelfsinshops`
--

DROP TABLE IF EXISTS `configsshopshelfsinshops`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configsshopshelfsinshops` (
  `shopId` int NOT NULL,
  `shelfId` int NOT NULL,
  PRIMARY KEY (`shopId`,`shelfId`),
  KEY `fk_configsshopshelfsinshops_shelfId_idx` (`shelfId`),
  CONSTRAINT `fk_configsshopshelfsinshops_shelfId` FOREIGN KEY (`shelfId`) REFERENCES `configshopshelfs` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_configsshopshelfsinshops_shopId` FOREIGN KEY (`shopId`) REFERENCES `configshops` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configsshopshelfsinshops`
--

LOCK TABLES `configsshopshelfsinshops` WRITE;
/*!40000 ALTER TABLE `configsshopshelfsinshops` DISABLE KEYS */;
INSERT INTO `configsshopshelfsinshops` VALUES (15,3),(16,3),(17,3),(21,3),(22,3),(23,3),(25,3),(27,3),(29,3),(31,3),(32,3),(38,3),(39,3),(55,3),(58,3),(15,6),(16,6),(17,6),(21,6),(22,6),(23,6),(25,6),(27,6),(29,6),(31,6),(32,6),(38,6),(39,6),(55,6),(58,6),(15,7),(16,7),(17,7),(21,7),(22,7),(23,7),(25,7),(27,7),(29,7),(31,7),(32,7),(38,7),(39,7),(55,7),(58,7),(15,8),(16,8),(17,8),(21,8),(22,8),(23,8),(25,8),(27,8),(29,8),(31,8),(32,8),(39,8),(55,8),(58,8),(15,9),(16,9),(17,9),(21,9),(22,9),(23,9),(25,9),(27,9),(29,9),(31,9),(32,9),(38,9),(39,9),(55,9),(58,9),(15,10),(16,10),(17,10),(21,10),(22,10),(23,10),(25,10),(27,10),(29,10),(31,10),(32,10),(38,10),(39,10),(55,10),(58,10),(15,11),(16,11),(17,11),(21,11),(22,11),(23,11),(25,11),(27,11),(29,11),(31,11),(32,11),(38,11),(39,11),(55,11),(58,11),(15,12),(16,12),(17,12),(18,12),(19,12),(20,12),(21,12),(22,12),(23,12),(24,12),(25,12),(27,12),(29,12),(30,12),(31,12),(32,12),(33,12),(38,12),(39,12),(55,12),(58,12),(30,13),(30,14),(21,15),(22,15),(23,15),(25,15),(27,15),(30,15),(31,15),(32,15),(39,15),(55,15),(58,15),(30,16),(30,17),(18,19),(19,19),(20,19),(24,19),(33,19),(15,23),(16,23),(17,23),(21,23),(22,23),(23,23),(25,23),(27,23),(29,23),(31,23),(32,23),(38,23),(39,23),(55,23),(58,23),(15,24),(16,24),(17,24),(29,24),(38,24),(15,25),(16,25),(17,25),(21,25),(22,25),(23,25),(25,25),(27,25),(29,25),(31,25),(32,25),(38,25),(39,25),(55,25),(58,25),(15,26),(17,26),(29,26),(38,26),(18,27),(19,27),(20,27),(24,27),(33,27),(18,28),(19,28),(20,28),(24,28),(33,28),(18,29),(19,29),(20,29),(24,29),(33,29),(41,30);
/*!40000 ALTER TABLE `configsshopshelfsinshops` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:45
