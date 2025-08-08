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
-- Table structure for table `configbeardstylesgraph`
--

DROP TABLE IF EXISTS `configbeardstylesgraph`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configbeardstylesgraph` (
  `predecessor` int NOT NULL,
  `successor` int NOT NULL,
  PRIMARY KEY (`predecessor`,`successor`),
  KEY `fk_configbeardstylesgraph_successor_idx` (`successor`),
  CONSTRAINT `fk_configbeardstylesgraph_predccessor` FOREIGN KEY (`predecessor`) REFERENCES `configbeardstyles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_configbeardstylesgraph_successor` FOREIGN KEY (`successor`) REFERENCES `configbeardstyles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configbeardstylesgraph`
--

LOCK TABLES `configbeardstylesgraph` WRITE;
/*!40000 ALTER TABLE `configbeardstylesgraph` DISABLE KEYS */;
INSERT INTO `configbeardstylesgraph` VALUES (62,33),(63,33),(33,40),(37,40),(46,40),(50,40),(44,43),(47,43),(53,43),(55,43),(57,43),(59,43),(60,43),(61,43),(34,44),(35,44),(36,44),(38,44),(39,44),(40,44),(41,44),(42,44),(45,44),(48,44),(49,44),(52,44),(54,44),(56,44),(58,44),(43,51);
/*!40000 ALTER TABLE `configbeardstylesgraph` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:14:03
