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
-- Table structure for table `configbeardstyles`
--

DROP TABLE IF EXISTS `configbeardstyles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configbeardstyles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `gtaId` int NOT NULL,
  `level` int NOT NULL,
  `name` varchar(45) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=64 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configbeardstyles`
--

LOCK TABLES `configbeardstyles` WRITE;
/*!40000 ALTER TABLE `configbeardstyles` DISABLE KEYS */;
INSERT INTO `configbeardstyles` VALUES (33,0,1,'Stoppeln'),(34,1,2,'Anker'),(35,2,2,'Goatee'),(36,3,2,'Baldo 1'),(37,4,1,'Kinbart'),(38,5,2,'Baldo kurz'),(39,6,2,'Chin Strap'),(40,7,2,'lange Stoppeln'),(41,8,2,'Anker + Chin Puff'),(42,9,2,'Chevron'),(43,10,4,'Vollbart'),(44,11,3,'kurzer Vollbart'),(45,12,2,'kurzer Goatee'),(46,13,1,'Stoppeln 2'),(47,14,3,'lange Stoppeln 2'),(48,15,2,'Chinstrap 2'),(49,16,2,'Anker 2'),(50,17,1,'Side Whiskers'),(51,18,5,'langer Vollbart'),(52,19,2,'Schnauzbart'),(53,20,3,'erw. Schnurrbart'),(54,21,2,'Horseshoe'),(55,22,3,'Zappa'),(56,23,2,'Schnauzbart 2'),(57,24,3,'Schnauz-Vollbart'),(58,25,2,'kurzer Horseshoe'),(59,26,3,'Mutton Chops'),(60,27,3,'Mutton Chops 2'),(61,28,3,'Kinn Vorhang'),(62,-1,0,'kein Bart'),(63,29,0,'kein Bart');
/*!40000 ALTER TABLE `configbeardstyles` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:16
