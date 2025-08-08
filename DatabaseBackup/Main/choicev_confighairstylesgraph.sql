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
-- Table structure for table `confighairstylesgraph`
--

DROP TABLE IF EXISTS `confighairstylesgraph`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `confighairstylesgraph` (
  `predecessor` int NOT NULL,
  `successor` int NOT NULL,
  PRIMARY KEY (`predecessor`,`successor`),
  KEY `fk_confighairstylesgraph_successor_idx` (`successor`),
  CONSTRAINT `fk_confighairstylesgraph_pre` FOREIGN KEY (`predecessor`) REFERENCES `confighairstyles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_confighairstylesgraph_suc` FOREIGN KEY (`successor`) REFERENCES `confighairstyles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `confighairstylesgraph`
--

LOCK TABLES `confighairstylesgraph` WRITE;
/*!40000 ALTER TABLE `confighairstylesgraph` DISABLE KEYS */;
INSERT INTO `confighairstylesgraph` VALUES (4,7),(7,9),(8,10),(10,12),(11,12),(9,15),(48,15),(12,20),(13,20),(18,20),(56,20),(15,26),(16,26),(147,26),(17,27),(19,27),(20,27),(22,27),(23,27),(24,27),(25,27),(26,27),(28,27),(30,27),(57,27),(59,27),(60,27),(61,27),(62,27),(139,27),(145,27),(31,37),(32,37),(33,37),(34,37),(35,37),(36,37),(37,38),(54,38),(21,39),(27,39),(29,39),(51,39),(58,39),(63,39),(14,42),(39,42),(40,42),(41,47),(42,47),(43,47),(44,47),(49,47),(52,47),(53,47),(55,47),(38,49),(45,52),(80,65),(96,66),(102,66),(97,69),(98,69),(76,72),(65,74),(66,75),(71,75),(82,75),(87,75),(92,75),(100,75),(144,76),(86,78),(99,84),(70,86),(89,86),(90,86),(91,86),(93,86),(94,86),(142,86),(69,87),(77,89),(72,96),(99,97),(144,99),(67,106),(68,106),(73,106),(75,106),(81,106),(83,106),(95,106),(113,106),(121,106),(124,106),(126,106),(83,111),(78,119),(80,119),(104,120),(109,120),(110,120),(115,120),(123,120),(127,120),(128,120),(129,120),(131,120),(132,120),(137,120),(140,120),(143,120),(113,126),(125,126),(135,126),(88,129),(101,129),(103,129),(105,129),(106,129),(107,129),(108,129),(111,129),(114,129),(118,129),(130,129),(134,129),(136,129),(119,130),(74,135),(79,135),(85,135),(133,135),(141,135),(144,140),(84,142),(64,144);
/*!40000 ALTER TABLE `confighairstylesgraph` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:13:36
